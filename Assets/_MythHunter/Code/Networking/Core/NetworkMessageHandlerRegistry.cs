using UnityEngine;
namespace MythHunter.Networking.Core
{
    /// <summary>
    /// Реєстр обробників мережевих повідомлень
    /// </summary>
    public class NetworkMessageHandlerRegistry : INetworkMessageHandlerRegistry
    {
        private readonly System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.Delegate>> _handlers =
            new System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.Delegate>>();
        private readonly MythHunter.Utils.Logging.IMythLogger _logger;

        [MythHunter.Core.DI.Inject]
        public NetworkMessageHandlerRegistry(MythHunter.Utils.Logging.IMythLogger logger)
        {
            _logger = logger;
        }

        public void RegisterHandler<T>(System.Action<T> handler) where T : MythHunter.Networking.Messages.INetworkMessage
        {
            System.Type messageType = typeof(T);

            if (!_handlers.TryGetValue(messageType, out var handlers))
            {
                handlers = new System.Collections.Generic.List<System.Delegate>();
                _handlers[messageType] = handlers;
            }

            handlers.Add(handler);
        }

        public void UnregisterHandler<T>(System.Action<T> handler) where T : MythHunter.Networking.Messages.INetworkMessage
        {
            System.Type messageType = typeof(T);

            if (_handlers.TryGetValue(messageType, out var handlers))
            {
                handlers.Remove(handler);

                if (handlers.Count == 0)
                {
                    _handlers.Remove(messageType);
                }
            }
        }

        public bool HandleMessage(MythHunter.Networking.Messages.INetworkMessage message)
        {
            System.Type messageType = message.GetType();

            if (!_handlers.TryGetValue(messageType, out var handlers))
            {
                _logger.LogWarning($"No handlers registered for message type: {messageType.Name}", "Network");
                return false;
            }

            bool handled = false;

            foreach (var handler in handlers)
            {
                try
                {
                    // Викликаємо обробник, передаючи повідомлення
                    handler.DynamicInvoke(message);
                    handled = true;
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"Error handling message {messageType.Name}: {ex.Message}", "Network", ex);
                }
            }

            return handled;
        }
    }
}
