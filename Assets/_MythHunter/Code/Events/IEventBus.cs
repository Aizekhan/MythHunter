using System;
using Cysharp.Threading.Tasks;

namespace MythHunter.Events
{
    /// <summary>
    /// Розширений інтерфейс шини подій з асинхронною підтримкою
    /// </summary>
    public interface IEventBus
    {
        // Синхронні методи
        void Subscribe<TEvent>(Action<TEvent> handler, EventPriority priority = EventPriority.Normal)
            where TEvent : struct, IEvent;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent;
        void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent;

        // Асинхронні методи
        void SubscribeAsync<TEvent>(Func<TEvent, UniTask> handler, EventPriority priority = EventPriority.Normal)
            where TEvent : struct, IEvent;
        void UnsubscribeAsync<TEvent>(Func<TEvent, UniTask> handler) where TEvent : struct, IEvent;
        UniTask PublishAsync<TEvent>(TEvent eventData) where TEvent : struct, IEvent;

        // Додаткові методи
        void Clear();
    }
}
