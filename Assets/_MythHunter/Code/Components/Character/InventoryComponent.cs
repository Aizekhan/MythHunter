// Assets/_MythHunter/Code/Components/Character/InventoryComponent.cs

using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Components.Character
{
    /// <summary>
    /// Компонент інвентаря героя
    /// </summary>
    public struct InventoryComponent : ISerializableComponent
    {
        public List<string> Items;
        public EquippedItems EquippedItems;
        public Bag Bag;

        // Реалізація серіалізації
        public byte[] Serialize()
        {
            using (var ms = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(ms))
            {
                // Серіалізація списку предметів
                writer.Write(Items != null ? Items.Count : 0);
                if (Items != null)
                {
                    foreach (var item in Items)
                    {
                        writer.Write(item ?? string.Empty);
                    }
                }

                // Серіалізація спорядження
                SerializeEquippedItems(writer, EquippedItems);

                // Серіалізація сумки
                SerializeBag(writer, Bag);

                return ms.ToArray();
            }
        }

        // Реалізація десеріалізації
        public void Deserialize(byte[] data)
        {
            using (var ms = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(ms))
            {
                // Десеріалізація списку предметів
                int itemCount = reader.ReadInt32();
                Items = new List<string>(itemCount);

                for (int i = 0; i < itemCount; i++)
                {
                    Items.Add(reader.ReadString());
                }

                // Десеріалізація спорядження
                EquippedItems = DeserializeEquippedItems(reader);

                // Десеріалізація сумки
                Bag = DeserializeBag(reader);
            }
        }

        private void SerializeEquippedItems(System.IO.BinaryWriter writer, EquippedItems items)
        {
            // Серіалізація броні
            writer.Write(items.Armor.Boots ?? string.Empty);
            writer.Write(items.Armor.Gloves ?? string.Empty);
            writer.Write(items.Armor.Chest ?? string.Empty);
            writer.Write(items.Armor.Helmet ?? string.Empty);

            // Артефакт
            writer.Write(items.Artifact ?? string.Empty);

            // Зброя
            writer.Write(items.Weapons.MainWeapon ?? string.Empty);
            writer.Write(items.Weapons.SecondaryWeapon ?? string.Empty);
            writer.Write(items.Weapons.AuxiliaryWeapon ?? string.Empty);
            writer.Write(items.Weapons.CurrentWeapon ?? string.Empty);
        }

        private EquippedItems DeserializeEquippedItems(System.IO.BinaryReader reader)
        {
            var items = new EquippedItems();

            // Десеріалізація броні
            items.Armor.Boots = reader.ReadString();
            items.Armor.Gloves = reader.ReadString();
            items.Armor.Chest = reader.ReadString();
            items.Armor.Helmet = reader.ReadString();

            // Артефакт
            items.Artifact = reader.ReadString();

            // Зброя
            items.Weapons.MainWeapon = reader.ReadString();
            items.Weapons.SecondaryWeapon = reader.ReadString();
            items.Weapons.AuxiliaryWeapon = reader.ReadString();
            items.Weapons.CurrentWeapon = reader.ReadString();

            return items;
        }

        private void SerializeBag(System.IO.BinaryWriter writer, Bag bag)
        {
            writer.Write(bag.Slots);

            // Серіалізація еліксирів
            writer.Write(bag.Elixirs != null ? bag.Elixirs.Count : 0);
            if (bag.Elixirs != null)
            {
                foreach (var elixir in bag.Elixirs)
                {
                    writer.Write(elixir ?? string.Empty);
                }
            }
        }

        private Bag DeserializeBag(System.IO.BinaryReader reader)
        {
            var bag = new Bag();

            bag.Slots = reader.ReadInt32();

            // Десеріалізація еліксирів
            int elixirCount = reader.ReadInt32();
            bag.Elixirs = new List<string>(elixirCount);

            for (int i = 0; i < elixirCount; i++)
            {
                bag.Elixirs.Add(reader.ReadString());
            }

            return bag;
        }
    }

    /// <summary>
    /// Структура для спорядженого обладнання героя
    /// </summary>
    public struct EquippedItems
    {
        public Armor Armor;
        public string Artifact;
        public Weapons Weapons;
    }

    /// <summary>
    /// Структура для броні героя
    /// </summary>
    public struct Armor
    {
        public string Boots;
        public string Gloves;
        public string Chest;
        public string Helmet;
    }

    /// <summary>
    /// Структура для зброї героя
    /// </summary>
    public struct Weapons
    {
        public string MainWeapon;
        public string SecondaryWeapon;
        public string AuxiliaryWeapon;
        public string CurrentWeapon;
    }

    /// <summary>
    /// Структура для сумки героя
    /// </summary>
    public struct Bag
    {
        public int Slots;
        public List<string> Elixirs;
    }
}
