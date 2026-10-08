// Шлях: Assets/_MythHunter/Code/Components/Combat/HealthComponent.cs
using MythHunter.Core.ECS;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// Компонент здоров'я
    /// </summary>
    public struct HealthComponent : IComponent, ISerializableComponent
    {
        public float CurrentHealth;
        public float MaxHealth;
        public bool IsInvulnerable;
        public bool IsDead;
        public float RegenRate;

        // Час останнього отримання урону
        public float LastDamageTime;

        // Час останнього регену здоров'я
        public float LastRegenTime;

        public byte[] Serialize()
        {
            using (var memoryStream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(memoryStream))
            {
                writer.Write(CurrentHealth);
                writer.Write(MaxHealth);
                writer.Write(IsInvulnerable);
                writer.Write(IsDead);
                writer.Write(RegenRate);
                writer.Write(LastDamageTime);
                writer.Write(LastRegenTime);

                return memoryStream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (var memoryStream = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(memoryStream))
            {
                CurrentHealth = reader.ReadSingle();
                MaxHealth = reader.ReadSingle();
                IsInvulnerable = reader.ReadBoolean();
                IsDead = reader.ReadBoolean();
                RegenRate = reader.ReadSingle();
                LastDamageTime = reader.ReadSingle();
                LastRegenTime = reader.ReadSingle();
            }
        }
    }
}
