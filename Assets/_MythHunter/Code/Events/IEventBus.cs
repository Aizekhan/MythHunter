using System;

namespace MythHunter.Events
{
    /// <summary>
    /// Інтерфейс шини подій
    /// </summary>
    public interface IEventBus
    {
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent;
        void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
        void Clear();
    }
}