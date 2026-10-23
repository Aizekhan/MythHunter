// Assets/_MythHunter/Code/GameLogic/Achievements/Components/MythHunterAchievementComponent.cs

using System;
using System.Collections.Generic;
using RPGFramework.ECS;

namespace MythHunter.GameLogic.Achievements
{
    /// <summary>
    /// MythHunter-specific achievement metadata component.
    /// Wraps Framework AchievementData with MythHunter UI/gameplay metadata.
    ///
    /// Design:
    /// - Dual-component pattern: Framework manages achievement state/progress
    /// - This component adds MythHunter-specific UI and gameplay metadata
    /// - Follows same pattern as Dialogue + MythHunterDialogueComponent
    /// - Metadata includes icons, display info, notifications, categories
    /// </summary>
    [Serializable]
    public struct MythHunterAchievementComponent : IComponent
    {
        /// <summary>
        /// Dictionary mapping achievement IDs to their MythHunter-specific metadata.
        /// Holds UI/gameplay data like icons, unlock messages, notification settings.
        /// Framework AchievementData holds the actual achievement progress and unlocks.
        /// </summary>
        public Dictionary<string, AchievementMetadata> Metadata;

        /// <summary>
        /// Factory method to create an empty achievement component.
        /// </summary>
        /// <returns>Initialized achievement component with empty metadata dictionary</returns>
        public static MythHunterAchievementComponent Create()
        {
            return new MythHunterAchievementComponent
            {
                Metadata = new Dictionary<string, AchievementMetadata>()
            };
        }

        /// <summary>
        /// Factory method to create achievement component with pre-populated metadata.
        /// </summary>
        /// <param name="initialMetadata">Initial achievement metadata dictionary</param>
        /// <returns>Initialized achievement component with provided metadata</returns>
        public static MythHunterAchievementComponent Create(Dictionary<string, AchievementMetadata> initialMetadata)
        {
            return new MythHunterAchievementComponent
            {
                Metadata = initialMetadata ?? new Dictionary<string, AchievementMetadata>()
            };
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>
        /// Gets metadata for a specific achievement.
        /// </summary>
        /// <param name="achievementId">Achievement ID</param>
        /// <returns>Achievement metadata, or default if not found</returns>
        public readonly AchievementMetadata GetMetadata(string achievementId)
        {
            if (Metadata != null && Metadata.TryGetValue(achievementId, out var metadata))
            {
                return metadata;
            }
            return default;
        }

        /// <summary>
        /// Checks if achievement has metadata registered.
        /// </summary>
        /// <param name="achievementId">Achievement ID</param>
        /// <returns>True if metadata exists</returns>
        public readonly bool HasMetadata(string achievementId)
        {
            return Metadata != null && Metadata.ContainsKey(achievementId);
        }

        /// <summary>
        /// Adds or updates metadata for an achievement.
        /// </summary>
        /// <param name="achievementId">Achievement ID</param>
        /// <param name="metadata">Achievement metadata</param>
        public void SetMetadata(string achievementId, AchievementMetadata metadata)
        {
            if (Metadata == null)
                Metadata = new Dictionary<string, AchievementMetadata>();

            Metadata[achievementId] = metadata;
        }

        /// <summary>
        /// Removes metadata for an achievement.
        /// </summary>
        /// <param name="achievementId">Achievement ID</param>
        /// <returns>True if metadata was removed, false if not found</returns>
        public bool RemoveMetadata(string achievementId)
        {
            if (Metadata != null && Metadata.ContainsKey(achievementId))
            {
                Metadata.Remove(achievementId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all achievements in a specific category.
        /// </summary>
        /// <param name="category">Achievement category to filter</param>
        /// <returns>List of achievement IDs matching category</returns>
        public readonly List<string> GetAchievementsByCategory(string category)
        {
            var result = new List<string>();

            if (Metadata == null)
                return result;

            foreach (var kvp in Metadata)
            {
                if (kvp.Value.Category == category)
                    result.Add(kvp.Key);
            }

            return result;
        }

        /// <summary>
        /// Gets all achievements that should show notifications.
        /// Useful for filtering which achievements to display popups for.
        /// </summary>
        /// <returns>List of achievement IDs with notifications enabled</returns>
        public readonly List<string> GetNotifiableAchievements()
        {
            var result = new List<string>();

            if (Metadata == null)
                return result;

            foreach (var kvp in Metadata)
            {
                if (kvp.Value.ShowNotification)
                    result.Add(kvp.Key);
            }

            return result;
        }

        /// <summary>
        /// Gets all achievements sorted by sort order within a category.
        /// </summary>
        /// <param name="category">Achievement category to filter</param>
        /// <returns>List of achievement IDs sorted by SortOrder</returns>
        public readonly List<string> GetAchievementsByCategorySorted(string category)
        {
            var achievementsInCategory = new List<(string id, int sortOrder)>();

            if (Metadata == null)
                return new List<string>();

            foreach (var kvp in Metadata)
            {
                if (kvp.Value.Category == category)
                    achievementsInCategory.Add((kvp.Key, kvp.Value.SortOrder));
            }

            // Sort by SortOrder
            achievementsInCategory.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));

            return achievementsInCategory.ConvertAll(x => x.id);
        }

        /// <summary>
        /// Gets all unique categories from registered achievements.
        /// </summary>
        /// <returns>List of unique category names</returns>
        public readonly List<string> GetAllCategories()
        {
            var categories = new HashSet<string>();

            if (Metadata == null)
                return new List<string>();

            foreach (var kvp in Metadata)
            {
                if (!string.IsNullOrEmpty(kvp.Value.Category))
                    categories.Add(kvp.Value.Category);
            }

            return new List<string>(categories);
        }

        /// <summary>
        /// Gets all registered achievement IDs with metadata.
        /// </summary>
        /// <returns>List of achievement IDs</returns>
        public readonly List<string> GetAllAchievementIds()
        {
            if (Metadata == null)
                return new List<string>();

            return new List<string>(Metadata.Keys);
        }

        /// <summary>
        /// Gets count of achievements in a specific category.
        /// </summary>
        /// <param name="category">Achievement category</param>
        /// <returns>Number of achievements in category</returns>
        public readonly int GetCategoryCount(string category)
        {
            if (Metadata == null)
                return 0;

            int count = 0;
            foreach (var kvp in Metadata)
            {
                if (kvp.Value.Category == category)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Gets all achievements with high-priority notifications.
        /// High-priority achievements should show prominent popups.
        /// </summary>
        /// <returns>List of high-priority achievement IDs</returns>
        public readonly List<string> GetHighPriorityAchievements()
        {
            var result = new List<string>();

            if (Metadata == null)
                return result;

            foreach (var kvp in Metadata)
            {
                if (kvp.Value.IsHighPriorityNotification())
                    result.Add(kvp.Key);
            }

            return result;
        }

        /// <summary>
        /// Checks if component has valid metadata dictionary.
        /// </summary>
        /// <returns>True if metadata initialized and not null</returns>
        public readonly bool IsValid()
        {
            return Metadata != null;
        }

        /// <summary>
        /// Gets total number of registered achievements.
        /// </summary>
        /// <returns>Count of achievements with metadata</returns>
        public readonly int GetTotalCount()
        {
            return Metadata?.Count ?? 0;
        }
    }
}
