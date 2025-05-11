// Шлях: Assets/_MythHunter/Code/Networking/Messages/NetworkEventMessage.cs
using System;
using MythHunter.Events;
using MythHunter.Events.Network;
using MythHunter.Networking.Messages;

namespace MythHunter.Networking.Messages
{
    /// <summary>
    /// Мережеве повідомлення для передачі подій
    /// </summary>
    [Serializable]
    public class NetworkEventMessage : INetworkMessage
    {
        public string EventType
        {
            get; set;
        }
        public byte[] EventData
        {
            get; set;
        }
        public bool IsReliable
        {
            get; set;
        }
        public NetworkEventPriority Priority
        {
            get; set;
        }

        public string GetMessageId()
        {
            return $"NetworkEvent_{EventType}_{Guid.NewGuid()}";
        }

        public byte[] Serialize()
        {
            using (var memoryStream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(memoryStream))
            {
                writer.Write(EventType ?? string.Empty);
                writer.Write(EventData?.Length ?? 0);

                if (EventData != null && EventData.Length > 0)
                    writer.Write(EventData);

                writer.Write(IsReliable);
                writer.Write((int)Priority);

                return memoryStream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (var memoryStream = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(memoryStream))
            {
                EventType = reader.ReadString();
                int dataLength = reader.ReadInt32();

                if (dataLength > 0)
                {
                    EventData = reader.ReadBytes(dataLength);
                }
                else
                {
                    EventData = new byte[0];
                }

                IsReliable = reader.ReadBoolean();
                Priority = (NetworkEventPriority)reader.ReadInt32();
            }
        }

        /// <summary>
        /// Десеріалізує подію з заданого типу
        /// </summary>
        public object DeserializeEvent(Type eventType)
        {
            // Знаходимо метод десеріалізації для конкретного типу
            if (eventType.GetInterface(nameof(MythHunter.Data.Serialization.ISerializable)) != null)
            {
                // Створюємо екземпляр потрібного типу
                var evt = Activator.CreateInstance(eventType);

                // Викликаємо метод Deserialize через рефлексію
                var method = eventType.GetMethod("Deserialize");
                method.Invoke(evt, new object[] { EventData });

                return evt;
            }

            return null;
        }
    }

    /// <summary>
    /// Типізоване мережеве повідомлення для подій
    /// </summary>
    [Serializable]
    public class NetworkEventMessage<TEvent> : NetworkEventMessage where TEvent : struct, IEvent
    {
        public TEvent Event
        {
            get
            {
                return (TEvent)DeserializeEvent(typeof(TEvent));
            }
            set
            {
                if (value is MythHunter.Data.Serialization.ISerializable serializable)
                {
                    EventData = serializable.Serialize();
                    EventType = typeof(TEvent).AssemblyQualifiedName;
                }
            }
        }
    }
}
