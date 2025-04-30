// RuneEvents.cs
using System;
using MythHunter.Events;

namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Подія кидання руни
    /// </summary>
    public struct RuneRolledEvent : IEvent
    {
        public int Value;         // Значення руни (2-12)
        public int Type;          // Тип руни (0=Бойова, 1=Рух, 2=Здоров'я)
        public string Description; // Опис ефекту
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія застосування ефекту руни
    /// </summary>
    public struct RuneEffectAppliedEvent : IEvent
    {
        public int EntityId;     // Сутність, до якої застосовано ефект
        public int Value;        // Значення руни
        public int Type;         // Тип руни
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
