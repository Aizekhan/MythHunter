// Assets/_MythHunter/Code/GameLogic/Loot/Data/SampleLootTables.cs

using RPGFramework.Loot;
using MythHunter.GameLogic.Loot.Utilities;
using System.Collections.Generic;

namespace MythHunter.GameLogic.Loot.Data
{
    /// <summary>
    /// Sample loot tables for testing and development.
    /// Provides pre-configured loot tables for common enemy types, chests, and bosses.
    ///
    /// Pattern:
    /// - Static factory methods for easy access
    /// - Uses LootTableBuilder for readable definitions
    /// - Categorized by entity type (enemies, chests, bosses)
    /// - Ready for registry population
    /// </summary>
    public static class SampleLootTables
    {
        // ============================================================
        // ENEMY LOOT TABLES
        // ============================================================

        /// <summary>
        /// Common goblin loot (low-level enemy).
        /// Drops: gold, health potions, basic equipment.
        /// </summary>
        public static ILootTable GoblinCommon()
        {
            return LootTableBuilder
                .Create("goblin_common", "Common Goblin Loot")
                .WithDrops(minDrops: 1, maxDrops: 2)
                .AddCommon("gold_coins", minQty: 5, maxQty: 15)
                .AddCommon("health_potion_minor", minQty: 1, maxQty: 2)
                .AddUncommon("leather_gloves")
                .AddUncommon("rusty_dagger")
                .AddGuaranteedDrop("exp_orb_small")
                .Build();
        }

        /// <summary>
        /// Elite goblin loot (mid-level enemy).
        /// Better quality drops, more gold.
        /// </summary>
        public static ILootTable GoblinElite()
        {
            return LootTableBuilder
                .Create("goblin_elite", "Elite Goblin Loot")
                .WithDrops(minDrops: 2, maxDrops: 4)
                .AddCommon("gold_coins", minQty: 25, maxQty: 50)
                .AddCommon("health_potion", minQty: 1, maxQty: 3)
                .AddUncommon("leather_armor")
                .AddUncommon("iron_dagger")
                .AddRare("enchanted_ring")
                .AddGuaranteedDrop("exp_orb_medium")
                .Build();
        }

        /// <summary>
        /// Skeleton warrior loot (undead enemy).
        /// Drops bones, soul shards, cursed items.
        /// </summary>
        public static ILootTable SkeletonWarrior()
        {
            return LootTableBuilder
                .Create("skeleton_warrior", "Skeleton Warrior Loot")
                .WithDrops(minDrops: 1, maxDrops: 3)
                .AddCommon("bone_fragment", minQty: 2, maxQty: 5)
                .AddCommon("soul_shard", minQty: 1, maxQty: 2)
                .AddUncommon("cursed_sword")
                .AddUncommon("bone_shield")
                .AddRare("soul_gem")
                .AddGuaranteedDrop("exp_orb_small")
                .Build();
        }

        // ============================================================
        // CHEST LOOT TABLES
        // ============================================================

        /// <summary>
        /// Basic wooden chest (common loot).
        /// Guaranteed 2-4 items, mix of consumables and equipment.
        /// </summary>
        public static ILootTable ChestWooden()
        {
            return LootTableBuilder
                .Create("chest_wooden", "Wooden Chest Loot")
                .WithDrops(minDrops: 2, maxDrops: 4)
                .AllowDuplicates(false)
                .AddCommon("gold_coins", minQty: 10, maxQty: 30)
                .AddCommon("health_potion", minQty: 1, maxQty: 3)
                .AddCommon("mana_potion", minQty: 1, maxQty: 2)
                .AddUncommon("iron_sword")
                .AddUncommon("leather_boots")
                .AddRare("magic_scroll")
                .Build();
        }

        /// <summary>
        /// Iron chest (better loot, dungeon reward).
        /// More rare items, higher quantities.
        /// </summary>
        public static ILootTable ChestIron()
        {
            return LootTableBuilder
                .Create("chest_iron", "Iron Chest Loot")
                .WithDrops(minDrops: 3, maxDrops: 5)
                .AllowDuplicates(false)
                .AddCommon("gold_coins", minQty: 50, maxQty: 100)
                .AddCommon("health_potion_greater", minQty: 2, maxQty: 4)
                .AddUncommon("steel_sword")
                .AddUncommon("plate_armor")
                .AddRare("enchanted_amulet")
                .AddRare("magic_scroll_rare")
                .AddEpic("legendary_gem")
                .Build();
        }

        /// <summary>
        /// Golden chest (rare, high-value loot).
        /// Guaranteed rare/epic items.
        /// </summary>
        public static ILootTable ChestGolden()
        {
            return LootTableBuilder
                .Create("chest_golden", "Golden Chest Loot")
                .WithDrops(minDrops: 4, maxDrops: 6)
                .AllowDuplicates(false)
                .AddCommon("gold_coins", minQty: 200, maxQty: 500)
                .AddUncommon("mana_potion_greater", minQty: 3, maxQty: 5)
                .AddRare("enchanted_weapon")
                .AddRare("enchanted_armor")
                .AddEpic("artifact_fragment")
                .AddLegendary("ancient_relic")
                .AddGuaranteedDrop("rare_gem")
                .Build();
        }

        // ============================================================
        // BOSS LOOT TABLES
        // ============================================================

        /// <summary>
        /// Goblin King boss loot.
        /// Guaranteed epic/legendary drops, high gold.
        /// </summary>
        public static ILootTable BossGoblinKing()
        {
            return LootTableBuilder
                .Create("boss_goblin_king", "Goblin King Boss Loot")
                .WithDrops(minDrops: 5, maxDrops: 8)
                .AllowDuplicates(false)
                .AddCommon("gold_coins", minQty: 500, maxQty: 1000)
                .AddRare("kings_crown")
                .AddRare("royal_scepter")
                .AddEpic("goblin_throne_fragment")
                .AddLegendary("goblin_kings_treasure")
                .AddGuaranteedDrop("exp_orb_huge")
                .AddGuaranteedDrop("boss_soul_goblin_king")
                .Build();
        }

        /// <summary>
        /// Ancient Dragon boss loot.
        /// Best loot in game, multiple guaranteed legendaries.
        /// </summary>
        public static ILootTable BossDragonAncient()
        {
            return LootTableBuilder
                .Create("boss_dragon_ancient", "Ancient Dragon Boss Loot")
                .WithDrops(minDrops: 8, maxDrops: 12)
                .AllowDuplicates(false)
                .AddCommon("gold_coins", minQty: 2000, maxQty: 5000)
                .AddRare("dragon_scale", minQty: 5, maxQty: 10)
                .AddRare("dragon_tooth", minQty: 2, maxQty: 5)
                .AddEpic("dragon_heart")
                .AddEpic("dragon_wing")
                .AddLegendary("dragonslayer_sword")
                .AddLegendary("dragon_hoard_key")
                .AddGuaranteedDrop("exp_orb_legendary")
                .AddGuaranteedDrop("boss_soul_ancient_dragon")
                .AddGuaranteedDrop("legendary_gem_ruby")
                .Build();
        }

        // ============================================================
        // REGISTRY HELPER
        // ============================================================

        /// <summary>
        /// Gets all sample loot tables for registry population.
        /// </summary>
        public static List<ILootTable> GetAllTables()
        {
            return new List<ILootTable>
            {
                // Enemies
                GoblinCommon(),
                GoblinElite(),
                SkeletonWarrior(),

                // Chests
                ChestWooden(),
                ChestIron(),
                ChestGolden(),

                // Bosses
                BossGoblinKing(),
                BossDragonAncient()
            };
        }
    }
}
