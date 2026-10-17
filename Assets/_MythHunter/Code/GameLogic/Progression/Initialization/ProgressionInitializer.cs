// Assets/_MythHunter/Code/GameLogic/Progression/Initialization/ProgressionInitializer.cs

using RPGFramework.Progression;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.GameLogic.Progression.Data;
using System;

namespace MythHunter.GameLogic.Progression.Initialization
{
    /// <summary>
    /// Initializes and populates progression systems on game start.
    /// Registers level tables, stat growth formulas, and unlocks.
    ///
    /// Usage:
    /// - Inject into game initialization flow (e.g., GameBootstrap, StartupSequence)
    /// - Call Initialize() during game startup
    /// - All progression data will be populated in Framework services
    ///
    /// Pattern:
    /// - Dependency injection for Framework services
    /// - Logging for tracking initialization
    /// - Extensible for custom progression data
    /// </summary>
    public class ProgressionInitializer
    {
        private readonly IStatGrowthFormula _statGrowthFormula;
        private readonly IUnlockManager _unlockManager;
        private readonly IMythLogger _logger;

        private const string LogCategory = "ProgressionInit";

        private bool _isInitialized = false;

        [Inject]
        public ProgressionInitializer(
            IStatGrowthFormula statGrowthFormula,
            IUnlockManager unlockManager,
            IMythLogger logger)
        {
            _statGrowthFormula = statGrowthFormula;
            _unlockManager = unlockManager;
            _logger = logger;
        }

        /// <summary>
        /// Initializes all progression systems with sample data.
        /// Should be called once during game startup.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                _logger.LogWarning("ProgressionInitializer already initialized, skipping", LogCategory);
                return;
            }

            _logger.LogInfo("Initializing progression systems...", LogCategory);

            try
            {
                // Register stat growth formulas
                RegisterStatGrowth();

                // Register unlocks
                RegisterUnlocks();

                _isInitialized = true;

                _logger.LogInfo(
                    "Progression systems initialized successfully.",
                    LogCategory
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize progression systems: {ex.Message}", LogCategory);
                throw;
            }
        }

        /// <summary>
        /// Registers all stat growth formulas.
        /// Uses sample stat growth data from SampleStatGrowth.
        /// </summary>
        private void RegisterStatGrowth()
        {
            _logger.LogInfo("Registering stat growth formulas...", LogCategory);

            // Register all unique stat growth data
            var allStatGrowth = SampleStatGrowth.GetAllUniqueStatGrowth();

            foreach (var statGrowth in allStatGrowth)
            {
                try
                {
                    _statGrowthFormula.RegisterStatGrowth(statGrowth);
                    _logger.LogDebug(
                        $"Registered stat growth: '{statGrowth.StatId}' " +
                        $"(Base: {statGrowth.BaseValue}, Growth: {statGrowth.GrowthPerLevel}, " +
                        $"Curve: {statGrowth.CurveType})",
                        LogCategory
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to register stat growth '{statGrowth.StatId}': {ex.Message}", LogCategory);
                }
            }

            _logger.LogInfo($"Registered {allStatGrowth.Count} unique stat growth formulas", LogCategory);
        }

        /// <summary>
        /// Registers all unlocks from SampleUnlocks.
        /// </summary>
        private void RegisterUnlocks()
        {
            _logger.LogInfo("Registering unlocks...", LogCategory);

            var allUnlocks = SampleUnlocks.GetAllUnlocks();

            foreach (var unlock in allUnlocks)
            {
                try
                {
                    _unlockManager.RegisterUnlock(unlock);
                    _logger.LogDebug(
                        $"Registered unlock: '{unlock.UnlockId}' - {unlock.Name} " +
                        $"(Type: {unlock.UnlockType}, Level: {unlock.RequiredLevel})",
                        LogCategory
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to register unlock '{unlock.UnlockId}': {ex.Message}", LogCategory);
                }
            }

            var unlockCounts = SampleUnlocks.GetUnlockCountsByType();
            _logger.LogInfo(
                $"Registered {unlockCounts["Total"]} unlocks: " +
                $"{unlockCounts["Ability"]} abilities, " +
                $"{unlockCounts["Feature"]} features, " +
                $"{unlockCounts["Equipment"]} equipment, " +
                $"{unlockCounts["Passive"]} passives",
                LogCategory
            );
        }

        /// <summary>
        /// Register custom stat growth formula at runtime.
        /// Useful for mod support, dynamic content, or archetype customization.
        /// </summary>
        /// <param name="statGrowthData">Stat growth data to register</param>
        public void RegisterCustomStatGrowth(StatGrowthData statGrowthData)
        {
            if (string.IsNullOrEmpty(statGrowthData.StatId))
            {
                _logger.LogWarning("Cannot register stat growth with empty StatId", LogCategory);
                return;
            }

            try
            {
                _statGrowthFormula.RegisterStatGrowth(statGrowthData);
                _logger.LogInfo(
                    $"Registered custom stat growth: '{statGrowthData.StatId}' " +
                    $"(Base: {statGrowthData.BaseValue}, Growth: {statGrowthData.GrowthPerLevel})",
                    LogCategory
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to register custom stat growth '{statGrowthData.StatId}': {ex.Message}", LogCategory);
            }
        }

        /// <summary>
        /// Register custom unlock at runtime.
        /// Useful for dynamic content, quests, or achievements.
        /// </summary>
        /// <param name="unlockData">Unlock data to register</param>
        public void RegisterCustomUnlock(UnlockData unlockData)
        {
            if (string.IsNullOrEmpty(unlockData.UnlockId))
            {
                _logger.LogWarning("Cannot register unlock with empty UnlockId", LogCategory);
                return;
            }

            try
            {
                _unlockManager.RegisterUnlock(unlockData);
                _logger.LogInfo(
                    $"Registered custom unlock: '{unlockData.UnlockId}' - {unlockData.Name} " +
                    $"(Type: {unlockData.UnlockType}, Level: {unlockData.RequiredLevel})",
                    LogCategory
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to register custom unlock '{unlockData.UnlockId}': {ex.Message}", LogCategory);
            }
        }

        /// <summary>
        /// Logs all registered stat growth formulas for debugging.
        /// </summary>
        public void LogRegisteredStatGrowth()
        {
            var allStatGrowth = SampleStatGrowth.GetAllUniqueStatGrowth();

            _logger.LogInfo($"=== Registered Stat Growth ({allStatGrowth.Count}) ===", LogCategory);

            foreach (var statGrowth in allStatGrowth)
            {
                // Calculate preview values at key levels
                float level1 = SampleStatGrowth.CalculateStatPreview(statGrowth, 1);
                float level50 = SampleStatGrowth.CalculateStatPreview(statGrowth, 50);
                float level100 = SampleStatGrowth.CalculateStatPreview(statGrowth, 100);

                _logger.LogInfo(
                    $"  [{statGrowth.StatId}] Curve: {statGrowth.CurveType}, " +
                    $"Level 1: {level1:F1}, Level 50: {level50:F1}, Level 100: {level100:F1}",
                    LogCategory
                );
            }
        }

        /// <summary>
        /// Logs all registered unlocks grouped by type.
        /// </summary>
        public void LogRegisteredUnlocks()
        {
            var unlockCounts = SampleUnlocks.GetUnlockCountsByType();

            _logger.LogInfo($"=== Registered Unlocks ({unlockCounts["Total"]}) ===", LogCategory);

            string[] types = { "Ability", "Feature", "Equipment", "Passive" };

            foreach (var type in types)
            {
                var unlocksOfType = SampleUnlocks.GetUnlocksByType(type);
                if (unlocksOfType.Count > 0)
                {
                    _logger.LogInfo($"  {type} Unlocks ({unlocksOfType.Count}):", LogCategory);

                    foreach (var unlock in unlocksOfType)
                    {
                        _logger.LogInfo(
                            $"    Level {unlock.RequiredLevel}: [{unlock.UnlockId}] {unlock.Name}",
                            LogCategory
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Logs level table summary for debugging.
        /// </summary>
        public void LogLevelTableSummary(ILevelTable levelTable, string tableName)
        {
            _logger.LogInfo($"=== Level Table: {tableName} ===", LogCategory);

            int[] levelsToShow = { 1, 5, 10, 20, 30, 40, 50, 75, 100 };
            string summary = SampleLevelTables.GetLevelTableSummary(levelTable, levelsToShow);

            _logger.LogInfo(summary, LogCategory);
        }
    }
}
