using RPGFramework.Networking;
using System.IO;

namespace MythHunter.Networking
{
    /// <summary>
    /// Sample networked entity for MythHunter.
    /// Demonstrates how to create a game-specific network entity.
    /// </summary>
    public class SampleNetworkedEntity : NetworkEntity
    {
        // Game-specific properties
        public string EntityName;
        public int Health;
        public int MaxHealth;
        public Vector3Network WorldPosition;
        public QuaternionNetwork WorldRotation;

        public SampleNetworkedEntity(int networkId, int ownerId)
            : base(networkId, ownerId, ReplicationMode.Delta)
        {
            EntityName = "Unknown";
            Health = 100;
            MaxHealth = 100;
            WorldPosition = Vector3Network.Zero;
            WorldRotation = QuaternionNetwork.Identity;
        }

        public override byte[] SerializeState()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(EntityName ?? "");
                writer.Write(Health);
                writer.Write(MaxHealth);
                writer.Write(WorldPosition.X);
                writer.Write(WorldPosition.Y);
                writer.Write(WorldPosition.Z);
                writer.Write(WorldRotation.X);
                writer.Write(WorldRotation.Y);
                writer.Write(WorldRotation.Z);
                writer.Write(WorldRotation.W);

                return ms.ToArray();
            }
        }

        public override void DeserializeState(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                EntityName = reader.ReadString();
                Health = reader.ReadInt32();
                MaxHealth = reader.ReadInt32();
                WorldPosition = new Vector3Network(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
                WorldRotation = new QuaternionNetwork(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
            }
        }
    }
}
