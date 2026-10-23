// Assets/_MythHunter/Code/GameLogic/Loot/Initialization/LootRegistryInitializer.cs

using RPGFramework.Loot;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.GameLogic.Loot.Data;
using System;

namespace MythHunter.GameLogic.Loot.Initialization
{
    /// <summary>
    /// Initializes and populates the loot table registry on game start.
    /// Registers all sample loot tables and provides hooks for custom table registration.
    ///
    /// Usage:
    /// - Inject into game initialization flow (e.g., GameBootstrap, StartupSequence)
    /// - Call Initialize() during game startup
    /// - Registry will be populated with sample tables from SampleLootTables
    ///
    /// Pattern:
    /// - Dependency injection for ILootTableRegistry
    /// - Logging for tracking initialization
    /// - Extensible for custom table registration
    /// </summary>
    public class LootRegistryInitializer
    {
        private readonly ILootTableRegistry _lootTableRegistry;
        private readonly IMythLogger _logger;

        private const string LogCategory = "LootRegistry";

        private bool _isInitialized = false;

        [Inject]
        public LootRegistryInitializer(
            ILootTableRegistry lootTableRegistry,
            IMythLogger logger)
        {
            _lootTableRegistry = lootTableRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Initializes the loot table registry with sample tables.
        /// Should be called once during game startup.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                _logger.LogWarning("LootRegistryInitializer already initialized, skipping", LogCategory);
                return;
            }

            _logger.LogInfo("Initializing loot table registry...", LogCategory);

            try
            {
                // Register all sample loot tables
                RegisterSampleTables();

                _isInitialized = true;

                _logger.LogInfo(
                    $"Loot table registry initialized successfully. " +
                    $"Registered {_lootTableRegistry.GetAllTables().Count} tables.",
                    LogCategory
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize loot table registry: {ex.Message}", LogCategory);
                throw;
            }
        }

        /// <summary>
        /// Registers all sample loot tables from SampleLootTables.
        /// </summary>
        private void RegisterSampleTables()
        {
            var tables = SampleLootTables.GetAllTables();

            foreach (var table in tables)
            {
                try
                {
                    _lootTableRegistry.RegisterTable(table);
                    _logger.LogDebug($"Registered loot table: '{table.TableId}' - {table.Name}", LogCategory);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to register loot table '{table.TableId}': {ex.Message}", LogCategory);
                }
            }

            _logger.LogInfo($"Registered {tables.Count} sample loot tables", LogCategory);
        }

        /// <summary>
        /// Registers a custom loot table at runtime.
        /// Useful for mod support, dynamic content, or procedural generation.
        /// </summary>
        /// <param name="table">Loot table to register</param>
        public void RegisterCustomTable(ILootTable table)
        {
            if (table == null)
            {
                _logger.LogWarning("Cannot register null loot table", LogCategory);
                return;
            }

            try
            {
                _lootTableRegistry.RegisterTable(table);
                _logger.LogInfo($"Registered custom loot table: '{table.TableId}' - {table.Name}", LogCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to register custom loot table '{table.TableId}': {ex.Message}", LogCategory);
            }
        }

        /// <summary>
        /// Unregisters a loot table by ID.
        /// Useful for dynamic content removal or mod unloading.
        /// </summary>
        /// <param name="tableId">Loot table ID to unregister</param>
        /// <returns>True if unregistered, false if not found</returns>
        public bool UnregisterTable(string tableId)
        {
            bool success = _lootTableRegistry.UnregisterTable(tableId);

            if (success)
            {
                _logger.LogInfo($"Unregistered loot table: '{tableId}'", LogCategory);
            }
            else
            {
                _logger.LogWarning($"Failed to unregister loot table '{tableId}' - not found in registry", LogCategory);
            }

            return success;
        }

        /// <summary>
        /// Clears all registered loot tables.
        /// Use with caution - typically only needed for testing or full reloads.
        /// </summary>
        public void ClearAllTables()
        {
            _lootTableRegistry.Clear();
            _isInitialized = false;
            _logger.LogWarning("Cleared all loot tables from registry", LogCategory);
        }

        /// <summary>
        /// Checks if a loot table is registered.
        /// </summary>
        /// <param name="tableId">Loot table ID to check</param>
        /// <returns>True if registered</returns>
        public bool HasTable(string tableId)
        {
            return _lootTableRegistry.HasTable(tableId);
        }

        /// <summary>
        /// Gets the number of registered loot tables.
        /// </summary>
        public int GetTableCount()
        {
            return _lootTableRegistry.GetAllTables().Count;
        }

        /// <summary>
        /// Logs all registered loot tables for debugging.
        /// </summary>
        public void LogRegisteredTables()
        {
            var tables = _lootTableRegistry.GetAllTables();

            _logger.LogInfo($"=== Registered Loot Tables ({tables.Count}) ===", LogCategory);

            foreach (var table in tables)
            {
                int entryCount = table.Entries?.Count ?? 0;
                int guaranteedCount = table.GuaranteedDrops?.Count ?? 0;

                _logger.LogInfo(
                    $"  [{table.TableId}] {table.Name} - " +
                    $"Entries: {entryCount}, Guaranteed: {guaranteedCount}, " +
                    $"Drops: {table.MinDrops}-{table.MaxDrops}",
                    LogCategory
                );
            }
        }
    }
}
