using System;

namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Подія запуску гри
    /// </summary>
    public struct GameStartedEvent : IEvent
    {
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія паузи гри
    /// </summary>
    public struct GamePausedEvent : IEvent
    {
        public bool IsPaused;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія завершення гри
    /// </summary>
    public struct GameEndedEvent : IEvent
    {
        public bool IsVictory;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.Critical;
    }
}
