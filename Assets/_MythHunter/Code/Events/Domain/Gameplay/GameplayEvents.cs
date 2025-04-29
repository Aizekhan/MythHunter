using System;
using MythHunter.Events;

namespace MythHunter.Events.Domain.Gameplay
{
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
}
