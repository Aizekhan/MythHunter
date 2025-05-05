using System;
using System.IO;
using System.Collections.Generic;
using MythHunter.Networking.Messages;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
namespace MythHunter.Networking.Serialization
{
    /// <summary>
    /// Серіалізатор мережевих повідомлень з використанням бінарного формату
    /// </summary>
    public class BinaryNetworkSerializer : INetworkSerializer
    {
        private readonly Dictionary<string, Type> _messageTypes;
        private readonly IMythLogger _logger;

        [Inject]
        public BinaryNetworkSerializer(IMythLogger logger)
        {
            _logger = logger;
            _messageTypes = new Dictionary<string, Type>();

            // Автоматично збираємо всі типи повідомлень
            RegisterMessageTypes();
        }

        private void RegisterMessageTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(INetworkMessage).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            _messageTypes[type.FullName] = type;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error scanning assembly {assembly.FullName}: {ex.Message}", "Network");
                }
            }

            _logger.LogInfo($"Registered {_messageTypes.Count} message types", "Network");
        }

        public byte[] Serialize<T>(T message) where T : INetworkMessage
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // Записуємо тип повідомлення
                writer.Write(typeof(T).FullName);

                // Записуємо дані повідомлення
                var messageData = message.Serialize();
                writer.Write(messageData.Length);
                writer.Write(messageData);

                return stream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] data) where T : INetworkMessage, new()
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // Читаємо тип повідомлення (але ігноруємо його, оскільки ми знаємо тип T)
                string typeName = reader.ReadString();

                // Читаємо дані повідомлення
                int dataLength = reader.ReadInt32();
                byte[] messageData = reader.ReadBytes(dataLength);

                var message = new T();
                message.Deserialize(messageData);

                return message;
            }
        }

        public INetworkMessage Deserialize(byte[] data, Type messageType)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // Читаємо тип повідомлення
                string typeName = reader.ReadString();

                if (!_messageTypes.TryGetValue(typeName, out var type))
                {
                    _logger.LogError($"Unknown message type: {typeName}", "Network");
                    return null;
                }

                // Читаємо дані повідомлення
                int dataLength = reader.ReadInt32();
                byte[] messageData = reader.ReadBytes(dataLength);

                var message = (INetworkMessage)Activator.CreateInstance(type);
                message.Deserialize(messageData);

                return message;
            }
        }
    }
}
