// Assets/_MythHunter/Code/Entities/Heroes/HeroFactory.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Entities.Archetypes;
using MythHunter.Utils.Logging;

namespace MythHunter.Entities.Heroes
{
    public interface IHeroFactory
    {
        int CreateHero(string archetypeId, string heroName, int teamId);
        int CreateCustomHero(string archetypeId, Dictionary<Type, object> overrides);
        string[] GetAvailableHeroArchetypeIds();
    }

    public class HeroFactory : IHeroFactory
    {
        private readonly IEntityManager _entityManager;
        private readonly IArchetypeSystem _archetypeSystem;
        private readonly IHeroArchetypeRegistry _heroArchetypeRegistry;
        private readonly IMythLogger _logger;

        [Inject]
        public HeroFactory(
            IEntityManager entityManager,
            IArchetypeSystem archetypeSystem,
            IHeroArchetypeRegistry heroArchetypeRegistry,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _archetypeSystem = archetypeSystem;
            _heroArchetypeRegistry = heroArchetypeRegistry;
            _logger = logger;
        }

        public int CreateHero(string archetypeId, string heroName, int teamId)
        {
            var overrides = new Dictionary<Type, object>();

            // Додаємо специфічні для героя характеристики
            if (!string.IsNullOrEmpty(heroName))
            {
                overrides[typeof(Components.Core.NameComponent)] = new Components.Core.NameComponent { Name = heroName };
            }

            // Встановлюємо команду
            overrides[typeof(Components.Combat.TeamComponent)] = new Components.Combat.TeamComponent { TeamId = teamId };

            // Створюємо героя за допомогою ArchetypeSystem
            int entityId = _archetypeSystem.CreateEntityFromArchetype(archetypeId, overrides);

            if (entityId >= 0)
            {
                _logger.LogInfo($"Created hero '{heroName}' with archetype '{archetypeId}' and ID {entityId}", "HeroFactory");
            }
            else
            {
                _logger.LogError($"Failed to create hero with archetype '{archetypeId}'", "HeroFactory");
            }

            return entityId;
        }

        public int CreateCustomHero(string archetypeId, Dictionary<Type, object> overrides)
        {
            // Створюємо героя з повністю власними параметрами
            int entityId = _archetypeSystem.CreateEntityFromArchetype(archetypeId, overrides);

            if (entityId >= 0)
            {
                _logger.LogInfo($"Created custom hero with archetype '{archetypeId}' and ID {entityId}", "HeroFactory");
            }
            else
            {
                _logger.LogError($"Failed to create custom hero with archetype '{archetypeId}'", "HeroFactory");
            }

            return entityId;
        }

        public string[] GetAvailableHeroArchetypeIds()
        {
            var archetypes = _heroArchetypeRegistry.GetAllHeroArchetypes();
            var ids = new string[archetypes.Count];

            int index = 0;
            foreach (var kvp in archetypes)
            {
                ids[index++] = kvp.Key;
            }

            return ids;
        }
    }
}
