// Assets/_MythHunter/Code/GameLogic/Skills/Components/MythHunterSkillsComponent.cs

using RPGFramework.ECS;
using RPGFramework.Skills;
using System;
using System.Collections.Generic;

namespace MythHunter.GameLogic.Skills
{
    /// <summary>
    /// MythHunter game-layer component for skill progression.
    /// Contains Framework SkillProgressData plus game-specific metadata.
    /// </summary>
    [Serializable]
    public struct MythHunterSkillsComponent : IComponent
    {
        /// <summary>
        /// Framework skill progress data (skill ID -> level mapping)
        /// </summary>
        public SkillProgressData ProgressData;

        /// <summary>
        /// Game-specific: List of favorited/pinned skills for quick access
        /// </summary>
        public List<string> FavoritedSkills;

        /// <summary>
        /// Game-specific: Timestamp when last skill was learned (for analytics)
        /// </summary>
        public float LastSkillLearnTime;

        /// <summary>
        /// Game-specific: Total skill points spent (for achievements/stats)
        /// </summary>
        public int TotalSkillPointsSpent;

        /// <summary>
        /// Game-specific: Skill tree ID this entity is currently viewing/specializing in
        /// </summary>
        public string ActiveSkillTreeId;

        #region Factory Methods

        /// <summary>
        /// Creates an empty skills component (no skills learned).
        /// </summary>
        public static MythHunterSkillsComponent CreateEmpty()
        {
            return new MythHunterSkillsComponent
            {
                ProgressData = SkillProgressData.CreateEmpty(),
                FavoritedSkills = new List<string>(),
                LastSkillLearnTime = 0f,
                TotalSkillPointsSpent = 0,
                ActiveSkillTreeId = null
            };
        }

        /// <summary>
        /// Creates a hero component with initial skills.
        /// </summary>
        public static MythHunterSkillsComponent CreateHero(List<string> startingSkills = null)
        {
            return new MythHunterSkillsComponent
            {
                ProgressData = startingSkills != null
                    ? SkillProgressData.CreateWithSkills(startingSkills)
                    : SkillProgressData.CreateEmpty(),
                FavoritedSkills = new List<string>(),
                LastSkillLearnTime = 0f,
                TotalSkillPointsSpent = startingSkills?.Count ?? 0,
                ActiveSkillTreeId = null
            };
        }

        /// <summary>
        /// Creates an enemy component with predefined skills at specific levels.
        /// </summary>
        public static MythHunterSkillsComponent CreateEnemy(Dictionary<string, int> skills, string skillTreeId = null)
        {
            var component = CreateEmpty();
            component.ActiveSkillTreeId = skillTreeId;

            if (skills != null)
            {
                foreach (var kvp in skills)
                {
                    component.ProgressData.SetSkillLevel(kvp.Key, kvp.Value);
                    component.TotalSkillPointsSpent += kvp.Value;
                }
            }

            return component;
        }

        /// <summary>
        /// Creates an NPC component with specific skills.
        /// </summary>
        public static MythHunterSkillsComponent CreateNPC(List<string> npcSkills)
        {
            return new MythHunterSkillsComponent
            {
                ProgressData = npcSkills != null
                    ? SkillProgressData.CreateWithSkills(npcSkills)
                    : SkillProgressData.CreateEmpty(),
                FavoritedSkills = new List<string>(),
                LastSkillLearnTime = 0f,
                TotalSkillPointsSpent = npcSkills?.Count ?? 0,
                ActiveSkillTreeId = null
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Adds a skill to favorites list.
        /// </summary>
        public void AddFavorite(string skillId)
        {
            if (FavoritedSkills == null)
                FavoritedSkills = new List<string>();

            if (!FavoritedSkills.Contains(skillId))
                FavoritedSkills.Add(skillId);
        }

        /// <summary>
        /// Removes a skill from favorites list.
        /// </summary>
        public void RemoveFavorite(string skillId)
        {
            FavoritedSkills?.Remove(skillId);
        }

        /// <summary>
        /// Checks if a skill is favorited.
        /// </summary>
        public bool IsFavorited(string skillId)
        {
            return FavoritedSkills != null && FavoritedSkills.Contains(skillId);
        }

        /// <summary>
        /// Increments total skill points spent.
        /// </summary>
        public void IncrementSkillPoints(int points = 1)
        {
            TotalSkillPointsSpent += points;
        }

        #endregion
    }
}
