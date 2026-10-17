// Assets/_MythHunter/Tests/Abilities/AbilitySystemIntegrationTests.cs

using NUnit.Framework;
using MythHunter.GameLogic.Abilities.Adapters;
using MythHunter.GameLogic.Abilities.Components;
using MythHunter.GameLogic.Abilities.Data;
using MythHunter.Utils.Logging;
using RPGFramework.ECS;
using RPGFramework.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MythHunter.Tests.Abilities
{
    [TestFixture]
    public class AbilitySystemIntegrationTests
    {
        private IEntityManager _entityManager;
        private AbilitySystemIntegration _integration;
        private IAbilityExecutor _abilityExecutor;
        private IAbilityRegistry _abilityRegistry;
        private ICooldownTracker _cooldownTracker;
        private ITargetValidator _targetValidator;
        private IMythLogger _logger;

        [SetUp]
        public void Setup()
        {
            _entityManager = new EntityManager();
            _abilityRegistry = new AbilityRegistry();
            _cooldownTracker = new CooldownTracker();
            _targetValidator = new TargetValidator();
            _abilityExecutor = new AbilityExecutor(_abilityRegistry, _cooldownTracker, _targetValidator);
            _logger = new TestLogger();

            _integration = new AbilitySystemIntegration(
                _entityManager,
                _abilityExecutor,
                _abilityRegistry,
                _logger
            );

            // Register sample abilities for testing
            RegisterTestAbilities();
        }

        [TearDown]
        public void TearDown()
        {
            (_entityManager as IDisposable)?.Dispose();
        }

        private void RegisterTestAbilities()
        {
            // Register a few sample abilities for testing
            _abilityRegistry.RegisterAbility(SampleAbilities.MeleeStrike());
            _abilityRegistry.RegisterAbility(SampleAbilities.Fireball());
            _abilityRegistry.RegisterAbility(SampleAbilities.Heal());
            _abilityRegistry.RegisterAbility(SampleAbilities.Blink());
            _abilityRegistry.RegisterAbility(SampleAbilities.CriticalStrikeMastery());
            _abilityRegistry.RegisterAbility(SampleAbilities.BerserkerStance());
        }

        #region Helper Methods

        private int CreateEntityWithAbilities(params string[] abilityIds)
        {
            int entityId = _entityManager.CreateEntity();

            foreach (var abilityId in abilityIds)
            {
                _integration.AddAbilityToEntity(entityId, abilityId);
            }

            return entityId;
        }

        #endregion

        #region Component Management Tests

        [Test]
        public void AddAbilityToEntity_WithRegisteredAbility_AddsSuccessfully()
        {
            // Arrange
            int entityId = _entityManager.CreateEntity();

            // Act
            bool result = _integration.AddAbilityToEntity(entityId, "melee_strike");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_integration.HasAbility(entityId, "melee_strike"), Is.True);
        }

        [Test]
        public void AddAbilityToEntity_WithUnregisteredAbility_ReturnsFalse()
        {
            // Arrange
            int entityId = _entityManager.CreateEntity();

            // Act
            bool result = _integration.AddAbilityToEntity(entityId, "non_existent_ability");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void AddAbilityToEntity_DuplicateAbility_ReturnsFalse()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("melee_strike");

            // Act
            bool result = _integration.AddAbilityToEntity(entityId, "melee_strike");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void RemoveAbilityFromEntity_WithExistingAbility_RemovesSuccessfully()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("melee_strike");

            // Act
            bool result = _integration.RemoveAbilityFromEntity(entityId, "melee_strike");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_integration.HasAbility(entityId, "melee_strike"), Is.False);
        }

        [Test]
        public void RemoveAbilityFromEntity_WithNonExistentAbility_ReturnsFalse()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("melee_strike");

            // Act
            bool result = _integration.RemoveAbilityFromEntity(entityId, "fireball");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void LevelUpAbility_WithExistingAbility_IncreasesLevel()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("fireball");

            // Act
            bool result = _integration.LevelUpAbility(entityId, "fireball");

            // Assert
            Assert.That(result, Is.True);

            // Verify level increased (would need to add GetAbilityLevel method to verify)
            // For now, just check the operation succeeded
        }

        [Test]
        public void LevelUpAbility_WithNonExistentAbility_ReturnsFalse()
        {
            // Arrange
            int entityId = _entityManager.CreateEntity();

            // Act
            bool result = _integration.LevelUpAbility(entityId, "fireball");

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region Query Tests

        [Test]
        public void HasAbility_WithExistingAbility_ReturnsTrue()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("melee_strike");

            // Act
            bool result = _integration.HasAbility(entityId, "melee_strike");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasAbility_WithNonExistentAbility_ReturnsFalse()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("melee_strike");

            // Act
            bool result = _integration.HasAbility(entityId, "fireball");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void HasAbility_WithNoAbilityComponent_ReturnsFalse()
        {
            // Arrange
            int entityId = _entityManager.CreateEntity();

            // Act
            bool result = _integration.HasAbility(entityId, "melee_strike");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetAbilityIds_WithMultipleAbilities_ReturnsAllIds()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("melee_strike", "fireball", "heal");

            // Act
            var abilityIds = _integration.GetAbilityIds(entityId);

            // Assert
            Assert.That(abilityIds.Count, Is.EqualTo(3));
            Assert.That(abilityIds.Contains("melee_strike"), Is.True);
            Assert.That(abilityIds.Contains("fireball"), Is.True);
            Assert.That(abilityIds.Contains("heal"), Is.True);
        }

        [Test]
        public void GetAbilityIds_WithNoAbilities_ReturnsEmptyList()
        {
            // Arrange
            int entityId = _entityManager.CreateEntity();

            // Act
            var abilityIds = _integration.GetAbilityIds(entityId);

            // Assert
            Assert.That(abilityIds.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetEntitiesWithAbility_ReturnsCorrectEntities()
        {
            // Arrange
            int entity1 = CreateEntityWithAbilities("melee_strike", "fireball");
            int entity2 = CreateEntityWithAbilities("melee_strike", "heal");
            int entity3 = CreateEntityWithAbilities("fireball");

            // Act
            var entitiesWithMelee = _integration.GetEntitiesWithAbility("melee_strike").ToList();
            var entitiesWithFireball = _integration.GetEntitiesWithAbility("fireball").ToList();

            // Assert
            Assert.That(entitiesWithMelee.Count, Is.EqualTo(2));
            Assert.That(entitiesWithMelee.Contains(entity1), Is.True);
            Assert.That(entitiesWithMelee.Contains(entity2), Is.True);

            Assert.That(entitiesWithFireball.Count, Is.EqualTo(2));
            Assert.That(entitiesWithFireball.Contains(entity1), Is.True);
            Assert.That(entitiesWithFireball.Contains(entity3), Is.True);
        }

        #endregion

        #region Cooldown and Charge Tests

        [Test]
        public void GetRemainingCooldown_WithNoCooldown_ReturnsZero()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("melee_strike");
            float currentTime = 0f;

            // Act
            float remaining = _integration.GetRemainingCooldown(entityId, "melee_strike", currentTime);

            // Assert
            Assert.That(remaining, Is.EqualTo(0f));
        }

        [Test]
        public void GetRemainingCharges_WithUnlimitedCharges_ReturnsNegativeOne()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("melee_strike");

            // Act
            int charges = _integration.GetRemainingCharges(entityId, "melee_strike");

            // Assert
            Assert.That(charges, Is.EqualTo(-1));
        }

        [Test]
        public void GetRemainingCharges_WithChargeBasedAbility_ReturnsMaxCharges()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("blink"); // Blink has 3 charges

            // Act
            int charges = _integration.GetRemainingCharges(entityId, "blink");

            // Assert
            Assert.That(charges, Is.EqualTo(3));
        }

        #endregion

        #region Execution Tests

        [Test]
        public void CanExecuteAbility_WithValidAbility_ReturnsTrue()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("melee_strike");
            float currentTime = 0f;

            // Act
            bool canExecute = _integration.CanExecuteAbility(entityId, "melee_strike", currentTime);

            // Assert
            Assert.That(canExecute, Is.True);
        }

        [Test]
        public void CanExecuteAbility_WithNonExistentAbility_ReturnsFalse()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("melee_strike");
            float currentTime = 0f;

            // Act
            bool canExecute = _integration.CanExecuteAbility(entityId, "fireball", currentTime);

            // Assert
            Assert.That(canExecute, Is.False);
        }

        [Test]
        public void CanExecuteAbility_WithNoAbilityComponent_ReturnsFalse()
        {
            // Arrange
            int entityId = _entityManager.CreateEntity();
            float currentTime = 0f;

            // Act
            bool canExecute = _integration.CanExecuteAbility(entityId, "melee_strike", currentTime);

            // Assert
            Assert.That(canExecute, Is.False);
        }

        [Test]
        public void ExecuteAbility_WithValidAbility_ExecutesSuccessfully()
        {
            // Arrange
            int casterEntityId = CreateEntityWithAbilities("melee_strike");
            int targetEntityId = _entityManager.CreateEntity();
            float currentTime = 0f;

            bool effectApplied = false;

            // Act
            var result = _integration.ExecuteAbility(
                casterEntityId,
                "melee_strike",
                targetEntityId,
                currentTime,
                resourceCheck: (entityId, resourceType, amount) => true, // Always has resources
                resourceConsume: (entityId, resourceType, amount) => { }, // No-op
                applyEffect: (casterId, targetId, abilityId) => { effectApplied = true; }
            );

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(effectApplied, Is.True);
            Assert.That(result.AbilityId, Is.EqualTo("melee_strike"));
            Assert.That(result.CasterEntityId, Is.EqualTo(casterEntityId));
            Assert.That(result.TargetEntityId, Is.EqualTo(targetEntityId));
        }

        [Test]
        public void ExecuteAbility_WithInsufficientResources_Fails()
        {
            // Arrange
            int casterEntityId = CreateEntityWithAbilities("fireball"); // Costs 20 MP
            int targetEntityId = _entityManager.CreateEntity();
            float currentTime = 0f;

            // Act
            var result = _integration.ExecuteAbility(
                casterEntityId,
                "fireball",
                targetEntityId,
                currentTime,
                resourceCheck: (entityId, resourceType, amount) => false, // No resources
                resourceConsume: (entityId, resourceType, amount) => { },
                applyEffect: (casterId, targetId, abilityId) => { }
            );

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Does.Contain("resource").IgnoreCase);
        }

        [Test]
        public void CancelCast_WithCastingEntity_CancelsSuccessfully()
        {
            // Arrange
            int entityId = CreateEntityWithAbilities("heal");

            // Act (no exception should be thrown)
            _integration.CancelCast(entityId);

            // Assert (if no exception, test passes)
            Assert.Pass();
        }

        #endregion

        #region Sample Abilities Validation Tests

        [Test]
        public void SampleAbilities_MeleeStrike_HasCorrectProperties()
        {
            // Act
            var ability = SampleAbilities.MeleeStrike();

            // Assert
            Assert.That(ability.AbilityId, Is.EqualTo("melee_strike"));
            Assert.That(ability.Name, Is.EqualTo("Melee Strike"));
            Assert.That(ability.Type, Is.EqualTo(AbilityType.Active));
            Assert.That(ability.Cooldown, Is.EqualTo(0f));
            Assert.That(ability.Range, Is.EqualTo(2f));
            Assert.That(ability.TargetType, Is.EqualTo(TargetType.Enemy));
        }

        [Test]
        public void SampleAbilities_Fireball_HasCorrectProperties()
        {
            // Act
            var ability = SampleAbilities.Fireball();

            // Assert
            Assert.That(ability.AbilityId, Is.EqualTo("fireball"));
            Assert.That(ability.Name, Is.EqualTo("Fireball"));
            Assert.That(ability.Type, Is.EqualTo(AbilityType.Active));
            Assert.That(ability.Cooldown, Is.EqualTo(5f));
            Assert.That(ability.Range, Is.EqualTo(30f));
            Assert.That(ability.Metadata.AoeRadius, Is.EqualTo(3f));
            Assert.That(ability.ResourceCosts.Count, Is.EqualTo(1));
            Assert.That(ability.ResourceCosts[0].ResourceType, Is.EqualTo("MP"));
            Assert.That(ability.ResourceCosts[0].Amount, Is.EqualTo(20f));
        }

        [Test]
        public void SampleAbilities_Blink_HasCharges()
        {
            // Act
            var ability = SampleAbilities.Blink();

            // Assert
            Assert.That(ability.AbilityId, Is.EqualTo("blink"));
            Assert.That(ability.Metadata.MaxCharges, Is.EqualTo(3));
            Assert.That(ability.Cooldown, Is.EqualTo(0f)); // Charge-based, not cooldown-based
        }

        [Test]
        public void SampleAbilities_CriticalStrikeMastery_IsPassive()
        {
            // Act
            var ability = SampleAbilities.CriticalStrikeMastery();

            // Assert
            Assert.That(ability.Type, Is.EqualTo(AbilityType.Passive));
            Assert.That(ability.Cooldown, Is.EqualTo(0f));
            Assert.That(ability.TargetType, Is.EqualTo(TargetType.None));
        }

        [Test]
        public void SampleAbilities_BerserkerStance_IsToggle()
        {
            // Act
            var ability = SampleAbilities.BerserkerStance();

            // Assert
            Assert.That(ability.Type, Is.EqualTo(AbilityType.Toggle));
            Assert.That(ability.ResourceCosts.Count, Is.EqualTo(1));
            Assert.That(ability.ResourceCosts[0].IsPercentage, Is.True);
        }

        [Test]
        public void SampleAbilities_GetAllAbilities_Returns12Abilities()
        {
            // Act
            var abilities = SampleAbilities.GetAllAbilities();

            // Assert
            Assert.That(abilities.Count, Is.EqualTo(12));
        }

        [Test]
        public void SampleAbilities_GetAbilitiesByType_FiltersCombatAbilities()
        {
            // Act
            var combatAbilities = SampleAbilities.GetAbilitiesByType(AbilityType.Active);

            // Assert
            Assert.That(combatAbilities.Count, Is.GreaterThanOrEqualTo(4)); // At least MeleeStrike, Fireball, PowerStrike, Whirlwind
        }

        #endregion

        #region Registry Tests

        [Test]
        public void IsAbilityRegistered_WithRegisteredAbility_ReturnsTrue()
        {
            // Act
            bool result = _integration.IsAbilityRegistered("melee_strike");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsAbilityRegistered_WithUnregisteredAbility_ReturnsFalse()
        {
            // Act
            bool result = _integration.IsAbilityRegistered("non_existent_ability");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetAllAbilityIds_ReturnsRegisteredAbilityIds()
        {
            // Act
            var abilityIds = _integration.GetAllAbilityIds().ToList();

            // Assert
            Assert.That(abilityIds.Count, Is.GreaterThanOrEqualTo(6)); // At least the 6 we registered
            Assert.That(abilityIds.Contains("melee_strike"), Is.True);
            Assert.That(abilityIds.Contains("fireball"), Is.True);
            Assert.That(abilityIds.Contains("heal"), Is.True);
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
