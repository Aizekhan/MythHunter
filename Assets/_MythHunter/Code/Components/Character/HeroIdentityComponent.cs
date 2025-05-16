// Assets/_MythHunter/Code/Components/Character/HeroIdentityComponent.cs

using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Components.Character
{
    /// <summary>
    /// Компонент з основною інформацією про героя
    /// </summary>
    public struct HeroIdentityComponent : ISerializableComponent
    {
        public string HeroID;
        public string Name;
        public string Race;
        public string Image;
        public List<string> Skins;

        // Реалізація серіалізації
        public byte[] Serialize()
        {
            using (var ms = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(ms))
            {
                writer.Write(HeroID ?? string.Empty);
                writer.Write(Name ?? string.Empty);
                writer.Write(Race ?? string.Empty);
                writer.Write(Image ?? string.Empty);

                // Серіалізація списку скінів
                writer.Write(Skins != null ? Skins.Count : 0);
                if (Skins != null)
                {
                    foreach (var skin in Skins)
                    {
                        writer.Write(skin ?? string.Empty);
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
                HeroID = reader.ReadString();
                Name = reader.ReadString();
                Race = reader.ReadString();
                Image = reader.ReadString();

                // Десеріалізація списку скінів
                int skinCount = reader.ReadInt32();
                Skins = new List<string>(skinCount);

                for (int i = 0; i < skinCount; i++)
                {
                    Skins.Add(reader.ReadString());
                }
            }
        }
    }
}
