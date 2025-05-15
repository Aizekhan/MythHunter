// Assets/_MythHunter/Code/Systems/Lobby/HeroSelectionSystem.cs
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
using MythHunter.Entities;
using MythHunter.Resources.Core;

namespace MythHunter.Systems.Lobby
{
    /// <summary>
    /// Система вибору героїв
    /// </summary>
    public class HeroSelectionSystem : SystemBase, IHeroSelectionSystem
    {
        private readonly IEntityManager _entityManager;
        private readonly IResourceManager _resourceManager;
        private readonly IArchetypeSystem _archetypeSystem;
        private readonly IArchetypeTemplateRegistry _archetypeTemplateRegistry;
        private readonly Dictionary<string, HeroInfo> _heroInfos = new Dictionary<string, HeroInfo>();

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

        public override void Initialize()
        {
            base.Initialize();
            LoadAvailableHeroes();
        }

        public void LoadAvailableHeroes()
        {
            _heroInfos.Clear();

            // Отримуємо всі доступні архетипи героїв з реєстру
            var archetypeIds = _archetypeTemplateRegistry.GetAllTemplateIds()
                .Where(id => id.StartsWith("Character") || id.StartsWith("Hero"))
                .ToList();

            _logger.LogInfo($"Found {archetypeIds.Count} hero archetypes in registry", "HeroSelection");

            foreach (var archetypeId in archetypeIds)
            {
                try
                {
                    // Тут можна задати вартість героя на основі якихось правил
                    // Наприклад, вартість може залежати від сили героя або його рідкості
                    int manaCost = CalculateManaCostForArchetype(archetypeId);

                    var heroInfo = new HeroInfo
                    {
                        ArchetypeId = archetypeId,
                        Name = GetHeroNameFromArchetype(archetypeId),
                        Description = GetHeroDescriptionFromArchetype(archetypeId),
                        Race = GetHeroRaceFromArchetype(archetypeId),
                        Class = GetHeroClassFromArchetype(archetypeId),
                        ManaCost = manaCost,
                        Category = GetCategoryFromHeroClass(GetHeroClassFromArchetype(archetypeId)),
                        IconPath = GetHeroIconPathFromArchetype(archetypeId)
                    };
                    if (string.IsNullOrEmpty(heroInfo.Name))
                    {
                        heroInfo.Name = archetypeId;
                        _logger.LogWarning($"Hero archetype {archetypeId} має порожнє Name, встановлено ArchetypeId як імʼя", "HeroSelection");
                    }
                    if (string.IsNullOrEmpty(heroInfo.IconPath))
                    {
                        _logger.LogWarning($"Hero archetype {archetypeId} має порожній IconPath", "HeroSelection");
                    }

                    _heroInfos.Add(archetypeId, heroInfo);

                    _logger.LogDebug($"Added hero archetype: {archetypeId}, Name: {heroInfo.Name}, ManaCost: {manaCost}", "HeroSelection");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to process hero archetype {archetypeId}: {ex.Message}", "HeroSelection", ex);
                }
            }

            _logger.LogInfo($"Loaded {_heroInfos.Count} hero archetypes", "HeroSelection");
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

            // Перевіряємо, чи достатньо мани
            if (heroInfo.ManaCost > remainingMana)
            {
                _logger.LogInfo($"Not enough mana to select hero {archetypeId} (cost: {heroInfo.ManaCost}, remaining: {remainingMana})", "HeroSelection");
                return false;
            }

            // Перевіряємо, чи не вибраний цей герой вже іншим гравцем
            foreach (var entityId in _entityManager.GetEntitiesWith<HeroSelectionComponent>())
            {
                var selection = _entityManager.GetComponent<HeroSelectionComponent>(entityId);

                if (selection.ArchetypeId == archetypeId && selection.IsSelected && selection.PlayerIndex != playerIndex)
                {
                    _logger.LogInfo($"Hero {archetypeId} already selected by player {selection.PlayerIndex}", "HeroSelection");
                    return false;
                }
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

        // Визначення вартості героя на основі архетипу
        private int CalculateManaCostForArchetype(string archetypeId)
        {
            // Тут можна реалізувати логіку визначення вартості героя
            // Можна використовувати компоненти героя для аналізу його сили

            // Простий приклад: вартість залежить від довжини ідентифікатора архетипу
            // У реальному коді ви б використовували щось більш осмислене
            return Math.Max(1, Math.Min(4, archetypeId.Length % 4 + 1));
        }

        // Отримання імені героя з архетипу
        private string GetHeroNameFromArchetype(string archetypeId)
        {
            // Видаляємо префікс "Character" або "Hero", якщо він є
            string name = archetypeId
                .Replace("Character", "")
                .Replace("Hero", "");

            // Додаємо пробіли перед великими літерами
            name = System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])", " $1").Trim();

            return name;
        }

        // Отримання опису героя з архетипу
        private string GetHeroDescriptionFromArchetype(string archetypeId)
        {
            // У реальній реалізації тут би використовувалися дані з архетипу
            return $"Герой з архетипу {archetypeId}";
        }

        // Отримання раси героя з архетипу
        private string GetHeroRaceFromArchetype(string archetypeId)
        {
            // Спрощена логіка визначення раси на основі ідентифікатора архетипу
            if (archetypeId.Contains("Elf") || archetypeId.Contains("Elven"))
                return "Ельф";
            if (archetypeId.Contains("Dwarf") || archetypeId.Contains("Dwarven"))
                return "Гном";
            if (archetypeId.Contains("Orc") || archetypeId.Contains("Orcish"))
                return "Орк";

            // За замовчуванням
            return "Людина";
        }

        // Отримання класу героя з архетипу
        private string GetHeroClassFromArchetype(string archetypeId)
        {
            // Спрощена логіка визначення класу на основі ідентифікатора архетипу
            if (archetypeId.Contains("Warrior") || archetypeId.Contains("Fighter"))
                return "Воїн";
            if (archetypeId.Contains("Mage") || archetypeId.Contains("Wizard"))
                return "Маг";
            if (archetypeId.Contains("Archer") || archetypeId.Contains("Ranger"))
                return "Лучник";
            if (archetypeId.Contains("Tank") || archetypeId.Contains("Defender"))
                return "Танк";
            if (archetypeId.Contains("Assassin") || archetypeId.Contains("Rogue"))
                return "Асасин";
            if (archetypeId.Contains("Support") || archetypeId.Contains("Healer"))
                return "Підтримка";

            // За замовчуванням
            return "Воїн";
        }

        // Отримання шляху до іконки героя з архетипу
        private string GetHeroIconPathFromArchetype(string archetypeId)
        {
            // У реальній реалізації тут би використовувалися дані з архетипу
            return $"Icons/{archetypeId}";
        }

        // Визначаємо категорію на основі класу героя
        private string GetCategoryFromHeroClass(string heroClass)
        {
            switch (heroClass.ToLower())
            {
                case "воїн":
                case "берсерк":
                case "паладин":
                    return "Бійці";
                case "маг":
                case "чарівник":
                case "чаклун":
                    return "Маги";
                case "лучник":
                case "мисливець":
                case "рейнджер":
                    return "Стрільці";
                case "танк":
                case "захисник":
                case "вартовий":
                    return "Захисники";
                case "асасин":
                case "розбійник":
                case "ніндзя":
                    return "Вбивці";
                case "підтримка":
                case "цілитель":
                case "бард":
                    return "Підтримка";
                default:
                    return "Інші";
            }
        }
    }

    /// <summary>
    /// Інформація про героя
    /// </summary>
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
