// Assets/_MythHunter/Tests/Combat/CombatComponentMigrationTests.cs
using NUnit.Framework;
using MythHunter.Components.Combat;
using MythHunter.GameLogic.Combat;
using MythHunter.GameLogic.Combat.Migration;
using MythHunter.Utils.Logging;
using RPGFramework.ECS;

namespace MythHunter.Tests.Combat
{
    [TestFixture]
    public class CombatComponentMigrationTests
    {
        private IEntityManager _entityManager;
        private CombatComponentMigration _migration;
        private IMythLogger _logger;

        [SetUp]
        public void Setup()
        {
            _entityManager = new EntityManager();
            _logger = new TestLogger();
            _migration = new CombatComponentMigration(_entityManager, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            (_entityManager as System.IDisposable)?.Dispose();
        }

        private int CreateEntityWithOldComponent()
        {
            int entityId = _entityManager.CreateEntity();

            var oldStats = new CombatStatsComponent
            {
                AttackPower = 20f,
                Defense = 10f,
                AttackSpeed = 1.2f,
                CriticalChance = 0.15f,
                CriticalMultiplier = 2.0f,
                Accuracy = 0.9f,
                DodgeChance = 0.1f,
                BlockChance = 0.15f,
                CounterAttackChance = 0.05f,
                Rage = 25f,
                MaxRage = 100f,
                Concentration = 30f,
                MaxConcentration = 100f,
                IsInCombat = true,
                CombatTargetEntityId = 999,
                LastAttackTime = 10.5f
            };

            _entityManager.AddComponent(entityId, oldStats);
            return entityId;
        }

        [Test]
        public void MigrateEntity_CreatesNewComponents()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            bool result = _migration.MigrateEntity(entityId);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_entityManager.HasComponent<MythHunterStatsComponent>(entityId), Is.True);
            Assert.That(_entityManager.HasComponent<MythHunterResourceComponent>(entityId), Is.True);
            Assert.That(_entityManager.HasComponent<CombatStateComponent>(entityId), Is.True);
        }

        [Test]
        public void MigrateEntity_PreservesStatValues()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            _migration.MigrateEntity(entityId);

            // Assert
            var newStats = _entityManager.GetComponent<MythHunterStatsComponent>(entityId);
            Assert.That(newStats.Stats.GetBaseStat(CombatStatType.AttackPower), Is.EqualTo(20f));
            Assert.That(newStats.Stats.GetBaseStat(CombatStatType.Defense), Is.EqualTo(10f));
            Assert.That(newStats.Stats.GetBaseStat(CombatStatType.CriticalChance), Is.EqualTo(0.15f));
        }

        [Test]
        public void MigrateEntity_PreservesResourceValues()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            _migration.MigrateEntity(entityId);

            // Assert
            var newResources = _entityManager.GetComponent<MythHunterResourceComponent>(entityId);
            Assert.That(newResources.Resources.GetCurrent(MythHunterResourceType.Rage), Is.EqualTo(25f));
            Assert.That(newResources.Resources.GetMax(MythHunterResourceType.Rage), Is.EqualTo(100f));
            Assert.That(newResources.Resources.GetCurrent(MythHunterResourceType.Concentration), Is.EqualTo(30f));
        }

        [Test]
        public void MigrateEntity_PreservesCombatState()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            _migration.MigrateEntity(entityId);

            // Assert
            var combatState = _entityManager.GetComponent<CombatStateComponent>(entityId);
            Assert.That(combatState.IsInCombat, Is.True);
            Assert.That(combatState.CombatTargetEntityId, Is.EqualTo(999));
            Assert.That(combatState.LastAttackTime, Is.EqualTo(10.5f));
        }

        [Test]
        public void MigrateEntity_AlreadyMigrated_ReturnsFalse()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();
            _migration.MigrateEntity(entityId); // First migration

            // Act
            bool result = _migration.MigrateEntity(entityId); // Try again

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void MigrateAllEntities_MigratesMultipleEntities()
        {
            // Arrange
            int entity1 = CreateEntityWithOldComponent();
            int entity2 = CreateEntityWithOldComponent();
            int entity3 = CreateEntityWithOldComponent();

            // Act
            int migratedCount = _migration.MigrateAllEntities();

            // Assert
            Assert.That(migratedCount, Is.EqualTo(3));
        }

        [Test]
        public void GetEntityStatus_NotMigrated_ReturnsCorrectStatus()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            var status = _migration.GetEntityStatus(entityId);

            // Assert
            Assert.That(status, Is.EqualTo(MigrationStatus.NotMigrated));
        }

        [Test]
        public void GetEntityStatus_Migrated_ReturnsCorrectStatus()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();
            _migration.MigrateEntity(entityId);

            // Act
            var status = _migration.GetEntityStatus(entityId);

            // Assert
            Assert.That(status, Is.EqualTo(MigrationStatus.Migrated));
        }

        [Test]
        public void RemoveOldComponents_RemovesOnlyMigratedEntities()
        {
            // Arrange
            int entity1 = CreateEntityWithOldComponent();
            int entity2 = CreateEntityWithOldComponent();
            _migration.MigrateEntity(entity1); // Migrate only first

            // Act
            int removedCount = _migration.RemoveOldComponents();

            // Assert
            Assert.That(removedCount, Is.EqualTo(1));
            Assert.That(_entityManager.HasComponent<CombatStatsComponent>(entity1), Is.False);
            Assert.That(_entityManager.HasComponent<CombatStatsComponent>(entity2), Is.True);
        }

        [Test]
        public void RollbackEntity_RemovesNewComponents()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();
            _migration.MigrateEntity(entityId);

            // Act
            bool result = _migration.RollbackEntity(entityId);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_entityManager.HasComponent<MythHunterStatsComponent>(entityId), Is.False);
            Assert.That(_entityManager.HasComponent<MythHunterResourceComponent>(entityId), Is.False);
            Assert.That(_entityManager.HasComponent<CombatStateComponent>(entityId), Is.False);
            Assert.That(_entityManager.HasComponent<CombatStatsComponent>(entityId), Is.True); // Old still exists
        }

        [Test]
        public void GetStatistics_ReturnsCorrectCounts()
        {
            // Arrange
            CreateEntityWithOldComponent(); // Not migrated
            int entity2 = CreateEntityWithOldComponent();
            _migration.MigrateEntity(entity2); // Migrated
            int entity3 = CreateEntityWithOldComponent();
            _migration.MigrateEntity(entity3);
            _migration.RemoveOldComponents(); // Cleaned entity3

            // Act
            var stats = _migration.GetStatistics();

            // Assert
            Assert.That(stats.NotMigrated, Is.EqualTo(1));
            Assert.That(stats.Migrated, Is.EqualTo(1));
            Assert.That(stats.MigratedAndCleaned, Is.EqualTo(1));
            Assert.That(stats.Total, Is.EqualTo(3));
        }

        [Test]
        public void GetStatistics_CalculatesProgressPercentage()
        {
            // Arrange
            CreateEntityWithOldComponent(); // Not migrated
            int entity2 = CreateEntityWithOldComponent();
            _migration.MigrateEntity(entity2); // Migrated

            // Act
            var stats = _migration.GetStatistics();

            // Assert
            // 1 out of 2 migrated = 50%
            Assert.That(stats.ProgressPercentage, Is.EqualTo(50f).Within(0.1f));
        }

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
