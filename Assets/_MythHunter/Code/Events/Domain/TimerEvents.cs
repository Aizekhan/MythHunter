using System;

namespace MythHunter.Events.Domain
{
    public struct TimerCreatedEvent : IEvent
    {
        public string TimerId;
        public string TimerName;
        public float Duration;
        public string Category;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    public struct TimerUpdatedEvent : IEvent
    {
        public string TimerId;
        public string TimerName;
        public float RemainingTime;
        public float TotalTime;
        public string Category;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Low;
    }

    public struct TimerCompletedEvent : IEvent
    {
        public string TimerId;
        public string TimerName;
        public string Category;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }
}
