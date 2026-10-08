// Assets/_MythHunter/Code/Components/Character/StatsComponent.cs

using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Components.Character
{
    /// <summary>
    /// Компонент характеристик героя
    /// </summary>
    public struct StatsComponent : ISerializableComponent
    {
        public Dictionary<StatType, float> Values;

        // Допоміжні методи для зручного отримання характеристик
        public float GetStat(StatType type, float defaultValue = 0f)
        {
            return Values != null && Values.TryGetValue(type, out var value) ? value : defaultValue;
        }

        public void SetStat(StatType type, float value)
        {
            if (Values == null)
                Values = new Dictionary<StatType, float>();

            Values[type] = value;
        }

        // Базові характеристики
        public float Level => GetStat(StatType.Level, 1);
        public float HP => GetStat(StatType.HP);
        public float HpRegen => GetStat(StatType.HpRegen);
        public float Stamina => GetStat(StatType.Stamina);
        public float StaminaRegen => GetStat(StatType.StaminaRegen);

        // Бойові характеристики
        public float Damage => GetStat(StatType.Damage);
        public float ArmorResistance => GetStat(StatType.ArmorResistance);
        public float CritChance => GetStat(StatType.CritChance);
        public float BlockChance => GetStat(StatType.BlockChance);

        // Реалізація серіалізації/десеріалізації
        public byte[] Serialize()
        {
            using (var ms = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(ms))
            {
                // Записуємо кількість характеристик
                writer.Write(Values != null ? Values.Count : 0);

                // Записуємо пари ключ-значення
                if (Values != null)
                {
                    foreach (var pair in Values)
                    {
                        writer.Write((int)pair.Key);
                        writer.Write(pair.Value);
                    }
                }

                return ms.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            Values = new Dictionary<StatType, float>();

            using (var ms = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(ms))
            {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    var key = (StatType)reader.ReadInt32();
                    var value = reader.ReadSingle();
                    Values[key] = value;
                }
            }
        }
    }
}
