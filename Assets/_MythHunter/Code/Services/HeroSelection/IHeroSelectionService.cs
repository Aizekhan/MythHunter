// Assets/_MythHunter/Code/Services/HeroSelection/IHeroSelectionService.cs
using Cysharp.Threading.Tasks;
using MythHunter.Events.Domain.HeroSelection;
using System.Collections.Generic;

namespace MythHunter.Services.HeroSelection
{
    /// <summary>
    /// EPIC 08 Phase 2: Hero selection service (decoupled from system architecture)
    /// Provides hero selection functionality without system coupling
    /// </summary>
    public interface IHeroSelectionService
    {
        /// <summary>
        /// Load available heroes
        /// </summary>
        UniTask LoadAvailableHeroes();

        /// <summary>
        /// Get hero information by archetype ID
        /// </summary>
        HeroInfo GetHeroInfo(string archetypeId);

        /// <summary>
        /// Check if player can select hero
        /// </summary>
        bool CanSelectHero(string archetypeId, int playerIndex, int remainingMana);

        /// <summary>
        /// Get heroes grouped by category for UI
        /// </summary>
        Dictionary<string, List<string>> GetHeroesByCategory();
    }
}
