// Assets/_MythHunter/Code/Events/Domain/HeroSelection/HeroSelectionQueryResponseEvent.cs
using System;
using System.Collections.Generic;

namespace MythHunter.Events.Domain.HeroSelection
{
    /// <summary>
    /// EPIC 08 Phase 2: Response to hero selection query
    /// Published by HeroSelectionSystem after processing query
    /// </summary>
    public class HeroSelectionQueryResponseEvent : IEvent
    {
        public HeroSelectionQueryEvent.QueryType Type { get; set; }
        public string RequestId { get; set; } // Matches request for tracking

        // CanSelectHero response
        public bool CanSelect { get; set; }

        // GetHeroInfo response
        public HeroInfo HeroInfo { get; set; }

        // GetHeroesByCategory response
        public Dictionary<string, List<string>> HeroesByCategory { get; set; }

        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Hero information structure
    /// </summary>
    public class HeroInfo
    {
        public string ArchetypeId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int ManaCost { get; set; }
        public string Description { get; set; }
    }
}
