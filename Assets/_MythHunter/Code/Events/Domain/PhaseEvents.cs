// Assets/_MythHunter/Code/Events/Domain/PhaseEvents.cs
using System;

namespace MythHunter.Events.Domain
{
    // Фази гри
    public enum GamePhase
    {
        None = 0,
        Rune,       // Фаза вибору руни
        Planning,   // Фаза планування руху
        Movement,   // Фаза руху
        Combat,     // Фаза бою (частина активної фази)
        Freeze      // Фаза завмирання
    }

    // Подія запиту на зміну фази
    public struct PhaseChangeRequestEvent : IEvent
    {
        public GamePhase RequestedPhase;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Critical;
    }

    // Подія зміни фази
    public struct PhaseChangedEvent : IEvent
    {
        public GamePhase PreviousPhase;
        public GamePhase CurrentPhase;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    // Подія початку фази
    public struct PhaseStartedEvent : IEvent
    {
        public GamePhase Phase;
        public float Duration;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    // Подія завершення фази
    public struct PhaseEndedEvent : IEvent
    {
        public GamePhase Phase;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    // Подія оновлення стану фази
    public struct PhaseUpdateEvent : IEvent
    {
        public GamePhase Phase;
        public float ElapsedTime;
        public float RemainingTime;
        public float TotalDuration;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
