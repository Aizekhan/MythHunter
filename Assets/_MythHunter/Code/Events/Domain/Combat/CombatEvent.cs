using System;

namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Подія, пов'язана з боєм
    /// </summary>
    public struct CombatEvent : IEvent
    {
        public int[] ParticipantIds;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }
}
