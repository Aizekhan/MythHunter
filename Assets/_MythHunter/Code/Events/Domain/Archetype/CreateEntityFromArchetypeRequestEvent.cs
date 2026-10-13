// Assets/_MythHunter/Code/Events/Domain/Archetype/CreateEntityFromArchetypeRequestEvent.cs
using System;

namespace MythHunter.Events.Domain.Archetype
{
    /// <summary>
    /// EPIC 08 Phase 2: Request to create entity from archetype
    /// Replaces direct ArchetypeSystem.CreateEntityFromArchetype() calls
    /// </summary>
    public struct CreateEntityFromArchetypeRequestEvent : IEvent
    {
        public string ArchetypeId { get; set; }
        public object AdditionalData { get; set; } // Optional context data
        public string RequestId { get; set; } // For tracking request/response
        public int PlayerIndex { get; set; } // Which player requested this
        public DateTime Timestamp { get; set; }

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
