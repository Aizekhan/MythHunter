// Шлях: Assets/_MythHunter/Code/Events/NetworkEvents/INetworkEventBus.cs
using System;

namespace MythHunter.Events.Network
{
    /// <summary>
    /// Інтерфейс для шини подій з підтримкою мережевої синхронізації
    /// </summary>
    public interface INetworkEventBus : IEventBus
    {
        /// <summary>
        /// Реєструє тип події як мережеву подію
        /// </summary>
        void RegisterNetworkEvent<TEvent>(NetworkEventAuthority authority, NetworkEventPriority priority)
            where TEvent : struct, IEvent;
    }
}
