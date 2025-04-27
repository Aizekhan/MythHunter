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
    }
    
    /// <summary>
    /// Подія паузи гри
    /// </summary>
    public struct GamePausedEvent : IEvent
    {
        public bool IsPaused;
        public DateTime Timestamp;
        
        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
    }
    
    /// <summary>
    /// Подія завершення гри
    /// </summary>
    public struct GameEndedEvent : IEvent
    {
        public bool IsVictory;
        public DateTime Timestamp;
        
        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
    }
}