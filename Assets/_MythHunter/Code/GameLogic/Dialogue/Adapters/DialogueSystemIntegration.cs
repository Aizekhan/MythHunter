// Assets/_MythHunter/Code/GameLogic/Dialogue/Adapters/DialogueSystemIntegration.cs

using System.Collections.Generic;
using RPGFramework.ECS;
using RPGFramework.Dialogue;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.GameLogic.Dialogue
{
    /// <summary>
    /// Integration adapter for dialogue system operations.
    /// Bridges Framework Dialogue System (RPGFramework.Dialogue.Runtime) with MythHunter game layer.
    ///
    /// Pattern (from EPIC 11 Phase 2+):
    /// - Greenfield integration (no legacy dialogue code to migrate)
    /// - Uses MythHunterDialogueComponent for entity dialogue metadata
    /// - Manages dialogue progression via Framework IDialogueManager
    /// - Provides unified interface for dialogue operations with metadata support
    ///
    /// Key Responsibilities:
    /// - Start/advance/end dialogues with metadata updates
    /// - Query active dialogues and available choices
    /// - Manage metadata lifecycle (add/remove as dialogues progress)
    /// - Support quest triggers and dialogue categories
    /// </summary>
    public class DialogueSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly IDialogueManager _dialogueManager;
        private readonly IMythLogger _logger;

        private const string LogCategory = "DialogueIntegration";

        [Inject]
        public DialogueSystemIntegration(
            IEntityManager entityManager,
            IDialogueManager dialogueManager,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _dialogueManager = dialogueManager;
            _logger = logger;
        }

        // ============================================================
        // DIALOGUE PROGRESSION
        // ============================================================

        /// <summary>
        /// Starts a dialogue for an entity with optional metadata.
        /// Ensures MythHunterDialogueComponent exists and updates metadata.
        /// </summary>
        /// <param name="entityId">Entity ID (typically hero)</param>
        /// <param name="dialogueId">Dialogue ID to start</param>
        /// <param name="metadata">Optional dialogue metadata to register</param>
        /// <returns>True if dialogue was successfully started</returns>
        public bool StartDialogue(int entityId, string dialogueId, DialogueMetadata? metadata = null)
        {
            // Ensure dialogue component exists
            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
            {
                dialogueComponent = MythHunterDialogueComponent.Create();
            }

            // Try to start dialogue via Framework
            bool started = _dialogueManager.StartDialogue(entityId, dialogueId);

            if (!started)
            {
                _logger.LogWarning($"Failed to start dialogue '{dialogueId}' for entity {entityId}", LogCategory);
                return false;
            }

            // Add metadata if provided
            if (metadata.HasValue)
            {
                dialogueComponent.SetMetadata(dialogueId, metadata.Value);
            }

            // Update component
            _entityManager.AddComponent(entityId, dialogueComponent);

            _logger.LogInfo(
                $"Entity {entityId} started dialogue '{dialogueId}'" +
                (metadata.HasValue ? $" with metadata (speaker={metadata.Value.SpeakerName}, category={metadata.Value.Category})" : ""),
                LogCategory
            );

            return true;
        }

        /// <summary>
        /// Advances dialogue to the next node, optionally selecting a choice.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="choiceId">Optional choice ID to select</param>
        /// <returns>True if dialogue was successfully advanced</returns>
        public bool AdvanceDialogue(int entityId, string choiceId = null)
        {
            bool advanced = _dialogueManager.AdvanceDialogue(entityId, choiceId);

            if (!advanced)
            {
                _logger.LogWarning($"Failed to advance dialogue for entity {entityId}" +
                    (choiceId != null ? $" with choice '{choiceId}'" : ""), LogCategory);
                return false;
            }

            _logger.LogInfo($"Entity {entityId} advanced dialogue" +
                (choiceId != null ? $" (selected choice '{choiceId}')" : ""), LogCategory);

            return true;
        }

        /// <summary>
        /// Ends the current dialogue for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        public void EndDialogue(int entityId)
        {
            _dialogueManager.EndDialogue(entityId);
            _logger.LogInfo($"Entity {entityId} ended dialogue", LogCategory);
        }

        // ============================================================
        // QUERY METHODS
        // ============================================================

        /// <summary>
        /// Gets the current dialogue node for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Current dialogue node or null if no active dialogue</returns>
        public IDialogueNode GetCurrentNode(int entityId)
        {
            return _dialogueManager.GetCurrentNode(entityId);
        }

        /// <summary>
        /// Gets available choices for the current dialogue node.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of available dialogue choices</returns>
        public IReadOnlyList<IDialogueChoice> GetAvailableChoices(int entityId)
        {
            return _dialogueManager.GetAvailableChoices(entityId);
        }

        /// <summary>
        /// Gets current progress for a specific dialogue.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="dialogueId">Dialogue ID</param>
        /// <returns>Dialogue progress or null if not found</returns>
        public IDialogueProgress GetDialogueProgress(int entityId, string dialogueId)
        {
            return _dialogueManager.GetDialogueProgress(entityId);
        }

        /// <summary>
        /// Gets all active dialogues for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of active dialogue IDs</returns>
        public List<string> GetActiveDialogues(int entityId)
        {
            var result = new List<string>();
            var progress = _dialogueManager.GetDialogueProgress(entityId);

            if (progress != null)
            {
                result.Add(progress.DialogueId);
            }

            return result;
        }

        /// <summary>
        /// Gets dialogues in a specific category that are active or available.
        /// Filters by metadata category from MythHunterDialogueComponent.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="category">Dialogue category to filter (Main, Side, Quest, Ambient)</param>
        /// <returns>List of dialogue IDs matching category</returns>
        public List<string> GetDialoguesByCategory(int entityId, string category)
        {
            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
                return new List<string>();

            return dialogueComponent.GetDialoguesByCategory(category);
        }

        /// <summary>
        /// Gets all quest-related dialogues (Main or Quest category).
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of quest dialogue IDs</returns>
        public List<string> GetQuestDialogues(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
                return new List<string>();

            return dialogueComponent.GetQuestDialogues();
        }

        /// <summary>
        /// Gets dialogues by speaker name.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="speakerName">Speaker name to filter</param>
        /// <returns>List of dialogue IDs with this speaker</returns>
        public List<string> GetDialoguesBySpeaker(int entityId, string speakerName)
        {
            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
                return new List<string>();

            return dialogueComponent.GetDialoguesBySpeaker(speakerName);
        }

        /// <summary>
        /// Gets dialogues that should appear in dialogue log.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of loggable dialogue IDs</returns>
        public List<string> GetLoggableDialogues(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
                return new List<string>();

            return dialogueComponent.GetLoggableDialogues();
        }

        // ============================================================
        // METADATA MANAGEMENT
        // ============================================================

        /// <summary>
        /// Gets metadata for a dialogue.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="dialogueId">Dialogue ID</param>
        /// <returns>Dialogue metadata or default if not found</returns>
        public DialogueMetadata GetDialogueMetadata(int entityId, string dialogueId)
        {
            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
                return default;

            return dialogueComponent.GetMetadata(dialogueId);
        }

        /// <summary>
        /// Registers or updates metadata for a dialogue.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="dialogueId">Dialogue ID</param>
        /// <param name="metadata">Dialogue metadata</param>
        /// <returns>True if metadata was registered</returns>
        public bool RegisterDialogueMetadata(int entityId, string dialogueId, DialogueMetadata metadata)
        {
            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
            {
                dialogueComponent = MythHunterDialogueComponent.Create();
            }

            dialogueComponent.SetMetadata(dialogueId, metadata);
            _entityManager.AddComponent(entityId, dialogueComponent);

            _logger.LogInfo($"Registered metadata for dialogue '{dialogueId}' on entity {entityId}", LogCategory);

            return true;
        }

        /// <summary>
        /// Unregisters metadata for a dialogue.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="dialogueId">Dialogue ID</param>
        /// <returns>True if metadata was removed</returns>
        public bool UnregisterDialogueMetadata(int entityId, string dialogueId)
        {
            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
                return false;

            bool removed = dialogueComponent.RemoveMetadata(dialogueId);

            if (removed)
            {
                _entityManager.AddComponent(entityId, dialogueComponent);
                _logger.LogInfo($"Unregistered metadata for dialogue '{dialogueId}' on entity {entityId}", LogCategory);
            }

            return removed;
        }

        // ============================================================
        // BULK OPERATIONS
        // ============================================================

        /// <summary>
        /// Registers metadata for multiple dialogues at once.
        /// Useful for dialogue hub initialization or batch setup.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="dialogueMetadata">Dictionary of dialogue ID to metadata</param>
        public void RegisterMultipleDialogueMetadata(int entityId, Dictionary<string, DialogueMetadata> dialogueMetadata)
        {
            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
            {
                dialogueComponent = MythHunterDialogueComponent.Create(new Dictionary<string, DialogueMetadata>(dialogueMetadata));
            }
            else
            {
                foreach (var kvp in dialogueMetadata)
                {
                    dialogueComponent.SetMetadata(kvp.Key, kvp.Value);
                }
            }

            _entityManager.AddComponent(entityId, dialogueComponent);

            _logger.LogInfo(
                $"Registered metadata for {dialogueMetadata.Count} dialogues on entity {entityId}",
                LogCategory
            );
        }

        /// <summary>
        /// Gets all dialogues with complete information (metadata).
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Dictionary of dialogue ID to metadata</returns>
        public Dictionary<string, DialogueMetadata> GetAllDialoguesWithMetadata(int entityId)
        {
            var result = new Dictionary<string, DialogueMetadata>();

            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
                return result;

            foreach (var dialogueId in dialogueComponent.GetAllDialogueIds())
            {
                result[dialogueId] = dialogueComponent.GetMetadata(dialogueId);
            }

            return result;
        }

        // ============================================================
        // STATISTICS
        // ============================================================

        /// <summary>
        /// Gets total number of active dialogues for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Number of active dialogues</returns>
        public int GetActiveDialogueCount(int entityId)
        {
            return GetActiveDialogues(entityId).Count;
        }

        /// <summary>
        /// Gets total number of registered dialogues.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Number of registered dialogues</returns>
        public int GetRegisteredDialogueCount(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterDialogueComponent>(entityId, out var dialogueComponent))
                return 0;

            return dialogueComponent.GetAllDialogueIds().Count;
        }

        /// <summary>
        /// Gets number of dialogues in a specific category.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="category">Dialogue category</param>
        /// <returns>Number of dialogues in category</returns>
        public int GetDialogueCountByCategory(int entityId, string category)
        {
            return GetDialoguesByCategory(entityId, category).Count;
        }

        /// <summary>
        /// Gets number of quest-related dialogues.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Number of quest dialogues</returns>
        public int GetQuestDialogueCount(int entityId)
        {
            return GetQuestDialogues(entityId).Count;
        }
    }
}
