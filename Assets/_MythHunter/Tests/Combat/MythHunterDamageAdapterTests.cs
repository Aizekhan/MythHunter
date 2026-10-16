// Assets/_MythHunter/Tests/Combat/MythHunterDamageAdapterTests.cs
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
    public class MythHunterDamageAdapterTests
    {
        private IEntityManager _entityManager;
        private MythHunterDamageAdapter _damageAdapter;
        private IDamageCalculator _damageCalculator;
        private IMythLogger _logger;

        [SetUp]
        public void Setup()
        {
            // Create test entity manager
            _entityManager = new EntityManager();

            // Create Framework damage calculator
            _damageCalculator = new DamageCalculator();

            // Create mock logger
            _logger = new TestLogger();

            // Create adapter
            _damageAdapter = new MythHunterDamageAdapter(
                _damageCalculator,
                _entityManager,
                _logger);
        }

        [TearDown]
        public void TearDown()
        {
            (_entityManager as System.IDisposable)?.Dispose();
        }

        private int CreateTestEntity(
            float attackPower = 10f,
            float defense = 5f,
            float maxHealth = 100f,
            float critChance = 0.0f,
            float critMultiplier = 1.5f)
        {
            int entityId = _entityManager.CreateEntity();

            // Add stats component
            var stats = MythHunterStatsComponent.CreateWithStats(
                attackPower,
                defense,
                attackSpeed: 1.0f,
                criticalChance: critChance,
                criticalMultiplier: critMultiplier);
            _entityManager.AddComponent(entityId, stats);

            // Add resources component
            var resources = MythHunterResourceComponent.CreateWithHealth(maxHealth);
            _entityManager.AddComponent(entityId, resources);

            // Add combat state
            var combatState = CombatStateComponent.Create();
            _entityManager.AddComponent(entityId, combatState);

            return entityId;
        }

        [Test]
        public void ApplyPhysicalDamage_ReducesTargetHealth()
        {
            // Arrange
            int attacker = CreateTestEntity(attackPower: 20f);
            int target = CreateTestEntity(defense: 5f, maxHealth: 100f);

            // Act
            DamageResult result = _damageAdapter.ApplyPhysicalDamage(attacker, target);

            // Assert
            Assert.That(result.FinalDamage, Is.GreaterThan(0));

            var targetResources = _entityManager.GetComponent<MythHunterResourceComponent>(target);
            float healthAfter = targetResources.HealthCurrent;

            Assert.That(healthAfter, Is.LessThan(100f));
            Assert.That(healthAfter, Is.EqualTo(100f - result.FinalDamage).Within(0.01f));
        }

        [Test]
        public void ApplyDamage_CriticalHit_DealsBonusDamage()
        {
            // Arrange - 100% crit chance
            int attacker = CreateTestEntity(
                attackPower: 20f,
                critChance: 1.0f,
                critMultiplier: 2.0f);
            int target = CreateTestEntity(defense: 0f, maxHealth: 100f);

            // Act
            DamageResult result = _damageAdapter.ApplyPhysicalDamage(attacker, target);

            // Assert
            Assert.That(result.IsCritical, Is.True);
            Assert.That(result.FinalDamage, Is.GreaterThan(20f)); // More than base damage
        }

        [Test]
        public void ApplyDamage_WithDefense_ReducesDamage()
        {
            // Arrange
            int attacker = CreateTestEntity(attackPower: 20f);
            int targetLowDefense = CreateTestEntity(defense: 5f, maxHealth: 100f);
            int targetHighDefense = CreateTestEntity(defense: 50f, maxHealth: 100f);

            // Act
            DamageResult resultLowDefense = _damageAdapter.ApplyPhysicalDamage(attacker, targetLowDefense);
            DamageResult resultHighDefense = _damageAdapter.ApplyPhysicalDamage(attacker, targetHighDefense);

            // Assert
            Assert.That(resultHighDefense.FinalDamage, Is.LessThan(resultLowDefense.FinalDamage));
        }

        [Test]
        public void ApplyDamage_MagicalDamage_UsesMagicalType()
        {
            // Arrange
            int attacker = CreateTestEntity(attackPower: 15f);
            int target = CreateTestEntity(defense: 10f, maxHealth: 100f);

            // Act
            DamageResult result = _damageAdapter.ApplyMagicalDamage(attacker, target);

            // Assert
            Assert.That(result.FinalDamage, Is.GreaterThan(0));
            Assert.That(result.DamageType, Is.EqualTo(DamageType.Magical));
        }

        [Test]
        public void ApplyDamage_WithMultiplier_IncreasesBaseDamage()
        {
            // Arrange
            int attacker = CreateTestEntity(attackPower: 20f);
            int target = CreateTestEntity(defense: 0f, maxHealth: 200f);

            // Act
            DamageResult normalDamage = _damageAdapter.ApplyPhysicalDamage(attacker, target, damageMultiplier: 1.0f);
            DamageResult boostedDamage = _damageAdapter.ApplyPhysicalDamage(attacker, target, damageMultiplier: 2.0f);

            // Assert
            Assert.That(boostedDamage.FinalDamage, Is.GreaterThan(normalDamage.FinalDamage));
        }

        [Test]
        public void WouldKill_ReturnsTrue_WhenDamageExceedsHealth()
        {
            // Arrange
            int target = CreateTestEntity(maxHealth: 50f);

            // Act
            bool wouldKill = _damageAdapter.WouldKill(target, 60f);

            // Assert
            Assert.That(wouldKill, Is.True);
        }

        [Test]
        public void WouldKill_ReturnsFalse_WhenDamageLessThanHealth()
        {
            // Arrange
            int target = CreateTestEntity(maxHealth: 100f);

            // Act
            bool wouldKill = _damageAdapter.WouldKill(target, 50f);

            // Assert
            Assert.That(wouldKill, Is.False);
        }

        [Test]
        public void GetDamageReduction_CalculatesCorrectPercentage()
        {
            // Arrange
            int entity = CreateTestEntity(defense: 50f);

            // Act
            float reduction = _damageAdapter.GetDamageReduction(entity, DamageType.Physical);

            // Assert
            // Formula: defense / (100 + defense) = 50 / 150 = 0.333...
            Assert.That(reduction, Is.EqualTo(50f / 150f).Within(0.001f));
        }

        [Test]
        public void ApplyDamage_InvalidAttacker_ReturnsDefault()
        {
            // Arrange
            int target = CreateTestEntity();
            int invalidAttacker = 9999;

            // Act
            DamageResult result = _damageAdapter.ApplyPhysicalDamage(invalidAttacker, target);

            // Assert
            Assert.That(result.FinalDamage, Is.EqualTo(0));
        }

        [Test]
        public void ApplyDamage_InvalidTarget_ReturnsDefault()
        {
            // Arrange
            int attacker = CreateTestEntity();
            int invalidTarget = 9999;

            // Act
            DamageResult result = _damageAdapter.ApplyPhysicalDamage(attacker, invalidTarget);

            // Assert
            Assert.That(result.FinalDamage, Is.EqualTo(0));
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
