// Шлях: Assets/_MythHunter/Code/Components/Combat/TeamComponent.cs
using MythHunter.Core.ECS;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// Компонент для визначення команди сутності (для розрізнення союзників/ворогів)
    /// </summary>
    public struct TeamComponent : IComponent, ISerializableComponent
    {
        public int TeamId;
        public bool IsNeutral;

        public byte[] Serialize()
        {
            using (var memoryStream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(memoryStream))
            {
                writer.Write(TeamId);
                writer.Write(IsNeutral);

                return memoryStream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (var memoryStream = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(memoryStream))
            {
                TeamId = reader.ReadInt32();
                IsNeutral = reader.ReadBoolean();
            }
        }
    }
}
