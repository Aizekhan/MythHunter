// Шлях: Assets/_MythHunter/Code/Components/Core/DescriptionComponent.cs
using System.IO;
using MythHunter.Core.ECS;
using MythHunter.Data.Serialization;

namespace MythHunter.Components.Core
{
    /// <summary>
    /// Компонент опису
    /// </summary>
    public struct DescriptionComponent : IComponent, ISerializable
    {
        public string Description;

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Description ?? string.Empty);
                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Description = reader.ReadString();
            }
        }
    }
}
