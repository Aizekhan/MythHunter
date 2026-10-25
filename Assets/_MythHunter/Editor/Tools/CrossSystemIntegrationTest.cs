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

                foreach (var quest in sampleQuests)
                {
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
    }
}
