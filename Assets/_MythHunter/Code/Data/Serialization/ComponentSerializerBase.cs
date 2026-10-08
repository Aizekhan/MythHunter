// Assets/_MythHunter/Code/Data/Serialization/ComponentSerializerBase.cs
using System.IO;
using MythHunter.Core.ECS;

namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// Базовий клас для серіалізаторів компонентів
    /// </summary>
    public abstract class ComponentSerializerBase<T> : IComponentSerializer<T> where T : struct, IComponent
    {
        public byte[] Serialize(T component)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                SerializeInternal(writer, component);
                return stream.ToArray();
            }
        }

        public T Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return DeserializeInternal(reader);
            }
        }

        protected abstract void SerializeInternal(BinaryWriter writer, T component);
        protected abstract T DeserializeInternal(BinaryReader reader);
    }
}
