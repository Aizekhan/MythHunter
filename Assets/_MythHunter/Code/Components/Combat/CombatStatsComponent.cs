// Шлях: Assets/_MythHunter/Code/Components/Combat/CombatStatsComponent.cs
using MythHunter.Core.ECS;
using UnityEngine;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// Компонент з характеристиками для бою
    /// </summary>
    public struct CombatStatsComponent : IComponent, ISerializableComponent
    {
        // Базові характеристики
        public float AttackPower;
        public float Defense;
        public float AttackSpeed;
        public float CriticalChance;
        public float CriticalMultiplier;
        public float Accuracy;
        public float DodgeChance;
        public float BlockChance;
        public float CounterAttackChance;

        // Характеристики, специфічні для бойової системи
        public float Rage;               // Поточна лють (0-100)
        public float MaxRage;            // Максимальна лють (зазвичай 100)
        public float Concentration;      // Поточна концентрація
        public float MaxConcentration;   // Максимальна концентрація (залежить від Витривалості)

        // Змінні для відстеження стану бою
        public bool IsInCombat;          // Чи знаходиться в бою
        public int CombatTargetEntityId; // ID цілі, з якою в бою
        public float LastAttackTime;     // Час останньої атаки

        public byte[] Serialize()
        {
            // Реалізація серіалізації
            using (var memoryStream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(memoryStream))
            {
                writer.Write(AttackPower);
                writer.Write(Defense);
                writer.Write(AttackSpeed);
                writer.Write(CriticalChance);
                writer.Write(CriticalMultiplier);
                writer.Write(Accuracy);
                writer.Write(DodgeChance);
                writer.Write(BlockChance);
                writer.Write(CounterAttackChance);
                writer.Write(Rage);
                writer.Write(MaxRage);
                writer.Write(Concentration);
                writer.Write(MaxConcentration);
                writer.Write(IsInCombat);
                writer.Write(CombatTargetEntityId);
                writer.Write(LastAttackTime);

                return memoryStream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            // Реалізація десеріалізації
            using (var memoryStream = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(memoryStream))
            {
                AttackPower = reader.ReadSingle();
                Defense = reader.ReadSingle();
                AttackSpeed = reader.ReadSingle();
                CriticalChance = reader.ReadSingle();
                CriticalMultiplier = reader.ReadSingle();
                Accuracy = reader.ReadSingle();
                DodgeChance = reader.ReadSingle();
                BlockChance = reader.ReadSingle();
                CounterAttackChance = reader.ReadSingle();
                Rage = reader.ReadSingle();
                MaxRage = reader.ReadSingle();
                Concentration = reader.ReadSingle();
                MaxConcentration = reader.ReadSingle();
                IsInCombat = reader.ReadBoolean();
                CombatTargetEntityId = reader.ReadInt32();
                LastAttackTime = reader.ReadSingle();
            }
        }
    }
}
