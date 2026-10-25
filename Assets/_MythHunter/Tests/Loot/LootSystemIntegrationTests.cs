// Assets/_MythHunter/Tests/Loot/LootSystemIntegrationTests.cs

using NUnit.Framework;
using MythHunter.GameLogic.Loot.Adapters;
using MythHunter.GameLogic.Loot.Components;
using MythHunter.GameLogic.Loot.Data;
using MythHunter.GameLogic.Loot.Utilities;
using MythHunter.Components.Movement;
using MythHunter.Utils.Logging;
using RPGFramework.ECS;
using RPGFramework.Loot;
using RPGFramework.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MythHunter.Tests.Loot
{
    [TestFixture]
    public class LootSystemIntegrationTests
    {
        private IEntityManager _entityManager;
        private LootSystemIntegration _integration;
        private ILootGenerator _lootGenerator;
        private ILootTableRegistry _lootTableRegistry;
        private IItemRegistry _itemRegistry;
        private IMythLogger _logger;

        [SetUp]
        public void Setup()
        {
            _entityManager = new EntityManager();
            _lootTableRegistry = new LootTableRegistry();
            _itemRegistry = new ItemRegistry();
            _lootGenerator = new LootGenerator(_lootTableRegistry);
            _logger = new TestLogger();

            _integration = new LootSystemIntegration(
                _entityManager,
                _lootGenerator,
                _lootTableRegistry,
                _logger
            );

            // Register sample items for testing
            RegisterTestItems();
        }

        [TearDown]
        public void TearDown()
        {
            (_entityManager as IDisposable)?.Dispose();
        }

        private void RegisterTestItems()
        {
            // Register basic test items
            // Cast to ItemRegistry to access RegisterItem method
            var registry = (ItemRegistry)_itemRegistry;

            registry.RegisterItem(new Item(
                itemId: "gold_coins",
                name: "Gold Coins",
                description: "Currency",
                itemType: ItemType.Currency,
                rarity: ItemRarity.Common,
                quality: ItemQuality.Normal,
                maxStackSize: 9999
            ));

            registry.RegisterItem(new Item(
                itemId: "health_potion",
                name: "Health Potion",
                description: "Restores health",
                itemType: ItemType.Consumable,
                rarity: ItemRarity.Common,
                quality: ItemQuality.Normal,
                maxStackSize: 99
            ));

            registry.RegisterItem(new Item(
                itemId: "leather_gloves",
                name: "Leather Gloves",
                description: "Basic armor",
                itemType: ItemType.Equipment,
                rarity: ItemRarity.Uncommon,
                quality: ItemQuality.Normal,
                maxStackSize: 1
            ));

            registry.RegisterItem(new Item(
                itemId: "exp_orb_small",
                name: "Small XP Orb",
                description: "Grants experience",
                itemType: ItemType.Consumable,
                rarity: ItemRarity.Common,
                quality: ItemQuality.Normal,
                maxStackSize: 99
            ));
        }

        #region Helper Methods

        private int CreateEntityWithLootComponent(string tableId, int level = 1)
        {
            int entityId = _entityManager.CreateEntity();
            var lootComp = MythHunterLootComponent.CreateEnemy(tableId, level);
            _entityManager.AddComponent(entityId, lootComp);

            // Add PositionComponent (required for loot drops)
            var posComp = new PositionComponent
            {
                Position = new Vector3(10f, 0f, 10f),
                Rotation = Quaternion.identity,
                Scale = Vector3.one
            };
            _entityManager.AddComponent(entityId, posComp);

            return entityId;
        }

        private ILootTable CreateTestLootTable()
        {
            return LootTableBuilder
                .Create("test_table", "Test Loot Table")
                .WithDrops(minDrops: 1, maxDrops: 3)
                .AddCommon("gold_coins", minQty: 10, maxQty: 20)
                .AddUncommon("health_potion", minQty: 1, maxQty: 2)
                .AddGuaranteedDrop("exp_orb_small")
                .Build();
        }

        #endregion

        #region LootTableBuilder Tests

        [Test]
        public void LootTableBuilder_Create_BuildsTableSuccessfully()
        {
            // Act
            var table = LootTableBuilder
                .Create("builder_test", "Builder Test Table")
                .WithDrops(minDrops: 2, maxDrops: 5)
                .Build();

            // Assert
            Assert.That(table, Is.Not.Null);
            Assert.That(table.TableId, Is.EqualTo("builder_test"));
            Assert.That(table.Name, Is.EqualTo("Builder Test Table"));
            Assert.That(table.MinDrops, Is.EqualTo(2));
            Assert.That(table.MaxDrops, Is.EqualTo(5));
        }

        [Test]
        public void LootTableBuilder_AddEntry_AddsToEntries()
        {
            // Act
            var table = LootTableBuilder
                .Create("entry_test", "Entry Test")
                .AddEntry("test_item", weight: 100, minQty: 1, maxQty: 3, dropChance: 0.5f)
                .Build();

            // Assert
            Assert.That(table.Entries, Is.Not.Null);
            Assert.That(table.Entries.Count, Is.EqualTo(1));
            Assert.That(table.Entries[0].ItemId, Is.EqualTo("test_item"));
            Assert.That(table.Entries[0].Weight, Is.EqualTo(100));
            Assert.That(table.Entries[0].MinQuantity, Is.EqualTo(1));
            Assert.That(table.Entries[0].MaxQuantity, Is.EqualTo(3));
            Assert.That(table.Entries[0].DropChance, Is.EqualTo(0.5f));
        }

        [Test]
        public void LootTableBuilder_AddRarityShortcuts_CreatesCorrectWeights()
        {
            // Act
            var table = LootTableBuilder
                .Create("rarity_test", "Rarity Test")
                .AddCommon("common_item")
                .AddUncommon("uncommon_item")
                .AddRare("rare_item")
                .AddEpic("epic_item")
                .AddLegendary("legendary_item")
                .Build();

            // Assert
            Assert.That(table.Entries.Count, Is.EqualTo(5));

            // Common: weight 100, dropChance 0.8
            Assert.That(table.Entries[0].Weight, Is.EqualTo(100));
            Assert.That(table.Entries[0].DropChance, Is.EqualTo(0.8f));

            // Uncommon: weight 50, dropChance 0.5
            Assert.That(table.Entries[1].Weight, Is.EqualTo(50));
            Assert.That(table.Entries[1].DropChance, Is.EqualTo(0.5f));

            // Rare: weight 20, dropChance 0.2
            Assert.That(table.Entries[2].Weight, Is.EqualTo(20));
            Assert.That(table.Entries[2].DropChance, Is.EqualTo(0.2f));

            // Epic: weight 5, dropChance 0.05
            Assert.That(table.Entries[3].Weight, Is.EqualTo(5));
            Assert.That(table.Entries[3].DropChance, Is.EqualTo(0.05f));

            // Legendary: weight 1, dropChance 0.01
            Assert.That(table.Entries[4].Weight, Is.EqualTo(1));
            Assert.That(table.Entries[4].DropChance, Is.EqualTo(0.01f));
        }

        [Test]
        public void LootTableBuilder_AddGuaranteedDrop_AddsToGuaranteedDrops()
        {
            // Act
            var table = LootTableBuilder
                .Create("guaranteed_test", "Guaranteed Test")
                .AddGuaranteedDrop("guaranteed_item_1")
                .AddGuaranteedDrop("guaranteed_item_2")
                .Build();

            // Assert
            Assert.That(table.GuaranteedDrops, Is.Not.Null);
            Assert.That(table.GuaranteedDrops.Count, Is.EqualTo(2));
            Assert.That(table.GuaranteedDrops.Contains("guaranteed_item_1"), Is.True);
            Assert.That(table.GuaranteedDrops.Contains("guaranteed_item_2"), Is.True);
        }

        [Test]
        public void LootTableBuilder_AllowDuplicates_SetsFlag()
        {
            // Act
            var table = LootTableBuilder
                .Create("duplicates_test", "Duplicates Test")
                .AllowDuplicates(true)
                .Build();

            // Assert
            Assert.That(table.AllowDuplicates, Is.True);
        }

        #endregion

        #region Sample Loot Tables Validation Tests

        [Test]
        public void SampleLootTables_GoblinCommon_HasCorrectStructure()
        {
            // Act
            var table = SampleLootTables.GoblinCommon();

            // Assert
            Assert.That(table.TableId, Is.EqualTo("goblin_common"));
            Assert.That(table.Name, Is.EqualTo("Common Goblin Loot"));
            Assert.That(table.MinDrops, Is.EqualTo(1));
            Assert.That(table.MaxDrops, Is.EqualTo(2));
            Assert.That(table.Entries.Count, Is.GreaterThan(0));
            Assert.That(table.GuaranteedDrops.Count, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void SampleLootTables_BossDragonAncient_HasGuaranteedDrops()
        {
            // Act
            var table = SampleLootTables.BossDragonAncient();

            // Assert
            Assert.That(table.TableId, Is.EqualTo("boss_dragon_ancient"));
            Assert.That(table.MinDrops, Is.EqualTo(8));
            Assert.That(table.MaxDrops, Is.EqualTo(12));
            Assert.That(table.GuaranteedDrops.Count, Is.GreaterThanOrEqualTo(3)); // exp_orb, boss_soul, gem
        }

        [Test]
        public void SampleLootTables_AllTables_HaveValidDropRanges()
        {
            // Act
            var allTables = SampleLootTables.GetAllTables();

            // Assert
            foreach (var table in allTables)
            {
                Assert.That(table.MinDrops, Is.GreaterThanOrEqualTo(0), $"Table {table.TableId} has invalid MinDrops");
                Assert.That(table.MaxDrops, Is.GreaterThanOrEqualTo(table.MinDrops), $"Table {table.TableId} has MaxDrops < MinDrops");
            }
        }

        [Test]
        public void SampleLootTables_GetAllTables_Returns8Tables()
        {
            // Act
            var allTables = SampleLootTables.GetAllTables();

            // Assert
            Assert.That(allTables.Count, Is.EqualTo(8));

            // Verify all expected table IDs are present
            var tableIds = allTables.Select(t => t.TableId).ToList();
            Assert.That(tableIds.Contains("goblin_common"), Is.True);
            Assert.That(tableIds.Contains("goblin_elite"), Is.True);
            Assert.That(tableIds.Contains("skeleton_warrior"), Is.True);
            Assert.That(tableIds.Contains("chest_wooden"), Is.True);
            Assert.That(tableIds.Contains("chest_iron"), Is.True);
            Assert.That(tableIds.Contains("chest_golden"), Is.True);
            Assert.That(tableIds.Contains("boss_goblin_king"), Is.True);
            Assert.That(tableIds.Contains("boss_dragon_ancient"), Is.True);
        }

        #endregion

        #region LootSystemIntegration Tests

        [Test]
        public void HasLootTable_WithLootComponent_ReturnsTrue()
        {
            // Arrange
            var testTable = CreateTestLootTable();
            _lootTableRegistry.RegisterTable(testTable);
            int entityId = CreateEntityWithLootComponent("test_table");

            // Act
            bool result = _integration.HasLootTable(entityId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasLootTable_WithoutLootComponent_ReturnsFalse()
        {
            // Arrange
            int entityId = _entityManager.CreateEntity();

            // Act
            bool result = _integration.HasLootTable(entityId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetLootTableId_WithLootComponent_ReturnsCorrectId()
        {
            // Arrange
            int entityId = CreateEntityWithLootComponent("test_table");

            // Act
            string tableId = _integration.GetLootTableId(entityId);

            // Assert
            Assert.That(tableId, Is.EqualTo("test_table"));
        }

        [Test]
        public void GetEntityLevel_WithLootComponent_ReturnsCorrectLevel()
        {
            // Arrange
            int entityId = CreateEntityWithLootComponent("test_table", level: 5);

            // Act
            int level = _integration.GetEntityLevel(entityId);

            // Assert
            Assert.That(level, Is.EqualTo(5));
        }

        [Test]
        public void GetLuckModifier_WithLootComponent_ReturnsCorrectModifier()
        {
            // Arrange
            // Create entity with custom luck modifier
            int entityId = _entityManager.CreateEntity();
            var lootComp = MythHunterLootComponent.CreateBoss("test_table", level: 1, luckBonus: 1.5f);
            _entityManager.AddComponent(entityId, lootComp);

            // Add PositionComponent
            var posComp = new PositionComponent
            {
                Position = new Vector3(10f, 0f, 10f),
                Rotation = Quaternion.identity,
                Scale = Vector3.one
            };
            _entityManager.AddComponent(entityId, posComp);

            // Act
            float luck = _integration.GetLuckModifier(entityId);

            // Assert
            Assert.That(luck, Is.EqualTo(1.5f));
        }

        [Test]
        public void IsLootGenerated_BeforeGeneration_ReturnsFalse()
        {
            // Arrange
            int entityId = CreateEntityWithLootComponent("test_table");

            // Act
            bool result = _integration.IsLootGenerated(entityId);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region Edge Case Tests

        [Test]
        public void GenerateLoot_WithMissingLootTable_ReturnsEmptyResult()
        {
            // Arrange
            int entityId = CreateEntityWithLootComponent("non_existent_table");

            // Act
            var result = _integration.GenerateLoot(entityId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Drops.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetEntitiesWithLoot_ReturnsOnlyEntitiesWithLootComponent()
        {
            // Arrange
            int entity1 = CreateEntityWithLootComponent("table1");
            int entity2 = CreateEntityWithLootComponent("table2");
            int entity3 = _entityManager.CreateEntity(); // No loot component

            // Act
            var entities = _integration.GetEntitiesWithLoot().ToList();

            // Assert
            Assert.That(entities.Contains(entity1), Is.True);
            Assert.That(entities.Contains(entity2), Is.True);
            Assert.That(entities.Contains(entity3), Is.False);
        }

        [Test]
        public void HasLootTableInRegistry_WithRegisteredTable_ReturnsTrue()
        {
            // Arrange
            var testTable = CreateTestLootTable();
            _lootTableRegistry.RegisterTable(testTable);

            // Act
            bool result = _integration.HasLootTableInRegistry("test_table");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasLootTableInRegistry_WithUnregisteredTable_ReturnsFalse()
        {
            // Act
            bool result = _integration.HasLootTableInRegistry("unregistered_table");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetAllLootTableIds_ReturnsAllRegisteredTableIds()
        {
            // Arrange
            var table1 = LootTableBuilder.Create("table1", "Table 1").Build();
            var table2 = LootTableBuilder.Create("table2", "Table 2").Build();
            _lootTableRegistry.RegisterTable(table1);
            _lootTableRegistry.RegisterTable(table2);

            // Act
            var tableIds = _integration.GetAllLootTableIds().ToList();

            // Assert
            Assert.That(tableIds.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(tableIds.Contains("table1"), Is.True);
            Assert.That(tableIds.Contains("table2"), Is.True);
        }

        #endregion

        // Simple test logger implementation
        private class TestLogger : IMythLogger
        {
            public void LogInfo(string message, string category = null) { }
            public void LogWarning(string message, string category = null) { }
            public void LogError(string message, string category = null, Exception exception = null) { }
            public void LogFatal(string message, string category = null) { }
            public void LogDebug(string message, string category = null) { }
            public void LogTrace(string message, string category = null) { }
            public void WithContext(string key, object value) { }
            public void ClearContext() { }
            public void SetDefaultCategory(string category) { }
            public void SetMinLogLevel(MythHunter.Utils.Logging.LogLevel level) { }
            public void EnableFileLogging(bool enable) { }
            public void Log(RPGFramework.Logging.LogSeverity severity, string message, string category = null, Exception exception = null) { }
        }
    }
}
