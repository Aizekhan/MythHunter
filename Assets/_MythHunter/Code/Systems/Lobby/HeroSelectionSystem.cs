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
            await LoadAvailableHeroes();
        }

        public async UniTask LoadAvailableHeroes()
        {
            _heroInfos.Clear();

            var archetypeIds = _archetypeTemplateRegistry.GetAllTemplateIds()
                .Where(id => id.StartsWith("Character") || id.StartsWith("Hero"))
                .ToList();

            _logger.LogInfo($"Found {archetypeIds.Count} hero archetypes in registry", "HeroSelection");

            foreach (var archetypeId in archetypeIds)
            {
                int manaCost = CalculateManaCostForArchetype(archetypeId);

                var heroArchetype = await _resourceManager.LoadAsync<HeroArchetypeSO>($"ScriptableObjects/Heroes/{archetypeId}");


                if (heroArchetype == null)
                {
                    _logger.LogWarning($"HeroArchetypeSO not found for ID: {archetypeId}", "HeroSelection");
                    continue;
                }

                var heroInfo = new HeroInfo
                {
                    ArchetypeId = archetypeId,
                    Name = heroArchetype.HeroName,
                    Description = heroArchetype.Description,
                    Race = heroArchetype.Race.ToString(),
                    Class = heroArchetype.Class.ToString(),
                    ManaCost = manaCost,
                    Category = GetCategoryFromHeroClass(heroArchetype.Class.ToString()),
                    IconPath = heroArchetype.IconPath
                };

                if (string.IsNullOrWhiteSpace(heroInfo.Name))
                {
                    heroInfo.Name = archetypeId;
                    _logger.LogWarning($"Hero archetype {archetypeId} має порожнє Name", "HeroSelection");
                }

                if (string.IsNullOrWhiteSpace(heroInfo.IconPath))
                {
                    _logger.LogWarning($"Hero archetype {archetypeId} має порожній IconPath", "HeroSelection");
                }

                _heroInfos.Add(archetypeId, heroInfo);
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

            if (heroInfo.ManaCost > remainingMana)
            {
                _logger.LogInfo($"Not enough mana to select hero {archetypeId} (cost: {heroInfo.ManaCost}, remaining: {remainingMana})", "HeroSelection");
                return false;
            }

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

        private int CalculateManaCostForArchetype(string archetypeId)
        {
            return Math.Max(1, Math.Min(4, archetypeId.Length % 4 + 1));
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
