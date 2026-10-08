// Шлях: Assets/_MythHunter/Code/Systems/Lobby/HeroSelectionSystem.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Components.Lobby;
using MythHunter.Events;
using MythHunter.Resources;
using MythHunter.Utils.Logging;
using MythHunter.Entities.Archetypes;
using MythHunter.Resources.Core;
using Cysharp.Threading.Tasks;
using MythHunter.Systems.Core;

namespace MythHunter.Systems.Lobby
{
    
    public class HeroSelectionSystem : SystemBase, IHeroSelectionSystem
    {
        private readonly IEntityManager _entityManager;
        private readonly IResourceManager _resourceManager;
        private readonly IArchetypeSystem _archetypeSystem;
        private readonly IArchetypeTemplateRegistry _archetypeTemplateRegistry;
        private readonly Dictionary<string, HeroInfo> _heroInfos = new();

        [Inject]
        public HeroSelectionSystem(
            IEntityManager entityManager,
            IResourceManager resourceManager,
            IArchetypeSystem archetypeSystem,
            IArchetypeTemplateRegistry archetypeTemplateRegistry,
            IEventBus eventBus,
            IMythLogger logger)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;
            _resourceManager = resourceManager;
            _archetypeSystem = archetypeSystem;
            _archetypeTemplateRegistry = archetypeTemplateRegistry;
        }

        public override async void Initialize()
        {
            base.Initialize();

            try
            {
                await LoadAvailableHeroes();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при ініціалізації HeroSelectionSystem: {ex.Message}", "HeroSelection", ex);
            }
        }

        // Assets/_MythHunter/Code/Systems/Lobby/HeroSelectionSystem.cs
        public  UniTask LoadAvailableHeroes() // правильно
        {
            _heroInfos.Clear();

            _logger.LogInfo("Початок завантаження героїв", "HeroSelection");

            try
            {
                // Спробуємо отримати архетипи з різних шляхів
                var heroArchetypes = UnityEngine.Resources.LoadAll<HeroArchetypeSO>("ScriptableObjects/Heroes");

                _logger.LogInfo($"Знайдено {heroArchetypes.Length} архетипів героїв", "HeroSelection");

                // Обробляємо знайдені архетипи напряму
                foreach (var heroArchetype in heroArchetypes)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(heroArchetype.ArchetypeId))
                        {
                            _logger.LogWarning($"HeroArchetype {heroArchetype.name} має порожній ArchetypeId", "HeroSelection");
                            continue;
                        }

                        // Додаткова перевірка на дублікати
                        if (_heroInfos.ContainsKey(heroArchetype.ArchetypeId))
                        {
                            _logger.LogWarning($"Дублікат архетипу героя: {heroArchetype.ArchetypeId}. Пропускаємо.", "HeroSelection");
                            continue;
                        }

                        var heroInfo = new HeroInfo
                        {
                            ArchetypeId = heroArchetype.ArchetypeId,
                            Name = string.IsNullOrEmpty(heroArchetype.HeroName) ? heroArchetype.ArchetypeId : heroArchetype.HeroName,
                            Description = heroArchetype.Description,
                            Race = heroArchetype.Race.ToString(),
                            Class = heroArchetype.Class.ToString(),
                            ManaCost = heroArchetype.ManaCost,
                            Category = GetCategoryFromHeroClass(heroArchetype.Class.ToString()),
                            IconPath = heroArchetype.IconPath
                        };
                        _logger.LogInfo($"Додано героя: {heroInfo.Name} (ID: {heroInfo.ArchetypeId})", "HeroSelection");
                        _heroInfos.Add(heroArchetype.ArchetypeId, heroInfo);
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Помилка при обробці архетипу {heroArchetype.name}: {ex.Message}", "HeroSelection", ex);
                    }
                }

                // Якщо не знайдено жодного героя, додаємо тестові дані
                if (_heroInfos.Count == 0)
                {
                    _logger.LogWarning("Не знайдено жодного архетипу героя, додаємо тестові дані", "HeroSelection");
                    AddTestHeroes();
                }

                _logger.LogInfo($"Завантажено {_heroInfos.Count} героїв", "HeroSelection");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Критична помилка при завантаженні героїв: {ex.Message}", "HeroSelection", ex);
                AddTestHeroes();
            }
            return UniTask.CompletedTask;
        }
        private void AddTestHeroes()
        {
            _logger.LogWarning("Додаємо тестові дані для героїв", "HeroSelection");

            // Додавання тестових героїв
            _heroInfos.Add("TestWarrior", new HeroInfo
            {
                ArchetypeId = "TestWarrior",
                Name = "Воїн",
                Description = "Потужний воїн близького бою",
                Race = "Human",
                Class = "Warrior",
                ManaCost = 2,
                Category = "Бійці",
                IconPath = "UI/Icons/warrior_icon"
            });

            _heroInfos.Add("TestMage", new HeroInfo
            {
                ArchetypeId = "TestMage",
                Name = "Маг",
                Description = "Могутній маг з дальніми атаками",
                Race = "Elf",
                Class = "Mage",
                ManaCost = 3,
                Category = "Маги",
                IconPath = "UI/Icons/mage_icon"
            });

            _heroInfos.Add("TestRogue", new HeroInfo
            {
                ArchetypeId = "TestRogue",
                Name = "Розбійник",
                Description = "Швидкий і небезпечний розбійник",
                Race = "Human",
                Class = "Rogue",
                ManaCost = 1,
                Category = "Вбивці",
                IconPath = "UI/Icons/rogue_icon"
            });

            _logger.LogInfo($"Додано {_heroInfos.Count} тестових героїв", "HeroSelection");
            return;
        }

        public HeroInfo GetHeroInfo(string archetypeId)
        {
            if (_heroInfos.TryGetValue(archetypeId, out var heroInfo))
            {
                return heroInfo;
            }

            _logger.LogWarning($"Hero with archetype ID {archetypeId} not found", "HeroSelection");
            return null;
        }

        public bool CanSelectHero(string archetypeId, int playerIndex, int remainingMana)
        {
            if (!_heroInfos.TryGetValue(archetypeId, out var heroInfo))
            {
                _logger.LogWarning($"Hero with archetype ID {archetypeId} not found", "HeroSelection");
                return false;
            }

            if (heroInfo.ManaCost > remainingMana)
            {
                _logger.LogInfo($"Not enough mana to select hero {archetypeId} (cost: {heroInfo.ManaCost}, remaining: {remainingMana})", "HeroSelection");
                return false;
            }

            // Перевіряємо, чи не вибраний цей герой іншим гравцем
            // Тут використовуємо більш надійний спосіб перевірки вибраних героїв
            var selectedHeroes = _entityManager.GetEntitiesWith<HeroSelectionComponent>()
                .Select(id => _entityManager.GetComponent<HeroSelectionComponent>(id))
                .Where(selection => selection.IsSelected && selection.ArchetypeId == archetypeId && selection.PlayerIndex != playerIndex)
                .ToList();

            if (selectedHeroes.Any())
            {
                _logger.LogInfo($"Hero {archetypeId} already selected by another player", "HeroSelection");
                return false;
            }

            return true;
        }

        public Dictionary<string, List<string>> GetHeroesByCategory()
        {
            var result = new Dictionary<string, List<string>>();

            foreach (var heroInfo in _heroInfos.Values)
            {
                if (!result.ContainsKey(heroInfo.Category))
                {
                    result[heroInfo.Category] = new List<string>();
                }

                result[heroInfo.Category].Add(heroInfo.ArchetypeId);
            }

            return result;
        }

       

        private string GetCategoryFromHeroClass(string heroClass)
        {
            return heroClass.ToLower() switch
            {
                "воїн" or "берсерк" or "паладин" => "Бійці",
                "маг" or "чарівник" or "чаклун" => "Маги",
                "лучник" or "мисливець" or "рейнджер" => "Стрільці",
                "танк" or "захисник" or "вартовий" => "Захисники",
                "асасин" or "розбійник" or "ніндзя" => "Вбивці",
                "підтримка" or "цілитель" or "бард" => "Підтримка",
                _ => "Інші",
            };
        }
    }

    public class HeroInfo
    {
        public string ArchetypeId
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public string Description
        {
            get; set;
        }
        public string Race
        {
            get; set;
        }
        public string Class
        {
            get; set;
        }
        public int ManaCost
        {
            get; set;
        }
        public string Category
        {
            get; set;
        }
        public string IconPath
        {
            get; set;
        }
    }
}
