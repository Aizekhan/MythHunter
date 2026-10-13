// Assets/_MythHunter/Code/Events/Domain/Archetype/EntityFromArchetypeCreatedEvent.cs
using System;

namespace MythHunter.Events.Domain.Archetype
{
    /// <summary>
    /// EPIC 08 Phase 2: Response when entity is created from archetype
    /// Published by ArchetypeSystem after creating entity
    /// </summary>
    public struct EntityFromArchetypeCreatedEvent : IEvent
    {
        public string ArchetypeId { get; set; }
        public int EntityId { get; set; } // Created entity ID
        public string RequestId { get; set; } // Matches request for tracking
        public int PlayerIndex { get; set; }
        public bool Success { get; set; } // Did creation succeed?
        public string ErrorMessage { get; set; } // If failed, why?
        public DateTime Timestamp { get; set; }

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
