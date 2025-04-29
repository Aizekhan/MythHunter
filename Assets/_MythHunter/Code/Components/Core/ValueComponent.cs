// Шлях: Assets/_MythHunter/Code/Components/Core/ValueComponent.cs
using System.IO;
using MythHunter.Core.ECS;
using MythHunter.Data.Serialization;

namespace MythHunter.Components.Core
{
    /// <summary>
    /// Компонент вартості
    /// </summary>
    public struct ValueComponent : IComponent, ISerializable
    {
        public int Value;

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Value);
                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Value = reader.ReadInt32();
            }
        }
    }
}
