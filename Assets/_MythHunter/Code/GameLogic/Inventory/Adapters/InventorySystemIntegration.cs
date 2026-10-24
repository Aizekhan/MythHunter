// Assets/_MythHunter/Code/GameLogic/Inventory/Adapters/InventorySystemIntegration.cs

using System.Collections.Generic;
using System.Linq;
using RPGFramework.ECS;
using RPGFramework.DI;
using MythHunter.Utils.Logging;
using MythHunter.GameLogic.Inventory.Components;
using MythHunter.Components.Character; // Old InventoryComponent

namespace MythHunter.GameLogic.Inventory.Adapters
{
    /// <summary>
    /// Integration helper for inventory operations with dual-component support.
    /// Supports both old InventoryComponent and new MythHunterInventoryComponent during migration.
    ///
    /// Pattern (from EPIC 12 Combat Integration):
    /// - Try new component first (MythHunterInventoryComponent)
    /// - Fall back to old component (InventoryComponent)
    /// - Provide unified API regardless of component type
    /// - Enable safe gradual migration
    ///
    /// All systems should use this helper instead of direct component access.
    /// </summary>
    public class InventorySystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        private const string LogCategory = "InventoryIntegration";

        [Inject]
        public InventorySystemIntegration(IEntityManager entityManager, IMythLogger logger)
        {
            _entityManager = entityManager;
            _logger = logger;
        }

        // ============================================================
        // ITEM CONTAINER OPERATIONS
        // ============================================================

        /// <summary>
        /// Adds item to entity's primary container (backpack).
        /// Supports both old and new components.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="itemId">Item definition ID</param>
        /// <returns>True if item added successfully, false if container full or entity invalid</returns>
        public bool AddItem(int entityId, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                _logger.LogWarning($"Cannot add null/empty item ID to entity {entityId}", LogCategory);
                return false;
            }

            // Try new component first
            if (_entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv))
            {
                if (newInv.Inventory.PrimaryContainer != null)
                {
                    bool added = newInv.Inventory.PrimaryContainer.Add(itemId);
                    if (added)
                    {
                        _entityManager.AddComponent(entityId, newInv); // Update component
                        _logger.LogInfo($"Added item '{itemId}' to entity {entityId} (new component)", LogCategory);
                    }
                    return added;
                }
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                if (oldInv.Items == null)
                    oldInv.Items = new List<string>();

                // Check capacity via Bag.Slots (if exists)
                int capacity = oldInv.Bag.Slots > 0 ? oldInv.Bag.Slots : 50; // Default 50
                if (oldInv.Items.Count >= capacity)
                {
                    _logger.LogWarning($"Inventory full for entity {entityId} (old component)", LogCategory);
                    return false;
                }

                oldInv.Items.Add(itemId);
                _entityManager.AddComponent(entityId, oldInv); // Update component
                _logger.LogInfo($"Added item '{itemId}' to entity {entityId} (old component)", LogCategory);
                return true;
            }

            _logger.LogWarning($"Entity {entityId} has no inventory component", LogCategory);
            return false;
        }

        /// <summary>
        /// Removes first occurrence of item from entity's primary container.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="itemId">Item definition ID</param>
        /// <returns>True if item removed, false if not found</returns>
        public bool RemoveItem(int entityId, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return false;

            // Try new component first
            if (_entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv))
            {
                if (newInv.Inventory.PrimaryContainer != null)
                {
                    bool removed = newInv.Inventory.PrimaryContainer.Remove(itemId);
                    if (removed)
                    {
                        _entityManager.AddComponent(entityId, newInv);
                        _logger.LogInfo($"Removed item '{itemId}' from entity {entityId}", LogCategory);
                    }
                    return removed;
                }
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                if (oldInv.Items != null && oldInv.Items.Remove(itemId))
                {
                    _entityManager.AddComponent(entityId, oldInv);
                    _logger.LogInfo($"Removed item '{itemId}' from entity {entityId} (old component)", LogCategory);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if entity's inventory contains specific item.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="itemId">Item definition ID</param>
        /// <returns>True if item exists in inventory</returns>
        public bool HasItem(int entityId, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return false;

            // Try new component
            if (_entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv))
            {
                return newInv.Inventory.PrimaryContainer?.Contains(itemId) ?? false;
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                return oldInv.Items?.Contains(itemId) ?? false;
            }

            return false;
        }

        /// <summary>
        /// Gets all items in entity's primary container.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Read-only list of item IDs, empty if no inventory</returns>
        public IReadOnlyList<string> GetAllItems(int entityId)
        {
            // Try new component
            if (_entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv))
            {
                return newInv.Inventory.PrimaryContainer?.GetAllItems() ?? (IReadOnlyList<string>)new List<string>();
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                if (oldInv.Items != null)
                    return oldInv.Items.AsReadOnly();
            }

            return new List<string>();
        }

        /// <summary>
        /// Gets item count in primary container.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Number of items in backpack</returns>
        public int GetInventoryCount(int entityId)
        {
            // Try new component
            if (_entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv))
            {
                return newInv.Inventory.PrimaryContainer?.Count ?? 0;
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                return oldInv.Items?.Count ?? 0;
            }

            return 0;
        }

        /// <summary>
        /// Checks if entity's primary container is full.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>True if at capacity, false otherwise</returns>
        public bool IsInventoryFull(int entityId)
        {
            // Try new component
            if (_entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv))
            {
                return newInv.Inventory.PrimaryContainer?.IsFull ?? false;
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                int capacity = oldInv.Bag.Slots > 0 ? oldInv.Bag.Slots : 50;
                return (oldInv.Items?.Count ?? 0) >= capacity;
            }

            return false;
        }

        // ============================================================
        // EQUIPMENT OPERATIONS
        // ============================================================

        /// <summary>
        /// Equips item to specified equipment slot.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="itemId">Item to equip</param>
        /// <param name="slot">Target equipment slot</param>
        /// <returns>True if equipped successfully</returns>
        public bool EquipItem(int entityId, string itemId, MythHunterEquipmentSlot slot)
        {
            if (string.IsNullOrEmpty(itemId))
                return false;

            // Try new component
            if (_entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv))
            {
                if (newInv.Inventory.EquipmentSlots != null &&
                    newInv.Inventory.EquipmentSlots.TryGetValue(slot, out var equipSlot))
                {
                    equipSlot.Equip(itemId);
                    _entityManager.AddComponent(entityId, newInv);
                    _logger.LogInfo($"Equipped '{itemId}' to {slot} on entity {entityId}", LogCategory);
                    return true;
                }
            }

            // Fallback to old component (map to old structure)
            if (_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                bool equipped = EquipToOldComponent(ref oldInv, itemId, slot);
                if (equipped)
                {
                    _entityManager.AddComponent(entityId, oldInv);
                    _logger.LogInfo($"Equipped '{itemId}' to {slot} on entity {entityId} (old component)", LogCategory);
                }
                return equipped;
            }

            return false;
        }

        /// <summary>
        /// Unequips item from specified slot.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="slot">Equipment slot to clear</param>
        /// <returns>Item ID that was unequipped, or null if slot was empty</returns>
        public string UnequipItem(int entityId, MythHunterEquipmentSlot slot)
        {
            // Try new component
            if (_entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv))
            {
                if (newInv.Inventory.EquipmentSlots != null &&
                    newInv.Inventory.EquipmentSlots.TryGetValue(slot, out var equipSlot))
                {
                    string unequipped = equipSlot.Unequip();
                    _entityManager.AddComponent(entityId, newInv);
                    if (!string.IsNullOrEmpty(unequipped))
                        _logger.LogInfo($"Unequipped '{unequipped}' from {slot} on entity {entityId}", LogCategory);
                    return unequipped;
                }
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                string unequipped = UnequipFromOldComponent(ref oldInv, slot);
                if (!string.IsNullOrEmpty(unequipped))
                {
                    _entityManager.AddComponent(entityId, oldInv);
                    _logger.LogInfo($"Unequipped '{unequipped}' from {slot} on entity {entityId} (old component)", LogCategory);
                }
                return unequipped;
            }

            return null;
        }

        /// <summary>
        /// Gets equipped item from specific slot.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="slot">Equipment slot</param>
        /// <returns>Item ID in slot, or null if slot empty</returns>
        public string GetEquippedItem(int entityId, MythHunterEquipmentSlot slot)
        {
            // Try new component
            if (_entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv))
            {
                if (newInv.Inventory.EquipmentSlots != null &&
                    newInv.Inventory.EquipmentSlots.TryGetValue(slot, out var equipSlot))
                {
                    return equipSlot.EquippedItem;
                }
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                return GetEquippedFromOldComponent(oldInv, slot);
            }

            return null;
        }

        /// <summary>
        /// Gets currently active weapon item ID.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Active weapon item ID, or null if no weapon equipped</returns>
        public string GetCurrentWeapon(int entityId)
        {
            // Try new component
            if (_entityManager.TryGetComponent<MythHunterInventoryComponent>(entityId, out var newInv))
            {
                return newInv.GetCurrentWeapon();
            }

            // Fallback to old component (use CurrentWeapon field)
            if (_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                return oldInv.EquippedItems.Weapons.CurrentWeapon;
            }

            return null;
        }

        // ============================================================
        // HELPER METHODS (OLD COMPONENT MAPPING)
        // ============================================================

        private bool EquipToOldComponent(ref InventoryComponent inv, string itemId, MythHunterEquipmentSlot slot)
        {
            switch (slot)
            {
                case MythHunterEquipmentSlot.Helmet:
                    inv.EquippedItems.Armor.Helmet = itemId;
                    return true;
                case MythHunterEquipmentSlot.Chest:
                    inv.EquippedItems.Armor.Chest = itemId;
                    return true;
                case MythHunterEquipmentSlot.Gloves:
                    inv.EquippedItems.Armor.Gloves = itemId;
                    return true;
                case MythHunterEquipmentSlot.Boots:
                    inv.EquippedItems.Armor.Boots = itemId;
                    return true;
                case MythHunterEquipmentSlot.MainWeapon:
                    inv.EquippedItems.Weapons.MainWeapon = itemId;
                    inv.EquippedItems.Weapons.CurrentWeapon = itemId; // Set as current
                    return true;
                case MythHunterEquipmentSlot.SecondaryWeapon:
                    inv.EquippedItems.Weapons.SecondaryWeapon = itemId;
                    return true;
                case MythHunterEquipmentSlot.AuxiliaryWeapon:
                    inv.EquippedItems.Weapons.AuxiliaryWeapon = itemId;
                    return true;
                case MythHunterEquipmentSlot.Artifact:
                    inv.EquippedItems.Artifact = itemId;
                    return true;
                default:
                    _logger.LogWarning($"Slot {slot} not supported in old component", LogCategory);
                    return false;
            }
        }

        private string UnequipFromOldComponent(ref InventoryComponent inv, MythHunterEquipmentSlot slot)
        {
            string item = null;
            switch (slot)
            {
                case MythHunterEquipmentSlot.Helmet:
                    item = inv.EquippedItems.Armor.Helmet;
                    inv.EquippedItems.Armor.Helmet = null;
                    break;
                case MythHunterEquipmentSlot.Chest:
                    item = inv.EquippedItems.Armor.Chest;
                    inv.EquippedItems.Armor.Chest = null;
                    break;
                case MythHunterEquipmentSlot.Gloves:
                    item = inv.EquippedItems.Armor.Gloves;
                    inv.EquippedItems.Armor.Gloves = null;
                    break;
                case MythHunterEquipmentSlot.Boots:
                    item = inv.EquippedItems.Armor.Boots;
                    inv.EquippedItems.Armor.Boots = null;
                    break;
                case MythHunterEquipmentSlot.MainWeapon:
                    item = inv.EquippedItems.Weapons.MainWeapon;
                    inv.EquippedItems.Weapons.MainWeapon = null;
                    if (inv.EquippedItems.Weapons.CurrentWeapon == item)
                        inv.EquippedItems.Weapons.CurrentWeapon = null;
                    break;
                case MythHunterEquipmentSlot.SecondaryWeapon:
                    item = inv.EquippedItems.Weapons.SecondaryWeapon;
                    inv.EquippedItems.Weapons.SecondaryWeapon = null;
                    break;
                case MythHunterEquipmentSlot.AuxiliaryWeapon:
                    item = inv.EquippedItems.Weapons.AuxiliaryWeapon;
                    inv.EquippedItems.Weapons.AuxiliaryWeapon = null;
                    break;
                case MythHunterEquipmentSlot.Artifact:
                    item = inv.EquippedItems.Artifact;
                    inv.EquippedItems.Artifact = null;
                    break;
            }
            return item;
        }

        private string GetEquippedFromOldComponent(InventoryComponent inv, MythHunterEquipmentSlot slot)
        {
            return slot switch
            {
                MythHunterEquipmentSlot.Helmet => inv.EquippedItems.Armor.Helmet,
                MythHunterEquipmentSlot.Chest => inv.EquippedItems.Armor.Chest,
                MythHunterEquipmentSlot.Gloves => inv.EquippedItems.Armor.Gloves,
                MythHunterEquipmentSlot.Boots => inv.EquippedItems.Armor.Boots,
                MythHunterEquipmentSlot.MainWeapon => inv.EquippedItems.Weapons.MainWeapon,
                MythHunterEquipmentSlot.SecondaryWeapon => inv.EquippedItems.Weapons.SecondaryWeapon,
                MythHunterEquipmentSlot.AuxiliaryWeapon => inv.EquippedItems.Weapons.AuxiliaryWeapon,
                MythHunterEquipmentSlot.Artifact => inv.EquippedItems.Artifact,
                _ => null
            };
        }

        // ============================================================
        // QUERY METHODS
        // ============================================================

        /// <summary>
        /// Gets all entities with inventory components (old or new).
        /// </summary>
        /// <returns>Union of entities with any inventory component type</returns>
        public IEnumerable<int> GetEntitiesWithInventory()
        {
            var entities = new HashSet<int>();

            foreach (var entityId in _entityManager.GetEntitiesWith<MythHunterInventoryComponent>())
                entities.Add(entityId);

            foreach (var entityId in _entityManager.GetEntitiesWith<InventoryComponent>())
                entities.Add(entityId);

            return entities;
        }

        /// <summary>
        /// Checks if entity has any inventory component (old or new).
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>True if entity has inventory</returns>
        public bool HasInventory(int entityId)
        {
            return _entityManager.HasComponent<MythHunterInventoryComponent>(entityId) ||
                   _entityManager.HasComponent<InventoryComponent>(entityId);
        }
    }
}
