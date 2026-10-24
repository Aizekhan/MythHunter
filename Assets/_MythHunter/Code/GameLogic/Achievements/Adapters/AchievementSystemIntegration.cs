// Assets/_MythHunter/Code/GameLogic/Achievements/Adapters/AchievementSystemIntegration.cs

using System.Collections.Generic;
using RPGFramework.ECS;
using RPGFramework.Achievements;
using RPGFramework.Achievements.Runtime;
using RPGFramework.Achievements.Runtime.Contracts;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.GameLogic.Achievements
{
    /// <summary>
    /// Integration adapter for achievement system operations.
    /// Bridges Framework Achievement System (RPGFramework.Achievements.Runtime) with MythHunter game layer.
    ///
    /// Pattern:
    /// - Greenfield integration (no legacy achievement code to migrate)
    /// - Uses MythHunterAchievementComponent for entity achievement metadata
    /// - Manages achievement progress via Framework IAchievementManager
    /// - Provides unified interface for achievement operations with metadata support
    ///
    /// Key Responsibilities:
    /// - Register achievements with metadata
    /// - Update achievement progress and unlock achievements
    /// - Query unlocked achievements and progress
    /// - Manage metadata lifecycle (add/remove as achievements progress)
    /// - Support retroactive unlocks and notifications
    /// </summary>
    public class AchievementSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly IAchievementManager _achievementManager;
        private readonly IAchievementRegistry _achievementRegistry;
        private readonly IMythLogger _logger;

        private const string LogCategory = "AchievementIntegration";

        [Inject]
        public AchievementSystemIntegration(
            IEntityManager entityManager,
            IAchievementManager achievementManager,
            IAchievementRegistry achievementRegistry,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _achievementManager = achievementManager;
            _achievementRegistry = achievementRegistry;
            _logger = logger;
        }

        // ============================================================
        // ACHIEVEMENT REGISTRATION
        // ============================================================

        /// <summary>
        /// Registers an achievement in the registry with optional metadata.
        /// Ensures MythHunterAchievementComponent exists and updates metadata.
        /// </summary>
        /// <param name="entityId">Entity ID (typically hero)</param>
        /// <param name="achievement">Achievement to register</param>
        /// <param name="metadata">Optional achievement metadata to register</param>
        /// <returns>True if achievement was successfully registered</returns>
        public bool RegisterAchievement(int entityId, IAchievement achievement, AchievementMetadata? metadata = null)
        {
            // Register in Framework registry
            _achievementRegistry.RegisterAchievement(achievement);

            // Ensure achievement component exists
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var achievementComponent))
            {
                achievementComponent = MythHunterAchievementComponent.Create();
            }

            // Add metadata if provided
            if (metadata.HasValue)
            {
                achievementComponent.SetMetadata(achievement.AchievementId, metadata.Value);
            }

            // Update component
            _entityManager.AddComponent(entityId, achievementComponent);

            _logger.LogInfo(
                $"Registered achievement '{achievement.AchievementId}' ({achievement.Name}) for entity {entityId}" +
                (metadata.HasValue ? $" with metadata (category={metadata.Value.Category})" : ""),
                LogCategory
            );

            return true;
        }

        /// <summary>
        /// Registers multiple achievements at once.
        /// Useful for batch initialization or loading achievement definitions.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="achievements">Dictionary of achievement to metadata</param>
        public void RegisterMultipleAchievements(int entityId, Dictionary<IAchievement, AchievementMetadata> achievements)
        {
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var achievementComponent))
            {
                achievementComponent = MythHunterAchievementComponent.Create();
            }

            foreach (var kvp in achievements)
            {
                _achievementRegistry.RegisterAchievement(kvp.Key);
                achievementComponent.SetMetadata(kvp.Key.AchievementId, kvp.Value);
            }

            _entityManager.AddComponent(entityId, achievementComponent);

            _logger.LogInfo(
                $"Registered {achievements.Count} achievements for entity {entityId}",
                LogCategory
            );
        }

        // ============================================================
        // ACHIEVEMENT PROGRESSION
        // ============================================================

        /// <summary>
        /// Updates progress for a specific achievement criterion.
        /// Automatically checks if achievement can be unlocked.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="achievementId">Achievement ID</param>
        /// <param name="criterionId">Criterion ID to update</param>
        /// <param name="currentValue">New current value</param>
        /// <returns>True if progress was updated</returns>
        public bool UpdateProgress(int entityId, string achievementId, string criterionId, int currentValue)
        {
            bool updated = _achievementManager.UpdateCriterionProgress(entityId, achievementId, criterionId, currentValue);

            if (updated)
            {
                _logger.LogInfo(
                    $"Entity {entityId} updated achievement '{achievementId}' criterion '{criterionId}' to {currentValue}",
                    LogCategory
                );
            }
            else
            {
                _logger.LogWarning(
                    $"Failed to update achievement '{achievementId}' criterion '{criterionId}' for entity {entityId}",
                    LogCategory
                );
            }

            return updated;
        }

        /// <summary>
        /// Manually unlocks an achievement for an entity.
        /// Used for special achievements or administrative unlocks.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="achievementId">Achievement ID to unlock</param>
        /// <returns>True if achievement was successfully unlocked</returns>
        public bool UnlockAchievement(int entityId, string achievementId)
        {
            bool unlocked = _achievementManager.UnlockAchievement(entityId, achievementId);

            if (unlocked)
            {
                _logger.LogInfo(
                    $"Entity {entityId} unlocked achievement '{achievementId}'",
                    LogCategory
                );

                // Get metadata for notification
                if (_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
                {
                    var metadata = component.GetMetadata(achievementId);
                    if (metadata.ShowNotification)
                    {
                        _logger.LogInfo(
                            $"Achievement notification: {metadata.UnlockMessage}",
                            LogCategory
                        );
                    }
                }
            }
            else
            {
                _logger.LogWarning(
                    $"Failed to unlock achievement '{achievementId}' for entity {entityId}",
                    LogCategory
                );
            }

            return unlocked;
        }

        // ============================================================
        // RETROACTIVE UNLOCKS
        // ============================================================

        /// <summary>
        /// Check and unlock achievements retroactively based on current entity state.
        /// Useful when achievements are added to the game after players have progressed.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="criterionType">Type of criteria to check (e.g., KillCount, QuestCompleted)</param>
        /// <param name="criterionData">Dictionary of criterion ID to current value</param>
        /// <returns>List of achievement IDs that were unlocked</returns>
        public IReadOnlyList<string> CheckRetroactiveUnlocks(
            int entityId,
            CriterionType criterionType,
            IReadOnlyDictionary<string, int> criterionData)
        {
            var unlockedAchievements = _achievementManager.CheckRetroactiveUnlocks(entityId, criterionType, criterionData);

            if (unlockedAchievements.Count > 0)
            {
                _logger.LogInfo(
                    $"Entity {entityId} retroactively unlocked {unlockedAchievements.Count} achievements of type {criterionType}",
                    LogCategory
                );

                // Log notification for each unlocked achievement
                if (_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
                {
                    foreach (var achievementId in unlockedAchievements)
                    {
                        var metadata = component.GetMetadata(achievementId);
                        if (metadata.ShowNotification)
                        {
                            _logger.LogInfo(
                                $"Retroactive achievement: {metadata.UnlockMessage}",
                                LogCategory
                            );
                        }
                    }
                }
            }

            return unlockedAchievements;
        }

        // ============================================================
        // QUERY METHODS
        // ============================================================

        /// <summary>
        /// Gets progress for a specific achievement.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="achievementId">Achievement ID</param>
        /// <returns>Achievement progress or null if not found</returns>
        public IAchievementProgress GetProgress(int entityId, string achievementId)
        {
            return _achievementManager.GetAchievementProgress(entityId, achievementId);
        }

        /// <summary>
        /// Gets all unlocked achievements for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of unlocked achievement IDs</returns>
        public IReadOnlyList<string> GetUnlockedAchievements(int entityId)
        {
            return _achievementManager.GetUnlockedAchievements(entityId);
        }

        /// <summary>
        /// Gets all achievements in progress for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of achievement progress</returns>
        public IReadOnlyList<IAchievementProgress> GetInProgressAchievements(int entityId)
        {
            return _achievementManager.GetInProgressAchievements(entityId);
        }

        /// <summary>
        /// Checks if an entity has unlocked a specific achievement.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="achievementId">Achievement ID to check</param>
        /// <returns>True if achievement is unlocked</returns>
        public bool IsAchievementUnlocked(int entityId, string achievementId)
        {
            return _achievementManager.IsAchievementUnlocked(entityId, achievementId);
        }

        /// <summary>
        /// Gets achievements by tier (Common, Rare, Epic, etc.).
        /// </summary>
        /// <param name="tier">Achievement tier to filter by</param>
        /// <returns>List of achievement IDs of the specified tier</returns>
        public IReadOnlyList<string> GetAchievementsByTier(AchievementTier tier)
        {
            return _achievementManager.GetAchievementsByTier(tier);
        }

        /// <summary>
        /// Gets achievements in a specific category (Combat, Exploration, etc.).
        /// Filters by metadata category from MythHunterAchievementComponent.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="category">Achievement category to filter</param>
        /// <returns>List of achievement IDs matching category</returns>
        public List<string> GetAchievementsByCategory(int entityId, string category)
        {
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
                return new List<string>();

            return component.GetAchievementsByCategory(category);
        }

        /// <summary>
        /// Gets all unique categories from registered achievements.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of unique category names</returns>
        public List<string> GetAllCategories(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
                return new List<string>();

            return component.GetAllCategories();
        }

        /// <summary>
        /// Gets achievements sorted by sort order within a category.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="category">Achievement category to filter</param>
        /// <returns>List of achievement IDs sorted by SortOrder</returns>
        public List<string> GetAchievementsByCategorySorted(int entityId, string category)
        {
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
                return new List<string>();

            return component.GetAchievementsByCategorySorted(category);
        }

        // ============================================================
        // METADATA MANAGEMENT
        // ============================================================

        /// <summary>
        /// Gets metadata for an achievement.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="achievementId">Achievement ID</param>
        /// <returns>Achievement metadata or default if not found</returns>
        public AchievementMetadata GetAchievementMetadata(int entityId, string achievementId)
        {
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
                return default;

            return component.GetMetadata(achievementId);
        }

        /// <summary>
        /// Registers or updates metadata for an achievement.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="achievementId">Achievement ID</param>
        /// <param name="metadata">Achievement metadata</param>
        /// <returns>True if metadata was registered</returns>
        public bool RegisterAchievementMetadata(int entityId, string achievementId, AchievementMetadata metadata)
        {
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
            {
                component = MythHunterAchievementComponent.Create();
            }

            component.SetMetadata(achievementId, metadata);
            _entityManager.AddComponent(entityId, component);

            _logger.LogInfo($"Registered metadata for achievement '{achievementId}' on entity {entityId}", LogCategory);

            return true;
        }

        /// <summary>
        /// Unregisters metadata for an achievement.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="achievementId">Achievement ID</param>
        /// <returns>True if metadata was removed</returns>
        public bool UnregisterAchievementMetadata(int entityId, string achievementId)
        {
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
                return false;

            bool removed = component.RemoveMetadata(achievementId);

            if (removed)
            {
                _entityManager.AddComponent(entityId, component);
                _logger.LogInfo($"Unregistered metadata for achievement '{achievementId}' on entity {entityId}", LogCategory);
            }

            return removed;
        }

        /// <summary>
        /// Gets all achievements with complete information (metadata).
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Dictionary of achievement ID to metadata</returns>
        public Dictionary<string, AchievementMetadata> GetAllAchievementsWithMetadata(int entityId)
        {
            var result = new Dictionary<string, AchievementMetadata>();

            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
                return result;

            foreach (var achievementId in component.GetAllAchievementIds())
            {
                result[achievementId] = component.GetMetadata(achievementId);
            }

            return result;
        }

        // ============================================================
        // INITIALIZATION
        // ============================================================

        /// <summary>
        /// Initializes achievement tracking for a new entity.
        /// Sets up tracking for all available achievements.
        /// </summary>
        /// <param name="entityId">Entity to initialize</param>
        public void InitializeEntityAchievements(int entityId)
        {
            _achievementManager.InitializeEntityAchievements(entityId);

            // Ensure component exists
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
            {
                component = MythHunterAchievementComponent.Create();
                _entityManager.AddComponent(entityId, component);
            }

            _logger.LogInfo($"Initialized achievement tracking for entity {entityId}", LogCategory);
        }

        // ============================================================
        // STATISTICS
        // ============================================================

        /// <summary>
        /// Gets total number of unlocked achievements for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Number of unlocked achievements</returns>
        public int GetUnlockedAchievementCount(int entityId)
        {
            return GetUnlockedAchievements(entityId).Count;
        }

        /// <summary>
        /// Gets total number of registered achievements.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Number of registered achievements</returns>
        public int GetRegisteredAchievementCount(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
                return 0;

            return component.GetTotalCount();
        }

        /// <summary>
        /// Gets number of achievements in a specific category.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="category">Achievement category</param>
        /// <returns>Number of achievements in category</returns>
        public int GetAchievementCountByCategory(int entityId, string category)
        {
            if (!_entityManager.TryGetComponent<MythHunterAchievementComponent>(entityId, out var component))
                return 0;

            return component.GetCategoryCount(category);
        }

        /// <summary>
        /// Gets completion percentage (unlocked / total).
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Completion percentage (0-100)</returns>
        public float GetCompletionPercentage(int entityId)
        {
            int total = GetRegisteredAchievementCount(entityId);
            if (total == 0)
                return 0f;

            int unlocked = GetUnlockedAchievementCount(entityId);
            return (float)unlocked / total * 100f;
        }
    }
}
