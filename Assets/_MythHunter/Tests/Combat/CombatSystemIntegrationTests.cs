// Assets/_MythHunter/Tests/Combat/CombatSystemIntegrationTests.cs
using NUnit.Framework;
using MythHunter.Components.Combat;
using MythHunter.GameLogic.Combat;
using MythHunter.GameLogic.Combat.Adapters;
using MythHunter.Utils.Logging;
using RPGFramework.Combat;
using RPGFramework.ECS;

namespace MythHunter.Tests.Combat
{
    [TestFixture]
    public class CombatSystemIntegrationTests
    {
        private IEntityManager _entityManager;
        private CombatSystemIntegration _integration;
        private MythHunterDamageAdapter _damageAdapter;
        private IMythLogger _logger;

        [SetUp]
        public void Setup()
        {
            _entityManager = new EntityManager();
            _logger = new TestLogger();
            _damageAdapter = new MythHunterDamageAdapter(new DamageCalculator(), _entityManager, _logger);
            _integration = new CombatSystemIntegration(_entityManager, _damageAdapter, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            (_entityManager as System.IDisposable)?.Dispose();
        }

        #region Test Entity Helpers

        private int CreateEntityWithNewComponents()
        {
            int entityId = _entityManager.CreateEntity();
            _entityManager.AddComponent(entityId, MythHunterStatsComponent.CreateWithStats(20f, 10f));
            _entityManager.AddComponent(entityId, MythHunterResourceComponent.CreateWithHealth(100f));
            _entityManager.AddComponent(entityId, CombatStateComponent.Create());
            return entityId;
        }

        private int CreateEntityWithOldComponent()
        {
            int entityId = _entityManager.CreateEntity();
            var oldStats = new CombatStatsComponent
            {
                AttackPower = 15f,
                Defense = 5f,
                AttackSpeed = 1.0f,
                CriticalChance = 0.1f,
                Rage = 25f,
                MaxRage = 100f,
                IsInCombat = false
            };
            _entityManager.AddComponent(entityId, oldStats);
            return entityId;
        }

        #endregion

        #region Combat State Tests

        [Test]
        public void IsInCombat_NewComponent_ReturnsCorrectValue()
        {
            // Arrange
            int entityId = CreateEntityWithNewComponents();
            var combatState = _entityManager.GetComponent<CombatStateComponent>(entityId);
            combatState.IsInCombat = true;
            _entityManager.SetComponent(entityId, combatState);

            // Act
            bool result = _integration.IsInCombat(entityId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsInCombat_OldComponent_ReturnsCorrectValue()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();
            var oldStats = _entityManager.GetComponent<CombatStatsComponent>(entityId);
            oldStats.IsInCombat = true;
            _entityManager.SetComponent(entityId, oldStats);

            // Act
            bool result = _integration.IsInCombat(entityId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void SetCombatState_NewComponent_UpdatesCorrectly()
        {
            // Arrange
            int entityId = CreateEntityWithNewComponents();

            // Act
            _integration.SetCombatState(entityId, true, 999);

            // Assert
            var combatState = _entityManager.GetComponent<CombatStateComponent>(entityId);
            Assert.That(combatState.IsInCombat, Is.True);
            Assert.That(combatState.CombatTargetEntityId, Is.EqualTo(999));
        }

        [Test]
        public void SetCombatState_OldComponent_UpdatesCorrectly()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            _integration.SetCombatState(entityId, true, 888);

            // Assert
            var oldStats = _entityManager.GetComponent<CombatStatsComponent>(entityId);
            Assert.That(oldStats.IsInCombat, Is.True);
            Assert.That(oldStats.CombatTargetEntityId, Is.EqualTo(888));
        }

        [Test]
        public void GetCombatTarget_NewComponent_ReturnsTarget()
        {
            // Arrange
            int entityId = CreateEntityWithNewComponents();
            _integration.SetCombatState(entityId, true, 777);

            // Act
            int target = _integration.GetCombatTarget(entityId);

            // Assert
            Assert.That(target, Is.EqualTo(777));
        }

        #endregion

        #region Stat Tests

        [Test]
        public void GetAttackPower_NewComponent_ReturnsValue()
        {
            // Arrange
            int entityId = CreateEntityWithNewComponents();

            // Act
            float attackPower = _integration.GetAttackPower(entityId);

            // Assert
            Assert.That(attackPower, Is.EqualTo(20f));
        }

        [Test]
        public void GetAttackPower_OldComponent_ReturnsValue()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            float attackPower = _integration.GetAttackPower(entityId);

            // Assert
            Assert.That(attackPower, Is.EqualTo(15f));
        }

        [Test]
        public void GetDefense_NewComponent_ReturnsValue()
        {
            // Arrange
            int entityId = CreateEntityWithNewComponents();

            // Act
            float defense = _integration.GetDefense(entityId);

            // Assert
            Assert.That(defense, Is.EqualTo(10f));
        }

        [Test]
        public void GetDefense_OldComponent_ReturnsValue()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            float defense = _integration.GetDefense(entityId);

            // Assert
            Assert.That(defense, Is.EqualTo(5f));
        }

        #endregion

        #region Resource Tests

        [Test]
        public void GetRage_NewComponent_ReturnsValue()
        {
            // Arrange
            int entityId = CreateEntityWithNewComponents();
            var resources = _entityManager.GetComponent<MythHunterResourceComponent>(entityId);
            resources.Resources.SetCurrent(MythHunterResourceType.Rage, 50f);
            _entityManager.SetComponent(entityId, resources);

            // Act
            float rage = _integration.GetRage(entityId);

            // Assert
            Assert.That(rage, Is.EqualTo(50f));
        }

        [Test]
        public void GetRage_OldComponent_ReturnsValue()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            float rage = _integration.GetRage(entityId);

            // Assert
            Assert.That(rage, Is.EqualTo(25f));
        }

        [Test]
        public void AddRage_NewComponent_IncreasesValue()
        {
            // Arrange
            int entityId = CreateEntityWithNewComponents();

            // Act
            _integration.AddRage(entityId, 20f);

            // Assert
            float rage = _integration.GetRage(entityId);
            Assert.That(rage, Is.EqualTo(20f));
        }

        [Test]
        public void AddRage_OldComponent_IncreasesValue()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            _integration.AddRage(entityId, 10f);

            // Assert
            var oldStats = _entityManager.GetComponent<CombatStatsComponent>(entityId);
            Assert.That(oldStats.Rage, Is.EqualTo(35f)); // 25 + 10
        }

        #endregion

        #region Damage Integration Tests

        [Test]
        public void ApplyDamage_BothNewComponents_UsesFrameworkCalculator()
        {
            // Arrange
            int attacker = CreateEntityWithNewComponents();
            int target = CreateEntityWithNewComponents();

            // Act
            float damage = _integration.ApplyDamage(attacker, target);

            // Assert
            Assert.That(damage, Is.GreaterThan(0));

            // Verify health was reduced
            var targetResources = _entityManager.GetComponent<MythHunterResourceComponent>(target);
            Assert.That(targetResources.Resources.GetCurrent(MythHunterResourceType.Health), Is.LessThan(100f));
        }

        [Test]
        public void ApplyDamage_WithMultiplier_IncreasesDamage()
        {
            // Arrange
            int attacker = CreateEntityWithNewComponents();
            int target1 = CreateEntityWithNewComponents();
            int target2 = CreateEntityWithNewComponents();

            // Act
            float normalDamage = _integration.ApplyDamage(attacker, target1, 1.0f);
            float boostedDamage = _integration.ApplyDamage(attacker, target2, 2.0f);

            // Assert
            Assert.That(boostedDamage, Is.GreaterThan(normalDamage));
        }

        #endregion

        #region Utility Tests

        [Test]
        public void HasNewComponents_WithAllNewComponents_ReturnsTrue()
        {
            // Arrange
            int entityId = CreateEntityWithNewComponents();

            // Act
            bool result = _integration.HasNewComponents(entityId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasNewComponents_WithoutComponents_ReturnsFalse()
        {
            // Arrange
            int entityId = _entityManager.CreateEntity();

            // Act
            bool result = _integration.HasNewComponents(entityId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void HasOldComponents_WithOldComponent_ReturnsTrue()
        {
            // Arrange
            int entityId = CreateEntityWithOldComponent();

            // Act
            bool result = _integration.HasOldComponents(entityId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasOldComponents_WithoutOldComponent_ReturnsFalse()
        {
            // Arrange
            int entityId = CreateEntityWithNewComponents();

            // Act
            bool result = _integration.HasOldComponents(entityId);

            // Assert
            Assert.That(result, Is.False);
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
