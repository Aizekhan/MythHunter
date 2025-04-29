using System;
using MythHunter.Events;

namespace MythHunter.Events.Domain.Combat
{
    /// <summary>
    /// Подія нанесення пошкодження
    /// </summary>
    public struct DamageAppliedEvent : IEvent
    {
        public int SourceEntityId;
        public int TargetEntityId;
        public float DamageAmount;
        public string DamageType;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія застосування лікування
    /// </summary>
    public struct HealingAppliedEvent : IEvent
    {
        public int SourceEntityId;
        public int TargetEntityId;
        public float HealingAmount;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія смерті сутності
    /// </summary>
    public struct EntityDeathEvent : IEvent
    {
        public int EntityId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.Critical;
    }
}
