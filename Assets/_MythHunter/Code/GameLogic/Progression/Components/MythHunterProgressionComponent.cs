// Assets/_MythHunter/Code/GameLogic/Progression/Components/MythHunterProgressionComponent.cs

using RPGFramework.ECS;
using RPGFramework.Progression;
using System.Collections.Generic;

namespace MythHunter.GameLogic.Progression.Components
{
    /// <summary>
    /// MythHunter component for tracking entity progression (XP, levels, unlocks).
    /// Stores progression state for integration with Framework Progression System.
    ///
    /// Pattern:
    /// - Wraps Framework ExperienceData for XP tracking
    /// - Tracks unlocked content by unlock ID
    /// - Stores stat growth history
    /// - Single component per entity (ECS constraint)
    ///
    /// Usage:
    /// - CreateHero(): Create progression for hero starting at level 1
    /// - CreateEnemy(level): Create progression for enemy at specific level
    /// - CreateNPC(level): Create progression for NPC
    /// - AddExperience(...): Update XP/level via ProgressionSystemIntegration
    ///
    /// Dependencies:
    /// - RPGFramework.Progression.ExperienceData (struct from Framework)
    /// - MythHunter.Core.ECS.IComponent
    /// </summary>
    public struct MythHunterProgressionComponent : IComponent
    {
        /// <summary>Current experience and level data (from Framework)</summary>
        public ExperienceData ExperienceData;

        /// <summary>List of unlocked content IDs (abilities, features, items)</summary>
        public List<string> UnlockedContent;

        /// <summary>Stat growth history: statId -> current value at this level</summary>
        public Dictionary<string, float> StatValues;

        /// <summary>Time when last XP was gained (for cooldowns, rested XP, etc.)</summary>
        public float LastXPGainTime;

        /// <summary>Total XP gained from kills (for statistics)</summary>
        public long TotalXPFromKills;

        /// <summary>Total XP gained from quests (for statistics)</summary>
        public long TotalXPFromQuests;

        // ============================================================
        // FACTORY METHODS
        // ============================================================

        /// <summary>
        /// Create progression component for hero starting at level 1.
        /// Heroes start with no unlocks (will gain them through leveling).
        /// </summary>
        /// <param name="xpToLevel2">XP required for level 2 (from level table)</param>
        public static MythHunterProgressionComponent CreateHero(long xpToLevel2)
        {
            return new MythHunterProgressionComponent
            {
                ExperienceData = new ExperienceData(1, 0, 0, xpToLevel2),
                UnlockedContent = new List<string>(),
                StatValues = new Dictionary<string, float>(),
                LastXPGainTime = 0f,
                TotalXPFromKills = 0,
                TotalXPFromQuests = 0
            };
        }

        /// <summary>
        /// Create progression component for enemy at specific level.
        /// Enemies don't gain XP or unlocks (static level).
        /// </summary>
        /// <param name="level">Enemy level</param>
        /// <param name="totalXP">Total XP for this level (from level table)</param>
        public static MythHunterProgressionComponent CreateEnemy(int level, long totalXP)
        {
            return new MythHunterProgressionComponent
            {
                ExperienceData = new ExperienceData(level, 0, totalXP, 0), // No XP to next level
                UnlockedContent = new List<string>(),
                StatValues = new Dictionary<string, float>(),
                LastXPGainTime = 0f,
                TotalXPFromKills = 0,
                TotalXPFromQuests = 0
            };
        }

        /// <summary>
        /// Create progression component for NPC at specific level.
        /// NPCs may have unlocks pre-configured (abilities, features).
        /// </summary>
        /// <param name="level">NPC level</param>
        /// <param name="totalXP">Total XP for this level (from level table)</param>
        /// <param name="unlockedContent">Pre-configured unlocks</param>
        public static MythHunterProgressionComponent CreateNPC(int level, long totalXP, List<string> unlockedContent = null)
        {
            return new MythHunterProgressionComponent
            {
                ExperienceData = new ExperienceData(level, 0, totalXP, 0), // No XP to next level
                UnlockedContent = unlockedContent ?? new List<string>(),
                StatValues = new Dictionary<string, float>(),
                LastXPGainTime = 0f,
                TotalXPFromKills = 0,
                TotalXPFromQuests = 0
            };
        }

        /// <summary>
        /// Create progression component for boss at specific level.
        /// Bosses may have enhanced stat values.
        /// </summary>
        /// <param name="level">Boss level</param>
        /// <param name="totalXP">Total XP for this level (from level table)</param>
        /// <param name="statMultiplier">Multiplier for base stats (e.g., 1.5 for 50% bonus)</param>
        public static MythHunterProgressionComponent CreateBoss(int level, long totalXP, float statMultiplier = 1.5f)
        {
            return new MythHunterProgressionComponent
            {
                ExperienceData = new ExperienceData(level, 0, totalXP, 0), // No XP to next level
                UnlockedContent = new List<string>(),
                StatValues = new Dictionary<string, float>(), // Stats will be set with multiplier
                LastXPGainTime = 0f,
                TotalXPFromKills = 0,
                TotalXPFromQuests = 0
            };
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>Get current level</summary>
        public int GetLevel()
        {
            return ExperienceData.Level;
        }

        /// <summary>Get current XP in this level</summary>
        public long GetCurrentXP()
        {
            return ExperienceData.CurrentXP;
        }

        /// <summary>Get total XP accumulated</summary>
        public long GetTotalXP()
        {
            return ExperienceData.TotalXP;
        }

        /// <summary>Get XP required for next level</summary>
        public long GetXPToNextLevel()
        {
            return ExperienceData.XPToNextLevel;
        }

        /// <summary>Get progress to next level (0.0 - 1.0)</summary>
        public float GetProgressToNextLevel()
        {
            return ExperienceData.ProgressToNextLevel;
        }

        /// <summary>Check if entity has unlocked specific content</summary>
        public bool HasUnlock(string unlockId)
        {
            return UnlockedContent != null && UnlockedContent.Contains(unlockId);
        }

        /// <summary>Add unlock to entity</summary>
        public void AddUnlock(string unlockId)
        {
            if (UnlockedContent == null)
                UnlockedContent = new List<string>();

            if (!UnlockedContent.Contains(unlockId))
                UnlockedContent.Add(unlockId);
        }

        /// <summary>Get stat value</summary>
        public float GetStatValue(string statId)
        {
            if (StatValues != null && StatValues.TryGetValue(statId, out float value))
                return value;
            return 0f;
        }

        /// <summary>Set stat value</summary>
        public void SetStatValue(string statId, float value)
        {
            if (StatValues == null)
                StatValues = new Dictionary<string, float>();

            StatValues[statId] = value;
        }

        /// <summary>Update multiple stat values from dictionary</summary>
        public void UpdateStats(Dictionary<string, float> newStats)
        {
            if (StatValues == null)
                StatValues = new Dictionary<string, float>();

            foreach (var kvp in newStats)
            {
                StatValues[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>Update experience data (called after XP gain)</summary>
        public void UpdateExperienceData(ExperienceData newData)
        {
            ExperienceData = newData;
        }

        /// <summary>Record XP gain from kill</summary>
        public void RecordKillXP(long xpAmount, float currentTime)
        {
            TotalXPFromKills += xpAmount;
            LastXPGainTime = currentTime;
        }

        /// <summary>Record XP gain from quest</summary>
        public void RecordQuestXP(long xpAmount, float currentTime)
        {
            TotalXPFromQuests += xpAmount;
            LastXPGainTime = currentTime;
        }
    }
}
