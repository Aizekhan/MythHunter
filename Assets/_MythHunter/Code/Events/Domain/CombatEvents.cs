// Шлях: Assets/_MythHunter/Code/Events/Domain/CombatEvents.cs
using System;
using MythHunter.Events;

namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Подія для запиту на початок бою
    /// </summary>
    public struct CombatStartRequestEvent : IEvent
    {
        public int AttackerEntityId;
        public int DefenderEntityId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія початку бою
    /// </summary>
    public struct CombatStartedEvent : IEvent
    {
        public int CombatId;
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
        public int CombatId;
        public int WinnerEntityId;
        public int[] ParticipantEntityIds;
        public CombatEndReason EndReason;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Причина завершення бою
    /// </summary>
    public enum CombatEndReason
    {
        Death,          // Смерть одного з учасників
        PhaseEnded,     // Закінчення активної фази
        Retreat,        // Відступ одного з учасників
        SystemForced    // Системне завершення (наприклад, адмін)
    }

    /// <summary>
    /// Подія нанесення пошкодження
    /// </summary>
    public struct DamageAppliedEvent : IEvent
    {
        public int SourceEntityId;
        public int TargetEntityId;
        public float DamageAmount;
        public DamageType DamageType;
        public bool IsCritical;
        public bool IsBlocked;
        public bool IsDodged;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Тип пошкодження
    /// </summary>
    public enum DamageType
    {
        Physical,
        Magical,
        Pure
    }

    /// <summary>
    /// Подія застосування лікування
    /// </summary>
    public struct HealingAppliedEvent : IEvent
    {
        public int SourceEntityId;
        public int TargetEntityId;
        public float HealingAmount;
        public bool IsCritical;
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
        public int KillerEntityId; // ID того, хто вбив (якщо є)
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Critical;
    }

    /// <summary>
    /// Подія зміни стійки
    /// </summary>
    public struct CombatStanceChangedEvent : IEvent
    {
        public int EntityId;
        public MythHunter.Components.Combat.CombatStance OldStance;
        public MythHunter.Components.Combat.CombatStance NewStance;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія використання активної здібності
    /// </summary>
    public struct ActiveAbilityUsedEvent : IEvent
    {
        public int EntityId;
        public string AbilityId;
        public int TargetEntityId;
        public bool IsSuccess;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія оновлення значення люті
    /// </summary>
    public struct RageUpdatedEvent : IEvent
    {
        public int EntityId;
        public float OldValue;
        public float NewValue;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія оновлення значення концентрації
    /// </summary>
    public struct ConcentrationUpdatedEvent : IEvent
    {
        public int EntityId;
        public float OldValue;
        public float NewValue;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
