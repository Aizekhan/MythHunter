// Шлях: Assets/_MythHunter/Code/Components/Character/HealthComponent.cs
using System.IO;
using MythHunter.Core.ECS;
using MythHunter.Data.Serialization;

namespace MythHunter.Components.Character
{
    /// <summary>
    /// Компонент здоров'я персонажа
    /// </summary>
    public struct HealthComponent : IComponent, ISerializable
    {
        public float CurrentHealth;
        public float MaxHealth;
        public bool IsDead;

        // Поля для регенерації
        public bool HasRegeneration;
        public float RegenAmount;
        public float RegenInterval;
        public float RegenTimer;

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(CurrentHealth);
                writer.Write(MaxHealth);
                writer.Write(IsDead);
                writer.Write(HasRegeneration);
                writer.Write(RegenAmount);
                writer.Write(RegenInterval);
                writer.Write(RegenTimer);

                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                CurrentHealth = reader.ReadSingle();
                MaxHealth = reader.ReadSingle();
                IsDead = reader.ReadBoolean();
                HasRegeneration = reader.ReadBoolean();
                RegenAmount = reader.ReadSingle();
                RegenInterval = reader.ReadSingle();
                RegenTimer = reader.ReadSingle();
            }
        }
    }
}
