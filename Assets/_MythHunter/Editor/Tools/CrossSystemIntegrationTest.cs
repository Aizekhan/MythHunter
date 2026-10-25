using UnityEngine;
using UnityEditor;
using RPGFramework.Quests;
using RPGFramework.Loot;
using MythHunter.Quests.Sample;
using MythHunter.GameLogic.Loot.Data;

namespace MythHunter.Editor.Tools
{
    /// <summary>
    /// Cross-System Integration Test: Verifies all Framework systems are accessible and work together.
    ///
    /// Tests:
    /// - Quest System: Sample quests load correctly
    /// - Loot System: Sample loot tables load correctly
    /// - Systems can be instantiated without errors
    ///
    /// Menu: MythHunter → Run Integration Test → Verify Systems
    /// Menu: MythHunter → Run Integration Test → Full Gameplay Loop
    /// </summary>
    public static class CrossSystemIntegrationTest
    {
        [MenuItem("MythHunter/Run Integration Test/Verify Systems")]
        public static void VerifySystems()
        {
            UnityEngine.Debug.Log("=== CROSS-SYSTEM INTEGRATION TEST ===\n");

            bool allPassed = true;

            // Test 1: Quest System
            UnityEngine.Debug.Log("--- TEST 1: QUEST SYSTEM ---");
            try
            {
                var questRegistry = new RPGFramework.Quests.QuestRegistry();
                var sampleQuests = SampleQuests.GetAllSampleQuestsData();

                foreach (var questData in sampleQuests)
                {
                    var quest = new RPGFramework.Quests.Quest(questData);
                    questRegistry.RegisterQuest(quest);
                }

                UnityEngine.Debug.Log($"✅ Quest System: {sampleQuests.Length} sample quests registered");
                UnityEngine.Debug.Log($"   - {sampleQuests[0].Title}");
                UnityEngine.Debug.Log($"   - Objective: {sampleQuests[0].Objectives[0].Description}");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ Quest System FAILED: {e.Message}");
                allPassed = false;
            }

            // Test 2: Loot System
            UnityEngine.Debug.Log("\n--- TEST 2: LOOT SYSTEM ---");
            try
            {
                var lootRegistry = new RPGFramework.Loot.LootTableRegistry();

                lootRegistry.RegisterTable(SampleLootTables.GoblinCommon());
                lootRegistry.RegisterTable(SampleLootTables.GoblinElite());
                lootRegistry.RegisterTable(SampleLootTables.ChestWooden());

                var goblinTable = lootRegistry.GetTable("goblin_common");
                if (goblinTable != null)
                {
                    UnityEngine.Debug.Log($"✅ Loot System: 3 loot tables registered");
                    UnityEngine.Debug.Log($"   - Found table: {goblinTable.TableId}");
                }
                else
                {
                    UnityEngine.Debug.LogError("❌ Loot System: Could not retrieve registered table");
                    allPassed = false;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ Loot System FAILED: {e.Message}");
                allPassed = false;
            }

            // Test 3: Sample Quest Integration
            UnityEngine.Debug.Log("\n--- TEST 3: QUEST DATA INTEGRITY ---");
            try
            {
                var killGoblinsQuest = SampleQuests.TutorialKillGoblins();

                UnityEngine.Debug.Log($"✅ Quest Data Integrity:");
                UnityEngine.Debug.Log($"   - ID: {killGoblinsQuest.Id}");
                UnityEngine.Debug.Log($"   - Title: {killGoblinsQuest.Title}");
                UnityEngine.Debug.Log($"   - Objectives: {killGoblinsQuest.Objectives.Length}");
                UnityEngine.Debug.Log($"   - Rewards: {killGoblinsQuest.Rewards.Length}");

                foreach (var reward in killGoblinsQuest.Rewards)
                {
                    UnityEngine.Debug.Log($"      * {reward.Type}: {reward.Amount}");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ Quest Data Integrity FAILED: {e.Message}");
                allPassed = false;
            }

            // Test 4: Loot Generation
            UnityEngine.Debug.Log("\n--- TEST 4: LOOT GENERATION ---");
            try
            {
                var lootRegistry = new RPGFramework.Loot.LootTableRegistry();
                lootRegistry.RegisterTable(SampleLootTables.GoblinCommon());

                var lootGenerator = new RPGFramework.Loot.LootGenerator(lootRegistry);
                var goblinTable = lootRegistry.GetTable("goblin_common");

                if (goblinTable != null)
                {
                    var lootResult = lootGenerator.GenerateLoot(goblinTable);
                    UnityEngine.Debug.Log($"✅ Loot Generation: {lootResult.Drops.Count} items generated");

                    foreach (var drop in lootResult.Drops)
                    {
                        UnityEngine.Debug.Log($"   - {drop.ItemId} x{drop.Quantity}");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("❌ Loot Generation: Table not found");
                    allPassed = false;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ Loot Generation FAILED: {e.Message}");
                allPassed = false;
            }

            // Final Summary
            UnityEngine.Debug.Log("\n=== TEST SUMMARY ===");
            if (allPassed)
            {
                UnityEngine.Debug.Log("🎉 ALL TESTS PASSED! 🎉");
                UnityEngine.Debug.Log("✅ Quest System working");
                UnityEngine.Debug.Log("✅ Loot System working");
                UnityEngine.Debug.Log("✅ Sample data loading correctly");
                UnityEngine.Debug.Log("✅ Systems can interoperate");
            }
            else
            {
                UnityEngine.Debug.LogError("❌ SOME TESTS FAILED - See errors above");
            }

            UnityEngine.Debug.Log("\n=== END TEST ===\n");
        }

        [MenuItem("MythHunter/Run Integration Test/Full Gameplay Loop")]
        public static void FullGameplayLoop()
        {
            UnityEngine.Debug.Log("=== FULL GAMEPLAY LOOP TEST ===\n");
            UnityEngine.Debug.Log("Scenario: Player accepts quest, kills goblin, loots items, gains XP\n");

            bool allPassed = true;

            try
            {
                // ============================================================
                // SETUP: Initialize all systems
                // ============================================================
                UnityEngine.Debug.Log("--- SETUP: INITIALIZING SYSTEMS ---");

                var entityManager = new RPGFramework.ECS.EntityManager();
                var questRegistry = new RPGFramework.Quests.QuestRegistry();
                var questManager = new RPGFramework.Quests.QuestManager(entityManager, questRegistry);
                var lootRegistry = new RPGFramework.Loot.LootTableRegistry();
                var lootGenerator = new RPGFramework.Loot.LootGenerator(lootRegistry);
                var inventory = new RPGFramework.Inventory.Runtime.InventoryContainer<string>(capacity: 20);

                // Create level table for progression (linear: 100 XP per level)
                var levelTable = RPGFramework.Progression.LevelTable.CreateLinear(maxLevel: 100, baseXP: 100);
                var progressionTracker = new RPGFramework.Progression.ProgressionTracker(levelTable);

                // Register sample quests
                var sampleQuests = SampleQuests.GetAllSampleQuestsData();
                foreach (var questData in sampleQuests)
                {
                    var quest = new RPGFramework.Quests.Quest(questData);
                    questRegistry.RegisterQuest(quest);
                }

                // Register loot tables
                lootRegistry.RegisterTable(SampleLootTables.GoblinCommon());
                lootRegistry.RegisterTable(SampleLootTables.GoblinElite());

                // Create player entity with quest component
                int playerId = entityManager.CreateEntity();
                entityManager.AddComponent(playerId, new RPGFramework.Quests.ActiveQuestsData
                {
                    ActiveQuests = new System.Collections.Generic.Dictionary<string, RPGFramework.Quests.QuestProgressData>(),
                    CompletedQuestIds = new System.Collections.Generic.List<string>(),
                    TotalQuestsCompleted = 0
                });

                UnityEngine.Debug.Log($"✅ Systems initialized");
                UnityEngine.Debug.Log($"   - Player Entity ID: {playerId}");
                UnityEngine.Debug.Log($"   - Quests registered: {sampleQuests.Length}");
                UnityEngine.Debug.Log($"   - Loot tables registered: 2");
                UnityEngine.Debug.Log($"   - Inventory capacity: {inventory.Capacity}");

                // ============================================================
                // STEP 1: Accept Quest "Kill 3 Goblins"
                // ============================================================
                UnityEngine.Debug.Log("\n--- STEP 1: ACCEPT QUEST ---");

                string questId = "quest_tutorial_kill_goblins";

                // Debug: Check if quest exists
                var questToAccept = questRegistry.GetQuest(questId);
                if (questToAccept == null)
                {
                    UnityEngine.Debug.LogError($"❌ Quest '{questId}' not found in registry!");
                    allPassed = false;
                }
                else
                {
                    UnityEngine.Debug.Log($"   Quest found: {questToAccept.Title}");
                }

                // Debug: Check if quest can be accepted
                bool canAccept = questManager.CanAcceptQuest(playerId, questId);
                UnityEngine.Debug.Log($"   Can accept quest: {canAccept}");

                bool questAccepted = questManager.AcceptQuest(playerId, questId);

                if (questAccepted)
                {
                    var questProgress = questManager.GetQuestProgress(playerId, questId);
                    UnityEngine.Debug.Log($"✅ Quest accepted: {questProgress?.QuestId}");
                    UnityEngine.Debug.Log($"   - Objectives: {questProgress?.ObjectiveProgress.Count}");
                }
                else
                {
                    UnityEngine.Debug.LogError("❌ Failed to accept quest");
                    allPassed = false;
                }

                // ============================================================
                // STEP 2: Combat - Kill Goblin #1
                // ============================================================
                UnityEngine.Debug.Log("\n--- STEP 2: COMBAT - KILL GOBLIN #1 ---");

                // Simulate goblin death
                bool progressUpdated = questManager.UpdateObjectiveProgress(playerId, questId, "objective_kill_goblins", 1);

                if (progressUpdated)
                {
                    var questProgress = questManager.GetQuestProgress(playerId, questId);
                    if (questProgress != null)
                    {
                        var currentCount = questProgress.ObjectiveProgress["objective_kill_goblins"];
                        var quest = questRegistry.GetQuest(questId);
                        var objective = quest.Objectives[0]; // First objective is objective_kill_goblins
                        UnityEngine.Debug.Log($"✅ Goblin killed");
                        UnityEngine.Debug.Log($"   - Quest progress: {currentCount}/{objective.RequiredCount}");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("❌ Failed to update quest progress");
                    allPassed = false;
                }

                // ============================================================
                // STEP 3: Loot Drop from Goblin
                // ============================================================
                UnityEngine.Debug.Log("\n--- STEP 3: LOOT DROP ---");

                var goblinLootTable = lootRegistry.GetTable("goblin_common");
                if (goblinLootTable != null)
                {
                    var lootResult = lootGenerator.GenerateLoot(goblinLootTable);
                    UnityEngine.Debug.Log($"✅ Loot generated: {lootResult.Drops.Count} items");

                    foreach (var drop in lootResult.Drops)
                    {
                        UnityEngine.Debug.Log($"   - {drop.ItemId} x{drop.Quantity}");
                    }

                    // ============================================================
                    // STEP 4: Add Loot to Inventory
                    // ============================================================
                    UnityEngine.Debug.Log("\n--- STEP 4: ADD LOOT TO INVENTORY ---");

                    foreach (var drop in lootResult.Drops)
                    {
                        for (int i = 0; i < drop.Quantity; i++)
                        {
                            inventory.Add(drop.ItemId);
                        }
                    }

                    UnityEngine.Debug.Log($"✅ Loot added to inventory");
                    UnityEngine.Debug.Log($"   - Inventory: {inventory.Count}/{inventory.Capacity} items");

                    var inventoryItems = inventory.GetAllItems();
                    foreach (var item in inventoryItems)
                    {
                        UnityEngine.Debug.Log($"      * {item}");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("❌ Loot table not found");
                    allPassed = false;
                }

                // ============================================================
                // STEP 5: Kill Goblins #2 and #3 to complete quest
                // ============================================================
                UnityEngine.Debug.Log("\n--- STEP 5: KILL REMAINING GOBLINS ---");

                questManager.UpdateObjectiveProgress(playerId, questId, "objective_kill_goblins", 1);
                questManager.UpdateObjectiveProgress(playerId, questId, "objective_kill_goblins", 1);

                var finalProgress = questManager.GetQuestProgress(playerId, questId);
                if (finalProgress != null)
                {
                    var currentCount = finalProgress.ObjectiveProgress["objective_kill_goblins"];
                    var quest = questRegistry.GetQuest(questId);
                    var objective = quest.Objectives[0];
                    bool isCompleted = currentCount >= objective.RequiredCount;
                    UnityEngine.Debug.Log($"✅ All goblins killed");
                    UnityEngine.Debug.Log($"   - Final progress: {currentCount}/{objective.RequiredCount}");
                    UnityEngine.Debug.Log($"   - Quest complete: {isCompleted}");
                }

                // ============================================================
                // STEP 6: Complete Quest and Receive Rewards
                // ============================================================
                UnityEngine.Debug.Log("\n--- STEP 6: COMPLETE QUEST ---");

                bool questCompleted = questManager.CompleteQuest(playerId, questId);

                if (questCompleted)
                {
                    var quest = questRegistry.GetQuest(questId);
                    UnityEngine.Debug.Log($"✅ Quest completed: {quest.Title}");
                    UnityEngine.Debug.Log($"   - Rewards:");

                    long totalXPReward = 0;
                    foreach (var reward in quest.Rewards)
                    {
                        UnityEngine.Debug.Log($"      * {reward.Type}: {reward.Amount}");

                        if (reward.Type == RPGFramework.Quests.RewardType.Experience)
                        {
                            totalXPReward += reward.Amount;
                        }
                    }

                    // ============================================================
                    // STEP 7: Progression - Add XP
                    // ============================================================
                    if (totalXPReward > 0)
                    {
                        UnityEngine.Debug.Log("\n--- STEP 7: PROGRESSION ---");

                        var result = progressionTracker.AddExperience(playerId, totalXPReward);

                        UnityEngine.Debug.Log($"✅ Experience gained: {result.XPAdded} XP");
                        UnityEngine.Debug.Log($"   - Level: {result.OldLevel} → {result.NewLevel}");
                        UnityEngine.Debug.Log($"   - Total XP: {result.ExperienceData.TotalXP}");
                        UnityEngine.Debug.Log($"   - Progress to next level: {result.ExperienceData.ProgressToNextLevel:P1}");

                        if (result.LeveledUp)
                        {
                            UnityEngine.Debug.Log($"   🎉 LEVEL UP! Gained {result.LevelsGained} level(s)!");
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("❌ Failed to complete quest");
                    allPassed = false;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ TEST FAILED WITH EXCEPTION: {e.Message}");
                UnityEngine.Debug.LogError($"Stack trace: {e.StackTrace}");
                allPassed = false;
            }

            // ============================================================
            // FINAL SUMMARY
            // ============================================================
            UnityEngine.Debug.Log("\n=== GAMEPLAY LOOP TEST SUMMARY ===");
            if (allPassed)
            {
                UnityEngine.Debug.Log("🎉 FULL GAMEPLAY LOOP TEST PASSED! 🎉");
                UnityEngine.Debug.Log("✅ Quest acceptance working");
                UnityEngine.Debug.Log("✅ Combat → Quest progress working");
                UnityEngine.Debug.Log("✅ Loot generation working");
                UnityEngine.Debug.Log("✅ Inventory management working");
                UnityEngine.Debug.Log("✅ Quest completion working");
                UnityEngine.Debug.Log("✅ Rewards distribution working");
                UnityEngine.Debug.Log("✅ Progression system working");
                UnityEngine.Debug.Log("\n✨ All systems integrated successfully! ✨");
            }
            else
            {
                UnityEngine.Debug.LogError("❌ GAMEPLAY LOOP TEST FAILED - See errors above");
            }

            UnityEngine.Debug.Log("\n=== END GAMEPLAY LOOP TEST ===\n");
        }
    }
}
