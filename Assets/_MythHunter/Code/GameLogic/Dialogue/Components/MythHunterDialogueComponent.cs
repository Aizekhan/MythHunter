// Assets/_MythHunter/Code/GameLogic/Dialogue/Components/MythHunterDialogueComponent.cs

using System;
using System.Collections.Generic;
using RPGFramework.ECS;

namespace MythHunter.GameLogic.Dialogue
{
    /// <summary>
    /// MythHunter-specific dialogue metadata component.
    /// Wraps Framework ActiveDialoguesData with MythHunter UI/gameplay metadata.
    ///
    /// Design:
    /// - Dual-component pattern: Framework manages dialogue state/progress
    /// - This component adds MythHunter-specific UI and gameplay metadata
    /// - Follows same pattern as Quests + MythHunterQuestComponent (Phase 2)
    /// - Metadata includes speaker visuals, audio, camera, quest triggers
    /// </summary>
    [Serializable]
    public struct MythHunterDialogueComponent : IComponent
    {
        /// <summary>
        /// Dictionary mapping dialogue IDs to their MythHunter-specific metadata.
        /// Holds UI/gameplay data like speaker portraits, voice actors, camera angles.
        /// Framework ActiveDialoguesData holds the actual dialogue progress.
        /// </summary>
        public Dictionary<string, DialogueMetadata> Metadata;

        /// <summary>
        /// Factory method to create an empty dialogue component.
        /// </summary>
        /// <returns>Initialized dialogue component with empty metadata dictionary</returns>
        public static MythHunterDialogueComponent Create()
        {
            return new MythHunterDialogueComponent
            {
                Metadata = new Dictionary<string, DialogueMetadata>()
            };
        }

        /// <summary>
        /// Factory method to create dialogue component with pre-populated metadata.
        /// </summary>
        /// <param name="initialMetadata">Initial dialogue metadata dictionary</param>
        /// <returns>Initialized dialogue component with provided metadata</returns>
        public static MythHunterDialogueComponent Create(Dictionary<string, DialogueMetadata> initialMetadata)
        {
            return new MythHunterDialogueComponent
            {
                Metadata = initialMetadata ?? new Dictionary<string, DialogueMetadata>()
            };
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>
        /// Gets metadata for a specific dialogue.
        /// </summary>
        /// <param name="dialogueId">Dialogue ID</param>
        /// <returns>Dialogue metadata, or default if not found</returns>
        public readonly DialogueMetadata GetMetadata(string dialogueId)
        {
            if (Metadata != null && Metadata.TryGetValue(dialogueId, out var metadata))
            {
                return metadata;
            }
            return default;
        }

        /// <summary>
        /// Checks if dialogue has metadata registered.
        /// </summary>
        /// <param name="dialogueId">Dialogue ID</param>
        /// <returns>True if metadata exists</returns>
        public readonly bool HasMetadata(string dialogueId)
        {
            return Metadata != null && Metadata.ContainsKey(dialogueId);
        }

        /// <summary>
        /// Adds or updates metadata for a dialogue.
        /// </summary>
        /// <param name="dialogueId">Dialogue ID</param>
        /// <param name="metadata">Dialogue metadata</param>
        public void SetMetadata(string dialogueId, DialogueMetadata metadata)
        {
            if (Metadata == null)
                Metadata = new Dictionary<string, DialogueMetadata>();

            Metadata[dialogueId] = metadata;
        }

        /// <summary>
        /// Removes metadata for a dialogue.
        /// </summary>
        /// <param name="dialogueId">Dialogue ID</param>
        /// <returns>True if metadata was removed, false if not found</returns>
        public bool RemoveMetadata(string dialogueId)
        {
            if (Metadata != null && Metadata.ContainsKey(dialogueId))
            {
                Metadata.Remove(dialogueId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all dialogues spoken by a specific character.
        /// </summary>
        /// <param name="speakerId">Speaker name to filter</param>
        /// <returns>List of dialogue IDs with this speaker</returns>
        public readonly List<string> GetDialoguesBySpeaker(string speakerId)
        {
            var result = new List<string>();

            if (Metadata == null)
                return result;

            foreach (var kvp in Metadata)
            {
                if (kvp.Value.SpeakerName == speakerId)
                    result.Add(kvp.Key);
            }

            return result;
        }

        /// <summary>
        /// Gets all quest-related dialogues (Main or Quest category).
        /// Useful for finding dialogues that trigger quests.
        /// </summary>
        /// <returns>List of quest-related dialogue IDs</returns>
        public readonly List<string> GetQuestDialogues()
        {
            var result = new List<string>();

            if (Metadata == null)
                return result;

            foreach (var kvp in Metadata)
            {
                if (kvp.Value.Category == "Main" || kvp.Value.Category == "Quest")
                    result.Add(kvp.Key);
            }

            return result;
        }

        /// <summary>
        /// Gets all dialogues in a specific category.
        /// </summary>
        /// <param name="category">Dialogue category to filter</param>
        /// <returns>List of dialogue IDs matching category</returns>
        public readonly List<string> GetDialoguesByCategory(string category)
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
        /// Gets all dialogues that should appear in dialogue log.
        /// </summary>
        /// <returns>List of loggable dialogue IDs</returns>
        public readonly List<string> GetLoggableDialogues()
        {
            var result = new List<string>();

            if (Metadata == null)
                return result;

            foreach (var kvp in Metadata)
            {
                if (kvp.Value.ShowInDialogueLog)
                    result.Add(kvp.Key);
            }

            return result;
        }

        /// <summary>
        /// Gets all registered dialogue IDs with metadata.
        /// </summary>
        /// <returns>List of dialogue IDs</returns>
        public readonly List<string> GetAllDialogueIds()
        {
            if (Metadata == null)
                return new List<string>();

            return new List<string>(Metadata.Keys);
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
