using System;
using System.IO;
using MythHunter.Networking.Messages;
using MythHunter.Utils.Logging;
using RPGFramework.DI;
using RPGFramework.Serialization;

namespace MythHunter.Networking.Serialization
{
    /// <summary>
    /// Binary network message serializer using ITypeRegistry for stable type identification.
    /// Replaces Type.FullName with stable type IDs to survive refactoring.
    /// </summary>
    public class BinaryNetworkSerializer : INetworkSerializer
    {
        private readonly ITypeRegistry _typeRegistry;
        private readonly IMythLogger _logger;

        [Inject]
        public BinaryNetworkSerializer(ITypeRegistry typeRegistry, IMythLogger logger)
        {
            _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
            _logger = logger;
        }

        /// <summary>
        /// Serializes a network message.
        /// Wire format: [TypeId (string)] [Version (ushort)] [PayloadLength (int32)] [Payload (byte[])]
        /// </summary>
        public byte[] Serialize<T>(T message) where T : INetworkMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var type = typeof(T);
            var typeId = _typeRegistry.GetTypeId(type);
            var version = _typeRegistry.GetVersion(type);

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // Write type metadata
                writer.Write(typeId);
                writer.Write(version);

                // Write message payload
                var messageData = message.Serialize();
                writer.Write(messageData.Length);
                writer.Write(messageData);

                return stream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] data) where T : INetworkMessage, new()
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // Read type metadata
                var typeId = reader.ReadString();
                var version = reader.ReadUInt16();

                // Validate type
                var expectedType = typeof(T);
                var expectedTypeId = _typeRegistry.GetTypeId(expectedType);

                if (typeId != expectedTypeId)
                {
                    _logger.LogError($"Type mismatch: expected '{expectedTypeId}' but got '{typeId}'", "Network");
                    return default;
                }

                // Read message payload
                var dataLength = reader.ReadInt32();
                var messageData = reader.ReadBytes(dataLength);

                var message = new T();
                message.Deserialize(messageData);

                return message;
            }
        }

        public INetworkMessage Deserialize(byte[] data, Type messageType)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // Read type metadata
                var typeId = reader.ReadString();
                var version = reader.ReadUInt16();

                // Get type from registry
                Type type;
                try
                {
                    type = _typeRegistry.GetType(typeId, version);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError($"Unknown message type: {typeId} v{version} - {ex.Message}", "Network");
                    return null;
                }

                if (!typeof(INetworkMessage).IsAssignableFrom(type))
                {
                    _logger.LogError($"Type {type.FullName} does not implement INetworkMessage", "Network");
                    return null;
                }

                // Read message payload
                var dataLength = reader.ReadInt32();
                var messageData = reader.ReadBytes(dataLength);

                var message = (INetworkMessage)Activator.CreateInstance(type);
                message.Deserialize(messageData);

                return message;
            }
        }
    }
}
