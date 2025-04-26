using System;

namespace MythHunter.Events
{
    /// <summary>
    /// Інтерфейс шини подій
    /// </summary>
    public interface IEventBus
    {
        void SubscribeAny(Action<IEvent> callback);
        void UnsubscribeAny(Action<IEvent> callback);
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent;
        void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
        void Clear();
    }
}
