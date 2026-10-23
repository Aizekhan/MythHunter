// Assets/_MythHunter/Code/GameLogic/Loot/Utilities/LootTableBuilder.cs

using System.Collections.Generic;
using RPGFramework.Loot;

namespace MythHunter.GameLogic.Loot.Utilities
{
    /// <summary>
    /// Fluent builder for creating loot tables programmatically.
    /// Simplifies loot table creation with readable, chainable API.
    ///
    /// Example usage:
    /// <code>
    /// var table = LootTableBuilder
    ///     .Create("goblin_common", "Common Goblin Loot")
    ///     .WithDrops(minDrops: 1, maxDrops: 3)
    ///     .AddEntry("gold_coins", weight: 100, minQty: 5, maxQty: 15, dropChance: 0.8f)
    ///     .AddEntry("health_potion", weight: 50, minQty: 1, maxQty: 2, dropChance: 0.3f)
    ///     .AddGuaranteedDrop("exp_orb_small")
    ///     .Build();
    /// </code>
    /// </summary>
    public class LootTableBuilder
    {
        private string _tableId;
        private string _name;
        private int _minDrops = 1;
        private int _maxDrops = 3;
        private bool _allowDuplicates = false;
        private readonly List<ILootEntry> _entries = new List<ILootEntry>();
        private readonly List<string> _guaranteedDrops = new List<string>();

        private LootTableBuilder(string tableId, string name)
        {
            _tableId = tableId;
            _name = name;
        }

        /// <summary>
        /// Creates a new loot table builder.
        /// </summary>
        /// <param name="tableId">Unique loot table ID (e.g., "goblin_common")</param>
        /// <param name="name">Display name for the loot table</param>
        public static LootTableBuilder Create(string tableId, string name)
        {
            return new LootTableBuilder(tableId, name);
        }

        /// <summary>
        /// Sets min/max number of drops from the loot table.
        /// Default: minDrops=1, maxDrops=3
        /// </summary>
        public LootTableBuilder WithDrops(int minDrops, int maxDrops)
        {
            _minDrops = minDrops;
            _maxDrops = maxDrops;
            return this;
        }

        /// <summary>
        /// Allows duplicate items in the same loot roll.
        /// Default: false
        /// </summary>
        public LootTableBuilder AllowDuplicates(bool allow = true)
        {
            _allowDuplicates = allow;
            return this;
        }

        /// <summary>
        /// Adds weighted loot entry with level filtering.
        /// </summary>
        /// <param name="itemId">Item definition ID</param>
        /// <param name="weight">Selection weight (higher = more likely)</param>
        /// <param name="minQty">Minimum quantity if selected</param>
        /// <param name="maxQty">Maximum quantity if selected</param>
        /// <param name="dropChance">Chance to actually drop even if selected (0.0-1.0)</param>
        /// <param name="minLevel">Minimum entity level to drop (default: 0)</param>
        /// <param name="maxLevel">Maximum entity level to drop (default: -1 = no limit)</param>
        public LootTableBuilder AddEntry(
            string itemId,
            int weight,
            int minQty = 1,
            int maxQty = 1,
            float dropChance = 1.0f,
            int minLevel = 0,
            int maxLevel = -1)
        {
            _entries.Add(new LootEntry(
                itemId: itemId,
                weight: weight,
                minQuantity: minQty,
                maxQuantity: maxQty,
                dropChance: dropChance,
                minLevel: minLevel,
                maxLevel: maxLevel
            ));
            return this;
        }

        /// <summary>
        /// Adds guaranteed drop (always drops, bypasses RNG).
        /// Note: GuaranteedDrops in Framework are just item IDs, not full entries.
        /// </summary>
        /// <param name="itemId">Item definition ID</param>
        public LootTableBuilder AddGuaranteedDrop(string itemId)
        {
            _guaranteedDrops.Add(itemId);
            return this;
        }

        /// <summary>
        /// Adds common item entry (weight: 100, drop chance: 80%).
        /// </summary>
        public LootTableBuilder AddCommon(string itemId, int minQty = 1, int maxQty = 1)
        {
            return AddEntry(itemId, weight: 100, minQty, maxQty, dropChance: 0.8f);
        }

        /// <summary>
        /// Adds uncommon item entry (weight: 50, drop chance: 50%).
        /// </summary>
        public LootTableBuilder AddUncommon(string itemId, int minQty = 1, int maxQty = 1)
        {
            return AddEntry(itemId, weight: 50, minQty, maxQty, dropChance: 0.5f);
        }

        /// <summary>
        /// Adds rare item entry (weight: 20, drop chance: 20%).
        /// </summary>
        public LootTableBuilder AddRare(string itemId, int minQty = 1, int maxQty = 1)
        {
            return AddEntry(itemId, weight: 20, minQty, maxQty, dropChance: 0.2f);
        }

        /// <summary>
        /// Adds epic item entry (weight: 5, drop chance: 5%).
        /// </summary>
        public LootTableBuilder AddEpic(string itemId, int minQty = 1, int maxQty = 1)
        {
            return AddEntry(itemId, weight: 5, minQty, maxQty, dropChance: 0.05f);
        }

        /// <summary>
        /// Adds legendary item entry (weight: 1, drop chance: 1%).
        /// </summary>
        public LootTableBuilder AddLegendary(string itemId, int minQty = 1, int maxQty = 1)
        {
            return AddEntry(itemId, weight: 1, minQty, maxQty, dropChance: 0.01f);
        }

        /// <summary>
        /// Builds and returns the loot table.
        /// </summary>
        public ILootTable Build()
        {
            return new LootTable(
                tableId: _tableId,
                name: _name,
                entries: _entries,
                minDrops: _minDrops,
                maxDrops: _maxDrops,
                allowDuplicates: _allowDuplicates,
                guaranteedDrops: _guaranteedDrops.Count > 0 ? _guaranteedDrops : null
            );
        }
    }
}
