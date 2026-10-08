// Шлях: Assets/_MythHunter/Code/Networking/Core/ServerNetworkSystem.cs
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Networking.Messages;
using MythHunter.Networking.Server;
using MythHunter.Utils.Logging;

namespace MythHunter.Networking.Core
{
    /// <summary>
    /// Серверна реалізація мережевої системи
    /// </summary>
    public class ServerNetworkSystem : IServerNetworkSystem
    {
        private readonly INetworkServer _server;
        private readonly IMythLogger _logger;

        public event Action<INetworkMessage> OnMessageReceived;
        public event Action<NetworkClientInfo, bool> OnClientConnectionChanged;

        public bool IsActive => _server.IsActive;
        public bool IsRunning => _server.IsRunning;

        [Inject]
        public ServerNetworkSystem(INetworkServer server, IMythLogger logger)
        {
            _server = server;
            _logger = logger;

            // Підписуємося на події сервера
            _server.OnMessageReceived += HandleServerMessage;
            _server.OnClientConnected += HandleClientConnectedToServer;
            _server.OnClientDisconnected += HandleClientDisconnectedFromServer;
        }

        public void StartServer(ushort port)
        {
            _server.Start(port);
            _logger.LogInfo($"Server started on port {port}", "Network");
        }

        public async UniTask DisconnectAsync()
        {
            if (_server.IsRunning)
            {
                await _server.StopAsync();
                _logger.LogInfo("Server stopped", "Network");
            }
        }

        public void SendMessage<T>(T message) where T : INetworkMessage
        {
            if (_server.IsRunning)
            {
                _server.BroadcastMessage(message);
            }
            else
            {
                _logger.LogWarning("Cannot send message: server not running", "Network");
            }
        }

        public void SendMessageToClient<T>(T message, int clientId) where T : INetworkMessage
        {
            if (_server.IsRunning)
            {
                _server.SendMessage(message, clientId);
            }
            else
            {
                _logger.LogWarning($"Cannot send message to client {clientId}: server not running", "Network");
            }
        }

        public void BroadcastMessage<T>(T message) where T : INetworkMessage
        {
            if (_server.IsRunning)
            {
                _server.BroadcastMessage(message);
            }
            else
            {
                _logger.LogWarning("Cannot broadcast message: server not running", "Network");
            }
        }

        public int[] GetConnectedClients()
        {
            return _server.GetConnectedClients();
        }

        private void HandleServerMessage(int clientId, INetworkMessage message)
        {
            // Тут можна додати додаткову обробку повідомлень з клієнта
            // Наприклад, фільтрацію недозволених повідомлень

            OnMessageReceived?.Invoke(message);
        }

        private void HandleClientConnectedToServer(int clientId)
        {
            var clientInfo = new NetworkClientInfo { ClientId = clientId };
            OnClientConnectionChanged?.Invoke(clientInfo, true);
            _logger.LogInfo($"Client {clientId} connected", "Network");
        }

        private void HandleClientDisconnectedFromServer(int clientId)
        {
            var clientInfo = new NetworkClientInfo { ClientId = clientId };
            OnClientConnectionChanged?.Invoke(clientInfo, false);
            _logger.LogInfo($"Client {clientId} disconnected", "Network");
        }
    }
}
