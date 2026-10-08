// Шлях: Assets/_MythHunter/Code/Events/NetworkEvents/INetworkEvent.cs
using MythHunter.Data.Serialization;

namespace MythHunter.Events.Network
{
    /// <summary>
    /// Інтерфейс для подій, які можуть передаватися мережею
    /// </summary>
    public interface INetworkEvent : IEvent, ISerializable
    {
        /// <summary>
        /// Отримує унікальний ідентифікатор події на мережевому рівні
        /// </summary>
        string GetNetworkEventId();

        /// <summary>
        /// Визначає, чи подія є надійною (гарантована доставка)
        /// </summary>
        bool IsReliable();

        /// <summary>
        /// Отримує пріоритет мережевої передачі
        /// </summary>
        NetworkEventPriority GetNetworkPriority();
    }
}
