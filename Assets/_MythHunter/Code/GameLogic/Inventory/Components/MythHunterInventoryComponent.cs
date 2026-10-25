// Assets/_MythHunter/Code/GameLogic/Inventory/Components/MythHunterInventoryComponent.cs

using RPGFramework.Inventory.Runtime;
using RPGFramework.ECS;

namespace MythHunter.GameLogic.Inventory.Components
{
    /// <summary>
    /// MythHunter-specific inventory component.
    /// Wraps Framework InventoryComponent with string item IDs and MythHunter equipment slots.
    ///
    /// Design:
    /// - Uses string IDs for backward compatibility with existing MythHunter item system
    /// - Leverages Framework generic inventory system for type-safe equipment management
    /// - Supports multiple containers (backpack, bank, quest items, etc.)
    /// - Tracks current active weapon for combat system integration
    /// </summary>
    public struct MythHunterInventoryComponent : IComponent
    {
        /// <summary>
        /// Framework inventory component using string IDs and MythHunter equipment slots.
        /// Provides generic container and equipment slot management.
        /// </summary>
        public InventoryComponent<string, MythHunterEquipmentSlot> Inventory;

        /// <summary>
        /// Currently active weapon slot for combat.
        /// Determines which equipped weapon is used in combat calculations.
        /// Defaults to MainWeapon.
        /// </summary>
        public MythHunterEquipmentSlot CurrentWeaponSlot;

        /// <summary>
        /// Whether automatic weapon switching is enabled.
        /// If true, combat system can auto-switch to alternative weapons when current weapon unavailable.
        /// </summary>
        public bool AutoSwitchWeapons;

        // ============================================================
        // FACTORY METHODS
        // ============================================================

        /// <summary>
        /// Creates hero inventory with full equipment slots.
        /// Includes all armor slots, weapon slots, and artifact.
        ///
        /// Equipment slots created:
        /// - Armor: Helmet, Chest, Gloves, Boots
        /// - Weapons: MainWeapon, SecondaryWeapon, AuxiliaryWeapon
        /// - Special: Artifact
        /// </summary>
        /// <param name="bagCapacity">Primary container capacity (backpack size), -1 for unlimited</param>
        /// <param name="autoSwitchWeapons">Enable automatic weapon switching, default true</param>
        /// <returns>Initialized hero inventory component</returns>
        public static MythHunterInventoryComponent CreateHero(int bagCapacity = 50, bool autoSwitchWeapons = true)
        {
            return new MythHunterInventoryComponent
            {
                Inventory = InventoryComponent<string, MythHunterEquipmentSlot>.Create(
                    bagCapacity,
                    // Armor slots
                    MythHunterEquipmentSlot.Helmet,
                    MythHunterEquipmentSlot.Chest,
                    MythHunterEquipmentSlot.Gloves,
                    MythHunterEquipmentSlot.Boots,
                    // Weapon slots
                    MythHunterEquipmentSlot.MainWeapon,
                    MythHunterEquipmentSlot.SecondaryWeapon,
                    MythHunterEquipmentSlot.AuxiliaryWeapon,
                    // Special slots
                    MythHunterEquipmentSlot.Artifact
                ),
                CurrentWeaponSlot = MythHunterEquipmentSlot.MainWeapon,
                AutoSwitchWeapons = autoSwitchWeapons
            };
        }

        /// <summary>
        /// Creates elite hero inventory with extended accessory slots.
        /// Includes all standard hero slots plus accessories (rings, necklace, belt, cape).
        /// Used for high-level heroes or special NPCs.
        /// </summary>
        /// <param name="bagCapacity">Primary container capacity, -1 for unlimited</param>
        /// <param name="autoSwitchWeapons">Enable automatic weapon switching, default true</param>
        /// <returns>Initialized elite hero inventory with accessory slots</returns>
        public static MythHunterInventoryComponent CreateEliteHero(int bagCapacity = 100, bool autoSwitchWeapons = true)
        {
            return new MythHunterInventoryComponent
            {
                Inventory = InventoryComponent<string, MythHunterEquipmentSlot>.Create(
                    bagCapacity,
                    // Armor slots
                    MythHunterEquipmentSlot.Helmet,
                    MythHunterEquipmentSlot.Chest,
                    MythHunterEquipmentSlot.Gloves,
                    MythHunterEquipmentSlot.Boots,
                    // Weapon slots
                    MythHunterEquipmentSlot.MainWeapon,
                    MythHunterEquipmentSlot.SecondaryWeapon,
                    MythHunterEquipmentSlot.AuxiliaryWeapon,
                    // Special slots
                    MythHunterEquipmentSlot.Artifact,
                    // Accessory slots
                    MythHunterEquipmentSlot.Ring1,
                    MythHunterEquipmentSlot.Ring2,
                    MythHunterEquipmentSlot.Necklace,
                    MythHunterEquipmentSlot.Belt,
                    MythHunterEquipmentSlot.Cape
                ),
                CurrentWeaponSlot = MythHunterEquipmentSlot.MainWeapon,
                AutoSwitchWeapons = autoSwitchWeapons
            };
        }

        /// <summary>
        /// Creates monster/enemy inventory (loot container only, no equipment).
        /// Simple container for dropped loot, no equipment slots.
        /// </summary>
        /// <param name="lootCapacity">Loot container capacity, -1 for unlimited</param>
        /// <returns>Initialized monster inventory (loot container only)</returns>
        public static MythHunterInventoryComponent CreateMonster(int lootCapacity = 10)
        {
            return new MythHunterInventoryComponent
            {
                Inventory = InventoryComponent<string, MythHunterEquipmentSlot>.CreateSimple(lootCapacity),
                CurrentWeaponSlot = MythHunterEquipmentSlot.MainWeapon,
                AutoSwitchWeapons = false
            };
        }

        /// <summary>
        /// Creates NPC merchant inventory (unlimited capacity, no equipment).
        /// Used for shop NPCs, vendors, traders.
        /// </summary>
        /// <returns>Initialized merchant inventory (unlimited capacity)</returns>
        public static MythHunterInventoryComponent CreateMerchant()
        {
            return new MythHunterInventoryComponent
            {
                Inventory = InventoryComponent<string, MythHunterEquipmentSlot>.CreateSimple(-1), // Unlimited
                CurrentWeaponSlot = MythHunterEquipmentSlot.MainWeapon,
                AutoSwitchWeapons = false
            };
        }

        /// <summary>
        /// Creates storage container inventory (large capacity, no equipment).
        /// Used for banks, chests, storage NPCs.
        /// </summary>
        /// <param name="storageCapacity">Storage capacity, default 200, -1 for unlimited</param>
        /// <returns>Initialized storage inventory</returns>
        public static MythHunterInventoryComponent CreateStorage(int storageCapacity = 200)
        {
            return new MythHunterInventoryComponent
            {
                Inventory = InventoryComponent<string, MythHunterEquipmentSlot>.CreateSimple(storageCapacity),
                CurrentWeaponSlot = MythHunterEquipmentSlot.MainWeapon,
                AutoSwitchWeapons = false
            };
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>
        /// Gets the currently active weapon item ID.
        /// Returns null if current weapon slot is empty.
        /// </summary>
        /// <returns>Item ID of current weapon, or null if slot empty</returns>
        public readonly string GetCurrentWeapon()
        {
            if (Inventory.EquipmentSlots != null &&
                Inventory.EquipmentSlots.TryGetValue(CurrentWeaponSlot, out var slot))
            {
                return slot.EquippedItem;
            }
            return null;
        }

        /// <summary>
        /// Switches active weapon to specified slot.
        /// Only works if target slot contains a weapon.
        /// </summary>
        /// <param name="targetSlot">Target weapon slot</param>
        /// <returns>True if switched successfully, false if slot empty or not a weapon slot</returns>
        public bool SwitchWeapon(MythHunterEquipmentSlot targetSlot)
        {
            if (!targetSlot.IsWeaponSlot())
                return false;

            if (Inventory.EquipmentSlots != null &&
                Inventory.EquipmentSlots.TryGetValue(targetSlot, out var slot) &&
                !string.IsNullOrEmpty(slot.EquippedItem))
            {
                CurrentWeaponSlot = targetSlot;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets total number of items in primary container.
        /// </summary>
        /// <returns>Item count in backpack/primary container</returns>
        public readonly int GetItemCount()
        {
            return Inventory.PrimaryContainer?.Count ?? 0;
        }

        /// <summary>
        /// Checks if primary container is full.
        /// </summary>
        /// <returns>True if container at capacity</returns>
        public readonly bool IsInventoryFull()
        {
            return Inventory.PrimaryContainer?.IsFull ?? false;
        }

        /// <summary>
        /// Checks if an equipment slot is occupied.
        /// </summary>
        /// <param name="slot">Equipment slot to check</param>
        /// <returns>True if slot has equipped item</returns>
        public readonly bool HasEquipment(MythHunterEquipmentSlot slot)
        {
            if (Inventory.EquipmentSlots != null &&
                Inventory.EquipmentSlots.TryGetValue(slot, out var equipSlot))
            {
                return !string.IsNullOrEmpty(equipSlot.EquippedItem);
            }
            return false;
        }
    }
}
