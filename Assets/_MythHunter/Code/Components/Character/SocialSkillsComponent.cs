// Assets/_MythHunter/Code/Components/Character/SocialSkillsComponent.cs

using MythHunter.Core.ECS;

namespace MythHunter.Components.Character
{
    /// <summary>
    /// Компонент соціальних навичок героя
    /// </summary>
    public struct SocialSkillsComponent : ISerializableComponent
    {
        public string Religion;
        public string Ideology;
        public string Class;
        public string[] Professions;

        // Реалізація серіалізації
        public byte[] Serialize()
        {
            using (var ms = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(ms))
            {
                writer.Write(Religion ?? string.Empty);
                writer.Write(Ideology ?? string.Empty);
                writer.Write(Class ?? string.Empty);

                // Серіалізація професій
                writer.Write(Professions != null ? Professions.Length : 0);
                if (Professions != null)
                {
                    foreach (var prof in Professions)
                    {
                        writer.Write(prof ?? string.Empty);
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
                Religion = reader.ReadString();
                Ideology = reader.ReadString();
                Class = reader.ReadString();

                int profCount = reader.ReadInt32();
                Professions = new string[profCount];

                for (int i = 0; i < profCount; i++)
                {
                    Professions[i] = reader.ReadString();
                }
            }
        }
    }
}
