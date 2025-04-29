// Шлях: Assets/_MythHunter/Code/Components/Movement/MovementComponent.cs
using System.IO;
using MythHunter.Core.ECS;
using MythHunter.Data.Serialization;
using UnityEngine;

namespace MythHunter.Components.Movement
{
    /// <summary>
    /// Компонент руху
    /// </summary>
    public struct MovementComponent : IComponent, ISerializable
    {
        public float Speed;
        public Vector3 Direction;
        public bool IsMoving;

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Speed);
                writer.Write(Direction.x);
                writer.Write(Direction.y);
                writer.Write(Direction.z);
                writer.Write(IsMoving);

                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Speed = reader.ReadSingle();
                Direction = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                );
                IsMoving = reader.ReadBoolean();
            }
        }
    }
}
