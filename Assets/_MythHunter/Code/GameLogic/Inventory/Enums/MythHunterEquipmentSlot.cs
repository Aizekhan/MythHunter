// Assets/_MythHunter/Code/GameLogic/Inventory/Enums/MythHunterEquipmentSlot.cs

namespace MythHunter.GameLogic.Inventory
{
    /// <summary>
    /// Equipment slot types for MythHunter entities.
    /// Used with Framework InventoryComponent for type-safe equipment management.
    /// </summary>
    public enum MythHunterEquipmentSlot
    {
        // ============================================================
        // ARMOR SLOTS (0-9)
        // ============================================================

        /// <summary>
        /// Head armor slot (helmets, hats, crowns)
        /// </summary>
        Helmet = 0,

        /// <summary>
        /// Chest armor slot (chest plates, robes, tunics)
        /// </summary>
        Chest = 1,

        /// <summary>
        /// Hand armor slot (gloves, gauntlets, bracers)
        /// </summary>
        Gloves = 2,

        /// <summary>
        /// Foot armor slot (boots, shoes, greaves)
        /// </summary>
        Boots = 3,

        // ============================================================
        // WEAPON SLOTS (10-19)
        // ============================================================

        /// <summary>
        /// Primary weapon slot (swords, axes, staffs)
        /// Default active weapon for combat
        /// </summary>
        MainWeapon = 10,

        /// <summary>
        /// Secondary weapon slot (off-hand weapon, shield)
        /// Can be switched to during combat
        /// </summary>
        SecondaryWeapon = 11,

        /// <summary>
        /// Auxiliary weapon slot (ranged weapon, backup weapon)
        /// For situational combat scenarios
        /// </summary>
        AuxiliaryWeapon = 12,

        // ============================================================
        // SPECIAL SLOTS (20-29)
        // ============================================================

        /// <summary>
        /// Artifact slot (unique legendary items)
        /// Grants special abilities or passive bonuses
        /// </summary>
        Artifact = 20,

        // ============================================================
        // ACCESSORY SLOTS (30-39) - Future Extensibility
        // ============================================================

        /// <summary>
        /// First ring slot
        /// </summary>
        Ring1 = 30,

        /// <summary>
        /// Second ring slot
        /// </summary>
        Ring2 = 31,

        /// <summary>
        /// Necklace/Amulet slot
        /// </summary>
        Necklace = 32,

        /// <summary>
        /// Belt/Waist slot
        /// </summary>
        Belt = 33,

        /// <summary>
        /// Cape/Cloak slot
        /// </summary>
        Cape = 34,

        // ============================================================
        // UTILITY SLOTS (40-49) - Future Extensibility
        // ============================================================

        /// <summary>
        /// Trinket slot (consumable-activated items)
        /// </summary>
        Trinket1 = 40,

        /// <summary>
        /// Second trinket slot
        /// </summary>
        Trinket2 = 41
    }

    /// <summary>
    /// Extension methods for MythHunterEquipmentSlot enum.
    /// </summary>
    public static class MythHunterEquipmentSlotExtensions
    {
        /// <summary>
        /// Checks if slot is an armor slot (Helmet, Chest, Gloves, Boots).
        /// </summary>
        public static bool IsArmorSlot(this MythHunterEquipmentSlot slot)
        {
            return slot >= MythHunterEquipmentSlot.Helmet && slot <= MythHunterEquipmentSlot.Boots;
        }

        /// <summary>
        /// Checks if slot is a weapon slot (Main, Secondary, Auxiliary).
        /// </summary>
        public static bool IsWeaponSlot(this MythHunterEquipmentSlot slot)
        {
            return slot >= MythHunterEquipmentSlot.MainWeapon && slot <= MythHunterEquipmentSlot.AuxiliaryWeapon;
        }

        /// <summary>
        /// Checks if slot is an accessory slot (Rings, Necklace, Belt, Cape).
        /// </summary>
        public static bool IsAccessorySlot(this MythHunterEquipmentSlot slot)
        {
            return slot >= MythHunterEquipmentSlot.Ring1 && slot <= MythHunterEquipmentSlot.Cape;
        }

        /// <summary>
        /// Gets display name for slot.
        /// </summary>
        public static string GetDisplayName(this MythHunterEquipmentSlot slot)
        {
            return slot switch
            {
                MythHunterEquipmentSlot.Helmet => "Helmet",
                MythHunterEquipmentSlot.Chest => "Chest Armor",
                MythHunterEquipmentSlot.Gloves => "Gloves",
                MythHunterEquipmentSlot.Boots => "Boots",
                MythHunterEquipmentSlot.MainWeapon => "Main Weapon",
                MythHunterEquipmentSlot.SecondaryWeapon => "Secondary Weapon",
                MythHunterEquipmentSlot.AuxiliaryWeapon => "Auxiliary Weapon",
                MythHunterEquipmentSlot.Artifact => "Artifact",
                MythHunterEquipmentSlot.Ring1 => "Ring 1",
                MythHunterEquipmentSlot.Ring2 => "Ring 2",
                MythHunterEquipmentSlot.Necklace => "Necklace",
                MythHunterEquipmentSlot.Belt => "Belt",
                MythHunterEquipmentSlot.Cape => "Cape",
                MythHunterEquipmentSlot.Trinket1 => "Trinket 1",
                MythHunterEquipmentSlot.Trinket2 => "Trinket 2",
                _ => slot.ToString()
            };
        }
    }
}
