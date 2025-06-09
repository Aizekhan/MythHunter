// Шлях: Assets/_MythHunter/Code/Networking/Core/INetworkSystem.cs
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;

namespace MythHunter.Networking.Core
{
    /// <summary>
    /// Базовий інтерфейс для мережевих систем
    /// </summary>
    public interface INetworkSystem
    {
        event Action<INetworkMessage> OnMessageReceived;
        event Action<NetworkClientInfo, bool> OnClientConnectionChanged;

        void SendMessage<T>(T message) where T : INetworkMessage;
        UniTask DisconnectAsync();

        bool IsActive
        {
            get;
        }
    }

    /// <summary>
    /// Інтерфейс для клієнтської мережевої системи
    /// </summary>
    public interface IClientNetworkSystem : INetworkSystem
    {
        UniTask<bool> ConnectToServerAsync(string address, ushort port);
        bool IsConnected
        {
            get;
        }
    }

    /// <summary>
    /// Інтерфейс для серверної мережевої системи
    /// </summary>
    public interface IServerNetworkSystem : INetworkSystem
    {
        void StartServer(ushort port);
        bool IsRunning
        {
            get;
        }
        int[] GetConnectedClients();
        void SendMessageToClient<T>(T message, int clientId) where T : INetworkMessage;
        void BroadcastMessage<T>(T message) where T : INetworkMessage;
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
