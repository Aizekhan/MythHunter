// Assets/_MythHunter/Editor/Inventory/InventoryComponentMigrationEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MythHunter.GameLogic.Inventory.Migration;
using MythHunter.Utils.Logging;
using RPGFramework.ECS;

namespace MythHunter.Editor.Inventory
{
    /// <summary>
    /// Unity Editor utility for migrating inventory components from old to new Framework-based system.
    /// Provides menu commands for safe entity migration.
    /// </summary>
    public static class InventoryComponentMigrationEditor
    {
        private const string MenuRoot = "MythHunter/Inventory Migration/";

        [MenuItem(MenuRoot + "Migrate All Entities", false, 1)]
        public static void MigrateAllEntities()
        {
            if (!EditorUtility.DisplayDialog(
                "Inventory Component Migration",
                "This will migrate ALL entities from old InventoryComponent to new Framework-based MythHunterInventoryComponent.\n\n" +
                "The old components will be preserved for safety. You can remove them later after validation.\n\n" +
                "Migration includes:\n" +
                "- Bag items → PrimaryContainer\n" +
                "- Elixirs → PrimaryContainer\n" +
                "- Equipment slots (Armor, Weapons, Artifact)\n" +
                "- Current weapon state\n\n" +
                "Continue?",
                "Migrate",
                "Cancel"))
            {
                return;
            }

            try
            {
                var entityManager = GetEntityManager();
                var logger = GetLogger();
                var migration = new InventoryComponentMigration(entityManager, logger);

                int migratedCount = migration.MigrateAllEntities();

                var stats = migration.GetStatistics();

                EditorUtility.DisplayDialog(
                    "Migration Complete",
                    $"Migration finished successfully!\n\n" +
                    $"Total entities: {stats.Total}\n" +
                    $"Not migrated: {stats.NotMigrated}\n" +
                    $"Migrated (with old): {stats.MigratedWithOld}\n" +
                    $"Migrated (cleaned): {stats.MigratedCleaned}\n" +
                    $"Progress: {stats.ProgressPercentage:F1}%\n\n" +
                    $"Entities migrated this run: {migratedCount}",
                    "OK");

                UnityEngine.Debug.Log($"[InventoryMigration] Migrated {migratedCount} entities. Total progress: {stats.ProgressPercentage:F1}%");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "Migration Failed",
                    $"An error occurred during migration:\n\n{ex.Message}",
                    "OK");
                UnityEngine.Debug.LogError($"[InventoryMigration] Migration failed: {ex}");
            }
        }

        [MenuItem(MenuRoot + "Show Migration Statistics", false, 2)]
        public static void ShowStatistics()
        {
            try
            {
                var entityManager = GetEntityManager();
                var logger = GetLogger();
                var migration = new InventoryComponentMigration(entityManager, logger);

                var stats = migration.GetStatistics();

                string message = $"Inventory Component Migration Statistics:\n\n" +
                                $"Total entities: {stats.Total}\n" +
                                $"Not migrated: {stats.NotMigrated}\n" +
                                $"Migrated (with old): {stats.MigratedWithOld}\n" +
                                $"Migrated (cleaned): {stats.MigratedCleaned}\n" +
                                $"Progress: {stats.ProgressPercentage:F1}%\n\n" +
                                $"Successful migrations: {stats.SuccessfulMigrations}\n" +
                                $"Failed migrations: {stats.FailedMigrations}";

                EditorUtility.DisplayDialog("Migration Statistics", message, "OK");
                UnityEngine.Debug.Log($"[InventoryMigration] {message}");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    $"Could not retrieve statistics:\n\n{ex.Message}",
                    "OK");
                UnityEngine.Debug.LogError($"[InventoryMigration] Failed to get statistics: {ex}");
            }
        }

        [MenuItem(MenuRoot + "Remove Old Components (After Validation)", false, 3)]
        public static void RemoveOldComponents()
        {
            if (!EditorUtility.DisplayDialog(
                "Remove Old Components",
                "WARNING: This will permanently remove old InventoryComponent from ALL migrated entities!\n\n" +
                "Only do this AFTER validating that inventory works correctly with new components.\n\n" +
                "This action cannot be undone without restoring from version control.\n\n" +
                "Continue?",
                "Remove",
                "Cancel"))
            {
                return;
            }

            // Second confirmation
            if (!EditorUtility.DisplayDialog(
                "Final Confirmation",
                "Are you absolutely sure?\n\n" +
                "This will delete old inventory components from migrated entities.",
                "Yes, Remove",
                "Cancel"))
            {
                return;
            }

            try
            {
                var entityManager = GetEntityManager();
                var logger = GetLogger();
                var migration = new InventoryComponentMigration(entityManager, logger);

                int removedCount = 0;
                var allEntities = new System.Collections.Generic.List<int>();

                // Get all entities with both components
                foreach (var entityId in entityManager.GetEntitiesWith<MythHunter.Components.Character.InventoryComponent>())
                {
                    if (entityManager.HasComponent<MythHunter.GameLogic.Inventory.Components.MythHunterInventoryComponent>(entityId))
                    {
                        allEntities.Add(entityId);
                    }
                }

                foreach (var entityId in allEntities)
                {
                    if (migration.RemoveOldComponent(entityId))
                    {
                        removedCount++;
                    }
                }

                var stats = migration.GetStatistics();

                EditorUtility.DisplayDialog(
                    "Components Removed",
                    $"Old components removed successfully!\n\n" +
                    $"Components removed: {removedCount}\n" +
                    $"Remaining migrated (cleaned): {stats.MigratedCleaned}\n" +
                    $"Remaining not migrated: {stats.NotMigrated}",
                    "OK");

                UnityEngine.Debug.Log($"[InventoryMigration] Removed {removedCount} old components");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "Removal Failed",
                    $"An error occurred:\n\n{ex.Message}",
                    "OK");
                UnityEngine.Debug.LogError($"[InventoryMigration] Component removal failed: {ex}");
            }
        }

        [MenuItem(MenuRoot + "Rollback Single Entity", false, 4)]
        public static void RollbackEntity()
        {
            string entityIdStr = EditorUtility.DisplayDialogComplex(
                "Rollback Entity",
                "Enter entity ID to rollback migration:",
                "Cancel",
                "",
                "") == 0 ? null : "";

            if (string.IsNullOrEmpty(entityIdStr))
            {
                return;
            }

            if (!int.TryParse(entityIdStr, out int entityId))
            {
                EditorUtility.DisplayDialog("Invalid Input", "Please enter a valid integer entity ID.", "OK");
                return;
            }

            try
            {
                var entityManager = GetEntityManager();
                var logger = GetLogger();
                var migration = new InventoryComponentMigration(entityManager, logger);

                bool success = migration.RollbackEntity(entityId);

                if (success)
                {
                    EditorUtility.DisplayDialog(
                        "Rollback Complete",
                        $"Entity {entityId} rolled back successfully.\n\nNew inventory components removed.",
                        "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Rollback Failed",
                        $"Could not rollback entity {entityId}.\n\nCheck console for details.",
                        "OK");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    $"Rollback failed:\n\n{ex.Message}",
                    "OK");
                UnityEngine.Debug.LogError($"[InventoryMigration] Rollback failed: {ex}");
            }
        }

        private static IEntityManager GetEntityManager()
        {
            // Editor utility: create temporary EntityManager for migration tasks
            // In edit mode, we don't have runtime DI available
            UnityEngine.Debug.LogWarning("[InventoryMigration] Creating temporary EntityManager for Editor migration");
            return new EntityManager();
        }

        private static IMythLogger GetLogger()
        {
            // Editor utility: create Unity logger wrapper
            return new UnityLogger();
        }
    }

    /// <summary>
    /// Simple Unity Debug.Log wrapper for editor scripts (shared with Combat migration)
    /// </summary>
    internal class UnityLogger : IMythLogger
    {
        public void LogInfo(string message, string category = null) =>
            UnityEngine.Debug.Log($"[{category ?? "MythHunter"}] {message}");

        public void LogWarning(string message, string category = null) =>
            UnityEngine.Debug.LogWarning($"[{category ?? "MythHunter"}] {message}");

        public void LogError(string message, string category = null, System.Exception exception = null) =>
            UnityEngine.Debug.LogError($"[{category ?? "MythHunter"}] {message}" + (exception != null ? $"\n{exception}" : ""));

        public void LogFatal(string message, string category = null) =>
            UnityEngine.Debug.LogError($"[FATAL][{category ?? "MythHunter"}] {message}");

        public void LogDebug(string message, string category = null) =>
            UnityEngine.Debug.Log($"[DEBUG][{category ?? "MythHunter"}] {message}");

        public void LogTrace(string message, string category = null) =>
            UnityEngine.Debug.Log($"[TRACE][{category ?? "MythHunter"}] {message}");

        public void WithContext(string key, object value) { }
        public void ClearContext() { }
        public void SetDefaultCategory(string category) { }
        public void SetMinLogLevel(MythHunter.Utils.Logging.LogLevel level) { }
        public void EnableFileLogging(bool enable) { }
        public void Log(RPGFramework.Logging.LogSeverity severity, string message, string category = null, System.Exception exception = null)
        {
            switch (severity)
            {
                case RPGFramework.Logging.LogSeverity.Error:
                    LogError(message, category, exception);
                    break;
                case RPGFramework.Logging.LogSeverity.Warning:
                    LogWarning(message, category);
                    break;
                default:
                    LogInfo(message, category);
                    break;
            }
        }
    }
}
#endif
