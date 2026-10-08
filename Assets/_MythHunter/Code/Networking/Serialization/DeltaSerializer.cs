// DeltaSerializer.cs
using System;
using System.Collections.Generic;
using System.IO;
using MythHunter.Utils.Logging;
using MythHunter.Data.Serialization;
namespace MythHunter.Networking.Serialization
{
    /// <summary>
    /// Серіалізатор для ефективної передачі змін компонентів
    /// </summary>
    public class DeltaSerializer
    {
        private readonly IMythLogger _logger;
        private readonly Dictionary<int, byte[]> _lastState = new Dictionary<int, byte[]>();

        public DeltaSerializer(IMythLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Створює патч, що містить лише зміни у стані сутності
        /// </summary>
        public byte[] CreateDelta<T>(int entityId, T currentState) where T : ISerializable
        {
            byte[] fullState = currentState.Serialize();

            if (!_lastState.TryGetValue(entityId, out var previousState))
            {
                // Перше оновлення - зберігаємо повний стан
                _lastState[entityId] = fullState;
                return CreateFullStatePacket(fullState);
            }

            // Створюємо різницю між поточним і попереднім станом
            byte[] delta = CreateStateDelta(previousState, fullState);

            // Оновлюємо збережений стан
            _lastState[entityId] = fullState;

            return delta.Length < fullState.Length * 0.8f ? delta : CreateFullStatePacket(fullState);
        }

        /// <summary>
        /// Відновлює повний стан з дельта-патчу
        /// </summary>
        public T ApplyDelta<T>(int entityId, byte[] delta) where T : ISerializable, new()
        {
            using (MemoryStream stream = new MemoryStream(delta))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                bool isFullState = reader.ReadBoolean();

                if (isFullState)
                {
                    // Повний стан
                    int length = reader.ReadInt32();
                    byte[] fullState = reader.ReadBytes(length);

                    // Зберігаємо стан
                    _lastState[entityId] = fullState;

                    // Десеріалізуємо
                    T result = new T();
                    result.Deserialize(fullState);
                    return result;
                }
                else
                {
                    // Дельта-оновлення
                    if (!_lastState.TryGetValue(entityId, out var baseState))
                    {
                        _logger.LogError($"Cannot apply delta: no base state for entity {entityId}");
                        return default;
                    }

                    // Зчитуємо кількість змін
                    int changeCount = reader.ReadInt32();

                    // Створюємо копію базового стану
                    byte[] newState = new byte[baseState.Length];
                    Array.Copy(baseState, newState, baseState.Length);

                    // Застосовуємо зміни
                    for (int i = 0; i < changeCount; i++)
                    {
                        int offset = reader.ReadInt32();
                        byte value = reader.ReadByte();

                        if (offset < newState.Length)
                            newState[offset] = value;
                    }

                    // Зберігаємо оновлений стан
                    _lastState[entityId] = newState;

                    // Десеріалізуємо
                    T result = new T();
                    result.Deserialize(newState);
                    return result;
                }
            }
        }

        /// <summary>
        /// Пакує повний стан
        /// </summary>
        private byte[] CreateFullStatePacket(byte[] state)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(true); // isFullState
                writer.Write(state.Length);
                writer.Write(state);

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Створює дельта-патч
        /// </summary>
        private byte[] CreateStateDelta(byte[] oldState, byte[] newState)
        {
            List<KeyValuePair<int, byte>> changes = new List<KeyValuePair<int, byte>>();

            // Знаходимо відмінності
            int minLength = Math.Min(oldState.Length, newState.Length);
            for (int i = 0; i < minLength; i++)
            {
                if (oldState[i] != newState[i])
                {
                    changes.Add(new KeyValuePair<int, byte>(i, newState[i]));
                }
            }

            // Якщо новий стан довший, додаємо нові байти
            for (int i = minLength; i < newState.Length; i++)
            {
                changes.Add(new KeyValuePair<int, byte>(i, newState[i]));
            }

            // Створюємо патч
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(false); // isFullState
                writer.Write(changes.Count);

                foreach (var change in changes)
                {
                    writer.Write(change.Key);   // offset
                    writer.Write(change.Value); // value
                }

                return stream.ToArray();
            }
        }
    }
}
