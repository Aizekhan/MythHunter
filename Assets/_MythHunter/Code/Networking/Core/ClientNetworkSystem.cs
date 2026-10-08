// Шлях: Assets/_MythHunter/Code/Networking/Core/ClientNetworkSystem.cs
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Networking.Client;
using MythHunter.Networking.Messages;
using MythHunter.Utils.Logging;

namespace MythHunter.Networking.Core
{
    /// <summary>
    /// Клієнтська реалізація мережевої системи
    /// </summary>
    public class ClientNetworkSystem : IClientNetworkSystem
    {
        private readonly INetworkClient _client;
        private readonly IMythLogger _logger;

        public event Action<INetworkMessage> OnMessageReceived;
        public event Action<NetworkClientInfo, bool> OnClientConnectionChanged;

        public bool IsActive => _client.IsActive;
        public bool IsConnected => _client.IsConnected;

        [Inject]
        public ClientNetworkSystem(INetworkClient client, IMythLogger logger)
        {
            _client = client;
            _logger = logger;

            // Підписуємося на події клієнта
            _client.OnMessageReceived += HandleClientMessage;
            _client.OnConnected += HandleClientConnected;
            _client.OnDisconnected += HandleClientDisconnected;
        }

        public async UniTask<bool> ConnectToServerAsync(string address, ushort port)
        {
            bool success = await _client.ConnectAsync(address, port);

            if (success)
                _logger.LogInfo($"Connected to server at {address}:{port}", "Network");
            else
                _logger.LogError($"Failed to connect to server at {address}:{port}", "Network");

            return success;
        }

        public async UniTask DisconnectAsync()
        {
            if (_client.IsConnected)
            {
                await _client.DisconnectAsync();
                _logger.LogInfo("Disconnected from server", "Network");
            }
        }

        public void SendMessage<T>(T message) where T : INetworkMessage
        {
            if (_client.IsConnected)
            {
                _client.SendMessage(message);
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

        private void HandleClientConnected()
        {
            var clientInfo = new NetworkClientInfo { ClientId = -1 }; // Клієнт не знає свій ID
            OnClientConnectionChanged?.Invoke(clientInfo, true);
            _logger.LogInfo("Connected to server", "Network");
        }

        private void HandleClientDisconnected()
        {
            var clientInfo = new NetworkClientInfo { ClientId = -1 };
            OnClientConnectionChanged?.Invoke(clientInfo, false);
            _logger.LogInfo("Disconnected from server", "Network");
        }
    }
}
