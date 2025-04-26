using System.IO;
using MythHunter.Core.ECS;
using MythHunter.Data.Serialization;

namespace MythHunter.Components.Core
{
    /// <summary>
    /// Компонент імені
    /// </summary>
    public struct NameComponent : IComponent, ISerializable
    {
        public string Name;
        
        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Name ?? string.Empty);
                return stream.ToArray();
            }
        }
        
        public void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Name = reader.ReadString();
            }
        }
    }
}