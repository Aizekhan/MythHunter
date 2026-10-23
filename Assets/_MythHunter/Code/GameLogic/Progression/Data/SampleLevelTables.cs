// Assets/_MythHunter/Code/GameLogic/Progression/Data/SampleLevelTables.cs

using RPGFramework.Progression;
using System.Collections.Generic;

namespace MythHunter.GameLogic.Progression.Data
{
    /// <summary>
    /// Sample level table definitions for testing and development.
    /// Provides pre-configured XP curves for different progression speeds.
    ///
    /// Pattern:
    /// - Static factory methods for easy access
    /// - Multiple curve types: Linear, Fast, Slow, Exponential
    /// - Uses Framework LevelTable implementation
    ///
    /// Usage:
    /// - GetStandardHeroTable(): Balanced progression for heroes (1-100)
    /// - GetFastLevelingTable(): Quick progression for testing (1-50)
    /// - GetSlowLevelingTable(): Hardcore progression (1-100)
    /// - GetExponentialTable(): Exponential curve for endgame (1-100)
    /// </summary>
    public static class SampleLevelTables
    {
        // ============================================================
        // STANDARD HERO PROGRESSION (Balanced)
        // ============================================================

        /// <summary>
        /// Standard hero leveling table (1-100).
        /// Balanced XP curve suitable for most RPGs.
        /// Uses polynomial curve: XP = baseXP * (level^2.5)
        /// Level 2: ~100 XP, Level 10: ~3162 XP, Level 50: ~177,827 XP, Level 100: ~1,000,000 XP
        /// </summary>
        public static ILevelTable GetStandardHeroTable()
        {
            // Using CreatePolynomial: baseXP * (level ^ power)
            return LevelTable.CreatePolynomial(100, 10, 2.5f);
        }

        // ============================================================
        // FAST LEVELING (For Testing)
        // ============================================================

        /// <summary>
        /// Fast leveling table (1-50).
        /// Quick progression for testing and tutorial zones.
        /// Uses polynomial curve: XP = 50 * level^2
        /// Level 2: ~100 XP, Level 10: ~2,850 XP, Level 25: ~10,625 XP, Level 50: ~40,825 XP
        /// </summary>
        public static ILevelTable GetFastLevelingTable()
        {
            // Using CreatePolynomial with quadratic curve (power=2)
            return LevelTable.CreatePolynomial(50, 50, 2.0f);
        }

        // ============================================================
        // SLOW LEVELING (Hardcore)
        // ============================================================

        /// <summary>
        /// Slow leveling table (1-100).
        /// Hardcore progression for dedicated players.
        /// Uses exponential curve: XP = baseXP * (multiplier^level)
        /// Level 2: ~115 XP, Level 10: ~8,101 XP, Level 50: ~1,476,974 XP, Level 100: ~270M+ XP
        /// </summary>
        public static ILevelTable GetSlowLevelingTable()
        {
            // Using CreateExponential with 1.15 multiplier
            return LevelTable.CreateExponential(100, 50, 1.15f);
        }

        // ============================================================
        // EXPONENTIAL ENDGAME (Power Scaling)
        // ============================================================

        /// <summary>
        /// Exponential leveling table (1-100).
        /// Steep curve for endgame power scaling.
        /// Uses exponential curve: XP = baseXP * (multiplier^level)
        /// Level 2: ~72 XP, Level 10: ~12,385 XP, Level 50: ~2.74B+ XP, Level 100: ~extremely high
        /// </summary>
        public static ILevelTable GetExponentialTable()
        {
            // Using CreateExponential with steep 1.2 multiplier
            return LevelTable.CreateExponential(100, 30, 1.2f);
        }

        // ============================================================
        // CUSTOM MILESTONE-BASED (Story Progression)
        // ============================================================

        /// <summary>
        /// Custom milestone-based table (1-60).
        /// XP requirements plateau at story milestones (10, 20, 30, etc.).
        /// Suitable for story-driven progression.
        /// </summary>
        public static ILevelTable GetMilestoneTable()
        {
            var customXP = new Dictionary<int, long>();

            // Calculate total XP for each level with milestone plateaus
            long totalXP = 0;
            for (int level = 1; level <= 60; level++)
            {
                if (level == 1)
                {
                    customXP[1] = 0;
                }
                else
                {
                    // Plateau every 10 levels
                    int milestone = ((level - 1) / 10) * 10;
                    double baseCurve = 100.0 * level * level;
                    double milestoneMultiplier = 1.0 + (milestone * 0.1); // +10% per milestone
                    long xpForThisLevel = (long)(baseCurve * milestoneMultiplier);

                    totalXP += xpForThisLevel;
                    customXP[level] = totalXP;
                }
            }

            return LevelTable.CreateCustom(customXP);
        }

        // ============================================================
        // UTILITY METHODS
        // ============================================================

        /// <summary>
        /// Get all sample level tables.
        /// Useful for testing multiple progression curves.
        /// </summary>
        public static Dictionary<string, ILevelTable> GetAllLevelTables()
        {
            return new Dictionary<string, ILevelTable>
            {
                { "standard_hero", GetStandardHeroTable() },
                { "fast_leveling", GetFastLevelingTable() },
                { "slow_hardcore", GetSlowLevelingTable() },
                { "exponential_endgame", GetExponentialTable() },
                { "milestone_story", GetMilestoneTable() }
            };
        }

        /// <summary>
        /// Get level table by ID.
        /// Returns null if not found.
        /// </summary>
        public static ILevelTable GetLevelTableById(string tableId)
        {
            var tables = GetAllLevelTables();
            return tables.ContainsKey(tableId) ? tables[tableId] : null;
        }

        /// <summary>
        /// Log level table XP values for debugging.
        /// </summary>
        public static string GetLevelTableSummary(ILevelTable table, int[] levelsToShow)
        {
            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Level Table Summary (Max Level: {table.MaxLevel}):");

            foreach (int level in levelsToShow)
            {
                if (level > table.MaxLevel) continue;

                long totalXP = table.GetTotalXPForLevel(level);
                long xpForNext = level < table.MaxLevel ? table.GetXPForNextLevel(level) : 0;

                summary.AppendLine($"  Level {level}: Total XP = {totalXP:N0}, XP to Next = {xpForNext:N0}");
            }

            return summary.ToString();
        }
    }
}
