// Assets/_MythHunter/Code/GameLogic/Quests/Data/QuestMetadata.cs

using System;

namespace MythHunter.Quests.Data
{
    /// <summary>
    /// MythHunter-specific quest metadata struct.
    /// Contains UI/gameplay metadata that extends Framework QuestData.
    ///
    /// This struct holds all visual, audio, and gameplay-specific information
    /// that is not part of the core quest progression system.
    /// Framework manages: Id, Title, Description, Objectives, Rewards, Prerequisites, MinLevel, IsRepeatable
    /// This struct manages: Icons, NPCs, Locations, Cutscenes, Audio, Journal Settings, Categories
    /// </summary>
    [Serializable]
    public struct QuestMetadata
    {
        /// <summary>
        /// Path to quest icon/thumbnail in Resources folder.
        /// Example: "Textures/Quests/quest_icon_kill_goblins"
        /// Used for quest log, quest markers, UI displays.
        /// </summary>
        public string IconPath;

        /// <summary>
        /// NPC entity ID or identifier that gives/completes the quest.
        /// Used for quest giver interactions and dialogue triggers.
        /// Example: "npc_merchant_tom", "npc_elder_graves"
        /// </summary>
        public string QuestGiverNpcId;

        /// <summary>
        /// Location/area ID where quest takes place or objective is located.
        /// Used for map markers, quest tracking, navigation hints.
        /// Example: "location_goblin_forest", "location_ancient_ruins"
        /// </summary>
        public string QuestLocationId;

        /// <summary>
        /// Cutscene ID to play when quest is completed.
        /// Optional - may be null for quests without completion cutscene.
        /// Used for narrative moments, important quest endings.
        /// Example: "cutscene_defeat_goblin_boss", "cutscene_artifact_recovered"
        /// </summary>
        public string CompletionCutsceneId;

        /// <summary>
        /// Audio path for sound to play when quest is accepted.
        /// Example: "Audio/SFX/quest_accept_notification"
        /// Provides audio feedback for quest UI interactions.
        /// </summary>
        public string AcceptedSoundPath;

        /// <summary>
        /// Audio path for sound to play when quest is completed.
        /// Example: "Audio/SFX/quest_complete_fanfare"
        /// Provides audio feedback for quest completion celebration.
        /// </summary>
        public string CompletedSoundPath;

        /// <summary>
        /// Whether this quest should be visible in the quest journal.
        /// False for hidden quests, secrets, Easter eggs.
        /// True for tracked quests, story quests, etc.
        /// </summary>
        public bool ShowInJournal;

        /// <summary>
        /// Quest category for organizing and filtering quests.
        /// Standard categories: "Main", "Side", "Repeatable", "Daily"
        /// Custom categories allowed for game-specific types.
        /// Used for UI sorting, quest log filtering, statistics.
        /// </summary>
        public string Category;

        // ============================================================
        // FACTORY METHODS
        // ============================================================

        /// <summary>
        /// Creates basic quest metadata with required fields only.
        /// Other fields default to empty/null.
        /// </summary>
        /// <param name="questGiverNpcId">Quest giver NPC ID</param>
        /// <param name="questLocationId">Quest location ID</param>
        /// <param name="category">Quest category (Main, Side, etc.)</param>
        /// <returns>Basic quest metadata</returns>
        public static QuestMetadata Create(string questGiverNpcId, string questLocationId, string category = "Side")
        {
            return new QuestMetadata
            {
                IconPath = string.Empty,
                QuestGiverNpcId = questGiverNpcId,
                QuestLocationId = questLocationId,
                CompletionCutsceneId = string.Empty,
                AcceptedSoundPath = string.Empty,
                CompletedSoundPath = string.Empty,
                ShowInJournal = true,
                Category = category
            };
        }

        /// <summary>
        /// Creates fully-specified quest metadata with all fields.
        /// </summary>
        /// <param name="iconPath">Icon path</param>
        /// <param name="questGiverNpcId">Quest giver NPC ID</param>
        /// <param name="questLocationId">Quest location ID</param>
        /// <param name="completionCutsceneId">Completion cutscene ID</param>
        /// <param name="acceptedSoundPath">Accepted sound path</param>
        /// <param name="completedSoundPath">Completed sound path</param>
        /// <param name="showInJournal">Show in journal flag</param>
        /// <param name="category">Quest category</param>
        /// <returns>Complete quest metadata</returns>
        public static QuestMetadata CreateFull(
            string iconPath,
            string questGiverNpcId,
            string questLocationId,
            string completionCutsceneId,
            string acceptedSoundPath,
            string completedSoundPath,
            bool showInJournal = true,
            string category = "Side")
        {
            return new QuestMetadata
            {
                IconPath = iconPath,
                QuestGiverNpcId = questGiverNpcId,
                QuestLocationId = questLocationId,
                CompletionCutsceneId = completionCutsceneId,
                AcceptedSoundPath = acceptedSoundPath,
                CompletedSoundPath = completedSoundPath,
                ShowInJournal = showInJournal,
                Category = category
            };
        }

        /// <summary>
        /// Creates quest metadata for main story quest.
        /// Main quests are always shown in journal and have priority.
        /// </summary>
        /// <param name="questGiverNpcId">Quest giver NPC ID</param>
        /// <param name="questLocationId">Quest location ID</param>
        /// <param name="iconPath">Icon path</param>
        /// <returns>Main story quest metadata</returns>
        public static QuestMetadata CreateMainQuest(
            string questGiverNpcId,
            string questLocationId,
            string iconPath = "")
        {
            return new QuestMetadata
            {
                IconPath = iconPath,
                QuestGiverNpcId = questGiverNpcId,
                QuestLocationId = questLocationId,
                CompletionCutsceneId = string.Empty,
                AcceptedSoundPath = string.Empty,
                CompletedSoundPath = string.Empty,
                ShowInJournal = true,
                Category = "Main"
            };
        }

        /// <summary>
        /// Creates quest metadata for side quest.
        /// </summary>
        /// <param name="questGiverNpcId">Quest giver NPC ID</param>
        /// <param name="questLocationId">Quest location ID</param>
        /// <returns>Side quest metadata</returns>
        public static QuestMetadata CreateSideQuest(string questGiverNpcId, string questLocationId)
        {
            return new QuestMetadata
            {
                IconPath = string.Empty,
                QuestGiverNpcId = questGiverNpcId,
                QuestLocationId = questLocationId,
                CompletionCutsceneId = string.Empty,
                AcceptedSoundPath = string.Empty,
                CompletedSoundPath = string.Empty,
                ShowInJournal = true,
                Category = "Side"
            };
        }

        /// <summary>
        /// Creates quest metadata for repeatable quest (daily/weekly).
        /// </summary>
        /// <param name="questGiverNpcId">Quest giver NPC ID</param>
        /// <param name="questLocationId">Quest location ID</param>
        /// <param name="isDaily">Is this a daily quest, default true</param>
        /// <returns>Repeatable quest metadata</returns>
        public static QuestMetadata CreateRepeatable(
            string questGiverNpcId,
            string questLocationId,
            bool isDaily = true)
        {
            return new QuestMetadata
            {
                IconPath = string.Empty,
                QuestGiverNpcId = questGiverNpcId,
                QuestLocationId = questLocationId,
                CompletionCutsceneId = string.Empty,
                AcceptedSoundPath = string.Empty,
                CompletedSoundPath = string.Empty,
                ShowInJournal = true,
                Category = isDaily ? "Daily" : "Repeatable"
            };
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>
        /// Checks if quest has all required metadata fields.
        /// Required: QuestGiverNpcId, QuestLocationId
        /// </summary>
        /// <returns>True if metadata is complete</returns>
        public readonly bool IsValid()
        {
            return !string.IsNullOrEmpty(QuestGiverNpcId) && !string.IsNullOrEmpty(QuestLocationId);
        }

        /// <summary>
        /// Checks if quest has completion audio configured.
        /// </summary>
        /// <returns>True if CompletedSoundPath is not empty</returns>
        public readonly bool HasCompletionSound()
        {
            return !string.IsNullOrEmpty(CompletedSoundPath);
        }

        /// <summary>
        /// Checks if quest has acceptance audio configured.
        /// </summary>
        /// <returns>True if AcceptedSoundPath is not empty</returns>
        public readonly bool HasAcceptanceSound()
        {
            return !string.IsNullOrEmpty(AcceptedSoundPath);
        }

        /// <summary>
        /// Checks if quest has completion cutscene configured.
        /// </summary>
        /// <returns>True if CompletionCutsceneId is not empty</returns>
        public readonly bool HasCompletionCutscene()
        {
            return !string.IsNullOrEmpty(CompletionCutsceneId);
        }

        /// <summary>
        /// Checks if quest has icon configured.
        /// </summary>
        /// <returns>True if IconPath is not empty</returns>
        public readonly bool HasIcon()
        {
            return !string.IsNullOrEmpty(IconPath);
        }

        /// <summary>
        /// Gets category display name (with proper capitalization).
        /// </summary>
        /// <returns>Formatted category name</returns>
        public readonly string GetCategoryDisplayName()
        {
            return Category switch
            {
                "Main" => "Main Quest",
                "Side" => "Side Quest",
                "Repeatable" => "Repeatable Quest",
                "Daily" => "Daily Quest",
                _ => Category
            };
        }
    }
}
