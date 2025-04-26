using System;
using MythHunter.Networking.Messages;

namespace MythHunter.Networking.Client
{
    /// <summary>
    /// Інтерфейс мережевого клієнта
    /// </summary>
    public interface INetworkClient
    {
        Task<bool> Connect(string address, ushort port);
        void Disconnect();
        void SendMessage<T>(T message) where T : INetworkMessage;
        event Action<INetworkMessage> OnMessageReceived;
        event Action OnConnected;
        event Action OnDisconnected;
        bool IsConnected { get; }
        bool IsActive { get; }
    }
}