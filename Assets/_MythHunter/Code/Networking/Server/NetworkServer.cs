using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;
using MythHunter.Networking.Serialization;
using MythHunter.Utils.Logging;

namespace MythHunter.Networking.Server
{
    /// <summary>
    /// Серверна частина мережевої системи
    /// </summary>
    public class NetworkServer : INetworkServer
    {
        private readonly INetworkSerializer _serializer;
        private readonly IMythLogger _logger;
        private bool _isRunning;
        private readonly HashSet<int> _connectedClients = new HashSet<int>();
        private int _nextClientId = 1;

        public event Action<int, INetworkMessage> OnMessageReceived;
        public event Action<int> OnClientConnected;
        public event Action<int> OnClientDisconnected;

        public bool IsRunning => _isRunning;
        public bool IsActive => _isRunning;

        [MythHunter.Core.DI.Inject]
        public NetworkServer(INetworkSerializer serializer, IMythLogger logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        public void Start(ushort port)
        {
            if (_isRunning)
            {
                _logger.LogWarning($"Server already running on port {port}", "Network");
                return;
            }

            try
            {
                _logger.LogInfo($"Starting server on port {port}", "Network");

                // Заглушка для реалізації

                _isRunning = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting server: {ex.Message}", "Network", ex);
            }
        }

        public async UniTask StopAsync()
        {
            if (!_isRunning)
                return;

            try
            {
                _logger.LogInfo("Stopping server", "Network");

                // Заглушка для реалізації
                await UniTask.Delay(100);

                // Відключаємо всіх клієнтів
                foreach (var clientId in new List<int>(_connectedClients))
                {
                    DisconnectClient(clientId);
                }

                _connectedClients.Clear();
                _isRunning = false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error stopping server: {ex.Message}", "Network", ex);
            }
        }

        public void SendMessage<T>(T message, int clientId) where T : INetworkMessage
        {
            if (!_isRunning)
            {
                _logger.LogWarning("Cannot send message: server not running", "Network");
                return;
            }

            if (!_connectedClients.Contains(clientId))
            {
                _logger.LogWarning($"Cannot send message: client {clientId} not connected", "Network");
                return;
            }

            try
            {
                var data = _serializer.Serialize(message);

                // Тут буде реальна відправка
                _logger.LogDebug($"Sending message to client {clientId}: {typeof(T).Name}, size: {data.Length} bytes", "Network");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message to client {clientId}: {ex.Message}", "Network", ex);
            }
        }

        public void BroadcastMessage<T>(T message) where T : INetworkMessage
        {
            if (!_isRunning)
            {
                _logger.LogWarning("Cannot broadcast message: server not running", "Network");
                return;
            }

            if (_connectedClients.Count == 0)
            {
                _logger.LogWarning("Cannot broadcast message: no clients connected", "Network");
                return;
            }

            try
            {
                var data = _serializer.Serialize(message);

                // Тут буде реальна відправка
                _logger.LogDebug($"Broadcasting message to {_connectedClients.Count} clients: {typeof(T).Name}, size: {data.Length} bytes", "Network");

                foreach (var clientId in _connectedClients)
                {
                    // Відправка кожному клієнту
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error broadcasting message: {ex.Message}", "Network", ex);
            }
        }

        // Метод для симуляції підключення клієнта (для тестів)
        public int SimulateClientConnection()
        {
            int clientId = _nextClientId++;
            _connectedClients.Add(clientId);
            OnClientConnected?.Invoke(clientId);
            return clientId;
        }

        // Метод для симуляції відключення клієнта
        public void DisconnectClient(int clientId)
        {
            if (_connectedClients.Remove(clientId))
            {
                OnClientDisconnected?.Invoke(clientId);
                _logger.LogInfo($"Client {clientId} disconnected", "Network");
            }
        }

        public int[] GetConnectedClients()
        {
            int[] result = new int[_connectedClients.Count];
            _connectedClients.CopyTo(result);
            return result;
        }
    }
}
