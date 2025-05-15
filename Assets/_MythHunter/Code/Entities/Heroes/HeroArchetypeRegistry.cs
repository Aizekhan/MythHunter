// Assets/_MythHunter/Code/Entities/Heroes/HeroArchetypeRegistry.cs
using System.Collections.Generic;
using System;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Entities.Archetypes
{
    // Утиліта для завантаження всіх архетипів героїв зі ScriptableObjects
    public class HeroArchetypeRegistry : IHeroArchetypeRegistry
    {
        private readonly ArchetypeTemplateRegistry _templateRegistry;
        private readonly IMythLogger _logger;
        private readonly Dictionary<string, HeroArchetypeSO> _heroArchetypes = new Dictionary<string, HeroArchetypeSO>();

        [Inject]
        public HeroArchetypeRegistry(ArchetypeTemplateRegistry templateRegistry, IMythLogger logger)
        {
            _templateRegistry = templateRegistry;
            _logger = logger;

            LoadAllHeroArchetypes();
        }

        private void LoadAllHeroArchetypes()
        {
            // Завантаження всіх архетипів героїв з Resources
            HeroArchetypeSO[] heroArchetypes = UnityEngine.Resources.LoadAll<HeroArchetypeSO>("ScriptableObjects/Heroes");

            foreach (var archetype in heroArchetypes)
            {
                if (string.IsNullOrEmpty(archetype.ArchetypeId))
                {
                    _logger.LogWarning($"Hero archetype {archetype.name} has empty ArchetypeId. Skipping.", "HeroArchetype");
                    continue;
                }

                _heroArchetypes[archetype.ArchetypeId] = archetype;
                archetype.RegisterWithArchetypeSystem(_templateRegistry);

                _logger.LogInfo($"Registered hero archetype: {archetype.ArchetypeId} - {archetype.HeroName}", "HeroArchetype");
            }

            _logger.LogInfo($"Loaded {_heroArchetypes.Count} hero archetypes", "HeroArchetype");
        }

        public IReadOnlyDictionary<string, HeroArchetypeSO> GetAllHeroArchetypes()
        {
            return _heroArchetypes;
        }

        public HeroArchetypeSO GetHeroArchetype(string archetypeId)
        {
            if (_heroArchetypes.TryGetValue(archetypeId, out var archetype))
            {
                return archetype;
            }

            _logger.LogWarning($"Hero archetype not found: {archetypeId}", "HeroArchetype");
            return null;
        }
    }

    public interface IHeroArchetypeRegistry
    {
        IReadOnlyDictionary<string, HeroArchetypeSO> GetAllHeroArchetypes();
        HeroArchetypeSO GetHeroArchetype(string archetypeId);
    }
}
