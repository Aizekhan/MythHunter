// Assets/_MythHunter/Code/Events/Domain/Lobby/LobbyEvents.cs
using System;
using MythHunter.Events;

namespace MythHunter.Events.Domain.Lobby
{
    /// <summary>
    /// Подія ініціалізації лоббі
    /// </summary>
    public struct LobbyInitializedEvent : IEvent
    {
        public int PlayerCount;
        public int ManaPerPlayer;
        public float SelectionTimeLimit;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія вибору героя
    /// </summary>
    public struct HeroSelectedEvent : IEvent
    {
        public int PlayerIndex;
        public string ArchetypeId;
        public int ManaCost;
        public int RemainingMana;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія підтвердження вибору
    /// </summary>
    public struct SelectionConfirmedEvent : IEvent
    {
        public int PlayerIndex;
        public string[] SelectedArchetypeIds;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія початку гри
    /// </summary>
    public struct GameStartRequestEvent : IEvent
    {
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія оновлення таймера вибору
    /// </summary>
    public struct SelectionTimerUpdatedEvent : IEvent
    {
        public float RemainingTime;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Low;
    }
}
