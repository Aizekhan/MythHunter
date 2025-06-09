// Assets/_MythHunter/Code/Events/Domain/MovementEvents.cs
using System.Collections.Generic;
using System;
using UnityEngine;
namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Запит на планування шляху
    /// </summary>
    public struct PathPlannedEvent : IEvent
    {
        public int EntityId;
        public List<Vector3> Waypoints;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія початку руху
    /// </summary>
    public struct MovementStartedEvent : IEvent
    {
        public int EntityId;
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія зупинки руху
    /// </summary>
    public struct MovementStoppedEvent : IEvent
    {
        public int EntityId;
        public Vector3 Position;
        public bool PathCompleted;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія оновлення позиції (для відображення)
    /// </summary>
    public struct PositionUpdatedEvent : IEvent
    {
        public int EntityId;
        public Vector3 Position;
        public Quaternion Rotation;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Low;
    }

    /// <summary>
    /// Подія виявлення об'єкта в області видимості
    /// </summary>
    public struct EntityDetectedEvent : IEvent
    {
        public int ObserverEntityId;
        public int DetectedEntityId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія втрати об'єкта з області видимості
    /// </summary>
    public struct EntityLostEvent : IEvent
    {
        public int ObserverEntityId;
        public int LostEntityId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія зміни напрямку погляду
    /// </summary>
    public struct LookDirectionChangedEvent : IEvent
    {
        public int EntityId;
        public Vector3 Direction;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
