// VersionedSerializer.cs
using System;
using System.IO;
using MythHunter.Utils.Logging;

namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// Серіалізатор з підтримкою версіонування
    /// </summary>
    public class VersionedSerializer
    {
        private readonly IMythLogger _logger;
        private const ushort CURRENT_VERSION = 1; // Збільшуйте при зміні формату

        public VersionedSerializer(IMythLogger logger)
        {
            _logger = logger;
        }

        public byte[] Serialize<T>(T data) where T : ISerializable
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Записуємо версію формату
                writer.Write(CURRENT_VERSION);

                // Записуємо ідентифікатор типу
                writer.Write(typeof(T).FullName);

                // Серіалізуємо дані
                byte[] serializedData = data.Serialize();
                writer.Write(serializedData.Length);
                writer.Write(serializedData);

                return stream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] data) where T : ISerializable, new()
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // Зчитуємо версію
                ushort version = reader.ReadUInt16();

                // Перевіряємо версію
                if (version > CURRENT_VERSION)
                {
                    _logger.LogWarning($"Attempting to deserialize newer format version: {version}, current: {CURRENT_VERSION}");
                }

                // Зчитуємо тип
                string typeName = reader.ReadString();
                if (typeName != typeof(T).FullName)
                {
                    _logger.LogWarning($"Type mismatch during deserialization. Expected: {typeof(T).FullName}, Got: {typeName}");
                }

                // Зчитуємо дані
                int dataLength = reader.ReadInt32();
                byte[] objectData = reader.ReadBytes(dataLength);

                // Створюємо об'єкт і десеріалізуємо
                T result = new T();
                result.Deserialize(objectData);

                return result;
            }
        }
    }
}
