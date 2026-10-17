// Assets/_MythHunter/Code/GameLogic/Loot/Components/MythHunterLootComponent.cs
using RPGFramework.ECS;

namespace MythHunter.GameLogic.Loot.Components
{
    /// <summary>
    /// MythHunter loot component for entities that can drop loot.
    /// Attached to enemies, chests, bosses, and other loot sources.
    /// Defines which loot table to use and generation parameters.
    /// </summary>
    /// <remarks>
    /// Greenfield Integration - No legacy loot system to migrate from.
    /// Pattern: Follows EPIC 13 Phase 1 component design (MythHunterInventoryComponent).
    /// </remarks>
    public struct MythHunterLootComponent : IComponent
    {
        /// <summary>
        /// Loot table ID from loot table registry.
        /// Defines which items can drop and their probabilities.
        /// Example: "goblin_common", "chest_dungeon_1", "boss_dragon"
        /// </summary>
        public string LootTableId;

        /// <summary>
        /// Entity level for level-based loot filtering.
        /// Determines which loot entries are valid based on MinLevel/MaxLevel.
        /// For enemies: Enemy level
        /// For chests: Dungeon/area level
        /// </summary>
        public int Level;

        /// <summary>
        /// Luck modifier for drop rate calculations.
        /// 1.0 = normal rates
        /// 1.5 = 50% bonus (boss aura, player magic find)
        /// 2.0 = double drop rates
        /// </summary>
        public float LuckModifier;

        /// <summary>
        /// Whether loot has been generated for this entity.
        /// Used for one-time loot sources (chests, event rewards).
        /// For enemies: typically false (loot generated on death)
        /// For chests: prevents re-looting after opening
        /// </summary>
        public bool LootGenerated;

        /// <summary>
        /// Optional custom seed for deterministic loot generation.
        /// Null = time-based random seed
        /// Set value = deterministic loot (useful for testing, replays, events)
        /// </summary>
        public int? CustomSeed;

        #region Factory Methods

        /// <summary>
        /// Creates loot component for standard enemy.
        /// Standard luck modifier (1.0), no custom seed.
        /// </summary>
        /// <param name="lootTableId">Loot table ID (e.g., "goblin_common")</param>
        /// <param name="level">Enemy level</param>
        /// <returns>Configured loot component</returns>
        public static MythHunterLootComponent CreateEnemy(string lootTableId, int level)
        {
            return new MythHunterLootComponent
            {
                LootTableId = lootTableId,
                Level = level,
                LuckModifier = 1.0f,
                LootGenerated = false,
                CustomSeed = null
            };
        }

        /// <summary>
        /// Creates loot component for boss enemy.
        /// Enhanced luck modifier for better drops.
        /// </summary>
        /// <param name="lootTableId">Boss loot table ID (e.g., "boss_dragon")</param>
        /// <param name="level">Boss level</param>
        /// <param name="luckBonus">Luck bonus multiplier (default: 1.5 = 50% bonus)</param>
        /// <returns>Configured boss loot component</returns>
        public static MythHunterLootComponent CreateBoss(string lootTableId, int level, float luckBonus = 1.5f)
        {
            return new MythHunterLootComponent
            {
                LootTableId = lootTableId,
                Level = level,
                LuckModifier = luckBonus,
                LootGenerated = false,
                CustomSeed = null
            };
        }

        /// <summary>
        /// Creates loot component for chest/container.
        /// One-time loot source (LootGenerated tracked).
        /// </summary>
        /// <param name="lootTableId">Chest loot table ID (e.g., "chest_dungeon_1")</param>
        /// <param name="level">Chest/area level</param>
        /// <returns>Configured chest loot component</returns>
        public static MythHunterLootComponent CreateChest(string lootTableId, int level)
        {
            return new MythHunterLootComponent
            {
                LootTableId = lootTableId,
                Level = level,
                LuckModifier = 1.0f,
                LootGenerated = false,
                CustomSeed = null
            };
        }

        /// <summary>
        /// Creates loot component for event/quest reward.
        /// Deterministic loot using custom seed (reproducible results).
        /// </summary>
        /// <param name="lootTableId">Event loot table ID</param>
        /// <param name="level">Event level</param>
        /// <param name="seed">Custom seed for deterministic generation</param>
        /// <returns>Configured event loot component</returns>
        public static MythHunterLootComponent CreateEvent(string lootTableId, int level, int seed)
        {
            return new MythHunterLootComponent
            {
                LootTableId = lootTableId,
                Level = level,
                LuckModifier = 1.0f,
                LootGenerated = false,
                CustomSeed = seed
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if loot has already been generated.
        /// Useful for preventing duplicate loot from chests/containers.
        /// </summary>
        public bool IsGenerated() => LootGenerated;

        /// <summary>
        /// Marks loot as generated.
        /// Call after generating loot to prevent re-generation.
        /// </summary>
        public void SetGenerated() => LootGenerated = true;

        /// <summary>
        /// Checks if loot component has a valid loot table ID.
        /// </summary>
        public bool HasLootTable() => !string.IsNullOrEmpty(LootTableId);

        /// <summary>
        /// Checks if component uses deterministic seed.
        /// </summary>
        public bool IsDeterministic() => CustomSeed.HasValue;

        /// <summary>
        /// Gets effective seed for loot generation.
        /// Returns custom seed if set, otherwise generates time-based seed.
        /// </summary>
        public int GetSeed() => CustomSeed ?? UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        /// <summary>
        /// Creates copy with modified luck modifier.
        /// Useful for applying temporary luck buffs.
        /// </summary>
        public MythHunterLootComponent WithLuck(float newLuckModifier)
        {
            var copy = this;
            copy.LuckModifier = newLuckModifier;
            return copy;
        }

        /// <summary>
        /// Creates copy with modified level.
        /// Useful for scaling loot to player level.
        /// </summary>
        public MythHunterLootComponent WithLevel(int newLevel)
        {
            var copy = this;
            copy.Level = newLevel;
            return copy;
        }

        #endregion
    }
}
