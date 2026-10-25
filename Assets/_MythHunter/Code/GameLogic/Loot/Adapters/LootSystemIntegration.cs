// Assets/_MythHunter/Code/GameLogic/Loot/Adapters/LootSystemIntegration.cs

using System.Collections.Generic;
using System.Linq;
using RPGFramework.ECS;
using RPGFramework.Loot;
using RPGFramework.DI;
using MythHunter.Utils.Logging;
using MythHunter.GameLogic.Loot.Components;
using MythHunter.GameLogic.Inventory.Components;
using UnityEngine;

namespace MythHunter.GameLogic.Loot.Adapters
{
    /// <summary>
    /// Integration helper for loot generation operations.
    /// Bridges Framework Loot System (RPGFramework.Loot.Runtime) with MythHunter game layer.
    ///
    /// Pattern (from EPIC 13 Phase 1):
    /// - Greenfield integration (no legacy loot code to migrate)
    /// - Uses MythHunterLootComponent for entity loot configuration
    /// - Generates loot via Framework ILootGenerator
    /// - Creates item entities via MythHunterItemComponent (Phase 1)
    ///
    /// Key Responsibilities:
    /// - Generate loot from entity components or table IDs
    /// - Create item entities in world from loot results
    /// - Query entities with loot capabilities
    /// - Integrate loot → items → inventory pickup flow
    /// </summary>
    public class LootSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly ILootGenerator _lootGenerator;
        private readonly ILootTableRegistry _lootTableRegistry;
        private readonly IMythLogger _logger;

        private const string LogCategory = "LootIntegration";

        [Inject]
        public LootSystemIntegration(
            IEntityManager entityManager,
            ILootGenerator lootGenerator,
            ILootTableRegistry lootTableRegistry,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _lootGenerator = lootGenerator;
            _lootTableRegistry = lootTableRegistry;
            _logger = logger;
        }

        // ============================================================
        // LOOT GENERATION OPERATIONS
        // ============================================================

        /// <summary>
        /// Generates loot from entity's loot component.
        /// Uses entity's MythHunterLootComponent configuration (table ID, level, luck, seed).
        /// Marks component as generated to prevent duplicate loot.
        /// </summary>
        /// <param name="entityId">Entity ID with MythHunterLootComponent</param>
        /// <returns>Loot result with generated drops, or empty result if invalid</returns>
        public LootResult GenerateLoot(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterLootComponent>(entityId, out var lootComponent))
            {
                _logger.LogWarning($"Entity {entityId} has no MythHunterLootComponent", LogCategory);
                return new LootResult
                {
                    Drops = new List<LootDrop>(),
                    TableId = string.Empty,
                    Level = 0,
                    LuckModifier = 1.0f,
                    Seed = 0,
                    Timestamp = 0f
                };
            }

            if (!lootComponent.HasLootTable())
            {
                _logger.LogWarning($"Entity {entityId} has no loot table ID configured", LogCategory);
                return new LootResult
                {
                    Drops = new List<LootDrop>(),
                    TableId = string.Empty,
                    Level = 0,
                    LuckModifier = 1.0f,
                    Seed = 0,
                    Timestamp = 0f
                };
            }

            if (lootComponent.IsGenerated())
            {
                _logger.LogInfo($"Loot already generated for entity {entityId}, skipping", LogCategory);
                return new LootResult
                {
                    Drops = new List<LootDrop>(),
                    TableId = lootComponent.LootTableId,
                    Level = lootComponent.Level,
                    LuckModifier = lootComponent.LuckModifier,
                    Seed = 0,
                    Timestamp = 0f
                };
            }

            // Generate loot using Framework system
            int seed = lootComponent.GetSeed();
            LootResult result = _lootGenerator.GenerateLootById(
                lootComponent.LootTableId,
                lootComponent.Level,
                lootComponent.LuckModifier,
                seed
            );

            // Mark component as generated
            lootComponent.SetGenerated();
            _entityManager.AddComponent(entityId, lootComponent);

            _logger.LogInfo(
                $"Generated {result.TotalCount} loot drops from table '{lootComponent.LootTableId}' " +
                $"for entity {entityId} (level={lootComponent.Level}, luck={lootComponent.LuckModifier:F2}, seed={seed})",
                LogCategory
            );

            return result;
        }

        /// <summary>
        /// Generates loot directly from table ID with custom parameters.
        /// Useful for programmatic loot generation (events, quests, rewards).
        /// Does NOT modify entity components.
        /// </summary>
        /// <param name="tableId">Loot table ID from registry</param>
        /// <param name="level">Entity/player level for level filtering</param>
        /// <param name="luckModifier">Luck modifier (1.0 = normal, 1.5 = 50% bonus)</param>
        /// <param name="customSeed">Optional seed for deterministic generation</param>
        /// <returns>Loot result with generated drops</returns>
        public LootResult GenerateLoot(string tableId, int level, float luckModifier = 1.0f, int? customSeed = null)
        {
            if (string.IsNullOrEmpty(tableId))
            {
                _logger.LogWarning("Cannot generate loot with null/empty table ID", LogCategory);
                return new LootResult
                {
                    Drops = new List<LootDrop>(),
                    TableId = string.Empty,
                    Level = 0,
                    LuckModifier = 1.0f,
                    Seed = 0,
                    Timestamp = 0f
                };
            }

            // Check if table exists
            if (!_lootTableRegistry.HasTable(tableId))
            {
                _logger.LogWarning($"Loot table '{tableId}' not found in registry", LogCategory);
                return new LootResult
                {
                    Drops = new List<LootDrop>(),
                    TableId = tableId,
                    Level = level,
                    LuckModifier = luckModifier,
                    Seed = 0,
                    Timestamp = 0f
                };
            }

            // Generate loot
            int seed = customSeed ?? Random.Range(int.MinValue, int.MaxValue);
            LootResult result = _lootGenerator.GenerateLootById(tableId, level, luckModifier, seed);

            _logger.LogInfo(
                $"Generated {result.TotalCount} loot drops from table '{tableId}' " +
                $"(level={level}, luck={luckModifier:F2}, seed={seed})",
                LogCategory
            );

            return result;
        }

        /// <summary>
        /// Generates loot and creates item entities in world at specified position.
        /// Integrates loot generation with item entity creation (Phase 1 Item system).
        /// </summary>
        /// <param name="sourceEntityId">Entity ID that is dropping loot (enemy, chest, boss)</param>
        /// <param name="dropPosition">World position for item entities</param>
        /// <returns>List of created item entity IDs (empty if no loot)</returns>
        public List<int> GenerateLootAndCreateItems(int sourceEntityId, Vector3 dropPosition)
        {
            var itemEntityIds = new List<int>();

            // Generate loot from source entity
            LootResult lootResult = GenerateLoot(sourceEntityId);

            if (!lootResult.HasLoot)
            {
                _logger.LogInfo($"No loot dropped from entity {sourceEntityId}", LogCategory);
                return itemEntityIds;
            }

            // Create item entity for each drop
            foreach (var drop in lootResult.Drops)
            {
                int itemEntityId = CreateItemEntity(drop.ItemId, drop.Quantity, dropPosition);
                itemEntityIds.Add(itemEntityId);

                _logger.LogInfo(
                    $"Created item entity {itemEntityId} for '{drop.ItemId}' x{drop.Quantity} at {dropPosition}",
                    LogCategory
                );
            }

            _logger.LogInfo(
                $"Created {itemEntityIds.Count} item entities from loot table '{lootResult.TableId}' at {dropPosition}",
                LogCategory
            );

            return itemEntityIds;
        }

        /// <summary>
        /// Generates loot and creates item entities from table ID directly.
        /// Useful for programmatic loot (events, quest rewards) with custom drop position.
        /// </summary>
        /// <param name="tableId">Loot table ID</param>
        /// <param name="level">Entity/player level</param>
        /// <param name="dropPosition">World position for item entities</param>
        /// <param name="luckModifier">Luck modifier (default: 1.0)</param>
        /// <param name="customSeed">Optional deterministic seed</param>
        /// <returns>List of created item entity IDs</returns>
        public List<int> GenerateLootAndCreateItems(
            string tableId,
            int level,
            Vector3 dropPosition,
            float luckModifier = 1.0f,
            int? customSeed = null)
        {
            var itemEntityIds = new List<int>();

            // Generate loot from table
            LootResult lootResult = GenerateLoot(tableId, level, luckModifier, customSeed);

            if (!lootResult.HasLoot)
            {
                _logger.LogInfo($"No loot generated from table '{tableId}'", LogCategory);
                return itemEntityIds;
            }

            // Create item entity for each drop
            foreach (var drop in lootResult.Drops)
            {
                int itemEntityId = CreateItemEntity(drop.ItemId, drop.Quantity, dropPosition);
                itemEntityIds.Add(itemEntityId);
            }

            _logger.LogInfo(
                $"Created {itemEntityIds.Count} item entities from table '{tableId}' at {dropPosition}",
                LogCategory
            );

            return itemEntityIds;
        }

        // ============================================================
        // ITEM ENTITY CREATION (Integration with Phase 1 Items)
        // ============================================================

        /// <summary>
        /// Creates item entity in world from item ID and quantity.
        /// Uses MythHunterItemComponent from Phase 1 integration.
        /// </summary>
        /// <param name="itemId">Item definition ID</param>
        /// <param name="quantity">Item quantity</param>
        /// <param name="position">World position (for future transform component)</param>
        /// <returns>Created item entity ID</returns>
        private int CreateItemEntity(string itemId, int quantity, Vector3 position)
        {
            // Create entity
            int itemEntityId = _entityManager.CreateEntity();

            // Add item component (Phase 1)
            var itemComponent = MythHunterItemComponent.Create(itemId, quantity);
            _entityManager.AddComponent(itemEntityId, itemComponent);

            // TODO: Add transform/position component when spatial system ready
            // TODO: Add physics/collider component for pickup detection
            // TODO: Add visual representation component

            return itemEntityId;
        }

        // ============================================================
        // QUERY METHODS
        // ============================================================

        /// <summary>
        /// Checks if entity has loot table configuration.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>True if entity has MythHunterLootComponent with valid table ID</returns>
        public bool HasLootTable(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterLootComponent>(entityId, out var lootComponent))
                return false;

            return lootComponent.HasLootTable();
        }

        /// <summary>
        /// Gets loot table ID from entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Loot table ID, or null if entity has no loot component</returns>
        public string GetLootTableId(int entityId)
        {
            if (_entityManager.TryGetComponent<MythHunterLootComponent>(entityId, out var lootComponent))
            {
                return lootComponent.LootTableId;
            }

            return null;
        }

        /// <summary>
        /// Gets all entities with loot table components.
        /// </summary>
        /// <returns>All entities that can drop loot</returns>
        public IEnumerable<int> GetEntitiesWithLoot()
        {
            return _entityManager.GetEntitiesWith<MythHunterLootComponent>();
        }

        /// <summary>
        /// Checks if entity's loot has already been generated.
        /// Useful for preventing duplicate loot from chests/containers.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>True if loot already generated, false otherwise</returns>
        public bool IsLootGenerated(int entityId)
        {
            if (_entityManager.TryGetComponent<MythHunterLootComponent>(entityId, out var lootComponent))
            {
                return lootComponent.IsGenerated();
            }

            return false;
        }

        /// <summary>
        /// Gets entity level from loot component.
        /// Useful for scaling loot to entity level.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Entity level, or 0 if no loot component</returns>
        public int GetEntityLevel(int entityId)
        {
            if (_entityManager.TryGetComponent<MythHunterLootComponent>(entityId, out var lootComponent))
            {
                return lootComponent.Level;
            }

            return 0;
        }

        /// <summary>
        /// Gets luck modifier from entity's loot component.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Luck modifier, or 1.0 if no loot component</returns>
        public float GetLuckModifier(int entityId)
        {
            if (_entityManager.TryGetComponent<MythHunterLootComponent>(entityId, out var lootComponent))
            {
                return lootComponent.LuckModifier;
            }

            return 1.0f;
        }

        // ============================================================
        // REGISTRY QUERY METHODS
        // ============================================================

        /// <summary>
        /// Checks if loot table exists in registry.
        /// </summary>
        /// <param name="tableId">Loot table ID</param>
        /// <returns>True if table registered</returns>
        public bool HasLootTableInRegistry(string tableId)
        {
            return _lootTableRegistry.HasTable(tableId);
        }

        /// <summary>
        /// Gets all registered loot table IDs.
        /// Useful for debugging and validation.
        /// </summary>
        /// <returns>All registered table IDs</returns>
        public IEnumerable<string> GetAllLootTableIds()
        {
            return _lootTableRegistry.GetAllTables().Select(t => t.TableId);
        }

        /// <summary>
        /// Gets loot table by ID from registry.
        /// </summary>
        /// <param name="tableId">Loot table ID</param>
        /// <returns>Loot table instance, or null if not found</returns>
        public ILootTable GetLootTable(string tableId)
        {
            return _lootTableRegistry.GetTable(tableId);
        }
    }
}
