// Assets/_MythHunter/Code/GameLogic/Quests/Adapters/QuestSystemIntegration.cs

using System.Collections.Generic;
using System.Linq;
using RPGFramework.ECS;
using RPGFramework.Quests;
using RPGFramework.DI;
using MythHunter.Utils.Logging;
using MythHunter.Quests.Components;
using MythHunter.Quests.Data;

namespace MythHunter.Quests.Adapters
{
    /// <summary>
    /// Integration adapter for quest system operations.
    /// Bridges Framework Quest System (RPGFramework.Quests.Runtime) with MythHunter game layer.
    ///
    /// Pattern (from EPIC 11 Phase 5):
    /// - Greenfield integration (no legacy quest code to migrate)
    /// - Uses MythHunterQuestComponent for entity quest metadata
    /// - Manages quest progression via Framework IQuestManager
    /// - Provides unified interface for quest operations with metadata support
    ///
    /// Key Responsibilities:
    /// - Accept/complete/abandon quests with metadata updates
    /// - Update quest objective progress
    /// - Query active and completed quests
    /// - Manage metadata lifecycle (add/remove as quests progress)
    /// </summary>
    public class QuestSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly IQuestManager _questManager;
        private readonly IMythLogger _logger;

        private const string LogCategory = "QuestIntegration";

        [Inject]
        public QuestSystemIntegration(
            IEntityManager entityManager,
            IQuestManager questManager,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _questManager = questManager;
            _logger = logger;
        }

        // ============================================================
        // QUEST ACCEPTANCE
        // ============================================================

        /// <summary>
        /// Accepts a quest for an entity with metadata support.
        /// Ensures MythHunterQuestComponent exists and updates metadata.
        /// </summary>
        /// <param name="entityId">Entity ID (typically hero)</param>
        /// <param name="questId">Quest ID to accept</param>
        /// <param name="metadata">Optional quest metadata to register</param>
        /// <returns>True if quest was successfully accepted</returns>
        public bool AcceptQuest(int entityId, string questId, QuestMetadata? metadata = null)
        {
            // Ensure quest component exists
            if (!_entityManager.TryGetComponent<MythHunterQuestComponent>(entityId, out var questComponent))
            {
                questComponent = MythHunterQuestComponent.Create();
            }

            // Try to accept quest via Framework
            bool accepted = _questManager.AcceptQuest(entityId, questId);

            if (!accepted)
            {
                _logger.LogWarning($"Failed to accept quest '{questId}' for entity {entityId}", LogCategory);
                return false;
            }

            // Add metadata if provided
            if (metadata.HasValue)
            {
                questComponent.SetMetadata(questId, metadata.Value);
            }

            // Update component
            _entityManager.AddComponent(entityId, questComponent);

            _logger.LogInfo(
                $"Entity {entityId} accepted quest '{questId}'" +
                (metadata.HasValue ? $" with metadata (category={metadata.Value.Category})" : ""),
                LogCategory
            );

            return true;
        }

        /// <summary>
        /// Checks if entity can accept a quest.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="questId">Quest ID to check</param>
        /// <returns>True if quest can be accepted</returns>
        public bool CanAcceptQuest(int entityId, string questId)
        {
            return _questManager.CanAcceptQuest(entityId, questId);
        }

        // ============================================================
        // QUEST COMPLETION
        // ============================================================

        /// <summary>
        /// Completes a quest for an entity.
        /// Plays completion audio/cutscene from metadata if configured.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="questId">Quest ID to complete</param>
        /// <param name="triggerCutscene">Whether to trigger completion cutscene if configured</param>
        /// <returns>True if quest was successfully completed</returns>
        public bool CompleteQuest(int entityId, string questId, bool triggerCutscene = true)
        {
            // Complete quest via Framework
            bool completed = _questManager.CompleteQuest(entityId, questId);

            if (!completed)
            {
                _logger.LogWarning($"Failed to complete quest '{questId}' for entity {entityId}", LogCategory);
                return false;
            }

            // Try to trigger completion audio/cutscene
            if (triggerCutscene && _entityManager.TryGetComponent<MythHunterQuestComponent>(entityId, out var questComponent))
            {
                var metadata = questComponent.GetMetadata(questId);

                if (metadata.HasCompletionSound())
                {
                    // TODO: Integrate with audio system
                    _logger.LogInfo($"Playing quest completion sound: {metadata.CompletedSoundPath}", LogCategory);
                }

                if (metadata.HasCompletionCutscene())
                {
                    // TODO: Integrate with cutscene system
                    _logger.LogInfo($"Triggering quest completion cutscene: {metadata.CompletionCutsceneId}", LogCategory);
                }
            }

            _logger.LogInfo($"Entity {entityId} completed quest '{questId}'", LogCategory);

            return true;
        }

        // ============================================================
        // QUEST ABANDONMENT
        // ============================================================

        /// <summary>
        /// Abandons an active quest for an entity.
        /// Removes associated metadata.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="questId">Quest ID to abandon</param>
        /// <returns>True if quest was successfully abandoned</returns>
        public bool AbandonQuest(int entityId, string questId)
        {
            // Abandon quest via Framework
            bool abandoned = _questManager.AbandonQuest(entityId, questId);

            if (!abandoned)
            {
                _logger.LogWarning($"Failed to abandon quest '{questId}' for entity {entityId}", LogCategory);
                return false;
            }

            // Remove metadata
            if (_entityManager.TryGetComponent<MythHunterQuestComponent>(entityId, out var questComponent))
            {
                questComponent.RemoveMetadata(questId);
                _entityManager.AddComponent(entityId, questComponent);
            }

            _logger.LogInfo($"Entity {entityId} abandoned quest '{questId}'", LogCategory);

            return true;
        }

        // ============================================================
        // PROGRESS UPDATES
        // ============================================================

        /// <summary>
        /// Updates progress for a quest objective.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="questId">Quest ID</param>
        /// <param name="objectiveId">Objective ID</param>
        /// <param name="progress">Progress amount to add</param>
        /// <returns>True if progress was updated</returns>
        public bool UpdateObjectiveProgress(int entityId, string questId, string objectiveId, int progress)
        {
            bool updated = _questManager.UpdateObjectiveProgress(entityId, questId, objectiveId, progress);

            if (!updated)
            {
                _logger.LogWarning(
                    $"Failed to update progress for quest '{questId}' objective '{objectiveId}' for entity {entityId}",
                    LogCategory
                );
                return false;
            }

            _logger.LogInfo(
                $"Updated objective progress: quest='{questId}', objective='{objectiveId}', progress={progress}",
                LogCategory
            );

            return true;
        }

        // ============================================================
        // QUERY METHODS
        // ============================================================

        /// <summary>
        /// Gets current progress for a specific quest.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="questId">Quest ID</param>
        /// <returns>Quest progress or null if not found</returns>
        public IQuestProgress GetQuestProgress(int entityId, string questId)
        {
            return _questManager.GetQuestProgress(entityId, questId);
        }

        /// <summary>
        /// Gets all active quests for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of active quest progress</returns>
        public IReadOnlyList<IQuestProgress> GetActiveQuests(int entityId)
        {
            return _questManager.GetActiveQuests(entityId);
        }

        /// <summary>
        /// Gets all completed quests for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of completed quest IDs</returns>
        public IReadOnlyList<string> GetCompletedQuests(int entityId)
        {
            return _questManager.GetCompletedQuests(entityId);
        }

        /// <summary>
        /// Gets active quests in a specific category.
        /// Filters by metadata category from MythHunterQuestComponent.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="category">Quest category to filter (Main, Side, etc.)</param>
        /// <returns>List of quest IDs matching category</returns>
        public List<string> GetActiveQuestsByCategory(int entityId, string category)
        {
            var result = new List<string>();

            if (!_entityManager.TryGetComponent<MythHunterQuestComponent>(entityId, out var questComponent))
                return result;

            var activeQuests = GetActiveQuests(entityId);
            var questIds = questComponent.GetQuestsByCategory(category);

            foreach (var questId in questIds)
            {
                if (activeQuests.Any(q => q.QuestId == questId))
                    result.Add(questId);
            }

            return result;
        }

        /// <summary>
        /// Gets active quests that should be shown in journal.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of journal-visible quest IDs</returns>
        public List<string> GetJournalQuests(int entityId)
        {
            var result = new List<string>();

            if (!_entityManager.TryGetComponent<MythHunterQuestComponent>(entityId, out var questComponent))
                return result;

            var activeQuests = GetActiveQuests(entityId);
            var journalQuests = questComponent.GetJournalQuests();

            foreach (var questId in journalQuests)
            {
                if (activeQuests.Any(q => q.QuestId == questId))
                    result.Add(questId);
            }

            return result;
        }

        // ============================================================
        // METADATA MANAGEMENT
        // ============================================================

        /// <summary>
        /// Gets metadata for an entity's quest.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="questId">Quest ID</param>
        /// <returns>Quest metadata or default if not found</returns>
        public QuestMetadata GetQuestMetadata(int entityId, string questId)
        {
            if (!_entityManager.TryGetComponent<MythHunterQuestComponent>(entityId, out var questComponent))
                return default;

            return questComponent.GetMetadata(questId);
        }

        /// <summary>
        /// Registers or updates metadata for a quest.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="questId">Quest ID</param>
        /// <param name="metadata">Quest metadata</param>
        /// <returns>True if metadata was registered</returns>
        public bool RegisterQuestMetadata(int entityId, string questId, QuestMetadata metadata)
        {
            if (!_entityManager.TryGetComponent<MythHunterQuestComponent>(entityId, out var questComponent))
            {
                questComponent = MythHunterQuestComponent.Create();
            }

            questComponent.SetMetadata(questId, metadata);
            _entityManager.AddComponent(entityId, questComponent);

            _logger.LogInfo($"Registered metadata for quest '{questId}' on entity {entityId}", LogCategory);

            return true;
        }

        /// <summary>
        /// Unregisters metadata for a quest.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="questId">Quest ID</param>
        /// <returns>True if metadata was removed</returns>
        public bool UnregisterQuestMetadata(int entityId, string questId)
        {
            if (!_entityManager.TryGetComponent<MythHunterQuestComponent>(entityId, out var questComponent))
                return false;

            bool removed = questComponent.RemoveMetadata(questId);

            if (removed)
            {
                _entityManager.AddComponent(entityId, questComponent);
                _logger.LogInfo($"Unregistered metadata for quest '{questId}' on entity {entityId}", LogCategory);
            }

            return removed;
        }

        // ============================================================
        // BULK OPERATIONS
        // ============================================================

        /// <summary>
        /// Registers metadata for multiple quests at once.
        /// Useful for quest hub initialization.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="questMetadata">Dictionary of quest ID to metadata</param>
        public void RegisterMultipleQuestMetadata(int entityId, Dictionary<string, QuestMetadata> questMetadata)
        {
            if (!_entityManager.TryGetComponent<MythHunterQuestComponent>(entityId, out var questComponent))
            {
                questComponent = MythHunterQuestComponent.Create(new Dictionary<string, QuestMetadata>(questMetadata));
            }
            else
            {
                foreach (var kvp in questMetadata)
                {
                    questComponent.SetMetadata(kvp.Key, kvp.Value);
                }
            }

            _entityManager.AddComponent(entityId, questComponent);

            _logger.LogInfo(
                $"Registered metadata for {questMetadata.Count} quests on entity {entityId}",
                LogCategory
            );
        }

        /// <summary>
        /// Gets all quests with complete information (progress + metadata).
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Dictionary of quest ID to (progress, metadata) tuples</returns>
        public Dictionary<string, (IQuestProgress progress, QuestMetadata metadata)> GetAllQuestsWithMetadata(int entityId)
        {
            var result = new Dictionary<string, (IQuestProgress, QuestMetadata)>();

            var activeQuests = GetActiveQuests(entityId);
            if (!_entityManager.TryGetComponent<MythHunterQuestComponent>(entityId, out var questComponent))
                return result;

            foreach (var progress in activeQuests)
            {
                var metadata = questComponent.GetMetadata(progress.QuestId);
                result[progress.QuestId] = (progress, metadata);
            }

            return result;
        }

        // ============================================================
        // STATISTICS
        // ============================================================

        /// <summary>
        /// Gets total number of active quests for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Number of active quests</returns>
        public int GetActiveQuestCount(int entityId)
        {
            return GetActiveQuests(entityId).Count;
        }

        /// <summary>
        /// Gets total number of completed quests for an entity.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>Number of completed quests</returns>
        public int GetCompletedQuestCount(int entityId)
        {
            return GetCompletedQuests(entityId).Count;
        }

        /// <summary>
        /// Gets number of active quests in a specific category.
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="category">Quest category</param>
        /// <returns>Number of quests in category</returns>
        public int GetQuestCountByCategory(int entityId, string category)
        {
            return GetActiveQuestsByCategory(entityId, category).Count;
        }
    }
}
