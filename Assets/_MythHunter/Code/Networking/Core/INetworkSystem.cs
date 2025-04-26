using System;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;

namespace MythHunter.Networking.Core
{
    /// <summary>
    /// Інтерфейс мережевої системи
    /// </summary>
    public interface INetworkSystem
    {
        void StartServer(ushort port);
        UniTask<bool> ConnectToServerAsync(string address, ushort port);
        UniTask DisconnectAsync();
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