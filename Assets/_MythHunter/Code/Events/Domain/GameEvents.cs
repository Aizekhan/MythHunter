// Assets/_MythHunter/Code/Events/Domain/GameEvents.cs
using System;
using MythHunter.Core.Game;

namespace MythHunter.Events.Domain
{
    // Подія запуску гри
    public struct GameStartedEvent : IEvent
    {
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    // Подія паузи гри
    public struct GamePausedEvent : IEvent
    {
        public bool IsPaused;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    // Подія завершення гри
    public struct GameEndedEvent : IEvent
    {
        public bool IsVictory;
        public int WinnerPlayerId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Critical;
    }
    /// <summary>
    /// Подія зміни стану гри
    /// </summary>
    public struct GameStateChangedEvent : IEvent
    {
        public GameStateType PreviousState;
        public GameStateType NewState;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }
    /// <summary>
    /// Подія, яка сигналізує про критичну помилку в ігровому процесі
    /// </summary>
    public struct GameErrorEvent : IEvent
    {
        public string ErrorMessage;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Critical;
    }
}
