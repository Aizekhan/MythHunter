// Шлях: Assets/_MythHunter/Code/Events/Domain/EntityEvents.cs

using System;

namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Подія створення сутності
    /// </summary>
    public struct EntityCreatedEvent : IEvent
    {
        public int EntityId;
        public string ArchetypeId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія знищення сутності
    /// </summary>
    public struct EntityDestroyedEvent : IEvent
    {
        public int EntityId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
