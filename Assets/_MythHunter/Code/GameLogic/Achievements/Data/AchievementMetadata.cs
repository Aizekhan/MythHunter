// Assets/_MythHunter/Code/GameLogic/Achievements/Data/AchievementMetadata.cs

using System;

namespace MythHunter.GameLogic.Achievements
{
    /// <summary>
    /// MythHunter-specific achievement metadata struct.
    /// Contains UI/gameplay metadata that extends Framework Achievement data.
    ///
    /// This struct holds all visual, audio, and gameplay-specific information
    /// that is not part of the core achievement tracking system.
    /// Framework manages: Id, Name, Description, Criteria, Progress, Unlocks
    /// This struct manages: Icon, Display info, Notifications, Category, Sorting
    /// </summary>
    [Serializable]
    public struct AchievementMetadata
    {
        /// <summary>
        /// Path to achievement icon in Resources folder.
        /// Example: "Textures/Achievements/first_blood", "Textures/Achievements/level_10"
        /// Used for achievement icon display in UI.
        /// </summary>
        public string IconPath;

        /// <summary>
        /// Display name of the achievement (may differ from Framework Name).
        /// Example: "First Blood!", "Experienced Adventurer"
        /// Used for UI display with custom formatting.
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// Message shown when achievement is unlocked.
        /// Example: "You've slain your first enemy!", "You've reached level 10!"
        /// Displayed in notification popup.
        /// </summary>
        public string UnlockMessage;

        /// <summary>
        /// Achievement category for organizing and filtering.
        /// Standard categories: "Combat", "Progression", "Exploration", "Social", "Collection", "Crafting"
        /// Custom categories allowed for game-specific types.
        /// Used for UI sorting, achievement log filtering, statistics.
        /// </summary>
        public string Category;

        /// <summary>
        /// Sort order for UI display within category.
        /// Lower numbers appear first. Allows manual ordering of achievements.
        /// </summary>
        public int SortOrder;

        /// <summary>
        /// Whether to show popup notification when unlocked.
        /// True for most achievements, false for hidden/meta achievements.
        /// </summary>
        public bool ShowNotification;

        /// <summary>
        /// Path to notification sound effect in Resources folder.
        /// Example: "Audio/SFX/achievement_unlock", "Audio/SFX/achievement_rare"
        /// Optional - may be empty for silent achievements.
        /// </summary>
        public string NotificationSound;

        // ============================================================
        // FACTORY METHODS
        // ============================================================

        /// <summary>
        /// Creates basic achievement metadata with required fields only.
        /// Other fields default to empty/false.
        /// </summary>
        /// <param name="iconPath">Achievement icon path</param>
        /// <param name="displayName">Display name</param>
        /// <param name="unlockMessage">Unlock message</param>
        /// <param name="category">Achievement category</param>
        /// <returns>Basic achievement metadata</returns>
        public static AchievementMetadata Create(
            string iconPath,
            string displayName,
            string unlockMessage,
            string category = "General")
        {
            return new AchievementMetadata
            {
                IconPath = iconPath,
                DisplayName = displayName,
                UnlockMessage = unlockMessage,
                Category = category,
                SortOrder = 0,
                ShowNotification = true,
                NotificationSound = "Audio/SFX/achievement_unlock"
            };
        }

        /// <summary>
        /// Creates fully-specified achievement metadata with all fields.
        /// </summary>
        /// <param name="iconPath">Achievement icon path</param>
        /// <param name="displayName">Display name</param>
        /// <param name="unlockMessage">Unlock message</param>
        /// <param name="category">Achievement category</param>
        /// <param name="sortOrder">Sort order</param>
        /// <param name="showNotification">Show notification flag</param>
        /// <param name="notificationSound">Notification sound path</param>
        /// <returns>Complete achievement metadata</returns>
        public static AchievementMetadata CreateFull(
            string iconPath,
            string displayName,
            string unlockMessage,
            string category,
            int sortOrder,
            bool showNotification,
            string notificationSound)
        {
            return new AchievementMetadata
            {
                IconPath = iconPath,
                DisplayName = displayName,
                UnlockMessage = unlockMessage,
                Category = category,
                SortOrder = sortOrder,
                ShowNotification = showNotification,
                NotificationSound = notificationSound
            };
        }

        /// <summary>
        /// Creates achievement metadata for combat achievements.
        /// Combat achievements use combat category and combat-specific sound.
        /// </summary>
        /// <param name="iconPath">Achievement icon path</param>
        /// <param name="displayName">Display name</param>
        /// <param name="unlockMessage">Unlock message</param>
        /// <param name="sortOrder">Sort order, default 0</param>
        /// <returns>Combat achievement metadata</returns>
        public static AchievementMetadata CreateCombatAchievement(
            string iconPath,
            string displayName,
            string unlockMessage,
            int sortOrder = 0)
        {
            return new AchievementMetadata
            {
                IconPath = iconPath,
                DisplayName = displayName,
                UnlockMessage = unlockMessage,
                Category = "Combat",
                SortOrder = sortOrder,
                ShowNotification = true,
                NotificationSound = "Audio/SFX/achievement_combat"
            };
        }

        /// <summary>
        /// Creates achievement metadata for progression achievements.
        /// Progression achievements use progression category and level-up sound.
        /// </summary>
        /// <param name="iconPath">Achievement icon path</param>
        /// <param name="displayName">Display name</param>
        /// <param name="unlockMessage">Unlock message</param>
        /// <param name="sortOrder">Sort order, default 0</param>
        /// <returns>Progression achievement metadata</returns>
        public static AchievementMetadata CreateProgressionAchievement(
            string iconPath,
            string displayName,
            string unlockMessage,
            int sortOrder = 0)
        {
            return new AchievementMetadata
            {
                IconPath = iconPath,
                DisplayName = displayName,
                UnlockMessage = unlockMessage,
                Category = "Progression",
                SortOrder = sortOrder,
                ShowNotification = true,
                NotificationSound = "Audio/SFX/achievement_progression"
            };
        }

        /// <summary>
        /// Creates achievement metadata for collection achievements.
        /// Collection achievements use collection category and collection sound.
        /// </summary>
        /// <param name="iconPath">Achievement icon path</param>
        /// <param name="displayName">Display name</param>
        /// <param name="unlockMessage">Unlock message</param>
        /// <param name="sortOrder">Sort order, default 0</param>
        /// <returns>Collection achievement metadata</returns>
        public static AchievementMetadata CreateCollectionAchievement(
            string iconPath,
            string displayName,
            string unlockMessage,
            int sortOrder = 0)
        {
            return new AchievementMetadata
            {
                IconPath = iconPath,
                DisplayName = displayName,
                UnlockMessage = unlockMessage,
                Category = "Collection",
                SortOrder = sortOrder,
                ShowNotification = true,
                NotificationSound = "Audio/SFX/achievement_collection"
            };
        }

        /// <summary>
        /// Creates achievement metadata for exploration achievements.
        /// Exploration achievements use exploration category and discovery sound.
        /// </summary>
        /// <param name="iconPath">Achievement icon path</param>
        /// <param name="displayName">Display name</param>
        /// <param name="unlockMessage">Unlock message</param>
        /// <param name="sortOrder">Sort order, default 0</param>
        /// <returns>Exploration achievement metadata</returns>
        public static AchievementMetadata CreateExplorationAchievement(
            string iconPath,
            string displayName,
            string unlockMessage,
            int sortOrder = 0)
        {
            return new AchievementMetadata
            {
                IconPath = iconPath,
                DisplayName = displayName,
                UnlockMessage = unlockMessage,
                Category = "Exploration",
                SortOrder = sortOrder,
                ShowNotification = true,
                NotificationSound = "Audio/SFX/achievement_exploration"
            };
        }

        /// <summary>
        /// Creates achievement metadata for social achievements.
        /// Social achievements use social category and social sound.
        /// </summary>
        /// <param name="iconPath">Achievement icon path</param>
        /// <param name="displayName">Display name</param>
        /// <param name="unlockMessage">Unlock message</param>
        /// <param name="sortOrder">Sort order, default 0</param>
        /// <returns>Social achievement metadata</returns>
        public static AchievementMetadata CreateSocialAchievement(
            string iconPath,
            string displayName,
            string unlockMessage,
            int sortOrder = 0)
        {
            return new AchievementMetadata
            {
                IconPath = iconPath,
                DisplayName = displayName,
                UnlockMessage = unlockMessage,
                Category = "Social",
                SortOrder = sortOrder,
                ShowNotification = true,
                NotificationSound = "Audio/SFX/achievement_social"
            };
        }

        /// <summary>
        /// Creates achievement metadata for hidden achievements.
        /// Hidden achievements don't show notifications until unlocked.
        /// </summary>
        /// <param name="iconPath">Achievement icon path</param>
        /// <param name="displayName">Display name</param>
        /// <param name="unlockMessage">Unlock message</param>
        /// <param name="category">Achievement category</param>
        /// <returns>Hidden achievement metadata</returns>
        public static AchievementMetadata CreateHiddenAchievement(
            string iconPath,
            string displayName,
            string unlockMessage,
            string category = "Hidden")
        {
            return new AchievementMetadata
            {
                IconPath = iconPath,
                DisplayName = displayName,
                UnlockMessage = unlockMessage,
                Category = category,
                SortOrder = 999,
                ShowNotification = false,
                NotificationSound = string.Empty
            };
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>
        /// Checks if achievement metadata has all required fields.
        /// Required: IconPath, DisplayName, UnlockMessage
        /// </summary>
        /// <returns>True if metadata is complete</returns>
        public readonly bool IsValid()
        {
            return !string.IsNullOrEmpty(IconPath) &&
                   !string.IsNullOrEmpty(DisplayName) &&
                   !string.IsNullOrEmpty(UnlockMessage);
        }

        /// <summary>
        /// Checks if achievement has notification sound configured.
        /// </summary>
        /// <returns>True if NotificationSound is not empty</returns>
        public readonly bool HasNotificationSound()
        {
            return !string.IsNullOrEmpty(NotificationSound);
        }

        /// <summary>
        /// Gets category display name (with proper capitalization).
        /// </summary>
        /// <returns>Formatted category name</returns>
        public readonly string GetCategoryDisplayName()
        {
            return Category switch
            {
                "Combat" => "Combat Achievements",
                "Progression" => "Character Progression",
                "Collection" => "Collection & Gathering",
                "Exploration" => "World Exploration",
                "Social" => "Social & Dialogue",
                "Crafting" => "Crafting & Creation",
                "Hidden" => "Hidden Achievements",
                _ => Category
            };
        }

        /// <summary>
        /// Checks if this is a high-priority notification achievement.
        /// High priority achievements should show prominent notifications.
        /// </summary>
        /// <returns>True if notification should be prominent</returns>
        public readonly bool IsHighPriorityNotification()
        {
            return ShowNotification && SortOrder < 100;
        }
    }
}
