// Шлях: Assets/_MythHunter/Code/Events/IEventQueue.cs

using System;

namespace MythHunter.Events
{
    /// <summary>
    /// Інтерфейс для черги подій
    /// </summary>
    public interface IEventQueue
    {
        void Enqueue<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
        void EnqueueAsync<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
        void Clear();
    }
}
