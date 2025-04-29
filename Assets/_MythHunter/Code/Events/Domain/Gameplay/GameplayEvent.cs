using System;

namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Подія, пов'язана з геймплеєм
    /// </summary>
    public struct GameplayEvent : IEvent
    {
        public string ActionType;
        public int EntityId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
