// Шлях: Assets/_MythHunter/Code/Events/Domain/Gameplay/GameplayEvents.cs
using System;
using MythHunter.Events;

namespace MythHunter.Events.Domain.Gameplay
{
    /// <summary>
    /// Базова подія, пов'язана з геймплеєм
    /// </summary>
    public struct GameplayEvent : IEvent
    {
        public string ActionType;
        public int EntityId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія запиту на створення персонажа
    /// </summary>
    public struct SpawnCharacterEvent : IEvent
    {
        public string CharacterName;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія успішного створення персонажа
    /// </summary>
    public struct CharacterSpawnedEvent : IEvent
    {
        public int EntityId;
        public string CharacterName;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія запиту на створення ворога
    /// </summary>
    public struct SpawnEnemyEvent : IEvent
    {
        public string EnemyName;
        public float Health;
        public float AttackPower;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія успішного створення ворога
    /// </summary>
    public struct EnemySpawnedEvent : IEvent
    {
        public int EntityId;
        public string EnemyName;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія підбирання предмета
    /// </summary>
    public struct ItemPickupEvent : IEvent
    {
        public int EntityId;
        public int ItemId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія використання предмета
    /// </summary>
    public struct ItemUsedEvent : IEvent
    {
        public int EntityId;
        public int ItemId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія початку взаємодії з об'єктом
    /// </summary>
    public struct InteractionStartedEvent : IEvent
    {
        public int EntityId;
        public int TargetId;
        public string InteractionType;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія завершення взаємодії з об'єктом
    /// </summary>
    public struct InteractionCompletedEvent : IEvent
    {
        public int EntityId;
        public int TargetId;
        public string InteractionType;
        public bool IsSuccess;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
