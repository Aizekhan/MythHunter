// Assets/_MythHunter/Code/GameLogic/Combat/Migration/CombatComponentMigration.cs
using MythHunter.Components.Combat;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;
using System.Collections.Generic;

namespace MythHunter.GameLogic.Combat.Migration
{
    /// <summary>
    /// Utility for migrating entities from old CombatStatsComponent to new Framework-based components.
    /// Converts:
    /// - CombatStatsComponent → MythHunterStatsComponent + MythHunterResourceComponent + CombatStateComponent
    /// </summary>
    public class CombatComponentMigration
    {
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        [Inject]
        public CombatComponentMigration(IEntityManager entityManager, IMythLogger logger)
        {
            _entityManager = entityManager;
            _logger = logger;
        }

        /// <summary>
        /// Migrate all entities with old CombatStatsComponent to new components
        /// </summary>
        /// <returns>Number of entities migrated</returns>
        public int MigrateAllEntities()
        {
            var entities = _entityManager.GetEntitiesWithComponent<CombatStatsComponent>();
            int migratedCount = 0;

            foreach (var entityId in entities)
            {
                if (MigrateEntity(entityId))
                {
                    migratedCount++;
                }
            }

            _logger.LogInfo($"Combat component migration complete: {migratedCount} entities migrated", "Migration");
            return migratedCount;
        }

        /// <summary>
        /// Migrate single entity from old to new components
        /// </summary>
        /// <param name="entityId">Entity to migrate</param>
        /// <returns>True if migration successful</returns>
        public bool MigrateEntity(int entityId)
        {
            // Check if entity has old component
            if (!_entityManager.HasComponent<CombatStatsComponent>(entityId))
            {
                _logger.LogWarning($"Entity {entityId} does not have CombatStatsComponent", "Migration");
                return false;
            }

            // Check if already migrated (has new components)
            if (_entityManager.HasComponent<MythHunterStatsComponent>(entityId))
            {
                _logger.LogInfo($"Entity {entityId} already migrated", "Migration");
                return false;
            }

            try
            {
                // Get old component data
                var oldStats = _entityManager.GetComponent<CombatStatsComponent>(entityId);

                // Create new stats component
                var newStats = MythHunterStatsComponent.Create();

                // Migrate base stats
                newStats.Stats.SetBaseStat(CombatStatType.AttackPower, oldStats.AttackPower);
                newStats.Stats.SetBaseStat(CombatStatType.Defense, oldStats.Defense);
                newStats.Stats.SetBaseStat(CombatStatType.AttackSpeed, oldStats.AttackSpeed);
                newStats.Stats.SetBaseStat(CombatStatType.CriticalChance, oldStats.CriticalChance);
                newStats.Stats.SetBaseStat(CombatStatType.CriticalMultiplier, oldStats.CriticalMultiplier);
                newStats.Stats.SetBaseStat(CombatStatType.Accuracy, oldStats.Accuracy);
                newStats.Stats.SetBaseStat(CombatStatType.DodgeChance, oldStats.DodgeChance);
                newStats.Stats.SetBaseStat(CombatStatType.BlockChance, oldStats.BlockChance);
                newStats.Stats.SetBaseStat(CombatStatType.CounterAttackChance, oldStats.CounterAttackChance);

                // Create new resources component
                var newResources = MythHunterResourceComponent.Create();

                // Migrate resources (Rage, Concentration)
                // Note: Health might be in separate HealthComponent, not migrating here
                newResources.Resources.SetCurrent(MythHunterResourceType.Rage, oldStats.Rage);
                newResources.Resources.SetMax(MythHunterResourceType.Rage, oldStats.MaxRage);

                newResources.Resources.SetCurrent(MythHunterResourceType.Concentration, oldStats.Concentration);
                newResources.Resources.SetMax(MythHunterResourceType.Concentration, oldStats.MaxConcentration);

                // Create combat state component
                var combatState = CombatStateComponent.Create();
                combatState.IsInCombat = oldStats.IsInCombat;
                combatState.CombatTargetEntityId = oldStats.CombatTargetEntityId;
                combatState.LastAttackTime = oldStats.LastAttackTime;

                // Add new components
                _entityManager.AddComponent(entityId, newStats);
                _entityManager.AddComponent(entityId, newResources);
                _entityManager.AddComponent(entityId, combatState);

                // Note: NOT removing old component yet for safety
                // Can be removed after validating migration success

                _logger.LogInfo($"Entity {entityId} migrated successfully", "Migration");
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Failed to migrate entity {entityId}: {ex.Message}", "Migration", ex);
                return false;
            }
        }

        /// <summary>
        /// Remove old CombatStatsComponent from migrated entities
        /// WARNING: Only call after validating migration success!
        /// </summary>
        /// <returns>Number of components removed</returns>
        public int RemoveOldComponents()
        {
            int removedCount = 0;
            var entities = _entityManager.GetEntitiesWithComponent<CombatStatsComponent>();

            foreach (var entityId in entities)
            {
                // Only remove if new components exist
                if (_entityManager.HasComponent<MythHunterStatsComponent>(entityId) &&
                    _entityManager.HasComponent<MythHunterResourceComponent>(entityId) &&
                    _entityManager.HasComponent<CombatStateComponent>(entityId))
                {
                    _entityManager.RemoveComponent<CombatStatsComponent>(entityId);
                    removedCount++;
                    _logger.LogInfo($"Removed old CombatStatsComponent from entity {entityId}", "Migration");
                }
            }

            _logger.LogInfo($"Removed {removedCount} old components", "Migration");
            return removedCount;
        }

        /// <summary>
        /// Rollback migration for entity (restore old component, remove new ones)
        /// </summary>
        public bool RollbackEntity(int entityId)
        {
            try
            {
                // Remove new components
                if (_entityManager.HasComponent<MythHunterStatsComponent>(entityId))
                {
                    _entityManager.RemoveComponent<MythHunterStatsComponent>(entityId);
                }

                if (_entityManager.HasComponent<MythHunterResourceComponent>(entityId))
                {
                    _entityManager.RemoveComponent<MythHunterResourceComponent>(entityId);
                }

                if (_entityManager.HasComponent<CombatStateComponent>(entityId))
                {
                    _entityManager.RemoveComponent<CombatStateComponent>(entityId);
                }

                _logger.LogInfo($"Rolled back migration for entity {entityId}", "Migration");
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Failed to rollback entity {entityId}: {ex.Message}", "Migration", ex);
                return false;
            }
        }

        /// <summary>
        /// Get migration status for entity
        /// </summary>
        public MigrationStatus GetEntityStatus(int entityId)
        {
            bool hasOld = _entityManager.HasComponent<CombatStatsComponent>(entityId);
            bool hasNew = _entityManager.HasComponent<MythHunterStatsComponent>(entityId);

            if (hasOld && hasNew)
                return MigrationStatus.Migrated;

            if (!hasOld && hasNew)
                return MigrationStatus.MigratedAndCleaned;

            if (hasOld && !hasNew)
                return MigrationStatus.NotMigrated;

            return MigrationStatus.NoComponents;
        }

        /// <summary>
        /// Get migration statistics
        /// </summary>
        public MigrationStats GetStatistics()
        {
            var stats = new MigrationStats();
            var allEntities = new List<int>();

            // Get all entities with old or new components
            var oldEntities = _entityManager.GetEntitiesWithComponent<CombatStatsComponent>();
            var newEntities = _entityManager.GetEntitiesWithComponent<MythHunterStatsComponent>();

            allEntities.AddRange(oldEntities);
            allEntities.AddRange(newEntities);

            // Count unique entities
            var uniqueEntities = new HashSet<int>(allEntities);

            foreach (var entityId in uniqueEntities)
            {
                var status = GetEntityStatus(entityId);
                switch (status)
                {
                    case MigrationStatus.NotMigrated:
                        stats.NotMigrated++;
                        break;
                    case MigrationStatus.Migrated:
                        stats.Migrated++;
                        break;
                    case MigrationStatus.MigratedAndCleaned:
                        stats.MigratedAndCleaned++;
                        break;
                }
            }

            return stats;
        }
    }

    /// <summary>
    /// Migration status for an entity
    /// </summary>
    public enum MigrationStatus
    {
        /// <summary>No combat components found</summary>
        NoComponents,

        /// <summary>Has old component, not yet migrated</summary>
        NotMigrated,

        /// <summary>Has both old and new components (migration in progress)</summary>
        Migrated,

        /// <summary>Has only new components (old removed)</summary>
        MigratedAndCleaned
    }

    /// <summary>
    /// Migration statistics
    /// </summary>
    public struct MigrationStats
    {
        public int NotMigrated;
        public int Migrated;
        public int MigratedAndCleaned;

        public int Total => NotMigrated + Migrated + MigratedAndCleaned;
        public float ProgressPercentage => Total > 0 ? ((float)(Migrated + MigratedAndCleaned) / Total) * 100f : 0f;
    }
}
