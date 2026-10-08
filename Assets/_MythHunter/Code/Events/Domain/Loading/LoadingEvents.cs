// Assets/_MythHunter/Code/Events/Domain/Loading/LoadingEvents.cs
using System;
using System.Collections.Generic;
using MythHunter.Events;

namespace MythHunter.Events.Domain.Loading
{
    /// <summary>
    /// Подія початку процесу завантаження
    /// </summary>
    public struct LoadingStartedEvent : IEvent
    {
        public string[] SelectedHeroArchetypes;
        public string MapId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія оновлення прогресу завантаження
    /// </summary>
    public struct LoadingProgressEvent : IEvent
    {
        public float Progress;
        public string Status;
        public LoadingStage Stage;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія завершення процесу завантаження
    /// </summary>
    public struct LoadingCompletedEvent : IEvent
    {
        public bool Success;
        public string[] CreatedEntityIds;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія помилки завантаження
    /// </summary>
    public struct LoadingErrorEvent : IEvent
    {
        public string ErrorMessage;
        public LoadingStage Stage;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Critical;
    }

    /// <summary>
    /// Стадії процесу завантаження
    /// </summary>
    public enum LoadingStage
    {
        None = 0,
        PreparingResources,
        LoadingHeroPrefabs,
        LoadingMapData,
        InitializingPools,
        CreatingEntities,
        SettingUpSystems,
        FinalSetup
    }
}
