// Assets/_MythHunter/Code/GameLogic/Inventory/Migration/InventoryComponentMigration.cs

using System.Collections.Generic;
using RPGFramework.ECS;
using RPGFramework.Inventory.Runtime;
using MythHunter.Utils.Logging;
using MythHunter.GameLogic.Inventory.Components;
using MythHunter.Components.Character; // Old InventoryComponent

namespace MythHunter.GameLogic.Inventory.Migration
{
    /// <summary>
    /// Migration utility for converting old InventoryComponent to new MythHunterInventoryComponent.
    ///
    /// Migration Strategy (Safe Dual-Component Approach):
    /// 1. Read old InventoryComponent
    /// 2. Create new MythHunterInventoryComponent
    /// 3. Copy items to PrimaryContainer
    /// 4. Map equipment slots (Helmet, Chest, Weapons, Artifact, etc.)
    /// 5. Preserve old component for rollback safety
    /// 6. Track migration statistics
    ///
    /// Pattern from EPIC 12 Combat Integration.
    /// </summary>
    public class InventoryComponentMigration
    {
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        private const string LogCategory = "InventoryMigration";

        // Migration statistics
        private int _totalEntitiesProcessed = 0;
        private int _successfulMigrations = 0;
        private int _failedMigrations = 0;
        private int _alreadyMigrated = 0;

        public InventoryComponentMigration(IEntityManager entityManager, IMythLogger logger)
        {
            _entityManager = entityManager;
            _logger = logger;
        }

        // ============================================================
        // MIGRATION OPERATIONS
        // ============================================================

        /// <summary>
        /// Migrates all entities with old InventoryComponent to new MythHunterInventoryComponent.
        /// Preserves old components for rollback safety.
        /// </summary>
        /// <returns>Number of entities migrated this run</returns>
        public int MigrateAllEntities()
        {
            _logger.LogInfo("Starting inventory component migration for all entities...", LogCategory);

            int migratedThisRun = 0;
            var entitiesToMigrate = new List<int>();

            // Collect entities with old component
            foreach (var entityId in _entityManager.GetEntitiesWith<InventoryComponent>())
            {
                entitiesToMigrate.Add(entityId);
            }

            _logger.LogInfo($"Found {entitiesToMigrate.Count} entities with old InventoryComponent", LogCategory);

            // Migrate each entity
            foreach (var entityId in entitiesToMigrate)
            {
                _totalEntitiesProcessed++;

                // Skip if already has new component
                if (_entityManager.HasComponent<MythHunterInventoryComponent>(entityId))
                {
                    _alreadyMigrated++;
                    _logger.LogInfo($"Entity {entityId} already has new component, skipping", LogCategory);
                    continue;
                }

                // Attempt migration
                if (MigrateEntity(entityId))
                {
                    migratedThisRun++;
                }
            }

            _logger.LogInfo($"Migration complete. Migrated {migratedThisRun} entities this run.", LogCategory);
            _logger.LogInfo($"Statistics: Total={_totalEntitiesProcessed}, Success={_successfulMigrations}, Failed={_failedMigrations}, AlreadyMigrated={_alreadyMigrated}", LogCategory);

            return migratedThisRun;
        }

        /// <summary>
        /// Migrates single entity from old to new inventory component.
        /// Preserves old component for rollback.
        /// </summary>
        /// <param name="entityId">Entity to migrate</param>
        /// <returns>True if migration successful</returns>
        public bool MigrateEntity(int entityId)
        {
            // Validate entity has old component
            if (!_entityManager.TryGetComponent<InventoryComponent>(entityId, out var oldInv))
            {
                _logger.LogWarning($"Entity {entityId} has no old InventoryComponent to migrate", LogCategory);
                _failedMigrations++;
                return false;
            }

            // Check if already migrated
            if (_entityManager.HasComponent<MythHunterInventoryComponent>(entityId))
            {
                _logger.LogInfo($"Entity {entityId} already migrated", LogCategory);
                _alreadyMigrated++;
                return false;
            }

            try
            {
                // Create new component with appropriate capacity
                int bagCapacity = oldInv.Bag.Slots > 0 ? oldInv.Bag.Slots : 50;
                var newInv = MythHunterInventoryComponent.CreateHero(bagCapacity);

                // Migrate items from old Items list to PrimaryContainer
                if (oldInv.Items != null && oldInv.Items.Count > 0)
                {
                    foreach (var itemId in oldInv.Items)
                    {
                        if (!string.IsNullOrEmpty(itemId) && newInv.Inventory.PrimaryContainer != null)
                        {
                            newInv.Inventory.PrimaryContainer.Add(itemId);
                        }
                    }
                    _logger.LogInfo($"Migrated {oldInv.Items.Count} items to entity {entityId}", LogCategory);
                }

                // Migrate elixirs from Bag to PrimaryContainer
                if (oldInv.Bag.Elixirs != null && oldInv.Bag.Elixirs.Count > 0)
                {
                    foreach (var elixir in oldInv.Bag.Elixirs)
                    {
                        if (!string.IsNullOrEmpty(elixir) && newInv.Inventory.PrimaryContainer != null)
                        {
                            newInv.Inventory.PrimaryContainer.Add(elixir);
                        }
                    }
                    _logger.LogInfo($"Migrated {oldInv.Bag.Elixirs.Count} elixirs to entity {entityId}", LogCategory);
                }

                // Migrate equipped items to equipment slots
                MigrateEquipment(ref newInv, oldInv);

                // Add new component (old component preserved automatically)
                _entityManager.AddComponent(entityId, newInv);

                _successfulMigrations++;
                _logger.LogInfo($"Successfully migrated entity {entityId} inventory", LogCategory);
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Failed to migrate entity {entityId}: {ex.Message}", LogCategory, ex);
                _failedMigrations++;
                return false;
            }
        }

        /// <summary>
        /// Rolls back entity to old component (removes new component).
        /// Emergency rollback if issues discovered after migration.
        /// </summary>
        /// <param name="entityId">Entity to rollback</param>
        /// <returns>True if rollback successful</returns>
        public bool RollbackEntity(int entityId)
        {
            if (!_entityManager.HasComponent<MythHunterInventoryComponent>(entityId))
            {
                _logger.LogWarning($"Entity {entityId} has no new component to rollback", LogCategory);
                return false;
            }

            if (!_entityManager.HasComponent<InventoryComponent>(entityId))
            {
                _logger.LogError($"Entity {entityId} has no old component to rollback to!", LogCategory);
                return false;
            }

            // Remove new component (old component remains)
            _entityManager.RemoveComponent<MythHunterInventoryComponent>(entityId);
            _logger.LogInfo($"Rolled back entity {entityId} to old InventoryComponent", LogCategory);
            return true;
        }

        /// <summary>
        /// Removes old component from migrated entities (cleanup after validation).
        /// Only call after extended validation period!
        /// </summary>
        /// <param name="entityId">Entity to clean up</param>
        /// <returns>True if old component removed</returns>
        public bool RemoveOldComponent(int entityId)
        {
            // Safety check: Must have new component
            if (!_entityManager.HasComponent<MythHunterInventoryComponent>(entityId))
            {
                _logger.LogWarning($"Entity {entityId} has no new component, cannot remove old", LogCategory);
                return false;
            }

            // Remove old component
            if (_entityManager.HasComponent<InventoryComponent>(entityId))
            {
                _entityManager.RemoveComponent<InventoryComponent>(entityId);
                _logger.LogInfo($"Removed old InventoryComponent from entity {entityId}", LogCategory);
                return true;
            }

            return false;
        }

        // ============================================================
        // STATISTICS
        // ============================================================

        /// <summary>
        /// Gets migration statistics.
        /// </summary>
        public MigrationStatistics GetStatistics()
        {
            int totalEntities = 0;
            int notMigrated = 0;
            int migratedWithOld = 0;
            int migratedCleaned = 0;

            // Count all entities with inventory
            var allEntities = new HashSet<int>();
            foreach (var id in _entityManager.GetEntitiesWith<InventoryComponent>())
                allEntities.Add(id);
            foreach (var id in _entityManager.GetEntitiesWith<MythHunterInventoryComponent>())
                allEntities.Add(id);

            totalEntities = allEntities.Count;

            foreach (var entityId in allEntities)
            {
                bool hasOld = _entityManager.HasComponent<InventoryComponent>(entityId);
                bool hasNew = _entityManager.HasComponent<MythHunterInventoryComponent>(entityId);

                if (hasOld && !hasNew)
                    notMigrated++;
                else if (hasOld && hasNew)
                    migratedWithOld++;
                else if (!hasOld && hasNew)
                    migratedCleaned++;
            }

            return new MigrationStatistics
            {
                Total = totalEntities,
                NotMigrated = notMigrated,
                MigratedWithOld = migratedWithOld,
                MigratedCleaned = migratedCleaned,
                SuccessfulMigrations = _successfulMigrations,
                FailedMigrations = _failedMigrations
            };
        }

        /// <summary>
        /// Resets migration statistics counters.
        /// </summary>
        public void ResetStatistics()
        {
            _totalEntitiesProcessed = 0;
            _successfulMigrations = 0;
            _failedMigrations = 0;
            _alreadyMigrated = 0;
        }

        // ============================================================
        // PRIVATE HELPERS
        // ============================================================

        private void MigrateEquipment(ref MythHunterInventoryComponent newInv, InventoryComponent oldInv)
        {
            // Safety check: Ensure equipment slots dictionary exists
            if (newInv.Inventory.EquipmentSlots == null)
            {
                _logger.LogWarning("New inventory has no equipment slots dictionary", LogCategory);
                return;
            }

            // Migrate armor
            EquipIfNotEmpty(ref newInv, oldInv.EquippedItems.Armor.Helmet, MythHunterEquipmentSlot.Helmet);
            EquipIfNotEmpty(ref newInv, oldInv.EquippedItems.Armor.Chest, MythHunterEquipmentSlot.Chest);
            EquipIfNotEmpty(ref newInv, oldInv.EquippedItems.Armor.Gloves, MythHunterEquipmentSlot.Gloves);
            EquipIfNotEmpty(ref newInv, oldInv.EquippedItems.Armor.Boots, MythHunterEquipmentSlot.Boots);

            // Migrate weapons
            EquipIfNotEmpty(ref newInv, oldInv.EquippedItems.Weapons.MainWeapon, MythHunterEquipmentSlot.MainWeapon);
            EquipIfNotEmpty(ref newInv, oldInv.EquippedItems.Weapons.SecondaryWeapon, MythHunterEquipmentSlot.SecondaryWeapon);
            EquipIfNotEmpty(ref newInv, oldInv.EquippedItems.Weapons.AuxiliaryWeapon, MythHunterEquipmentSlot.AuxiliaryWeapon);

            // Migrate artifact
            EquipIfNotEmpty(ref newInv, oldInv.EquippedItems.Artifact, MythHunterEquipmentSlot.Artifact);

            // Determine current weapon slot
            string currentWeapon = oldInv.EquippedItems.Weapons.CurrentWeapon;
            if (!string.IsNullOrEmpty(currentWeapon))
            {
                if (currentWeapon == oldInv.EquippedItems.Weapons.MainWeapon)
                    newInv.CurrentWeaponSlot = MythHunterEquipmentSlot.MainWeapon;
                else if (currentWeapon == oldInv.EquippedItems.Weapons.SecondaryWeapon)
                    newInv.CurrentWeaponSlot = MythHunterEquipmentSlot.SecondaryWeapon;
                else if (currentWeapon == oldInv.EquippedItems.Weapons.AuxiliaryWeapon)
                    newInv.CurrentWeaponSlot = MythHunterEquipmentSlot.AuxiliaryWeapon;
            }
        }

        private void EquipIfNotEmpty(ref MythHunterInventoryComponent inv, string itemId, MythHunterEquipmentSlot slot)
        {
            if (string.IsNullOrEmpty(itemId))
                return;

            if (inv.Inventory.EquipmentSlots.TryGetValue(slot, out var equipSlot))
            {
                equipSlot.Equip(itemId);
                _logger.LogInfo($"Migrated equipment: {itemId} → {slot}", LogCategory);
            }
        }
    }

    /// <summary>
    /// Migration statistics data structure.
    /// </summary>
    public struct MigrationStatistics
    {
        /// <summary>Total entities with inventory (old or new)</summary>
        public int Total;

        /// <summary>Entities with only old component</summary>
        public int NotMigrated;

        /// <summary>Entities with both old and new components</summary>
        public int MigratedWithOld;

        /// <summary>Entities with only new component (old removed)</summary>
        public int MigratedCleaned;

        /// <summary>Successful migrations this session</summary>
        public int SuccessfulMigrations;

        /// <summary>Failed migrations this session</summary>
        public int FailedMigrations;

        /// <summary>Progress percentage (0-100)</summary>
        public readonly float ProgressPercentage
        {
            get
            {
                if (Total == 0) return 0f;
                int migrated = MigratedWithOld + MigratedCleaned;
                return (migrated / (float)Total) * 100f;
            }
        }
    }
}
