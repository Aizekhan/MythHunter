// Assets/_MythHunter/Tests/Inventory/InventorySystemIntegrationTests.cs
using NUnit.Framework;
using MythHunter.Components.Character;
using MythHunter.GameLogic.Inventory;
using MythHunter.GameLogic.Inventory.Adapters;
using MythHunter.GameLogic.Inventory.Components;
using MythHunter.Utils.Logging;
using RPGFramework.ECS;
using System.Collections.Generic;
using System.Linq;

namespace MythHunter.Tests.Inventory
{
    [TestFixture]
    public class InventorySystemIntegrationTests
    {
        private IEntityManager _entityManager;
        private InventorySystemIntegration _integration;
        private IMythLogger _logger;

        [SetUp]
        public void Setup()
        {
            _entityManager = new EntityManager();
            _logger = new TestLogger();
            _integration = new InventorySystemIntegration(_entityManager, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            (_entityManager as System.IDisposable)?.Dispose();
        }

        #region Test Entity Helpers

        private int CreateEntityWithNewInventory(int capacity = 50)
        {
            int entityId = _entityManager.CreateEntity();
            var newInv = MythHunterInventoryComponent.CreateHero(capacity);
            _entityManager.AddComponent(entityId, newInv);
            return entityId;
        }

        private int CreateEntityWithOldInventory()
        {
            int entityId = _entityManager.CreateEntity();
            var oldInv = new InventoryComponent
            {
                Items = new List<string>(),
                EquippedItems = new EquippedItems
                {
                    Armor = new Armor(),
                    Weapons = new Weapons()
                },
                Bag = new Bag { Slots = 50, Elixirs = new List<string>() }
            };
            _entityManager.AddComponent(entityId, oldInv);
            return entityId;
        }

        #endregion

        #region Item Container Tests

        [Test]
        public void AddItem_NewComponent_AddsSuccessfully()
        {
            // Arrange
            int entityId = CreateEntityWithNewInventory();

            // Act
            bool result = _integration.AddItem(entityId, "item_sword_01");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_integration.HasItem(entityId, "item_sword_01"), Is.True);
        }

        [Test]
        public void AddItem_OldComponent_AddsSuccessfully()
        {
            // Arrange
            int entityId = CreateEntityWithOldInventory();

            // Act
            bool result = _integration.AddItem(entityId, "item_potion_health");

            // Assert
            Assert.That(result, Is.True);
            var oldInv = _entityManager.GetComponent<InventoryComponent>(entityId);
            Assert.That(oldInv.Items.Contains("item_potion_health"), Is.True);
        }

        [Test]
        public void RemoveItem_NewComponent_RemovesSuccessfully()
        {
            // Arrange
            int entityId = CreateEntityWithNewInventory();
            _integration.AddItem(entityId, "item_artifact_01");

            // Act
            bool result = _integration.RemoveItem(entityId, "item_artifact_01");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_integration.HasItem(entityId, "item_artifact_01"), Is.False);
        }

        [Test]
        public void GetInventoryCount_NewComponent_ReturnsCorrectCount()
        {
            // Arrange
            int entityId = CreateEntityWithNewInventory();
            _integration.AddItem(entityId, "item1");
            _integration.AddItem(entityId, "item2");
            _integration.AddItem(entityId, "item3");

            // Act
            int count = _integration.GetInventoryCount(entityId);

            // Assert
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public void IsInventoryFull_NewComponent_ReturnsFalseWhenNotFull()
        {
            // Arrange
            int entityId = CreateEntityWithNewInventory(capacity: 10);
            _integration.AddItem(entityId, "item1");

            // Act
            bool isFull = _integration.IsInventoryFull(entityId);

            // Assert
            Assert.That(isFull, Is.False);
        }

        #endregion

        #region Equipment Tests

        [Test]
        public void EquipItem_NewComponent_EquipsToCorrectSlot()
        {
            // Arrange
            int entityId = CreateEntityWithNewInventory();

            // Act
            bool result = _integration.EquipItem(entityId, "helmet_iron_01", MythHunterEquipmentSlot.Helmet);

            // Assert
            Assert.That(result, Is.True);
            var equipped = _integration.GetEquippedItem(entityId, MythHunterEquipmentSlot.Helmet);
            Assert.That(equipped, Is.EqualTo("helmet_iron_01"));
        }

        [Test]
        public void UnequipItem_NewComponent_RemovesFromSlot()
        {
            // Arrange
            int entityId = CreateEntityWithNewInventory();
            _integration.EquipItem(entityId, "chest_steel_01", MythHunterEquipmentSlot.Chest);

            // Act
            string unequipped = _integration.UnequipItem(entityId, MythHunterEquipmentSlot.Chest);

            // Assert
            Assert.That(unequipped, Is.EqualTo("chest_steel_01"));
            var equipped = _integration.GetEquippedItem(entityId, MythHunterEquipmentSlot.Chest);
            Assert.That(equipped, Is.Null);
        }

        [Test]
        public void GetCurrentWeapon_NewComponent_ReturnsCorrectWeapon()
        {
            // Arrange
            int entityId = CreateEntityWithNewInventory();
            _integration.EquipItem(entityId, "sword_legendary", MythHunterEquipmentSlot.MainWeapon);

            // Act
            string currentWeapon = _integration.GetCurrentWeapon(entityId);

            // Assert
            Assert.That(currentWeapon, Is.EqualTo("sword_legendary"));
        }

        [Test]
        public void EquipItem_OldComponent_MapsToOldStructure()
        {
            // Arrange
            int entityId = CreateEntityWithOldInventory();

            // Act
            bool result = _integration.EquipItem(entityId, "gloves_leather", MythHunterEquipmentSlot.Gloves);

            // Assert
            Assert.That(result, Is.True);
            var oldInv = _entityManager.GetComponent<InventoryComponent>(entityId);
            Assert.That(oldInv.EquippedItems.Armor.Gloves, Is.EqualTo("gloves_leather"));
        }

        #endregion

        #region Query Tests

        [Test]
        public void HasInventory_WithNewComponent_ReturnsTrue()
        {
            // Arrange
            int entityId = CreateEntityWithNewInventory();

            // Act
            bool result = _integration.HasInventory(entityId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasInventory_WithOldComponent_ReturnsTrue()
        {
            // Arrange
            int entityId = CreateEntityWithOldInventory();

            // Act
            bool result = _integration.HasInventory(entityId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasInventory_WithoutComponents_ReturnsFalse()
        {
            // Arrange
            int entityId = _entityManager.CreateEntity();

            // Act
            bool result = _integration.HasInventory(entityId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetAllItems_NewComponent_ReturnsAllItems()
        {
            // Arrange
            int entityId = CreateEntityWithNewInventory();
            _integration.AddItem(entityId, "item_a");
            _integration.AddItem(entityId, "item_b");
            _integration.AddItem(entityId, "item_c");

            // Act
            var items = _integration.GetAllItems(entityId);

            // Assert
            Assert.That(items.Count, Is.EqualTo(3));
            Assert.That(items.Contains("item_a"), Is.True);
            Assert.That(items.Contains("item_b"), Is.True);
            Assert.That(items.Contains("item_c"), Is.True);
        }

        [Test]
        public void GetEntitiesWithInventory_ReturnsAllInventoryEntities()
        {
            // Arrange
            int entity1 = CreateEntityWithNewInventory();
            int entity2 = CreateEntityWithOldInventory();
            int entity3 = _entityManager.CreateEntity(); // No inventory

            // Act
            var entities = _integration.GetEntitiesWithInventory().ToList();

            // Assert
            Assert.That(entities.Contains(entity1), Is.True);
            Assert.That(entities.Contains(entity2), Is.True);
            Assert.That(entities.Contains(entity3), Is.False);
        }

        #endregion

        // Simple test logger implementation
        private class TestLogger : IMythLogger
        {
            public void LogInfo(string message, string category = null) { }
            public void LogWarning(string message, string category = null) { }
            public void LogError(string message, string category = null, System.Exception exception = null) { }
            public void LogFatal(string message, string category = null) { }
            public void LogDebug(string message, string category = null) { }
            public void LogTrace(string message, string category = null) { }
            public void WithContext(string key, object value) { }
            public void ClearContext() { }
            public void SetDefaultCategory(string category) { }
            public void SetMinLogLevel(MythHunter.Utils.Logging.LogLevel level) { }
            public void EnableFileLogging(bool enable) { }
            public void Log(RPGFramework.Logging.LogSeverity severity, string message, string category = null, System.Exception exception = null) { }
        }
    }
}
