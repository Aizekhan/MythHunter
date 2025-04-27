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
    /// Подія зміни фази
    /// </summary>
    public struct PhaseChangedEvent : IEvent
    {
        public GamePhase PreviousPhase;
        public GamePhase CurrentPhase;
        public DateTime Timestamp;
        
        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
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
    }
    
    /// <summary>
    /// Подія завершення фази
    /// </summary>
    public struct PhaseEndedEvent : IEvent
    {
        public GamePhase Phase;
        public DateTime Timestamp;
        
        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
    }
    /// <summary>
    /// Подія запиту на перехід до наступної фази
    /// </summary>
    public struct RequestNextPhaseEvent : IEvent
    {
        public GamePhase CurrentPhase;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
    }

    /// <summary>
    /// Подія зміни таймера фази
    /// </summary>
    public struct PhaseTimerUpdatedEvent : IEvent
    {
        public GamePhase Phase;
        public float RemainingTime;
        public float TotalTime;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
    }
}
