using System;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;
using MythHunter.Utils.Logging;
using MythHunter.Networking.Client;
using MythHunter.Networking.Server;
using MythHunter.Networking.Serialization;
using MythHunter.Core.DI;
namespace MythHunter.Networking.Core
{
    /// <summary>
    /// Основна реалізація мережевої системи
    /// </summary>
    public class NetworkSystem : INetworkSystem
    {
        private readonly INetworkClient _client;
        private readonly INetworkServer _server;
        private readonly INetworkSerializer _serializer;
        private readonly IMythLogger _logger;

        public event Action<INetworkMessage> OnMessageReceived;
        public event Action<NetworkClientInfo, bool> OnClientConnectionChanged;

        public bool IsServer
        {
            get; private set;
        }
        public bool IsClient
        {
            get; private set;
        }
        public bool IsConnected => IsClient && _client.IsConnected;

        [Inject]
        public NetworkSystem(INetworkClient client, INetworkServer server,
                            INetworkSerializer serializer, IMythLogger logger)
        {
            _client = client;
            _server = server;
            _serializer = serializer;
            _logger = logger;

            // Підписуємося на події клієнта
            _client.OnMessageReceived += HandleClientMessage;
            _client.OnConnected += HandleClientConnected;
            _client.OnDisconnected += HandleClientDisconnected;

            // Підписуємося на події сервера
            _server.OnMessageReceived += HandleServerMessage;
            _server.OnClientConnected += HandleClientConnectedToServer;
            _server.OnClientDisconnected += HandleClientDisconnectedFromServer;
        }

        public void StartServer(ushort port)
        {
            if (IsClient)
            {
                _logger.LogWarning("Cannot start server while connected as client. Disconnect first.", "Network");
                return;
            }

            _server.Start(port);
            IsServer = true;
            _logger.LogInfo($"Server started on port {port}", "Network");
        }

        public async UniTask<bool> ConnectToServerAsync(string address, ushort port)
        {
            if (IsServer)
            {
                _logger.LogWarning("Cannot connect as client while running as server. Stop server first.", "Network");
                return false;
            }

            bool success = await _client.ConnectAsync(address, port);
            IsClient = success;

            if (success)
                _logger.LogInfo($"Connected to server at {address}:{port}", "Network");
            else
                _logger.LogError($"Failed to connect to server at {address}:{port}", "Network");

            return success;
        }

        public async UniTask DisconnectAsync()
        {
            if (IsClient)
            {
                await _client.DisconnectAsync();
                IsClient = false;
                _logger.LogInfo("Disconnected from server", "Network");
            }

            if (IsServer)
            {
                await _server.StopAsync();
                IsServer = false;
                _logger.LogInfo("Server stopped", "Network");
            }
        }

        public void SendMessage<T>(T message) where T : INetworkMessage
        {
            if (IsClient && _client.IsConnected)
            {
                _client.SendMessage(message);
            }
            else if (IsServer)
            {
                _server.BroadcastMessage(message);
            }
            else
            {
                _logger.LogWarning("Cannot send message: not connected", "Network");
            }
        }

        private void HandleClientMessage(INetworkMessage message)
        {
            OnMessageReceived?.Invoke(message);
        }

        private void HandleServerMessage(int clientId, INetworkMessage message)
        {
            OnMessageReceived?.Invoke(message);
        }

        private void HandleClientConnected()
        {
            _logger.LogInfo("Connected to server", "Network");
        }

        private void HandleClientDisconnected()
        {
            IsClient = false;
            _logger.LogInfo("Disconnected from server", "Network");
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
