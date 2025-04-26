using System;
using MythHunter.Networking.Messages;

namespace MythHunter.Networking.Core
{
    /// <summary>
    /// Інтерфейс мережевої системи
    /// </summary>
    public interface INetworkSystem
    {
        void StartServer(ushort port);
        Task<bool> ConnectToServer(string address, ushort port);
        void Disconnect();
        void SendMessage<T>(T message) where T : INetworkMessage;
        event Action<INetworkMessage> OnMessageReceived;
        event Action<NetworkClientInfo, bool> OnClientConnectionChanged;
        bool IsServer { get; }
        bool IsClient { get; }
        bool IsConnected { get; }
    }
    
    /// <summary>
    /// Інформація про клієнта
    /// </summary>
    public struct NetworkClientInfo
    {
        public int ClientId;
        public string Address;
    }
}