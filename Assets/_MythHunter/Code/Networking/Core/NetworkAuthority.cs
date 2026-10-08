// Шлях: Assets/_MythHunter/Code/Networking/Core/NetworkAuthority.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Networking.Messages;
using MythHunter.Utils.Logging;

namespace MythHunter.Networking.Core
{
    /// <summary>
    /// Система вирішення авторитетності для мережевих операцій
    /// </summary>
    public class NetworkAuthority
    {
        private readonly IEventBus _eventBus;
        private readonly IClientNetworkSystem _clientSystem;
        private readonly IServerNetworkSystem _serverSystem;
        private readonly IMythLogger _logger;

        private readonly HashSet<Type> _serverAuthoritativeMsgs = new HashSet<Type>();
        private readonly HashSet<Type> _clientAuthoritativeMsgs = new HashSet<Type>();
        private readonly HashSet<Type> _sharedAuthoritativeMsgs = new HashSet<Type>();

        private bool _isServer;
        private bool _isClient;

        [Inject]
        public NetworkAuthority(
            IEventBus eventBus,
            IClientNetworkSystem clientSystem,
            IServerNetworkSystem serverSystem,
            IMythLogger logger)
        {
            _eventBus = eventBus;
            _clientSystem = clientSystem;
            _serverSystem = serverSystem;
            _logger = logger;

            // Реєструємо стандартні авторитетні повідомлення
            RegisterDefaultAuthorityRules();
        }

        /// <summary>
        /// Визначає сервер як активний
        /// </summary>
        public void SetServerActive(bool isActive)
        {
            _isServer = isActive;
            _logger.LogInfo($"Network authority: server mode {(_isServer ? "enabled" : "disabled")}", "Network");
        }

        /// <summary>
        /// Визначає клієнт як активний
        /// </summary>
        public void SetClientActive(bool isActive)
        {
            _isClient = isActive;
            _logger.LogInfo($"Network authority: client mode {(_isClient ? "enabled" : "disabled")}", "Network");
        }

        /// <summary>
        /// Реєструє тип повідомлення як серверний авторитет
        /// </summary>
        public void RegisterServerAuthority<T>() where T : INetworkMessage
        {
            Type messageType = typeof(T);
            _serverAuthoritativeMsgs.Add(messageType);
            _clientAuthoritativeMsgs.Remove(messageType);
            _sharedAuthoritativeMsgs.Remove(messageType);

            _logger.LogDebug($"Registered message {messageType.Name} as server authoritative", "Network");
        }

        /// <summary>
        /// Реєструє тип повідомлення як клієнтський авторитет
        /// </summary>
        public void RegisterClientAuthority<T>() where T : INetworkMessage
        {
            Type messageType = typeof(T);
            _clientAuthoritativeMsgs.Add(messageType);
            _serverAuthoritativeMsgs.Remove(messageType);
            _sharedAuthoritativeMsgs.Remove(messageType);

            _logger.LogDebug($"Registered message {messageType.Name} as client authoritative", "Network");
        }

        /// <summary>
        /// Реєструє тип повідомлення як спільний авторитет
        /// </summary>
        public void RegisterSharedAuthority<T>() where T : INetworkMessage
        {
            Type messageType = typeof(T);
            _sharedAuthoritativeMsgs.Add(messageType);
            _serverAuthoritativeMsgs.Remove(messageType);
            _clientAuthoritativeMsgs.Remove(messageType);

            _logger.LogDebug($"Registered message {messageType.Name} as shared authoritative", "Network");
        }

        /// <summary>
        /// Перевіряє, чи може система обробляти повідомлення
        /// </summary>
        public bool CanProcessMessage<T>(T message) where T : INetworkMessage
        {
            Type messageType = typeof(T);

            // Спільні повідомлення можуть оброблятися будь-ким
            if (_sharedAuthoritativeMsgs.Contains(messageType))
                return true;

            // Серверні повідомлення можуть оброблятися тільки сервером
            if (_serverAuthoritativeMsgs.Contains(messageType))
                return _isServer;

            // Клієнтські повідомлення можуть оброблятися тільки клієнтом
            if (_clientAuthoritativeMsgs.Contains(messageType))
                return _isClient;

            // За замовчуванням дозволяємо обробку
            return true;
        }

        /// <summary>
        /// Надсилає повідомлення відповідно до авторитетності
        /// </summary>
        public void SendMessage<T>(T message) where T : INetworkMessage
        {
            Type messageType = typeof(T);

            // Визначаємо, хто має надсилати повідомлення
            bool serverShouldSend = _isServer && (_serverAuthoritativeMsgs.Contains(messageType) || _sharedAuthoritativeMsgs.Contains(messageType));
            bool clientShouldSend = _isClient && (_clientAuthoritativeMsgs.Contains(messageType) || _sharedAuthoritativeMsgs.Contains(messageType));

            if (serverShouldSend)
            {
                _serverSystem.SendMessage(message);
            }
            else if (clientShouldSend)
            {
                _clientSystem.SendMessage(message);
            }
            else
            {
                _logger.LogWarning($"Cannot send message {messageType.Name}: no authorized sender available", "Network");
            }
        }

        /// <summary>
        /// Реєструє стандартні правила авторитетності
        /// </summary>
        private void RegisterDefaultAuthorityRules()
        {
            // Тут можна додати реєстрацію стандартних правил
            // Наприклад:
            // RegisterServerAuthority<SpawnEntityMessage>();
            // RegisterClientAuthority<MoveRequestMessage>();
            // RegisterSharedAuthority<ChatMessage>();
        }
    }
}
