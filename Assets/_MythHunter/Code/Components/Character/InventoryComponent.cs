// Шлях: Assets/_MythHunter/Code/Components/Character/InventoryComponent.cs
using System.IO;
using MythHunter.Core.ECS;
using MythHunter.Data.Serialization;

namespace MythHunter.Components.Character
{
    /// <summary>
    /// Компонент інвентаря
    /// </summary>
    public struct InventoryComponent : IComponent, ISerializable
    {
        public int Capacity;
        public int[] ItemIds;
        public int ItemCount;

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Capacity);
                writer.Write(ItemCount);

                // Записуємо тільки фактично використані слоти
                for (int i = 0; i < ItemCount; i++)
                {
                    writer.Write(ItemIds[i]);
                }

                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Capacity = reader.ReadInt32();
                ItemCount = reader.ReadInt32();

                // Створюємо масив потрібного розміру
                ItemIds = new int[Capacity];

                // Зчитуємо тільки фактично використані слоти
                for (int i = 0; i < ItemCount; i++)
                {
                    ItemIds[i] = reader.ReadInt32();
                }
            }
        }
    }
}
