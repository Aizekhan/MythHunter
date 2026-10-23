// Assets/_MythHunter/Code/GameLogic/Quests/Components/MythHunterQuestComponent.cs

using System;
using System.Collections.Generic;
using RPGFramework.ECS;
using MythHunter.Quests.Data;

namespace MythHunter.Quests.Components
{
    /// <summary>
    /// MythHunter-specific quest metadata component.
    /// Wraps Framework ActiveQuestsData with MythHunter UI/gameplay metadata.
    ///
    /// Design:
    /// - Dual-component pattern: Framework manages quest state/progress
    /// - This component adds MythHunter-specific UI and gameplay metadata
    /// - Follows same pattern as Inventory + InventoryComponent (Phase 1)
    /// - Metadata includes UI paths, audio, journal settings, quest categories
    /// </summary>
    public struct MythHunterQuestComponent : IComponent
    {
        /// <summary>
        /// Dictionary mapping quest IDs to their MythHunter-specific metadata.
        /// Holds UI/gameplay data like icon paths, NPC IDs, audio, journal settings.
        /// Framework ActiveQuestsData holds the actual quest progress.
        /// </summary>
        public Dictionary<string, QuestMetadata> Metadata;

        /// <summary>
        /// Factory method to create an empty quest component.
        /// </summary>
        /// <returns>Initialized quest component with empty metadata dictionary</returns>
        public static MythHunterQuestComponent Create()
        {
            return new MythHunterQuestComponent
            {
                Metadata = new Dictionary<string, QuestMetadata>()
            };
        }

        /// <summary>
        /// Factory method to create quest component with pre-populated metadata.
        /// </summary>
        /// <param name="initialMetadata">Initial quest metadata dictionary</param>
        /// <returns>Initialized quest component with provided metadata</returns>
        public static MythHunterQuestComponent Create(Dictionary<string, QuestMetadata> initialMetadata)
        {
            return new MythHunterQuestComponent
            {
                Metadata = initialMetadata ?? new Dictionary<string, QuestMetadata>()
            };
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>
        /// Gets metadata for a specific quest.
        /// </summary>
        /// <param name="questId">Quest ID</param>
        /// <returns>Quest metadata, or default if not found</returns>
        public readonly QuestMetadata GetMetadata(string questId)
        {
            if (Metadata != null && Metadata.TryGetValue(questId, out var metadata))
            {
                return metadata;
            }
            return default;
        }

        /// <summary>
        /// Checks if quest has metadata registered.
        /// </summary>
        /// <param name="questId">Quest ID</param>
        /// <returns>True if metadata exists</returns>
        public readonly bool HasMetadata(string questId)
        {
            return Metadata != null && Metadata.ContainsKey(questId);
        }

        /// <summary>
        /// Adds or updates metadata for a quest.
        /// </summary>
        /// <param name="questId">Quest ID</param>
        /// <param name="metadata">Quest metadata</param>
        public void SetMetadata(string questId, QuestMetadata metadata)
        {
            if (Metadata == null)
                Metadata = new Dictionary<string, QuestMetadata>();

            Metadata[questId] = metadata;
        }

        /// <summary>
        /// Removes metadata for a quest.
        /// </summary>
        /// <param name="questId">Quest ID</param>
        /// <returns>True if metadata was removed, false if not found</returns>
        public bool RemoveMetadata(string questId)
        {
            if (Metadata != null && Metadata.ContainsKey(questId))
            {
                Metadata.Remove(questId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all registered quest IDs with metadata.
        /// </summary>
        /// <returns>List of quest IDs</returns>
        public readonly List<string> GetAllQuestIds()
        {
            if (Metadata == null)
                return new List<string>();

            return new List<string>(Metadata.Keys);
        }

        /// <summary>
        /// Gets all quests in a specific category.
        /// </summary>
        /// <param name="category">Quest category to filter</param>
        /// <returns>List of quest IDs matching category</returns>
        public readonly List<string> GetQuestsByCategory(string category)
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
        /// Gets all quests that should be shown in journal.
        /// </summary>
        /// <returns>List of journal-visible quest IDs</returns>
        public readonly List<string> GetJournalQuests()
        {
            var result = new List<string>();

            if (Metadata == null)
                return result;

            foreach (var kvp in Metadata)
            {
                if (kvp.Value.ShowInJournal)
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
    }
}
