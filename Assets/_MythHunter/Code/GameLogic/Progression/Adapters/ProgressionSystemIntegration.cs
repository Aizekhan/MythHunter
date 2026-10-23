// Assets/_MythHunter/Code/GameLogic/Progression/Adapters/ProgressionSystemIntegration.cs

using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.GameLogic.Progression.Components;
using RPGFramework.ECS;
using RPGFramework.Progression;
using System.Collections.Generic;

namespace MythHunter.GameLogic.Progression.Adapters
{
    /// <summary>
    /// Integration adapter between Framework Progression System and MythHunter.
    /// Bridges IProgressionTracker, IStatGrowthFormula, IUnlockManager with game-layer components.
    ///
    /// Responsibilities:
    /// - XP gain and level-up operations
    /// - Stat growth on level-up
    /// - Unlock management (abilities, features)
    /// - Component queries and updates
    ///
    /// Pattern:
    /// - Dependency injection for Framework services
    /// - Component-based state management
    /// - Event publication for UI/audio (via callbacks)
    ///
    /// Usage:
    /// - AddExperience(entityId, xpAmount, ...): Award XP and handle level-ups
    /// - GetProgressionData(entityId): Query current progression state
    /// - AchieveUnlock(entityId, unlockId): Grant unlock to entity
    /// - ApplyStatGrowth(entityId, fromLevel, toLevel): Apply stat increases
    ///
    /// Dependencies:
    /// - IEntityManager: Component storage
    /// - IProgressionTracker: XP/level calculations
    /// - IStatGrowthFormula: Stat calculations on level-up
    /// - IUnlockManager: Unlock tracking
    /// - IMythLogger: Logging
    /// </summary>
    public class ProgressionSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly IProgressionTracker _progressionTracker;
        private readonly IStatGrowthFormula _statGrowthFormula;
        private readonly IUnlockManager _unlockManager;
        private readonly IMythLogger _logger;

        private const string LogCategory = "Progression";

        [Inject]
        public ProgressionSystemIntegration(
            IEntityManager entityManager,
            IProgressionTracker progressionTracker,
            IStatGrowthFormula statGrowthFormula,
            IUnlockManager unlockManager,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _progressionTracker = progressionTracker;
            _statGrowthFormula = statGrowthFormula;
            _unlockManager = unlockManager;
            _logger = logger;
        }

        // ============================================================
        // XP AND LEVELING
        // ============================================================

        /// <summary>
        /// Add experience to entity and handle level-ups.
        /// Updates component state, applies stat growth, grants unlocks.
        /// </summary>
        /// <param name="entityId">Entity gaining XP</param>
        /// <param name="xpAmount">Amount of XP to add</param>
        /// <param name="xpSource">Source of XP (Kill, Quest, Event, etc.)</param>
        /// <param name="currentTime">Current game time (for tracking)</param>
        /// <param name="onLevelUp">Callback when level-up occurs (for UI/audio/VFX)</param>
        /// <param name="onStatIncrease">Callback when stats increase (statId, oldValue, newValue)</param>
        /// <param name="onUnlockAchieved">Callback when unlock is granted</param>
        /// <returns>Progression result with level changes</returns>
        public ProgressionResult AddExperience(
            int entityId,
            long xpAmount,
            string xpSource,
            float currentTime,
            System.Action<int, int, int> onLevelUp = null, // entityId, oldLevel, newLevel
            System.Action<int, string, float, float> onStatIncrease = null, // entityId, statId, oldValue, newValue
            System.Action<int, string> onUnlockAchieved = null) // entityId, unlockId
        {
            if (!TryGetProgressionComponent(entityId, out var component))
            {
                _logger.LogWarning($"No progression component found for entity {entityId}", LogCategory);
                return default;
            }

            int oldLevel = component.GetLevel();

            // Add XP via Framework tracker
            ProgressionResult result = _progressionTracker.AddExperience(entityId, xpAmount);

            // Update component with new XP data
            component.UpdateExperienceData(result.ExperienceData);

            // Record XP source
            if (xpSource == "Kill")
                component.RecordKillXP(xpAmount, currentTime);
            else if (xpSource == "Quest")
                component.RecordQuestXP(xpAmount, currentTime);
            else
                component.LastXPGainTime = currentTime;

            // Save component
            _entityManager.AddComponent(entityId, component);

            _logger.LogDebug(
                $"Entity {entityId} gained {xpAmount} XP from {xpSource}. " +
                $"Level: {oldLevel} -> {result.NewLevel}, Total XP: {result.ExperienceData.TotalXP}",
                LogCategory
            );

            // Handle level-up
            if (result.LeveledUp)
            {
                _logger.LogInfo(
                    $"Entity {entityId} leveled up! {oldLevel} -> {result.NewLevel} (+{result.LevelsGained} levels)",
                    LogCategory
                );

                // Apply stat growth
                ApplyStatGrowth(entityId, oldLevel, result.NewLevel, onStatIncrease);

                // Grant unlocks
                GrantUnlocksForLevel(entityId, oldLevel, result.NewLevel, onUnlockAchieved);

                // Invoke level-up callback
                onLevelUp?.Invoke(entityId, oldLevel, result.NewLevel);
            }

            return result;
        }

        /// <summary>
        /// Set entity level directly (for testing, admin commands, scaling).
        /// </summary>
        public void SetLevel(int entityId, int level, float currentTime)
        {
            if (!TryGetProgressionComponent(entityId, out var component))
            {
                _logger.LogWarning($"No progression component found for entity {entityId}", LogCategory);
                return;
            }

            int oldLevel = component.GetLevel();

            // Set level via Framework tracker
            _progressionTracker.SetLevel(entityId, level);

            // Get updated XP data
            ExperienceData newData = _progressionTracker.GetExperienceData(entityId);
            component.UpdateExperienceData(newData);

            // Apply stat growth if leveled up
            if (level > oldLevel)
            {
                ApplyStatGrowth(entityId, oldLevel, level, null);
                GrantUnlocksForLevel(entityId, oldLevel, level, null);
            }

            component.LastXPGainTime = currentTime;
            _entityManager.AddComponent(entityId, component);

            _logger.LogInfo($"Entity {entityId} level set to {level} (was {oldLevel})", LogCategory);
        }

        // ============================================================
        // STAT GROWTH
        // ============================================================

        /// <summary>
        /// Apply stat growth when leveling up.
        /// Calculates new stat values and updates component.
        /// </summary>
        public void ApplyStatGrowth(
            int entityId,
            int fromLevel,
            int toLevel,
            System.Action<int, string, float, float> onStatIncrease = null)
        {
            if (!TryGetProgressionComponent(entityId, out var component))
                return;

            // Get stat changes from growth formula
            Dictionary<string, float> statChanges = _statGrowthFormula.GetStatChangesForLevelUp(fromLevel, toLevel);

            if (statChanges.Count == 0)
            {
                _logger.LogDebug($"No stat growth configured for level {fromLevel} -> {toLevel}", LogCategory);
                return;
            }

            // Apply each stat change
            foreach (var kvp in statChanges)
            {
                string statId = kvp.Key;
                float newValue = kvp.Value;
                float oldValue = component.GetStatValue(statId);

                component.SetStatValue(statId, newValue);

                _logger.LogDebug(
                    $"Entity {entityId} stat '{statId}' increased: {oldValue:F1} -> {newValue:F1} (level {toLevel})",
                    LogCategory
                );

                onStatIncrease?.Invoke(entityId, statId, oldValue, newValue);
            }

            _entityManager.AddComponent(entityId, component);

            _logger.LogInfo($"Applied stat growth to entity {entityId} for level {toLevel} ({statChanges.Count} stats)", LogCategory);
        }

        /// <summary>
        /// Get stat value at specific level (without modifying entity).
        /// </summary>
        public float GetStatValueAtLevel(string statId, int level)
        {
            return _statGrowthFormula.CalculateStatAtLevel(statId, level);
        }

        // ============================================================
        // UNLOCKS
        // ============================================================

        /// <summary>
        /// Grant unlocks when leveling up.
        /// Checks unlock manager for available unlocks and adds them to component.
        /// </summary>
        public void GrantUnlocksForLevel(
            int entityId,
            int fromLevel,
            int toLevel,
            System.Action<int, string> onUnlockAchieved = null)
        {
            if (!TryGetProgressionComponent(entityId, out var component))
                return;

            // Get unlocks for all levels between fromLevel and toLevel
            for (int level = fromLevel + 1; level <= toLevel; level++)
            {
                List<UnlockData> unlocks = _unlockManager.GetUnlocksForLevel(level);

                foreach (var unlock in unlocks)
                {
                    if (!component.HasUnlock(unlock.UnlockId))
                    {
                        component.AddUnlock(unlock.UnlockId);
                        _unlockManager.AchieveUnlock(entityId, unlock.UnlockId);

                        _logger.LogInfo(
                            $"Entity {entityId} unlocked '{unlock.Name}' ({unlock.UnlockType}) at level {level}",
                            LogCategory
                        );

                        onUnlockAchieved?.Invoke(entityId, unlock.UnlockId);
                    }
                }
            }

            _entityManager.AddComponent(entityId, component);
        }

        /// <summary>
        /// Manually grant unlock to entity (for quests, achievements, etc.).
        /// </summary>
        public void AchieveUnlock(int entityId, string unlockId)
        {
            if (!TryGetProgressionComponent(entityId, out var component))
            {
                _logger.LogWarning($"No progression component found for entity {entityId}", LogCategory);
                return;
            }

            if (component.HasUnlock(unlockId))
            {
                _logger.LogDebug($"Entity {entityId} already has unlock '{unlockId}'", LogCategory);
                return;
            }

            component.AddUnlock(unlockId);
            _unlockManager.AchieveUnlock(entityId, unlockId);
            _entityManager.AddComponent(entityId, component);

            _logger.LogInfo($"Entity {entityId} manually unlocked '{unlockId}'", LogCategory);
        }

        /// <summary>
        /// Check if entity has unlocked specific content.
        /// </summary>
        public bool HasUnlock(int entityId, string unlockId)
        {
            if (!TryGetProgressionComponent(entityId, out var component))
                return false;

            return component.HasUnlock(unlockId);
        }

        /// <summary>
        /// Get all unlocks for entity.
        /// </summary>
        public List<string> GetUnlockedContent(int entityId)
        {
            if (!TryGetProgressionComponent(entityId, out var component))
                return new List<string>();

            return component.UnlockedContent ?? new List<string>();
        }

        // ============================================================
        // QUERIES
        // ============================================================

        /// <summary>
        /// Get progression data for entity.
        /// </summary>
        public ExperienceData GetProgressionData(int entityId)
        {
            if (TryGetProgressionComponent(entityId, out var component))
                return component.ExperienceData;

            return default;
        }

        /// <summary>
        /// Get current level for entity.
        /// </summary>
        public int GetLevel(int entityId)
        {
            if (TryGetProgressionComponent(entityId, out var component))
                return component.GetLevel();

            return 1; // Default level
        }

        /// <summary>
        /// Get total XP for entity.
        /// </summary>
        public long GetTotalXP(int entityId)
        {
            if (TryGetProgressionComponent(entityId, out var component))
                return component.GetTotalXP();

            return 0;
        }

        /// <summary>
        /// Get all entities with progression components.
        /// </summary>
        public List<int> GetAllProgressingEntities()
        {
            var entities = new List<int>();
            foreach (var entityId in _entityManager.GetEntitiesWith<MythHunterProgressionComponent>())
            {
                entities.Add(entityId);
            }
            return entities;
        }

        /// <summary>
        /// Get all entities at specific level.
        /// </summary>
        public List<int> GetEntitiesAtLevel(int level)
        {
            var entities = new List<int>();
            var allEntities = GetAllProgressingEntities();

            foreach (int entityId in allEntities)
            {
                if (GetLevel(entityId) == level)
                    entities.Add(entityId);
            }

            return entities;
        }

        /// <summary>
        /// Get all entities within level range.
        /// </summary>
        public List<int> GetEntitiesInLevelRange(int minLevel, int maxLevel)
        {
            var entities = new List<int>();
            var allEntities = GetAllProgressingEntities();

            foreach (int entityId in allEntities)
            {
                int level = GetLevel(entityId);
                if (level >= minLevel && level <= maxLevel)
                    entities.Add(entityId);
            }

            return entities;
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        private bool TryGetProgressionComponent(int entityId, out MythHunterProgressionComponent component)
        {
            return _entityManager.TryGetComponent(entityId, out component);
        }
    }
}
