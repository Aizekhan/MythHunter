// Assets/_MythHunter/Code/Events/Domain/Preload/PreloadEvents.cs
using System;
using MythHunter.Events;

namespace MythHunter.Events.Domain.Preload
{
    /// <summary>
    /// Подія початку preload для сцени
    /// </summary>
    public struct PreloadStartedEvent : IEvent
    {
        public string SceneName;
        public int TotalResources;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія оновлення прогресу preload
    /// </summary>
    public struct PreloadProgressUpdatedEvent : IEvent
    {
        public string SceneName;
        public float Progress; // 0.0 - 1.0
        public int LoadedResources;
        public int TotalResources;
        public string CurrentResource;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Low; // Часті оновлення
    }

    /// <summary>
    /// Подія завершення preload для сцени
    /// </summary>
    public struct PreloadCompletedEvent : IEvent
    {
        public string SceneName;
        public int TotalLoadedResources;
        public float TotalTime;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія помилки preload
    /// </summary>
    public struct PreloadResourceFailedEvent : IEvent
    {
        public string SceneName;
        public string ResourceKey;
        public string ErrorMessage;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }
}
