using UnityEngine;

namespace MythHunter.Networking.Core
{
    /// <summary>
    /// Інтерфейс реєстру обробників мережевих повідомлень
    /// </summary>
    public interface INetworkMessageHandlerRegistry
    {
        void RegisterHandler<T>(System.Action<T> handler) where T : MythHunter.Networking.Messages.INetworkMessage;
        void UnregisterHandler<T>(System.Action<T> handler) where T : MythHunter.Networking.Messages.INetworkMessage;
        bool HandleMessage(MythHunter.Networking.Messages.INetworkMessage message);
    }

}
