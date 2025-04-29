using System;

namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Фази гри
    /// </summary>
    public enum GamePhase
    {
        None = 0,
        Rune,
        Planning,
        Movement,
        Combat,
        Freeze
    }

    /// <summary>
    /// Подія запиту на зміну фази
    /// </summary>
    public struct PhaseChangeRequestEvent : IEvent
    {
        public GamePhase RequestedPhase;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.Critical;
    }

    /// <summary>
    /// Подія зміни фази
    /// </summary>
    public struct PhaseChangedEvent : IEvent
    {
        public GamePhase PreviousPhase;
        public GamePhase CurrentPhase;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія початку фази
    /// </summary>
    public struct PhaseStartedEvent : IEvent
    {
        public GamePhase Phase;
        public float Duration;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія завершення фази
    /// </summary>
    public struct PhaseEndedEvent : IEvent
    {
        public GamePhase Phase;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }
}
