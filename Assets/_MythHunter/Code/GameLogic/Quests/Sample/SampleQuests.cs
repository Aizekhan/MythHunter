// Assets/_MythHunter/Code/GameLogic/Quests/Sample/SampleQuests.cs

using System.Collections.Generic;
using RPGFramework.Quests;
using MythHunter.Quests.Data;

namespace MythHunter.Quests.Sample
{
    /// <summary>
    /// Sample quest definitions for testing and development.
    /// Demonstrates various quest types and patterns:
    /// - Tutorial/introductory quest (Kill enemies)
    /// - Collection quest (Gather items)
    /// - Exploration quest (Visit locations)
    /// - Chain quest (Requires prerequisite)
    /// - Daily/Repeatable quest
    ///
    /// These are greenfield sample quests demonstrating the Framework Quest System
    /// with MythHunter metadata integration.
    /// </summary>
    public static class SampleQuests
    {
        // ============================================================
        // TUTORIAL QUEST - Kill 3 Goblins
        // ============================================================

        public static QuestData TutorialKillGoblins()
        {
            return new QuestData
            {
                Id = "quest_tutorial_kill_goblins",
                Title = "The Goblin Menace",
                Description = "A gang of goblins has been terrorizing the village. " +
                              "The Elder wants you to deal with them. Kill 3 goblins to prove yourself.",
                Objectives = new[]
                {
                    new QuestObjectiveData
                    {
                        Id = "objective_kill_goblins",
                        Description = "Kill 3 goblins",
                        RequiredCount = 3
                    }
                },
                Rewards = new[]
                {
                    new QuestRewardData
                    {
                        RewardId = "reward_experience",
                        Type = RewardType.Experience,
                        Amount = 100
                    },
                    new QuestRewardData
                    {
                        RewardId = "reward_gold",
                        Type = RewardType.Currency,
                        Amount = 50
                    }
                },
                PrerequisiteQuestIds = new string[] { },
                MinimumLevel = 1,
                IsRepeatable = false
            };
        }

        public static QuestMetadata TutorialKillGoblinsMetadata()
        {
            return QuestMetadata.CreateMainQuest(
                questGiverNpcId: "npc_elder_graves",
                questLocationId: "location_village",
                iconPath: "Textures/Quests/quest_icon_kill_goblins"
            );
        }

        // ============================================================
        // COLLECTION QUEST - Gather Medicinal Herbs
        // ============================================================

        public static QuestData CollectionGatherHerbs()
        {
            return new QuestData
            {
                Id = "quest_gather_medicinal_herbs",
                Title = "Medicinal Herbs",
                Description = "The village healer needs medicinal herbs to treat the sick. " +
                              "Gather 5 bundles of healing herbs from the forest.",
                Objectives = new[]
                {
                    new QuestObjectiveData
                    {
                        Id = "objective_collect_herbs",
                        Description = "Collect 5 bundles of medicinal herbs",
                        RequiredCount = 5
                    }
                },
                Rewards = new[]
                {
                    new QuestRewardData
                    {
                        RewardId = "reward_experience",
                        Type = RewardType.Experience,
                        Amount = 150
                    },
                    new QuestRewardData
                    {
                        RewardId = "reward_gold",
                        Type = RewardType.Currency,
                        Amount = 75
                    },
                    new QuestRewardData
                    {
                        Type = RewardType.Item,
                        RewardId = "potion_healing_major"
                    }
                },
                PrerequisiteQuestIds = new string[] { },
                MinimumLevel = 3,
                IsRepeatable = false
            };
        }

        public static QuestMetadata CollectionGatherHerbsMetadata()
        {
            return QuestMetadata.CreateSideQuest(
                questGiverNpcId: "npc_healer_elara",
                questLocationId: "location_forest"
            );
        }

        // ============================================================
        // EXPLORATION QUEST - Visit Ancient Ruins
        // ============================================================

        public static QuestData ExplorationAncientRuins()
        {
            return new QuestData
            {
                Id = "quest_explore_ancient_ruins",
                Title = "Lost Artifacts",
                Description = "A scholar believes ancient ruins hold valuable artifacts. " +
                              "Explore the ruins and retrieve the golden artifact.",
                Objectives = new[]
                {
                    new QuestObjectiveData
                    {
                        Id = "objective_visit_ruins",
                        Description = "Reach the Ancient Ruins location",
                        RequiredCount = 1
                    },
                    new QuestObjectiveData
                    {
                        Id = "objective_find_artifact",
                        Description = "Locate the golden artifact",
                        RequiredCount = 1
                    }
                },
                Rewards = new[]
                {
                    new QuestRewardData
                    {
                        RewardId = "reward_experience",
                        Type = RewardType.Experience,
                        Amount = 200
                    },
                    new QuestRewardData
                    {
                        RewardId = "reward_gold",
                        Type = RewardType.Currency,
                        Amount = 100
                    },
                    new QuestRewardData
                    {
                        Type = RewardType.Item,
                        RewardId = "artifact_golden_amulet"
                    }
                },
                PrerequisiteQuestIds = new string[] { },
                MinimumLevel = 5,
                IsRepeatable = false
            };
        }

        public static QuestMetadata ExplorationAncientRuinsMetadata()
        {
            return QuestMetadata.CreateFull(
                iconPath: "Textures/Quests/quest_icon_ancient_ruins",
                questGiverNpcId: "npc_scholar_marcus",
                questLocationId: "location_ancient_ruins",
                completionCutsceneId: "cutscene_artifact_found",
                acceptedSoundPath: "Audio/SFX/quest_accept_exploration",
                completedSoundPath: "Audio/SFX/quest_complete_discovery",
                showInJournal: true,
                category: "Side"
            );
        }

        // ============================================================
        // CHAIN QUEST - Defeat the Goblin Boss (Requires Tutorial)
        // ============================================================

        public static QuestData ChainGoblinBoss()
        {
            return new QuestData
            {
                Id = "quest_defeat_goblin_boss",
                Title = "The Goblin King",
                Description = "The goblin leader has been spotted in the nearby cave. " +
                              "Defeat him to end the goblin threat once and for all.",
                Objectives = new[]
                {
                    new QuestObjectiveData
                    {
                        Id = "objective_defeat_boss",
                        Description = "Defeat the Goblin King",
                        RequiredCount = 1
                    }
                },
                Rewards = new[]
                {
                    new QuestRewardData
                    {
                        RewardId = "reward_experience",
                        Type = RewardType.Experience,
                        Amount = 300
                    },
                    new QuestRewardData
                    {
                        RewardId = "reward_gold",
                        Type = RewardType.Currency,
                        Amount = 200
                    },
                    new QuestRewardData
                    {
                        Type = RewardType.Item,
                        RewardId = "sword_iron_boss_drop"
                    }
                },
                PrerequisiteQuestIds = new[] { "quest_tutorial_kill_goblins" },
                MinimumLevel = 3,
                IsRepeatable = false
            };
        }

        public static QuestMetadata ChainGoblinBossMetadata()
        {
            return QuestMetadata.CreateMainQuest(
                questGiverNpcId: "npc_elder_graves",
                questLocationId: "location_goblin_cave",
                iconPath: "Textures/Quests/quest_icon_boss_fight"
            );
        }

        // ============================================================
        // DAILY REPEATABLE QUEST - Patrol Perimeter
        // ============================================================

        public static QuestData DailyPatrolPerimeter()
        {
            return new QuestData
            {
                Id = "quest_daily_patrol_perimeter",
                Title = "Daily Patrol",
                Description = "Help keep the village safe by patrolling the perimeter. " +
                              "Defeat any enemies you encounter during patrol.",
                Objectives = new[]
                {
                    new QuestObjectiveData
                    {
                        Id = "objective_patrol_complete",
                        Description = "Complete patrol of village perimeter",
                        RequiredCount = 1
                    },
                    new QuestObjectiveData
                    {
                        Id = "objective_enemies_defeated",
                        Description = "Defeat 5 enemies during patrol",
                        RequiredCount = 5
                    }
                },
                Rewards = new[]
                {
                    new QuestRewardData
                    {
                        RewardId = "reward_experience",
                        Type = RewardType.Experience,
                        Amount = 75
                    },
                    new QuestRewardData
                    {
                        RewardId = "reward_gold",
                        Type = RewardType.Currency,
                        Amount = 40
                    }
                },
                PrerequisiteQuestIds = new string[] { },
                MinimumLevel = 1,
                IsRepeatable = true  // Important: Mark as repeatable
            };
        }

        public static QuestMetadata DailyPatrolPerimeterMetadata()
        {
            return QuestMetadata.CreateRepeatable(
                questGiverNpcId: "npc_captain_kane",
                questLocationId: "location_village_perimeter",
                isDaily: true
            );
        }

        // ============================================================
        // QUEST BATCH FACTORY
        // ============================================================

        /// <summary>
        /// Creates all sample quests with their metadata as a complete package.
        /// Useful for initializing a quest hub or testing system.
        /// </summary>
        /// <returns>Dictionary of quest ID to (QuestData, QuestMetadata) tuples</returns>
        public static Dictionary<string, (QuestData quest, QuestMetadata metadata)> GetAllSampleQuests()
        {
            return new Dictionary<string, (QuestData, QuestMetadata)>
            {
                {
                    "quest_tutorial_kill_goblins",
                    (TutorialKillGoblins(), TutorialKillGoblinsMetadata())
                },
                {
                    "quest_gather_medicinal_herbs",
                    (CollectionGatherHerbs(), CollectionGatherHerbsMetadata())
                },
                {
                    "quest_explore_ancient_ruins",
                    (ExplorationAncientRuins(), ExplorationAncientRuinsMetadata())
                },
                {
                    "quest_defeat_goblin_boss",
                    (ChainGoblinBoss(), ChainGoblinBossMetadata())
                },
                {
                    "quest_daily_patrol_perimeter",
                    (DailyPatrolPerimeter(), DailyPatrolPerimeterMetadata())
                }
            };
        }

        /// <summary>
        /// Gets all sample quests as QuestData array (for registry initialization).
        /// </summary>
        /// <returns>Array of QuestData objects</returns>
        public static QuestData[] GetAllSampleQuestsData()
        {
            return new[]
            {
                TutorialKillGoblins(),
                CollectionGatherHerbs(),
                ExplorationAncientRuins(),
                ChainGoblinBoss(),
                DailyPatrolPerimeter()
            };
        }

        /// <summary>
        /// Gets all sample quest metadata (for MythHunterQuestComponent initialization).
        /// </summary>
        /// <returns>Dictionary of quest ID to metadata</returns>
        public static Dictionary<string, QuestMetadata> GetAllSampleQuestsMetadata()
        {
            return new Dictionary<string, QuestMetadata>
            {
                { "quest_tutorial_kill_goblins", TutorialKillGoblinsMetadata() },
                { "quest_gather_medicinal_herbs", CollectionGatherHerbsMetadata() },
                { "quest_explore_ancient_ruins", ExplorationAncientRuinsMetadata() },
                { "quest_defeat_goblin_boss", ChainGoblinBossMetadata() },
                { "quest_daily_patrol_perimeter", DailyPatrolPerimeterMetadata() }
            };
        }

        /// <summary>
        /// Gets sample main story quests only.
        /// </summary>
        /// <returns>Dictionary of main quest ID to (QuestData, QuestMetadata) tuples</returns>
        public static Dictionary<string, (QuestData quest, QuestMetadata metadata)> GetMainStoryQuests()
        {
            return new Dictionary<string, (QuestData, QuestMetadata)>
            {
                {
                    "quest_tutorial_kill_goblins",
                    (TutorialKillGoblins(), TutorialKillGoblinsMetadata())
                },
                {
                    "quest_defeat_goblin_boss",
                    (ChainGoblinBoss(), ChainGoblinBossMetadata())
                }
            };
        }

        /// <summary>
        /// Gets sample side quests only.
        /// </summary>
        /// <returns>Dictionary of side quest ID to (QuestData, QuestMetadata) tuples</returns>
        public static Dictionary<string, (QuestData quest, QuestMetadata metadata)> GetSideQuests()
        {
            return new Dictionary<string, (QuestData, QuestMetadata)>
            {
                {
                    "quest_gather_medicinal_herbs",
                    (CollectionGatherHerbs(), CollectionGatherHerbsMetadata())
                },
                {
                    "quest_explore_ancient_ruins",
                    (ExplorationAncientRuins(), ExplorationAncientRuinsMetadata())
                }
            };
        }

        /// <summary>
        /// Gets sample repeatable/daily quests only.
        /// </summary>
        /// <returns>Dictionary of repeatable quest ID to (QuestData, QuestMetadata) tuples</returns>
        public static Dictionary<string, (QuestData quest, QuestMetadata metadata)> GetRepeatableQuests()
        {
            return new Dictionary<string, (QuestData, QuestMetadata)>
            {
                {
                    "quest_daily_patrol_perimeter",
                    (DailyPatrolPerimeter(), DailyPatrolPerimeterMetadata())
                }
            };
        }
    }
}
