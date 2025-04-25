using MythHunter.Core.ECS;
using System.IO;
using MythHunter.Data.Serialization;

namespace MythHunter.Components.Movement
{
    /// <summary>
/// Velocity component
/// </summary>
    public struct VelocityComponent : IComponent, ISerializable
    {
        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Write component data
                return stream.ToArray();
            }
        }
        
        public void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // Read component data
            }
        }
    }
}
