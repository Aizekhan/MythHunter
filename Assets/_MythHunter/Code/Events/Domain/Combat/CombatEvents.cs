// Шлях: Assets/_MythHunter/Code/Events/Domain/Combat/CombatEvents.cs
using System;
using MythHunter.Events;

namespace MythHunter.Events.Domain.Combat
{
    /// <summary>
    /// Подія початку бою
    /// </summary>
    public struct CombatStartedEvent : IEvent
    {
        public int[] ParticipantEntityIds;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія завершення бою
    /// </summary>
    public struct CombatEndedEvent : IEvent
    {
        public int WinnerEntityId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }

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

    /// <summary>
    /// Подія, пов'язана з боєм (базова подія для загальних випадків)
    /// </summary>
    public struct CombatEvent : IEvent
    {
        public int[] ParticipantIds;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }
}
