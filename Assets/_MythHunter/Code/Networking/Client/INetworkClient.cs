using System;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;

namespace MythHunter.Networking.Client
{
    /// <summary>
    /// Інтерфейс мережевого клієнта
    /// </summary>
    public interface INetworkClient
    {
        UniTask<bool> ConnectAsync(string address, ushort port);
        UniTask DisconnectAsync();
        void SendMessage<T>(T message) where T : INetworkMessage;
        event Action<INetworkMessage> OnMessageReceived;
        event Action OnConnected;
        event Action OnDisconnected;
        bool IsConnected { get; }
        bool IsActive { get; }
    }
}