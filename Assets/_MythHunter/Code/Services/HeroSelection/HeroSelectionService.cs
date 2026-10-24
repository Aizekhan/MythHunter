// Assets/_MythHunter/Code/Services/HeroSelection/HeroSelectionService.cs
using Cysharp.Threading.Tasks;
using RPGFramework.DI;
using MythHunter.Events.Domain.HeroSelection;
using MythHunter.Systems.Lobby;
using MythHunter.Utils.Logging;
using System;
using System.Collections.Generic;

namespace MythHunter.Services.HeroSelection
{
    /// <summary>
    /// EPIC 08 Phase 2: Hero selection service implementation
    /// Delegates to HeroSelectionSystem but provides service-level interface
    /// This breaks system-to-system coupling by introducing service layer
    /// </summary>
    public class HeroSelectionService : IHeroSelectionService
    {
        private readonly IHeroSelectionSystem _heroSelectionSystem;
        private readonly IMythLogger _logger;

        [Inject]
        public HeroSelectionService(
            IHeroSelectionSystem heroSelectionSystem,
            IMythLogger logger)
        {
            _heroSelectionSystem = heroSelectionSystem;
            _logger = logger;
        }

        public UniTask LoadAvailableHeroes()
        {
            return _heroSelectionSystem.LoadAvailableHeroes();
        }

        public Events.Domain.HeroSelection.HeroInfo GetHeroInfo(string archetypeId)
        {
            var systemHeroInfo = _heroSelectionSystem.GetHeroInfo(archetypeId);

            if (systemHeroInfo == null)
                return null;

            // Convert from system HeroInfo to service HeroInfo (from events namespace)
            return new Events.Domain.HeroSelection.HeroInfo
            {
                ArchetypeId = systemHeroInfo.ArchetypeId,
                Name = systemHeroInfo.Name,
                Category = systemHeroInfo.Category,
                ManaCost = systemHeroInfo.ManaCost,
                Description = systemHeroInfo.Description
            };
        }

        public bool CanSelectHero(string archetypeId, int playerIndex, int remainingMana)
        {
            return _heroSelectionSystem.CanSelectHero(archetypeId, playerIndex, remainingMana);
        }

        public Dictionary<string, List<string>> GetHeroesByCategory()
        {
            return _heroSelectionSystem.GetHeroesByCategory();
        }
    }
}
