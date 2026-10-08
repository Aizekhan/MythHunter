// Шлях: Assets/_MythHunter/Code/Components/Combat/CombatAbilityComponent.cs
using System;
using MythHunter.Core.ECS;

namespace MythHunter.Components.Combat
{
    /// <summary>
    /// Тип бойової здібності
    /// </summary>
    public enum CombatAbilityType
    {
        Active,     // Активна здібність, вимагає ручного використання
        Passive,    // Пасивна здібність, працює автоматично
        Reactive    // Реактивна, спрацьовує при певних умовах
    }

    /// <summary>
    /// Компонент бойових здібностей
    /// </summary>
    public struct CombatAbilityComponent : IComponent, ISerializableComponent
    {
        // ID активної здібності
        public string ActiveAbilityId;

        // Чи використана активна здібність у поточному бою
        public bool IsActiveAbilityUsed;

        // Час останнього використання активної здібності
        public float LastActiveUseTime;

        // Масив пасивних здібностей (ідентифікатори)
        public string[] PassiveAbilityIds;

        // Автоматичне використання активної здібності
        public bool UseAutoActivation;

        // Умова для автоматичного використання (% здоров'я)
        public float AutoActivationHealthThreshold;

        public byte[] Serialize()
        {
            using (var memoryStream = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(memoryStream))
            {
                writer.Write(ActiveAbilityId ?? string.Empty);
                writer.Write(IsActiveAbilityUsed);
                writer.Write(LastActiveUseTime);

                // Серіалізація масиву пасивних здібностей
                int passiveCount = PassiveAbilityIds?.Length ?? 0;
                writer.Write(passiveCount);

                if (passiveCount > 0)
                {
                    for (int i = 0; i < passiveCount; i++)
                    {
                        writer.Write(PassiveAbilityIds[i] ?? string.Empty);
                    }
                }

                writer.Write(UseAutoActivation);
                writer.Write(AutoActivationHealthThreshold);

                return memoryStream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (var memoryStream = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(memoryStream))
            {
                ActiveAbilityId = reader.ReadString();
                IsActiveAbilityUsed = reader.ReadBoolean();
                LastActiveUseTime = reader.ReadSingle();

                // Десеріалізація масиву пасивних здібностей
                int passiveCount = reader.ReadInt32();

                if (passiveCount > 0)
                {
                    PassiveAbilityIds = new string[passiveCount];
                    for (int i = 0; i < passiveCount; i++)
                    {
                        PassiveAbilityIds[i] = reader.ReadString();
                    }
                }
                else
                {
                    PassiveAbilityIds = Array.Empty<string>();
                }

                UseAutoActivation = reader.ReadBoolean();
                AutoActivationHealthThreshold = reader.ReadSingle();
            }
        }
    }
}
