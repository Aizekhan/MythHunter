using System;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;
using MythHunter.Networking.Serialization;
using MythHunter.Utils.Logging;

namespace MythHunter.Networking.Client
{
    /// <summary>
    /// Клієнтська частина мережевої системи
    /// </summary>
    public class NetworkClient : INetworkClient
    {
        private readonly INetworkSerializer _serializer;
        private readonly IMythLogger _logger;
        private bool _isConnected;
        private bool _isActive;

        public event Action<INetworkMessage> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;

        public bool IsConnected => _isConnected;
        public bool IsActive => _isActive;

        [MythHunter.Core.DI.Inject]
        public NetworkClient(INetworkSerializer serializer, IMythLogger logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        public async UniTask<bool> ConnectAsync(string address, ushort port)
        {
            try
            {
                _logger.LogInfo($"Connecting to server at {address}:{port}", "Network");

                // Заглушка для реалізації
                await UniTask.Delay(100);

                _isConnected = true;
                _isActive = true;

                OnConnected?.Invoke();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error connecting to server: {ex.Message}", "Network", ex);
                return false;
            }
        }

        public async UniTask DisconnectAsync()
        {
            if (!_isConnected)
                return;

            try
            {
                _logger.LogInfo("Disconnecting from server", "Network");

                // Заглушка для реалізації
                await UniTask.Delay(100);

                _isConnected = false;
                _isActive = false;

                OnDisconnected?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error disconnecting from server: {ex.Message}", "Network", ex);
            }
        }

        public void SendMessage<T>(T message) where T : INetworkMessage
        {
            if (!_isConnected)
            {
                _logger.LogWarning("Cannot send message: not connected", "Network");
                return;
            }

            try
            {
                var data = _serializer.Serialize(message);

                // Тут буде реальна відправка
                _logger.LogDebug($"Sending message: {typeof(T).Name}, size: {data.Length} bytes", "Network");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}", "Network", ex);
            }
        }
    }
}
