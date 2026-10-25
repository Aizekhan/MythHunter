// Assets/_MythHunter/Code/GameLogic/Progression/Data/SampleStatGrowth.cs

using RPGFramework.Progression;
using System.Collections.Generic;

namespace MythHunter.GameLogic.Progression.Data
{
    /// <summary>
    /// Sample stat growth definitions for testing and development.
    /// Provides pre-configured stat curves for different character archetypes.
    ///
    /// Pattern:
    /// - Static factory methods for stat growth data
    /// - Covers combat stats (HP, Damage, Defense) and secondary stats (Speed, Crit, etc.)
    /// - Uses Framework StatGrowthData and GrowthCurveType
    ///
    /// Usage:
    /// - GetWarriorStatGrowth(): High HP/Defense, moderate damage
    /// - GetMageStatGrowth(): High mana/spell power, low HP
    /// - GetRogueStatGrowth(): High crit/speed, moderate HP
    /// - GetAllStatGrowth(): Get all stat growth configurations
    /// </summary>
    public static class SampleStatGrowth
    {
        // ============================================================
        // WARRIOR STATS (Tank/Melee)
        // ============================================================

        /// <summary>
        /// Warrior stat growth: High HP and Defense, moderate Damage.
        /// Level 1: HP=100, Damage=10, Defense=10
        /// Level 50: HP=2000, Damage=150, Defense=200
        /// Level 100: HP=5000, Damage=350, Defense=500
        /// </summary>
        public static List<StatGrowthData> GetWarriorStatGrowth()
        {
            return new List<StatGrowthData>
            {
                // Health Points - Linear growth with high base
                new StatGrowthData("HP", 100f, 50f, 0f, GrowthCurveType.Linear),

                // Physical Damage - Moderate linear growth
                new StatGrowthData("PhysicalDamage", 10f, 3.5f, 0f, GrowthCurveType.Linear),

                // Physical Defense - High linear growth
                new StatGrowthData("PhysicalDefense", 10f, 5f, 0f, GrowthCurveType.Linear),

                // Magical Defense - Moderate growth
                new StatGrowthData("MagicalDefense", 5f, 2f, 0f, GrowthCurveType.Linear),

                // Stamina - High growth for abilities
                new StatGrowthData("Stamina", 100f, 10f, 0f, GrowthCurveType.Linear),

                // Critical Chance - Slow growth
                new StatGrowthData("CriticalChance", 5f, 0.2f, 0f, GrowthCurveType.Linear),

                // Attack Speed - Slow growth
                new StatGrowthData("AttackSpeed", 1.0f, 0.01f, 0f, GrowthCurveType.Linear)
            };
        }

        // ============================================================
        // MAGE STATS (Caster/DPS)
        // ============================================================

        /// <summary>
        /// Mage stat growth: High Mana and Spell Power, low HP.
        /// Level 1: HP=60, Mana=150, SpellPower=15
        /// Level 50: HP=800, Mana=3000, SpellPower=250
        /// Level 100: HP=1500, Mana=7000, SpellPower=600
        /// </summary>
        public static List<StatGrowthData> GetMageStatGrowth()
        {
            return new List<StatGrowthData>
            {
                // Health Points - Low growth
                new StatGrowthData("HP", 60f, 15f, 0f, GrowthCurveType.Linear),

                // Mana Points - High linear growth
                new StatGrowthData("MP", 150f, 70f, 0f, GrowthCurveType.Linear),

                // Spell Power - Polynomial growth
                new StatGrowthData("SpellPower", 15f, 1f, 0f, GrowthCurveType.Polynomial),

                // Physical Defense - Very low growth
                new StatGrowthData("PhysicalDefense", 3f, 0.5f, 0f, GrowthCurveType.Linear),

                // Magical Defense - Moderate growth
                new StatGrowthData("MagicalDefense", 8f, 3f, 0f, GrowthCurveType.Linear),

                // Spell Critical - Moderate growth
                new StatGrowthData("SpellCritical", 5f, 0.3f, 0f, GrowthCurveType.Linear),

                // Cast Speed - Moderate growth
                new StatGrowthData("CastSpeed", 1.0f, 0.015f, 0f, GrowthCurveType.Linear)
            };
        }

        // ============================================================
        // ROGUE STATS (Assassin/DPS)
        // ============================================================

        /// <summary>
        /// Rogue stat growth: High Crit and Speed, moderate HP.
        /// Level 1: HP=80, Damage=12, Crit=10%
        /// Level 50: HP=1500, Damage=200, Crit=35%
        /// Level 100: HP=3500, Damage=450, Crit=65%
        /// </summary>
        public static List<StatGrowthData> GetRogueStatGrowth()
        {
            return new List<StatGrowthData>
            {
                // Health Points - Moderate growth
                new StatGrowthData("HP", 80f, 35f, 0f, GrowthCurveType.Linear),

                // Physical Damage - Polynomial growth
                new StatGrowthData("PhysicalDamage", 12f, 1f, 0f, GrowthCurveType.Polynomial),

                // Physical Defense - Low growth
                new StatGrowthData("PhysicalDefense", 6f, 2f, 0f, GrowthCurveType.Linear),

                // Magical Defense - Low growth
                new StatGrowthData("MagicalDefense", 4f, 1.5f, 0f, GrowthCurveType.Linear),

                // Energy - Moderate growth
                new StatGrowthData("Energy", 100f, 8f, 0f, GrowthCurveType.Linear),

                // Critical Chance - Polynomial growth
                new StatGrowthData("CriticalChance", 10f, 0.1f, 0f, GrowthCurveType.Polynomial),

                // Attack Speed - High growth
                new StatGrowthData("AttackSpeed", 1.2f, 0.02f, 0f, GrowthCurveType.Linear),

                // Evasion - Moderate growth
                new StatGrowthData("Evasion", 5f, 0.25f, 0f, GrowthCurveType.Linear)
            };
        }

        // ============================================================
        // CLERIC STATS (Support/Healer)
        // ============================================================

        /// <summary>
        /// Cleric stat growth: High Mana and Healing Power, moderate HP.
        /// Level 1: HP=70, Mana=120, HealingPower=20
        /// Level 50: HP=1200, Mana=2500, HealingPower=300
        /// Level 100: HP=2800, Mana=6000, HealingPower=700
        /// </summary>
        public static List<StatGrowthData> GetClericStatGrowth()
        {
            return new List<StatGrowthData>
            {
                // Health Points - Moderate growth
                new StatGrowthData("HP", 70f, 28f, 0f, GrowthCurveType.Linear),

                // Mana Points - High growth
                new StatGrowthData("MP", 120f, 60f, 0f, GrowthCurveType.Linear),

                // Healing Power - Polynomial growth
                new StatGrowthData("HealingPower", 20f, 1f, 0f, GrowthCurveType.Polynomial),

                // Physical Defense - Moderate growth
                new StatGrowthData("PhysicalDefense", 7f, 2.5f, 0f, GrowthCurveType.Linear),

                // Magical Defense - Moderate growth
                new StatGrowthData("MagicalDefense", 9f, 3.5f, 0f, GrowthCurveType.Linear),

                // Spell Critical - Moderate growth
                new StatGrowthData("SpellCritical", 5f, 0.25f, 0f, GrowthCurveType.Linear),

                // Mana Regeneration - High growth
                new StatGrowthData("ManaRegen", 5f, 1f, 0f, GrowthCurveType.Linear)
            };
        }

        // ============================================================
        // UTILITY METHODS
        // ============================================================

        /// <summary>
        /// Get all archetype stat growth configurations.
        /// Useful for populating stat growth formula on initialization.
        /// </summary>
        public static Dictionary<string, List<StatGrowthData>> GetAllStatGrowth()
        {
            return new Dictionary<string, List<StatGrowthData>>
            {
                { "warrior", GetWarriorStatGrowth() },
                { "mage", GetMageStatGrowth() },
                { "rogue", GetRogueStatGrowth() },
                { "cleric", GetClericStatGrowth() }
            };
        }

        /// <summary>
        /// Get stat growth by archetype ID.
        /// Returns null if not found.
        /// </summary>
        public static List<StatGrowthData> GetStatGrowthByArchetype(string archetypeId)
        {
            var allGrowth = GetAllStatGrowth();
            return allGrowth.ContainsKey(archetypeId) ? allGrowth[archetypeId] : null;
        }

        /// <summary>
        /// Get a flattened list of all unique stat growth data.
        /// Useful for registering all stats at once.
        /// </summary>
        public static List<StatGrowthData> GetAllUniqueStatGrowth()
        {
            var uniqueStats = new Dictionary<string, StatGrowthData>();
            var allGrowth = GetAllStatGrowth();

            foreach (var archetype in allGrowth.Values)
            {
                foreach (var statGrowth in archetype)
                {
                    if (!uniqueStats.ContainsKey(statGrowth.StatId))
                    {
                        uniqueStats[statGrowth.StatId] = statGrowth;
                    }
                }
            }

            return new List<StatGrowthData>(uniqueStats.Values);
        }

        /// <summary>
        /// Calculate stat value preview at specific level.
        /// Uses the Framework's CalculateAtLevel method.
        /// </summary>
        public static float CalculateStatPreview(StatGrowthData growthData, int level)
        {
            return growthData.CalculateAtLevel(level);
        }
    }
}
