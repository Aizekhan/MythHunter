// Файл: Assets/_MythHunter/Code/Events/IDebugEventBus.cs
using System;

namespace MythHunter.Events
{
    /// <summary>
    /// Інтерфейс для дебаг-підтримки подій
    /// </summary>
    public interface IDebugEventBus
    {
        /// <summary>
        /// Підписується на всі події для дебагу
        /// </summary>
        void SubscribeToAllEvents(Action<IEvent, Type> handler);

        /// <summary>
        /// Відписується від всіх подій
        /// </summary>
        void UnsubscribeFromAllEvents(Action<IEvent, Type> handler);
    }
}
