using System;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;

namespace MythHunter.Networking.Server
{
    /// <summary>
    /// Інтерфейс мережевого сервера
    /// </summary>
    public interface INetworkServer
    {
        void Start(ushort port);
        UniTask StopAsync();
        void SendMessage<T>(T message, int clientId) where T : INetworkMessage;
        void BroadcastMessage<T>(T message) where T : INetworkMessage;
        event Action<int, INetworkMessage> OnMessageReceived;
        event Action<int> OnClientConnected;
        event Action<int> OnClientDisconnected;
        bool IsRunning { get; }
        bool IsActive { get; }
        int[] GetConnectedClients();
    }
}