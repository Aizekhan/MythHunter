// Assets/_MythHunter/Code/GameLogic/Achievements/Sample/SampleAchievements.cs

using System.Collections.Generic;
using RPGFramework.Achievements;
using RPGFramework.Achievements.Runtime.Contracts;

namespace MythHunter.GameLogic.Achievements.Sample
{
    /// <summary>
    /// Sample achievement definitions for testing and development.
    /// Demonstrates various achievement types and patterns across multiple categories:
    /// - Combat achievements (kill enemies, deal damage)
    /// - Progression achievements (reach levels, gain experience)
    /// - Collection achievements (gather gold, craft items)
    /// - Quest achievements (complete quests)
    /// - Social achievements (dialogue completion, faction reputation)
    /// - Exploration achievements (travel distance)
    /// - Meta achievements (complete all other achievements)
    ///
    /// These are greenfield sample achievements demonstrating the Framework Achievement System
    /// with MythHunter metadata integration.
    /// </summary>
    public static class SampleAchievements
    {
        // ============================================================
        // COMBAT ACHIEVEMENTS
        // ============================================================

        /// <summary>
        /// Achievement: First Blood (Common)
        /// Kill your first enemy
        /// </summary>
        public static (IAchievement achievement, AchievementMetadata metadata) FirstBlood()
        {
            var achievement = Achievement.Create(
                achievementId: "achievement_first_blood",
                name: "First Blood",
                description: "Kill your first enemy",
                tier: AchievementTier.Common,
                criteria: new List<IAchievementCriterion>
                {
                    AchievementCriterion.Create(
                        criterionId: "criterion_kill_any_enemy",
                        type: CriterionType.KillCount,
                        requiredCount: 1,
                        targetId: "any_enemy"
                    )
                },
                points: 5
            );

            var metadata = AchievementMetadata.CreateCombatAchievement(
                iconPath: "Textures/Achievements/first_blood",
                displayName: "First Blood!",
                unlockMessage: "You've slain your first enemy! The path of the warrior begins.",
                sortOrder: 1
            );

            return (achievement, metadata);
        }

        /// <summary>
        /// Achievement: Battle Scarred (Rare)
        /// Deal 100,000 total damage
        /// </summary>
        public static (IAchievement achievement, AchievementMetadata metadata) BattleScarred()
        {
            var achievement = Achievement.Create(
                achievementId: "achievement_battle_scarred",
                name: "Battle Scarred",
                description: "Deal 100,000 total damage in combat",
                tier: AchievementTier.Rare,
                criteria: new List<IAchievementCriterion>
                {
                    AchievementCriterion.Create(
                        criterionId: "criterion_damage_dealt",
                        type: CriterionType.DamageDealt,
                        requiredCount: 100000,
                        targetId: "total_damage"
                    )
                },
                points: 25
            );

            var metadata = AchievementMetadata.CreateCombatAchievement(
                iconPath: "Textures/Achievements/battle_scarred",
                displayName: "Battle Scarred",
                unlockMessage: "You've dealt 100,000 damage! A true warrior's record.",
                sortOrder: 10
            );

            return (achievement, metadata);
        }

        // ============================================================
        // PROGRESSION ACHIEVEMENTS
        // ============================================================

        /// <summary>
        /// Achievement: Experienced Adventurer (Uncommon)
        /// Reach level 10
        /// </summary>
        public static (IAchievement achievement, AchievementMetadata metadata) ExperiencedAdventurer()
        {
            var achievement = Achievement.Create(
                achievementId: "achievement_experienced_adventurer",
                name: "Experienced Adventurer",
                description: "Reach character level 10",
                tier: AchievementTier.Uncommon,
                criteria: new List<IAchievementCriterion>
                {
                    AchievementCriterion.Create(
                        criterionId: "criterion_level_10",
                        type: CriterionType.LevelReached,
                        requiredCount: 10,
                        targetId: "character_level"
                    )
                },
                points: 10
            );

            var metadata = AchievementMetadata.CreateProgressionAchievement(
                iconPath: "Textures/Achievements/level_10",
                displayName: "Experienced Adventurer",
                unlockMessage: "You've reached level 10! Your journey is well underway.",
                sortOrder: 2
            );

            return (achievement, metadata);
        }

        // ============================================================
        // COLLECTION ACHIEVEMENTS
        // ============================================================

        /// <summary>
        /// Achievement: Wealthy Merchant (Rare)
        /// Collect 10,000 gold
        /// </summary>
        public static (IAchievement achievement, AchievementMetadata metadata) WealthyMerchant()
        {
            var achievement = Achievement.Create(
                achievementId: "achievement_wealthy_merchant",
                name: "Wealthy Merchant",
                description: "Accumulate 10,000 gold",
                tier: AchievementTier.Rare,
                criteria: new List<IAchievementCriterion>
                {
                    AchievementCriterion.Create(
                        criterionId: "criterion_gold_collected",
                        type: CriterionType.ItemCollected,
                        requiredCount: 10000,
                        targetId: "gold"
                    )
                },
                points: 20
            );

            var metadata = AchievementMetadata.CreateCollectionAchievement(
                iconPath: "Textures/Achievements/wealthy_merchant",
                displayName: "Wealthy Merchant",
                unlockMessage: "You've amassed 10,000 gold! True wealth is within your grasp.",
                sortOrder: 5
            );

            return (achievement, metadata);
        }

        /// <summary>
        /// Achievement: Master Crafter (Epic)
        /// Craft 50 items
        /// </summary>
        public static (IAchievement achievement, AchievementMetadata metadata) MasterCrafter()
        {
            var achievement = Achievement.Create(
                achievementId: "achievement_master_crafter",
                name: "Master Crafter",
                description: "Craft a total of 50 items",
                tier: AchievementTier.Epic,
                criteria: new List<IAchievementCriterion>
                {
                    AchievementCriterion.Create(
                        criterionId: "criterion_items_crafted",
                        type: CriterionType.Custom,
                        requiredCount: 50,
                        targetId: "items_crafted"
                    )
                },
                points: 30
            );

            var metadata = AchievementMetadata.Create(
                iconPath: "Textures/Achievements/master_crafter",
                displayName: "Master Crafter",
                unlockMessage: "You've crafted 50 items! Your skills rival the greatest artisans.",
                category: "Crafting"
            );

            return (achievement, metadata);
        }

        // ============================================================
        // QUEST ACHIEVEMENTS
        // ============================================================

        /// <summary>
        /// Achievement: Quest Completionist (Legendary)
        /// Complete 100 quests
        /// </summary>
        public static (IAchievement achievement, AchievementMetadata metadata) QuestCompletionist()
        {
            var achievement = Achievement.Create(
                achievementId: "achievement_quest_completionist",
                name: "Quest Completionist",
                description: "Complete 100 quests",
                tier: AchievementTier.Legendary,
                criteria: new List<IAchievementCriterion>
                {
                    AchievementCriterion.Create(
                        criterionId: "criterion_quests_completed",
                        type: CriterionType.QuestCompleted,
                        requiredCount: 100,
                        targetId: "any_quest"
                    )
                },
                points: 50
            );

            var metadata = AchievementMetadata.Create(
                iconPath: "Textures/Achievements/quest_completionist",
                displayName: "Quest Completionist",
                unlockMessage: "You've completed 100 quests! A legendary hero's dedication.",
                category: "Quest"
            );

            return (achievement, metadata);
        }

        // ============================================================
        // SOCIAL ACHIEVEMENTS
        // ============================================================

        /// <summary>
        /// Achievement: Social Butterfly (Common)
        /// Complete 10 dialogues
        /// </summary>
        public static (IAchievement achievement, AchievementMetadata metadata) SocialButterfly()
        {
            var achievement = Achievement.Create(
                achievementId: "achievement_social_butterfly",
                name: "Social Butterfly",
                description: "Complete 10 unique dialogues with NPCs",
                tier: AchievementTier.Common,
                criteria: new List<IAchievementCriterion>
                {
                    AchievementCriterion.Create(
                        criterionId: "criterion_dialogues_completed",
                        type: CriterionType.Custom,
                        requiredCount: 10,
                        targetId: "dialogues_completed"
                    )
                },
                points: 5
            );

            var metadata = AchievementMetadata.CreateSocialAchievement(
                iconPath: "Textures/Achievements/social_butterfly",
                displayName: "Social Butterfly",
                unlockMessage: "You've had 10 conversations! People appreciate your company.",
                sortOrder: 1
            );

            return (achievement, metadata);
        }

        /// <summary>
        /// Achievement: Reputation Master (Epic)
        /// Reach Revered reputation with any faction
        /// </summary>
        public static (IAchievement achievement, AchievementMetadata metadata) ReputationMaster()
        {
            var achievement = Achievement.Create(
                achievementId: "achievement_reputation_master",
                name: "Reputation Master",
                description: "Reach Revered standing with any faction",
                tier: AchievementTier.Epic,
                criteria: new List<IAchievementCriterion>
                {
                    AchievementCriterion.Create(
                        criterionId: "criterion_faction_revered",
                        type: CriterionType.Custom,
                        requiredCount: 1,
                        targetId: "faction_revered"
                    )
                },
                points: 35
            );

            var metadata = AchievementMetadata.CreateSocialAchievement(
                iconPath: "Textures/Achievements/reputation_master",
                displayName: "Reputation Master",
                unlockMessage: "You've reached Revered standing! The faction honors your deeds.",
                sortOrder: 20
            );

            return (achievement, metadata);
        }

        // ============================================================
        // EXPLORATION ACHIEVEMENTS
        // ============================================================

        /// <summary>
        /// Achievement: Explorer (Uncommon)
        /// Travel 50,000 distance units
        /// </summary>
        public static (IAchievement achievement, AchievementMetadata metadata) Explorer()
        {
            var achievement = Achievement.Create(
                achievementId: "achievement_explorer",
                name: "Explorer",
                description: "Travel a total distance of 50,000 units",
                tier: AchievementTier.Uncommon,
                criteria: new List<IAchievementCriterion>
                {
                    AchievementCriterion.Create(
                        criterionId: "criterion_distance_traveled",
                        type: CriterionType.DistanceTraveled,
                        requiredCount: 50000,
                        targetId: "total_distance"
                    )
                },
                points: 15
            );

            var metadata = AchievementMetadata.CreateExplorationAchievement(
                iconPath: "Textures/Achievements/explorer",
                displayName: "Explorer",
                unlockMessage: "You've traveled 50,000 units! The world is your canvas.",
                sortOrder: 3
            );

            return (achievement, metadata);
        }

        // ============================================================
        // META ACHIEVEMENTS
        // ============================================================

        /// <summary>
        /// Achievement: Legendary Hero (Mythic)
        /// Complete all other achievements
        /// </summary>
        public static (IAchievement achievement, AchievementMetadata metadata) LegendaryHero()
        {
            var achievement = new Achievement(
                achievementId: "achievement_legendary_hero",
                name: "Legendary Hero",
                description: "Unlock all other achievements in the game",
                category: "Meta",
                points: 100,
                tier: AchievementTier.Mythic,
                criteria: new List<IAchievementCriterion>
                {
                    AchievementCriterion.Create(
                        criterionId: "criterion_all_achievements",
                        type: CriterionType.Custom,
                        requiredCount: 1,
                        targetId: "all_achievements_unlocked"
                    )
                },
                rewards: new List<IAchievementReward>(),
                prerequisites: new List<string>
                {
                    "achievement_first_blood",
                    "achievement_experienced_adventurer",
                    "achievement_wealthy_merchant",
                    "achievement_master_crafter",
                    "achievement_quest_completionist",
                    "achievement_social_butterfly",
                    "achievement_reputation_master",
                    "achievement_battle_scarred",
                    "achievement_explorer"
                },
                isHidden: false,
                isSecret: false
            );

            var metadata = AchievementMetadata.CreateFull(
                iconPath: "Textures/Achievements/legendary_hero",
                displayName: "Legendary Hero",
                unlockMessage: "You've unlocked all achievements! You are a true legend!",
                category: "Meta",
                sortOrder: 999,
                showNotification: true,
                notificationSound: "Audio/SFX/achievement_legendary"
            );

            return (achievement, metadata);
        }

        // ============================================================
        // BATCH FACTORY METHODS
        // ============================================================

        /// <summary>
        /// Creates all sample achievements with their metadata as a complete package.
        /// Useful for initializing achievement system or testing.
        /// </summary>
        /// <returns>Dictionary of achievement ID to (Achievement, Metadata) tuples</returns>
        public static Dictionary<string, (IAchievement achievement, AchievementMetadata metadata)> GetAllSampleAchievements()
        {
            return new Dictionary<string, (IAchievement, AchievementMetadata)>
            {
                { "achievement_first_blood", FirstBlood() },
                { "achievement_experienced_adventurer", ExperiencedAdventurer() },
                { "achievement_wealthy_merchant", WealthyMerchant() },
                { "achievement_master_crafter", MasterCrafter() },
                { "achievement_quest_completionist", QuestCompletionist() },
                { "achievement_social_butterfly", SocialButterfly() },
                { "achievement_reputation_master", ReputationMaster() },
                { "achievement_battle_scarred", BattleScarred() },
                { "achievement_explorer", Explorer() },
                { "achievement_legendary_hero", LegendaryHero() }
            };
        }

        /// <summary>
        /// Gets all sample achievements as Achievement array (for registry initialization).
        /// </summary>
        /// <returns>Array of IAchievement objects</returns>
        public static IAchievement[] GetAllSampleAchievementsData()
        {
            return new IAchievement[]
            {
                FirstBlood().achievement,
                ExperiencedAdventurer().achievement,
                WealthyMerchant().achievement,
                MasterCrafter().achievement,
                QuestCompletionist().achievement,
                SocialButterfly().achievement,
                ReputationMaster().achievement,
                BattleScarred().achievement,
                Explorer().achievement,
                LegendaryHero().achievement
            };
        }

        /// <summary>
        /// Gets all sample achievement metadata (for MythHunterAchievementComponent initialization).
        /// </summary>
        /// <returns>Dictionary of achievement ID to metadata</returns>
        public static Dictionary<string, AchievementMetadata> GetAllSampleAchievementsMetadata()
        {
            return new Dictionary<string, AchievementMetadata>
            {
                { "achievement_first_blood", FirstBlood().metadata },
                { "achievement_experienced_adventurer", ExperiencedAdventurer().metadata },
                { "achievement_wealthy_merchant", WealthyMerchant().metadata },
                { "achievement_master_crafter", MasterCrafter().metadata },
                { "achievement_quest_completionist", QuestCompletionist().metadata },
                { "achievement_social_butterfly", SocialButterfly().metadata },
                { "achievement_reputation_master", ReputationMaster().metadata },
                { "achievement_battle_scarred", BattleScarred().metadata },
                { "achievement_explorer", Explorer().metadata },
                { "achievement_legendary_hero", LegendaryHero().metadata }
            };
        }

        /// <summary>
        /// Gets achievements by category.
        /// </summary>
        /// <param name="category">Category to filter by</param>
        /// <returns>Dictionary of achievement ID to (Achievement, Metadata) tuples</returns>
        public static Dictionary<string, (IAchievement achievement, AchievementMetadata metadata)> GetAchievementsByCategory(string category)
        {
            var result = new Dictionary<string, (IAchievement, AchievementMetadata)>();
            var allAchievements = GetAllSampleAchievements();

            foreach (var kvp in allAchievements)
            {
                if (kvp.Value.metadata.Category == category)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets achievements by tier.
        /// </summary>
        /// <param name="tier">Tier to filter by</param>
        /// <returns>Dictionary of achievement ID to (Achievement, Metadata) tuples</returns>
        public static Dictionary<string, (IAchievement achievement, AchievementMetadata metadata)> GetAchievementsByTier(AchievementTier tier)
        {
            var result = new Dictionary<string, (IAchievement, AchievementMetadata)>();
            var allAchievements = GetAllSampleAchievements();

            foreach (var kvp in allAchievements)
            {
                if (kvp.Value.achievement.Tier == tier)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets combat achievements only.
        /// </summary>
        /// <returns>Dictionary of combat achievement ID to (Achievement, Metadata) tuples</returns>
        public static Dictionary<string, (IAchievement achievement, AchievementMetadata metadata)> GetCombatAchievements()
        {
            return GetAchievementsByCategory("Combat");
        }

        /// <summary>
        /// Gets progression achievements only.
        /// </summary>
        /// <returns>Dictionary of progression achievement ID to (Achievement, Metadata) tuples</returns>
        public static Dictionary<string, (IAchievement achievement, AchievementMetadata metadata)> GetProgressionAchievements()
        {
            return GetAchievementsByCategory("Progression");
        }

        /// <summary>
        /// Gets collection achievements only.
        /// </summary>
        /// <returns>Dictionary of collection achievement ID to (Achievement, Metadata) tuples</returns>
        public static Dictionary<string, (IAchievement achievement, AchievementMetadata metadata)> GetCollectionAchievements()
        {
            return GetAchievementsByCategory("Collection");
        }

        /// <summary>
        /// Gets social achievements only.
        /// </summary>
        /// <returns>Dictionary of social achievement ID to (Achievement, Metadata) tuples</returns>
        public static Dictionary<string, (IAchievement achievement, AchievementMetadata metadata)> GetSocialAchievements()
        {
            return GetAchievementsByCategory("Social");
        }

        /// <summary>
        /// Gets exploration achievements only.
        /// </summary>
        /// <returns>Dictionary of exploration achievement ID to (Achievement, Metadata) tuples</returns>
        public static Dictionary<string, (IAchievement achievement, AchievementMetadata metadata)> GetExplorationAchievements()
        {
            return GetAchievementsByCategory("Exploration");
        }
    }
}
