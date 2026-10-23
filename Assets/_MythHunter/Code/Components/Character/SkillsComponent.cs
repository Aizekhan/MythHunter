// Assets/_MythHunter/Code/Components/Character/SkillsComponent.cs

using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Components.Character
{
    /// <summary>
    /// Компонент навичок героя
    /// </summary>
    public struct SkillsComponent : ISerializableComponent
    {
        public List<Skill> PassiveSkills;
        public List<Skill> ActiveSkills;

        // Реалізація серіалізації
        // Продовження Assets/_MythHunter/Code/Components/Character/SkillsComponent.cs

        // Реалізація серіалізації
        public byte[] Serialize()
        {
            using (var ms = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(ms))
            {
                // Серіалізація пасивних навичок
                writer.Write(PassiveSkills != null ? PassiveSkills.Count : 0);
                if (PassiveSkills != null)
                {
                    foreach (var skill in PassiveSkills)
                    {
                        writer.Write(skill.Id ?? string.Empty);
                        writer.Write(skill.Level);
                    }
                }

                // Серіалізація активних навичок
                writer.Write(ActiveSkills != null ? ActiveSkills.Count : 0);
                if (ActiveSkills != null)
                {
                    foreach (var skill in ActiveSkills)
                    {
                        writer.Write(skill.Id ?? string.Empty);
                        writer.Write(skill.Level);
                    }
                }

                return ms.ToArray();
            }
        }

        // Реалізація десеріалізації
        public void Deserialize(byte[] data)
        {
            using (var ms = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(ms))
            {
                // Десеріалізація пасивних навичок
                int passiveCount = reader.ReadInt32();
                PassiveSkills = new List<Skill>(passiveCount);

                for (int i = 0; i < passiveCount; i++)
                {
                    PassiveSkills.Add(new Skill
                    {
                        Id = reader.ReadString(),
                        Level = reader.ReadInt32()
                    });
                }

                // Десеріалізація активних навичок
                int activeCount = reader.ReadInt32();
                ActiveSkills = new List<Skill>(activeCount);

                for (int i = 0; i < activeCount; i++)
                {
                    ActiveSkills.Add(new Skill
                    {
                        Id = reader.ReadString(),
                        Level = reader.ReadInt32()
                    });
                }
            }
        }
    }

    /// <summary>
    /// Структура для зберігання інформації про навичку
    /// </summary>
    public struct Skill
    {
        public string Id;
        public int Level;
    }
}
