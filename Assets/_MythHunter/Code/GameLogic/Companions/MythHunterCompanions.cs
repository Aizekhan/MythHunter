using RPGFramework.Companions.Runtime;
using System;

namespace MythHunter.GameLogic.Companions
{
    /// <summary>
    /// MythHunter-specific companion definitions and factory.
    /// Defines the 5 core companions available in MythHunter.
    /// </summary>
    public static class MythHunterCompanions
    {
        /// <summary>
        /// Creates a Wolf companion - Loyal Combat Pet
        /// A fierce hunter that excels in combat and tracking.
        /// </summary>
        public static ICompanion CreateWolf(ICompanionManager manager, int ownerId, string customName = "Fenrir")
        {
            var companion = manager.CreateCompanion($"wolf_{Guid.NewGuid()}", ownerId, CompanionType.CombatPet, customName);

            // Configure wolf-specific stats
            var stats = manager.GetCompanionStats(companion.CompanionId);
            if (stats is CompanionStats wolfStats)
            {
                wolfStats.SetStats(new CompanionStatsData
                {
                    MaxHealth = 120,
                    CurrentHealth = 120,
                    Attack = 18, // High attack
                    Defense = 12,
                    Speed = 1.4f, // Fast
                    Hunger = 100,
                    Happiness = 60,
                    Experience = 0,
                    ExperienceToNextLevel = 100,
                    HealthRegenRate = 1.2f,
                    HungerDecayRate = 1.2f // Wolves eat a lot
                });
            }

            // Set initial loyalty higher (wolves are naturally loyal)
            companion.ModifyLoyalty(25); // Start at Friendly

            return companion;
        }

        /// <summary>
        /// Creates a Horse companion - Fast Travel Mount
        /// A reliable mount for fast travel across the realm.
        /// </summary>
        public static ICompanion CreateHorse(ICompanionManager manager, int ownerId, string customName = "Shadowfax")
        {
            var companion = manager.CreateCompanion($"horse_{Guid.NewGuid()}", ownerId, CompanionType.Mount, customName);

            // Configure horse-specific stats
            var stats = manager.GetCompanionStats(companion.CompanionId);
            if (stats is CompanionStats horseStats)
            {
                horseStats.SetStats(new CompanionStatsData
                {
                    MaxHealth = 180,
                    CurrentHealth = 180,
                    Attack = 8, // Low attack
                    Defense = 20, // Good defense
                    Speed = 2.5f, // Very fast
                    Hunger = 100,
                    Happiness = 50,
                    Experience = 0,
                    ExperienceToNextLevel = 100,
                    HealthRegenRate = 1.8f,
                    HungerDecayRate = 1.5f // Horses need regular feeding
                });
            }

            // Horses start neutral
            companion.ModifyLoyalty(10);

            return companion;
        }

        /// <summary>
        /// Creates a Cat companion - Lucky Non-Combat Pet
        /// Provides luck bonus and finds hidden items.
        /// </summary>
        public static ICompanion CreateCat(ICompanionManager manager, int ownerId, string customName = "Whiskers")
        {
            var companion = manager.CreateCompanion($"cat_{Guid.NewGuid()}", ownerId, CompanionType.NonCombatPet, customName);

            // Configure cat-specific stats
            var stats = manager.GetCompanionStats(companion.CompanionId);
            if (stats is CompanionStats catStats)
            {
                catStats.SetStats(new CompanionStatsData
                {
                    MaxHealth = 40,
                    CurrentHealth = 40,
                    Attack = 3, // Very low attack
                    Defense = 5,
                    Speed = 1.1f,
                    Hunger = 100,
                    Happiness = 70, // Cats are generally content
                    Experience = 0,
                    ExperienceToNextLevel = 100,
                    HealthRegenRate = 0.4f,
                    HungerDecayRate = 0.8f // Cats don't eat much
                });
            }

            // Cats are independent, start neutral
            companion.ModifyLoyalty(0);

            return companion;
        }

        /// <summary>
        /// Creates a Fire Elemental companion - Powerful Temporary Summon
        /// A powerful magical summon with high damage but limited duration.
        /// </summary>
        public static ICompanion CreateFireElemental(ICompanionManager manager, int ownerId, string customName = "Ignis")
        {
            var companion = manager.CreateCompanion($"fire_elemental_{Guid.NewGuid()}", ownerId, CompanionType.Summon, customName);

            // Configure fire elemental stats
            var stats = manager.GetCompanionStats(companion.CompanionId);
            if (stats is CompanionStats elementalStats)
            {
                elementalStats.SetStats(new CompanionStatsData
                {
                    MaxHealth = 100,
                    CurrentHealth = 100,
                    Attack = 35, // Very high attack
                    Defense = 8, // Low defense (glass cannon)
                    Speed = 1.6f,
                    Hunger = 100, // Doesn't need food
                    Happiness = 50, // Neutral (no emotions)
                    Experience = 0,
                    ExperienceToNextLevel = 100,
                    HealthRegenRate = 0f, // No regeneration
                    HungerDecayRate = 0f // Doesn't get hungry
                });
            }

            // Summons have no loyalty (bound by magic)
            companion.ModifyLoyalty(0);

            return companion;
        }

        /// <summary>
        /// Creates a Raven companion - Scout and Messenger Familiar
        /// A mystical familiar that can scout ahead and deliver messages.
        /// </summary>
        public static ICompanion CreateRaven(ICompanionManager manager, int ownerId, string customName = "Huginn")
        {
            var companion = manager.CreateCompanion($"raven_{Guid.NewGuid()}", ownerId, CompanionType.Familiar, customName);

            // Configure raven-specific stats
            var stats = manager.GetCompanionStats(companion.CompanionId);
            if (stats is CompanionStats ravenStats)
            {
                ravenStats.SetStats(new CompanionStatsData
                {
                    MaxHealth = 50,
                    CurrentHealth = 50,
                    Attack = 8,
                    Defense = 6,
                    Speed = 2.0f, // Fast (flying)
                    Hunger = 100,
                    Happiness = 55,
                    Experience = 0,
                    ExperienceToNextLevel = 100,
                    HealthRegenRate = 0.5f,
                    HungerDecayRate = 0.6f
                });
            }

            // Familiars are magically bound, start friendly
            companion.ModifyLoyalty(30);

            return companion;
        }

        /// <summary>
        /// Creates sample abilities for each companion type.
        /// These demonstrate how abilities can be attached to companions.
        /// </summary>
        public static class Abilities
        {
            /// <summary>
            /// Wolf's Howl - Intimidates nearby enemies, reducing their attack.
            /// </summary>
            public static ICompanionAbility CreateWolfHowl()
            {
                return new CompanionAbility(
                    "wolf_howl",
                    "Howl",
                    "Intimidates nearby enemies, reducing their attack by 20% for 10 seconds",
                    cooldown: 30f,
                    requiredLevel: 3,
                    isPassive: false,
                    abilityEffect: (companion, targetId) =>
                    {
                        // In a full implementation, this would apply a debuff to nearby enemies
                        // For now, just return success
                        return companion.Level >= 3;
                    }
                );
            }

            /// <summary>
            /// Horse's Sprint - Temporarily increases movement speed.
            /// </summary>
            public static ICompanionAbility CreateHorseSprint()
            {
                return new CompanionAbility(
                    "horse_sprint",
                    "Sprint",
                    "Increases movement speed by 50% for 8 seconds",
                    cooldown: 45f,
                    requiredLevel: 2,
                    isPassive: false,
                    abilityEffect: (companion, targetId) =>
                    {
                        // In a full implementation, this would apply a speed buff
                        return companion.Level >= 2;
                    }
                );
            }

            /// <summary>
            /// Cat's Lucky Find - Passive ability that increases item drop rate.
            /// </summary>
            public static ICompanionAbility CreateCatLuckyFind()
            {
                return new CompanionAbility(
                    "cat_lucky_find",
                    "Lucky Find",
                    "Passive: Increases item drop rate by 15%",
                    cooldown: 0f,
                    requiredLevel: 1,
                    isPassive: true,
                    abilityEffect: (companion, targetId) =>
                    {
                        // Passive abilities are always active
                        return true;
                    }
                );
            }

            /// <summary>
            /// Fire Elemental's Fireball - Powerful ranged fire attack.
            /// </summary>
            public static ICompanionAbility CreateElementalFireball()
            {
                return new CompanionAbility(
                    "fire_elemental_fireball",
                    "Fireball",
                    "Launches a powerful fireball dealing 50 fire damage",
                    cooldown: 8f,
                    requiredLevel: 1,
                    isPassive: false,
                    abilityEffect: (companion, targetId) =>
                    {
                        // In a full implementation, this would deal damage to the target
                        return targetId >= 0;
                    }
                );
            }

            /// <summary>
            /// Raven's Scout - Reveals nearby enemies and points of interest.
            /// </summary>
            public static ICompanionAbility CreateRavenScout()
            {
                return new CompanionAbility(
                    "raven_scout",
                    "Scout",
                    "Reveals all enemies and points of interest within 100 meters for 30 seconds",
                    cooldown: 60f,
                    requiredLevel: 2,
                    isPassive: false,
                    abilityEffect: (companion, targetId) =>
                    {
                        // In a full implementation, this would reveal map areas
                        return companion.Level >= 2;
                    }
                );
            }
        }
    }
}
