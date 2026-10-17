// Assets/_MythHunter/Code/GameLogic/Inventory/Components/MythHunterItemComponent.cs

using RPGFramework.Items;
using RPGFramework.ECS;

namespace MythHunter.GameLogic.Inventory.Components
{
    /// <summary>
    /// Component for entities that ARE items (loot drops, world items, pickups).
    /// Represents an item instance in the game world before it's picked up.
    ///
    /// Use cases:
    /// - Loot dropped by enemies
    /// - Items placed in world (chests, quest items)
    /// - Trade/drop items between players
    /// - Visual item representations in world
    ///
    /// Design:
    /// - Uses string ID to reference item definition in item database
    /// - Tracks quantity for stackable items
    /// - Stores quality/rarity for procedural loot generation
    /// - Future: Can extend with durability, enchantments, custom data
    /// </summary>
    public struct MythHunterItemComponent : IComponent
    {
        // ============================================================
        // CORE ITEM DATA
        // ============================================================

        /// <summary>
        /// Item definition ID (references item in item database/registry).
        /// Examples: "sword_iron_01", "potion_health_small", "artifact_legendary_ring"
        /// </summary>
        public string ItemId;

        /// <summary>
        /// Stack quantity for stackable items.
        /// - For stackable items (potions, arrows, materials): 1-999+
        /// - For unique/equipment items: Always 1
        /// - Default: 1
        /// </summary>
        public int Quantity;

        // ============================================================
        // QUALITY & RARITY (for procedural generation)
        // ============================================================

        /// <summary>
        /// Item quality level.
        /// Affects item stats/value for procedurally generated items.
        /// Framework enum: Normal, Magical, Rare, Unique, Set
        /// </summary>
        public ItemQuality Quality;

        /// <summary>
        /// Item rarity tier.
        /// Affects drop chance and visual effects.
        /// Framework enum: Common, Uncommon, Rare, Epic, Legendary
        /// </summary>
        public ItemRarity Rarity;

        // ============================================================
        // METADATA (optional, future extensibility)
        // ============================================================

        /// <summary>
        /// Whether this item was procedurally generated.
        /// If true, Quality/Rarity may override base item definition.
        /// </summary>
        public bool IsProcedurallyGenerated;

        /// <summary>
        /// Custom data string (JSON, serialized data, etc.).
        /// Used for unique item properties, enchantments, custom stats.
        /// Future: Replace with structured enchantment system.
        /// </summary>
        public string CustomData;

        // ============================================================
        // FACTORY METHODS
        // ============================================================

        /// <summary>
        /// Creates basic item with default quality/rarity.
        /// </summary>
        /// <param name="itemId">Item definition ID</param>
        /// <param name="quantity">Stack quantity, default 1</param>
        /// <returns>Initialized item component</returns>
        public static MythHunterItemComponent Create(string itemId, int quantity = 1)
        {
            return new MythHunterItemComponent
            {
                ItemId = itemId,
                Quantity = quantity > 0 ? quantity : 1,
                Quality = ItemQuality.Normal,
                Rarity = ItemRarity.Common,
                IsProcedurallyGenerated = false,
                CustomData = null
            };
        }

        /// <summary>
        /// Creates item with specified quality and rarity (for loot generation).
        /// </summary>
        /// <param name="itemId">Item definition ID</param>
        /// <param name="quantity">Stack quantity</param>
        /// <param name="quality">Item quality tier</param>
        /// <param name="rarity">Item rarity tier</param>
        /// <returns>Initialized item with quality/rarity</returns>
        public static MythHunterItemComponent CreateWithQuality(
            string itemId,
            int quantity,
            ItemQuality quality,
            ItemRarity rarity)
        {
            return new MythHunterItemComponent
            {
                ItemId = itemId,
                Quantity = quantity > 0 ? quantity : 1,
                Quality = quality,
                Rarity = rarity,
                IsProcedurallyGenerated = true,
                CustomData = null
            };
        }

        /// <summary>
        /// Creates stackable item (potions, materials, consumables).
        /// Always sets quality to Normal, rarity to Common.
        /// </summary>
        /// <param name="itemId">Item definition ID</param>
        /// <param name="quantity">Stack quantity</param>
        /// <returns>Initialized stackable item</returns>
        public static MythHunterItemComponent CreateStackable(string itemId, int quantity)
        {
            return new MythHunterItemComponent
            {
                ItemId = itemId,
                Quantity = quantity > 0 ? quantity : 1,
                Quality = ItemQuality.Normal,
                Rarity = ItemRarity.Common,
                IsProcedurallyGenerated = false,
                CustomData = null
            };
        }

        /// <summary>
        /// Creates legendary/unique item with custom data.
        /// For quest items, artifacts, special rewards.
        /// </summary>
        /// <param name="itemId">Item definition ID</param>
        /// <param name="rarity">Item rarity (typically Legendary)</param>
        /// <param name="customData">Custom JSON/serialized data</param>
        /// <returns>Initialized unique item</returns>
        public static MythHunterItemComponent CreateUnique(
            string itemId,
            ItemRarity rarity = ItemRarity.Legendary,
            string customData = null)
        {
            return new MythHunterItemComponent
            {
                ItemId = itemId,
                Quantity = 1,
                Quality = ItemQuality.Unique,
                Rarity = rarity,
                IsProcedurallyGenerated = false,
                CustomData = customData
            };
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>
        /// Checks if item is valid (has ID and positive quantity).
        /// </summary>
        /// <returns>True if item is valid</returns>
        public readonly bool IsValid()
        {
            return !string.IsNullOrEmpty(ItemId) && Quantity > 0;
        }

        /// <summary>
        /// Checks if item is stackable (quantity > 1 possible).
        /// Note: Actual stackability should be determined by item definition.
        /// This only checks current quantity.
        /// </summary>
        /// <returns>True if quantity > 1</returns>
        public readonly bool IsStacked()
        {
            return Quantity > 1;
        }

        /// <summary>
        /// Adds quantity to stack.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        public void AddQuantity(int amount)
        {
            if (amount > 0)
                Quantity += amount;
        }

        /// <summary>
        /// Removes quantity from stack.
        /// </summary>
        /// <param name="amount">Amount to remove</param>
        /// <returns>True if successful, false if not enough quantity</returns>
        public bool RemoveQuantity(int amount)
        {
            if (amount <= 0 || amount > Quantity)
                return false;

            Quantity -= amount;
            return true;
        }

        /// <summary>
        /// Splits stack into two parts.
        /// </summary>
        /// <param name="splitAmount">Amount to split off</param>
        /// <param name="remainder">Remaining item component</param>
        /// <returns>New item component with split amount, or default if invalid</returns>
        public readonly MythHunterItemComponent Split(int splitAmount, out MythHunterItemComponent remainder)
        {
            remainder = this;

            if (splitAmount <= 0 || splitAmount >= Quantity)
                return default;

            var newItem = this;
            newItem.Quantity = splitAmount;
            remainder.Quantity = Quantity - splitAmount;

            return newItem;
        }

        /// <summary>
        /// Gets display name for quality.
        /// </summary>
        /// <returns>Quality display string</returns>
        public readonly string GetQualityDisplayName()
        {
            return Quality switch
            {
                ItemQuality.Normal => "Normal",
                ItemQuality.Magical => "Magical",
                ItemQuality.Rare => "Rare",
                ItemQuality.Unique => "Unique",
                ItemQuality.Set => "Set",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Gets display name for rarity.
        /// </summary>
        /// <returns>Rarity display string</returns>
        public readonly string GetRarityDisplayName()
        {
            return Rarity switch
            {
                ItemRarity.Common => "Common",
                ItemRarity.Uncommon => "Uncommon",
                ItemRarity.Rare => "Rare",
                ItemRarity.Epic => "Epic",
                ItemRarity.Legendary => "Legendary",
                _ => "Unknown"
            };
        }
    }
}
