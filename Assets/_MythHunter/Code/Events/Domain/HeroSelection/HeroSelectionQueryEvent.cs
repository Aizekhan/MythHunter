// Assets/_MythHunter/Code/Events/Domain/HeroSelection/HeroSelectionQueryEvent.cs
using System;

namespace MythHunter.Events.Domain.HeroSelection
{
    /// <summary>
    /// EPIC 08 Phase 2: Query hero selection information
    /// Replaces direct HeroSelectionSystem queries
    /// </summary>
    public class HeroSelectionQueryEvent : IEvent
    {
        public enum QueryType
        {
            CanSelectHero,      // Can player select this hero?
            GetHeroInfo,        // Get hero information
            GetHeroesByCategory // Get all heroes grouped by category
        }

        public QueryType Type { get; set; }
        public string ArchetypeId { get; set; } // For CanSelectHero, GetHeroInfo
        public int PlayerIndex { get; set; }    // For CanSelectHero
        public int RemainingMana { get; set; }  // For CanSelectHero
        public string RequestId { get; set; }   // For tracking request/response
        public DateTime Timestamp { get; set; }

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
