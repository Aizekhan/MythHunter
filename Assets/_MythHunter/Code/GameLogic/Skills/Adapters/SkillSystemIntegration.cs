// Assets/_MythHunter/Code/GameLogic/Skills/Adapters/SkillSystemIntegration.cs

using MythHunter.GameLogic.Skills;
using MythHunter.Utils.Logging;
using RPGFramework.ECS;
using RPGFramework.Skills;
using System;
using System.Collections.Generic;

namespace MythHunter.GameLogic.Skills.Adapters
{
    /// <summary>
    /// Adapter that integrates Framework Skills System with MythHunter game layer.
    /// Bridges MythHunterSkillsComponent with Framework ISkillManager, ISkillTree.
    /// </summary>
    public class SkillSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly ISkillTree _skillTree;
        private readonly IMythLogger _logger;

        public SkillSystemIntegration(
            IEntityManager entityManager,
            ISkillTree skillTree,
            IMythLogger logger)
        {
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _skillTree = skillTree ?? throw new ArgumentNullException(nameof(skillTree));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Skill Learning

        /// <summary>
        /// Attempts to learn a skill for an entity.
        /// Returns true if successful, false if prerequisites not met.
        /// </summary>
        public bool LearnSkill(
            int entityId,
            string skillId,
            int characterLevel,
            float currentTime,
            Action<int, string, int> onSkillLearned = null)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
            {
                _logger.LogWarning($"Entity {entityId} does not have MythHunterSkillsComponent", "SkillSystemIntegration");
                return false;
            }

            ISkill skill = _skillTree.GetSkill(skillId);
            if (skill == null)
            {
                _logger.LogWarning($"Skill {skillId} not found in skill tree", "SkillSystemIntegration");
                return false;
            }

            // Check if already learned
            if (component.ProgressData.HasSkill(skillId))
            {
                _logger.LogDebug($"Entity {entityId} already has skill {skillId}", "SkillSystemIntegration");
                return false;
            }

            // Check prerequisites
            var progress = new SkillProgress(component.ProgressData);
            if (!_skillTree.CanLearnSkill(progress, characterLevel, skillId))
            {
                _logger.LogDebug($"Entity {entityId} cannot learn skill {skillId} - prerequisites not met", "SkillSystemIntegration");
                return false;
            }

            // Learn skill at level 1
            component.ProgressData.SetSkillLevel(skillId, 1);
            component.LastSkillLearnTime = currentTime;
            component.IncrementSkillPoints(1);

            _entityManager.AddComponent(entityId, component);

            _logger.LogInfo($"Entity {entityId} learned skill {skillId}", "SkillSystemIntegration");

            // Trigger callback
            onSkillLearned?.Invoke(entityId, skillId, 1);

            return true;
        }

        /// <summary>
        /// Increases the level of a skill.
        /// </summary>
        public bool IncreaseSkillLevel(
            int entityId,
            string skillId,
            float currentTime,
            Action<int, string, int, int> onSkillLevelUp = null)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return false;

            ISkill skill = _skillTree.GetSkill(skillId);
            if (skill == null)
                return false;

            int currentLevel = component.ProgressData.GetSkillLevel(skillId);
            if (currentLevel == 0)
                return false; // Not learned yet

            if (currentLevel >= skill.MaxLevel)
                return false; // Already at max level

            int newLevel = currentLevel + 1;
            component.ProgressData.SetSkillLevel(skillId, newLevel);
            component.LastSkillLearnTime = currentTime;
            component.IncrementSkillPoints(1);

            _entityManager.AddComponent(entityId, component);

            _logger.LogInfo($"Entity {entityId} increased skill {skillId} to level {newLevel}", "SkillSystemIntegration");

            // Trigger callback
            onSkillLevelUp?.Invoke(entityId, skillId, currentLevel, newLevel);

            return true;
        }

        #endregion

        #region Queries

        /// <summary>
        /// Gets the current level of a skill.
        /// </summary>
        public int GetSkillLevel(int entityId, string skillId)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return 0;

            return component.ProgressData.GetSkillLevel(skillId);
        }

        /// <summary>
        /// Checks if an entity has learned a skill.
        /// </summary>
        public bool HasSkill(int entityId, string skillId)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return false;

            return component.ProgressData.HasSkill(skillId);
        }

        /// <summary>
        /// Checks if an entity can learn a specific skill.
        /// </summary>
        public bool CanLearnSkill(int entityId, string skillId, int characterLevel)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return false;

            var progress = new SkillProgress(component.ProgressData);
            return _skillTree.CanLearnSkill(progress, characterLevel, skillId);
        }

        /// <summary>
        /// Gets all skills learned by an entity.
        /// </summary>
        public List<string> GetLearnedSkills(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return new List<string>();

            return component.ProgressData.SkillLevels != null
                ? new List<string>(component.ProgressData.SkillLevels.Keys)
                : new List<string>();
        }

        /// <summary>
        /// Gets all skills available to learn (prerequisites satisfied).
        /// </summary>
        public List<ISkill> GetAvailableSkills(int entityId, int characterLevel)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return new List<ISkill>();

            var progress = new SkillProgress(component.ProgressData);
            return new List<ISkill>(_skillTree.GetAvailableSkills(progress, characterLevel));
        }

        /// <summary>
        /// Gets skill progress for an entity.
        /// </summary>
        public ISkillProgress GetSkillProgress(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return new SkillProgress(SkillProgressData.CreateEmpty());

            return new SkillProgress(component.ProgressData);
        }

        /// <summary>
        /// Gets total skill points spent by an entity.
        /// </summary>
        public int GetTotalSkillPointsSpent(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return 0;

            return component.TotalSkillPointsSpent;
        }

        #endregion

        #region Game-Specific Features

        /// <summary>
        /// Adds a skill to entity's favorites.
        /// </summary>
        public void AddFavoriteSkill(int entityId, string skillId)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return;

            component.AddFavorite(skillId);
            _entityManager.AddComponent(entityId, component);
        }

        /// <summary>
        /// Removes a skill from entity's favorites.
        /// </summary>
        public void RemoveFavoriteSkill(int entityId, string skillId)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return;

            component.RemoveFavorite(skillId);
            _entityManager.AddComponent(entityId, component);
        }

        /// <summary>
        /// Gets favorited skills for an entity.
        /// </summary>
        public List<string> GetFavoriteSkills(int entityId)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return new List<string>();

            return component.FavoritedSkills ?? new List<string>();
        }

        /// <summary>
        /// Sets the active skill tree for an entity.
        /// </summary>
        public void SetActiveSkillTree(int entityId, string skillTreeId)
        {
            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return;

            component.ActiveSkillTreeId = skillTreeId;
            _entityManager.AddComponent(entityId, component);
        }

        #endregion

        #region Stat Integration

        /// <summary>
        /// Calculates total stat bonuses from all learned skills.
        /// Returns dictionary of statId -> total bonus value.
        /// </summary>
        public Dictionary<string, float> CalculateStatBonuses(int entityId)
        {
            var bonuses = new Dictionary<string, float>();

            if (!_entityManager.TryGetComponent<MythHunterSkillsComponent>(entityId, out var component))
                return bonuses;

            if (component.ProgressData.SkillLevels == null)
                return bonuses;

            foreach (var skillEntry in component.ProgressData.SkillLevels)
            {
                string skillId = skillEntry.Key;
                int skillLevel = skillEntry.Value;

                ISkill skill = _skillTree.GetSkill(skillId);
                if (skill == null)
                    continue;

                foreach (var bonus in skill.Bonuses)
                {
                    float bonusValue = bonus.CalculateBonus(skillLevel);

                    if (!bonuses.ContainsKey(bonus.StatId))
                        bonuses[bonus.StatId] = 0f;

                    // Sum all bonuses (caller can apply them based on BonusType)
                    bonuses[bonus.StatId] += bonusValue;
                }
            }

            return bonuses;
        }

        #endregion
    }
}
