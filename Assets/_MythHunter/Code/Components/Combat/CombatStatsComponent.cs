// Шлях: Assets/_MythHunter/Code/Components/Combat/CombatStatsComponent.cs
using System.IO;
using MythHunter.Core.ECS;
using MythHunter.Data.Serialization;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// Компонент бойових характеристик
    /// </summary>
    public struct CombatStatsComponent : IComponent, ISerializable
    {
        public float AttackPower;
        public float Defense;
        public float CriticalChance;
        public float CriticalMultiplier;

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(AttackPower);
                writer.Write(Defense);
                writer.Write(CriticalChance);
                writer.Write(CriticalMultiplier);

                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                AttackPower = reader.ReadSingle();
                Defense = reader.ReadSingle();
                CriticalChance = reader.ReadSingle();
                CriticalMultiplier = reader.ReadSingle();
            }
        }
    }
}
