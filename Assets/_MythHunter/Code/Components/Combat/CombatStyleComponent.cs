// Шлях: Assets/_MythHunter/Code/Components/Combat/CombatStyleComponent.cs
using MythHunter.Core.ECS;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// Тип бойової стійки
    /// </summary>
    public enum CombatStance
    {
        Balanced = 0,   // Збалансована
        Aggressive = 1, // Агресивна
        Defensive = 2   // Захисна
    }

    /// <summary>
    /// Компонент стилю бою
    /// </summary>
    public struct CombatStyleComponent : IComponent, ISerializableComponent
    {
        public CombatStance CurrentStance;
        public CombatStance DefaultStance;
        public bool UseAutoStanceChange; // Чи використовувати автозміну стійки

        // Модифікатори характеристик для різних стійок
        // Наприклад, множники для атрибутів при зміні стійки
        public float AggressiveAttackMod;
        public float AggressiveDefenseMod;
        public float AggressiveDodgeMod;
        public float AggressiveBlockMod;

        public float DefensiveAttackMod;
        public float DefensiveDefenseMod;
        public float DefensiveDodgeMod;
        public float DefensiveBlockMod;

        // Час з останньої зміни стійки
        public float LastStanceChangeTime;

        public byte[] Serialize()
        {
            using (var memoryStream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(memoryStream))
            {
                writer.Write((int)CurrentStance);
                writer.Write((int)DefaultStance);
                writer.Write(UseAutoStanceChange);

                writer.Write(AggressiveAttackMod);
                writer.Write(AggressiveDefenseMod);
                writer.Write(AggressiveDodgeMod);
                writer.Write(AggressiveBlockMod);

                writer.Write(DefensiveAttackMod);
                writer.Write(DefensiveDefenseMod);
                writer.Write(DefensiveDodgeMod);
                writer.Write(DefensiveBlockMod);

                writer.Write(LastStanceChangeTime);

                return memoryStream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (var memoryStream = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(memoryStream))
            {
                CurrentStance = (CombatStance)reader.ReadInt32();
                DefaultStance = (CombatStance)reader.ReadInt32();
                UseAutoStanceChange = reader.ReadBoolean();

                AggressiveAttackMod = reader.ReadSingle();
                AggressiveDefenseMod = reader.ReadSingle();
                AggressiveDodgeMod = reader.ReadSingle();
                AggressiveBlockMod = reader.ReadSingle();

                DefensiveAttackMod = reader.ReadSingle();
                DefensiveDefenseMod = reader.ReadSingle();
                DefensiveDodgeMod = reader.ReadSingle();
                DefensiveBlockMod = reader.ReadSingle();

                LastStanceChangeTime = reader.ReadSingle();
            }
        }
    }
}
