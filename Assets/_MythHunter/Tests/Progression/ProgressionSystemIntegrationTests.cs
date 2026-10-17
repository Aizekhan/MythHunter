// Assets/_MythHunter/Tests/Progression/ProgressionSystemIntegrationTests.cs

using NUnit.Framework;
using MythHunter.GameLogic.Progression.Adapters;
using MythHunter.GameLogic.Progression.Components;
using MythHunter.Utils.Logging;
using RPGFramework.ECS;
using RPGFramework.Progression;
using System.Collections.Generic;

namespace MythHunter.Tests.Progression
{
    [TestFixture]
    public class ProgressionSystemIntegrationTests
    {
        private IEntityManager _entityManager;
        private IProgressionTracker _progressionTracker;
        private IStatGrowthFormula _statGrowthFormula;
        private IUnlockManager _unlockManager;
        private ILevelTable _levelTable;
        private ProgressionSystemIntegration _integration;
        private IMythLogger _logger;

        [SetUp]
        public void Setup()
        {
            _entityManager = new EntityManager();
            _logger = new TestLogger();

            // Create level table for testing (simple linear curve)
            _levelTable = LevelTable.CreateLinear(100, 100); // 100 levels, 100 XP per level base

            // Create Framework services
            _progressionTracker = new ProgressionTracker(_levelTable);
            _statGrowthFormula = new StatGrowthFormula();
            _unlockManager = new UnlockManager();

            // Create integration
            _integration = new ProgressionSystemIntegration(
                _entityManager,
                _progressionTracker,
                _statGrowthFormula,
                _unlockManager,
                _logger
            );

            // Register test stat growth
            _statGrowthFormula.RegisterStatGrowth(new StatGrowthData("HP", 100f, 10f, 0f, GrowthCurveType.Linear));
            _statGrowthFormula.RegisterStatGrowth(new StatGrowthData("AttackPower", 10f, 2f, 0f, GrowthCurveType.Linear));
            _statGrowthFormula.RegisterStatGrowth(new StatGrowthData("Defense", 5f, 1f, 0f, GrowthCurveType.Linear));

            // Register test unlocks
            _unlockManager.RegisterUnlock(new UnlockData("ability_fireball", "Fireball", 5, "Ability"));
            _unlockManager.RegisterUnlock(new UnlockData("ability_heal", "Heal", 10, "Ability"));
            _unlockManager.RegisterUnlock(new UnlockData("feature_crafting", "Crafting", 15, "Feature"));
        }

        [TearDown]
        public void TearDown()
        {
            (_entityManager as System.IDisposable)?.Dispose();
        }

        #region Test Entity Helpers

        private int CreateEntityWithProgression(int startingLevel = 1)
        {
            int entityId = _entityManager.CreateEntity();
            long xp = _levelTable.GetTotalXPForLevel(startingLevel);
            var component = MythHunterProgressionComponent.CreateHero(xp);
            _entityManager.AddComponent(entityId, component);

            // Initialize tracker
            _progressionTracker.InitializeEntity(entityId, _levelTable);

            return entityId;
        }

        #endregion

        #region XP and Leveling Tests

        [Test]
        public void AddExperience_AddsXPCorrectly()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);
            long initialXP = _integration.GetTotalXP(entityId);

            // Act
            var result = _integration.AddExperience(entityId, 500, "Kill", 0f);

            // Assert
            Assert.That(_integration.GetTotalXP(entityId), Is.EqualTo(initialXP + 500));
        }

        [Test]
        public void AddExperience_CausesLevelUp()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);
            long xpForLevel2 = _levelTable.GetXPForNextLevel(1);

            // Act
            var result = _integration.AddExperience(entityId, xpForLevel2, "Quest", 0f);

            // Assert
            Assert.That(result.LeveledUp, Is.True);
            Assert.That(result.NewLevel, Is.EqualTo(2));
            Assert.That(result.LevelsGained, Is.EqualTo(1));
            Assert.That(_integration.GetLevel(entityId), Is.EqualTo(2));
        }

        [Test]
        public void AddExperience_MultipleLevels_GrantsAllLevels()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);
            long xpForLevel5 = _levelTable.GetTotalXPForLevel(5);

            // Act
            var result = _integration.AddExperience(entityId, xpForLevel5, "Quest", 0f);

            // Assert
            Assert.That(result.LeveledUp, Is.True);
            Assert.That(result.NewLevel, Is.EqualTo(5));
            Assert.That(result.LevelsGained, Is.EqualTo(4)); // Levels 2, 3, 4, 5
            Assert.That(_integration.GetLevel(entityId), Is.EqualTo(5));
        }

        [Test]
        public void SetLevel_SetsLevelDirectly()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);

            // Act
            _integration.SetLevel(entityId, 10, 0f);

            // Assert
            Assert.That(_integration.GetLevel(entityId), Is.EqualTo(10));
        }

        #endregion

        #region Stat Growth Tests

        [Test]
        public void ApplyStatGrowth_IncreasesStats()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);
            _entityManager.TryGetComponent<MythHunterProgressionComponent>(entityId, out var component);

            // Set initial stat values
            component.SetStatValue("HP", 100f);
            component.SetStatValue("AttackPower", 10f);
            _entityManager.AddComponent(entityId, component);

            // Act
            _integration.ApplyStatGrowth(entityId, 1, 2);

            // Assert
            _entityManager.TryGetComponent<MythHunterProgressionComponent>(entityId, out component);
            float newHP = _statGrowthFormula.CalculateStatAtLevel("HP", 2);
            float newAttackPower = _statGrowthFormula.CalculateStatAtLevel("AttackPower", 2);

            Assert.That(component.GetStatValue("HP"), Is.EqualTo(newHP));
            Assert.That(component.GetStatValue("AttackPower"), Is.EqualTo(newAttackPower));
        }

        [Test]
        public void GetStatValueAtLevel_ReturnsCorrectValue()
        {
            // Act
            float hpAtLevel10 = _integration.GetStatValueAtLevel("HP", 10);

            // Assert
            float expected = 100f + (10f * (10 - 1)); // BaseValue + (GrowthPerLevel * levels)
            Assert.That(hpAtLevel10, Is.EqualTo(expected));
        }

        #endregion

        #region Unlock Tests

        [Test]
        public void GrantUnlocksForLevel_GrantsUnlocksAtLevel5()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);

            // Act
            _integration.GrantUnlocksForLevel(entityId, 1, 5);

            // Assert
            Assert.That(_integration.HasUnlock(entityId, "ability_fireball"), Is.True);
        }

        [Test]
        public void GrantUnlocksForLevel_GrantsMultipleUnlocks()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);

            // Act
            _integration.GrantUnlocksForLevel(entityId, 1, 15);

            // Assert
            Assert.That(_integration.HasUnlock(entityId, "ability_fireball"), Is.True); // Level 5
            Assert.That(_integration.HasUnlock(entityId, "ability_heal"), Is.True); // Level 10
            Assert.That(_integration.HasUnlock(entityId, "feature_crafting"), Is.True); // Level 15
        }

        [Test]
        public void AchieveUnlock_GrantsUnlockManually()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);

            // Act
            _integration.AchieveUnlock(entityId, "ability_fireball");

            // Assert
            Assert.That(_integration.HasUnlock(entityId, "ability_fireball"), Is.True);
        }

        [Test]
        public void GetUnlockedContent_ReturnsAllUnlocks()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);
            _integration.GrantUnlocksForLevel(entityId, 1, 10);

            // Act
            var unlocks = _integration.GetUnlockedContent(entityId);

            // Assert
            Assert.That(unlocks, Contains.Item("ability_fireball"));
            Assert.That(unlocks, Contains.Item("ability_heal"));
        }

        #endregion

        #region Query Tests

        [Test]
        public void GetProgressionData_ReturnsCorrectData()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(5);
            long xpForLevel5 = _levelTable.GetTotalXPForLevel(5);

            // Act
            var data = _integration.GetProgressionData(entityId);

            // Assert
            Assert.That(data.CurrentLevel, Is.EqualTo(5));
            Assert.That(data.TotalXP, Is.EqualTo(xpForLevel5));
        }

        [Test]
        public void GetAllProgressingEntities_ReturnsAllEntities()
        {
            // Arrange
            int entity1 = CreateEntityWithProgression(1);
            int entity2 = CreateEntityWithProgression(5);
            int entity3 = CreateEntityWithProgression(10);

            // Act
            var entities = _integration.GetAllProgressingEntities();

            // Assert
            Assert.That(entities, Has.Count.EqualTo(3));
            Assert.That(entities, Contains.Item(entity1));
            Assert.That(entities, Contains.Item(entity2));
            Assert.That(entities, Contains.Item(entity3));
        }

        [Test]
        public void GetEntitiesAtLevel_ReturnsOnlyEntitiesAtLevel()
        {
            // Arrange
            int entity1 = CreateEntityWithProgression(5);
            int entity2 = CreateEntityWithProgression(5);
            int entity3 = CreateEntityWithProgression(10);

            // Act
            var entitiesAtLevel5 = _integration.GetEntitiesAtLevel(5);

            // Assert
            Assert.That(entitiesAtLevel5, Has.Count.EqualTo(2));
            Assert.That(entitiesAtLevel5, Contains.Item(entity1));
            Assert.That(entitiesAtLevel5, Contains.Item(entity2));
            Assert.That(entitiesAtLevel5, Does.Not.Contain(entity3));
        }

        [Test]
        public void GetEntitiesInLevelRange_ReturnsEntitiesInRange()
        {
            // Arrange
            int entity1 = CreateEntityWithProgression(5);
            int entity2 = CreateEntityWithProgression(10);
            int entity3 = CreateEntityWithProgression(15);
            int entity4 = CreateEntityWithProgression(20);

            // Act
            var entitiesInRange = _integration.GetEntitiesInLevelRange(10, 15);

            // Assert
            Assert.That(entitiesInRange, Has.Count.EqualTo(2));
            Assert.That(entitiesInRange, Contains.Item(entity2));
            Assert.That(entitiesInRange, Contains.Item(entity3));
            Assert.That(entitiesInRange, Does.Not.Contain(entity1));
            Assert.That(entitiesInRange, Does.Not.Contain(entity4));
        }

        #endregion

        #region Integration Tests with Callbacks

        [Test]
        public void AddExperience_LevelUp_TriggersCallbacks()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);
            long xpForLevel2 = _levelTable.GetXPForNextLevel(1);

            bool levelUpCalled = false;
            int levelUpEntity = 0;
            int oldLevel = 0;
            int newLevel = 0;

            // Act
            var result = _integration.AddExperience(
                entityId,
                xpForLevel2,
                "Quest",
                0f,
                onLevelUp: (entId, oldLvl, newLvl) =>
                {
                    levelUpCalled = true;
                    levelUpEntity = entId;
                    oldLevel = oldLvl;
                    newLevel = newLvl;
                }
            );

            // Assert
            Assert.That(levelUpCalled, Is.True);
            Assert.That(levelUpEntity, Is.EqualTo(entityId));
            Assert.That(oldLevel, Is.EqualTo(1));
            Assert.That(newLevel, Is.EqualTo(2));
        }

        [Test]
        public void AddExperience_LevelUp_TriggersStatIncreaseCallback()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);
            long xpForLevel2 = _levelTable.GetXPForNextLevel(1);

            List<string> statsIncreased = new List<string>();

            // Act
            var result = _integration.AddExperience(
                entityId,
                xpForLevel2,
                "Quest",
                0f,
                onStatIncrease: (entId, statId, oldValue, newValue) =>
                {
                    statsIncreased.Add(statId);
                }
            );

            // Assert
            Assert.That(statsIncreased, Is.Not.Empty);
            Assert.That(statsIncreased, Contains.Item("HP"));
            Assert.That(statsIncreased, Contains.Item("AttackPower"));
        }

        [Test]
        public void AddExperience_LevelUp_TriggersUnlockCallback()
        {
            // Arrange
            int entityId = CreateEntityWithProgression(1);
            long xpForLevel5 = _levelTable.GetTotalXPForLevel(5);

            List<string> unlocksGranted = new List<string>();

            // Act
            var result = _integration.AddExperience(
                entityId,
                xpForLevel5,
                "Quest",
                0f,
                onUnlockAchieved: (entId, unlockId) =>
                {
                    unlocksGranted.Add(unlockId);
                }
            );

            // Assert
            Assert.That(unlocksGranted, Contains.Item("ability_fireball"));
        }

        #endregion

        #region Component Creation Tests

        [Test]
        public void CreateHero_CreatesCorrectComponent()
        {
            // Act
            var component = MythHunterProgressionComponent.CreateHero(100);

            // Assert
            Assert.That(component.ExperienceData.TotalXP, Is.EqualTo(100));
            Assert.That(component.UnlockedContent, Is.Not.Null);
            Assert.That(component.StatValues, Is.Not.Null);
        }

        [Test]
        public void CreateEnemy_CreatesCorrectComponent()
        {
            // Act
            var component = MythHunterProgressionComponent.CreateEnemy(5, 1000);

            // Assert
            Assert.That(component.ExperienceData.CurrentLevel, Is.EqualTo(5));
            Assert.That(component.ExperienceData.TotalXP, Is.EqualTo(1000));
        }

        [Test]
        public void CreateNPC_CreatesCorrectComponent()
        {
            // Arrange
            var unlocks = new List<string> { "ability_talk", "feature_shop" };

            // Act
            var component = MythHunterProgressionComponent.CreateNPC(10, 5000, unlocks);

            // Assert
            Assert.That(component.ExperienceData.CurrentLevel, Is.EqualTo(10));
            Assert.That(component.UnlockedContent, Has.Count.EqualTo(2));
            Assert.That(component.UnlockedContent, Contains.Item("ability_talk"));
        }

        #endregion
    }
}
