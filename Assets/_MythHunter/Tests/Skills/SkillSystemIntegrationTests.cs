// Assets/_MythHunter/Tests/Skills/SkillSystemIntegrationTests.cs

using NUnit.Framework;
using MythHunter.GameLogic.Skills;
using MythHunter.GameLogic.Skills.Adapters;
using MythHunter.Utils.Logging;
using MythHunter.Tests.Progression; // For TestLogger
using RPGFramework.ECS;
using RPGFramework.Skills;
using System.Collections.Generic;

namespace MythHunter.Tests.Skills
{
    [TestFixture]
    public class SkillSystemIntegrationTests
    {
        private IEntityManager _entityManager;
        private ISkillTree _skillTree;
        private SkillSystemIntegration _integration;
        private IMythLogger _logger;

        [SetUp]
        public void Setup()
        {
            _entityManager = new EntityManager();
            _logger = new TestLogger();

            // Create test skill tree
            _skillTree = CreateTestSkillTree();

            // Create integration
            _integration = new SkillSystemIntegration(_entityManager, _skillTree, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            (_entityManager as System.IDisposable)?.Dispose();
        }

        private ISkillTree CreateTestSkillTree()
        {
            var tree = new SkillTree("tree_warrior", "Warrior Skills", "Combat skills for warriors");

            // Basic skill (no prerequisites)
            var basicSkill = SkillData.Create("skill_power_strike", "Power Strike", "+5 attack per level", 5, SkillType.Combat);
            basicSkill.Bonuses = new List<SkillBonusData>
            {
                SkillBonusData.Create("AttackPower", 5f, BonusType.Additive)
            };
            ((SkillTree)tree).AddSkill(new Skill(basicSkill));

            // Advanced skill (requires character level 10 + basic skill at level 3)
            var advancedSkill = SkillData.Create("skill_devastating_blow", "Devastating Blow", "Powerful attack", 3, SkillType.Active);
            advancedSkill.Prerequisites = new List<SkillPrerequisiteData>
            {
                SkillPrerequisiteData.CreateCharacterLevelRequirement(10),
                SkillPrerequisiteData.CreateSkillRequirement("skill_power_strike", 3)
            };
            advancedSkill.Bonuses = new List<SkillBonusData>
            {
                SkillBonusData.Create("CriticalChance", 2f, BonusType.Additive)
            };
            ((SkillTree)tree).AddSkill(new Skill(advancedSkill));

            return tree;
        }

        private int CreateEntityWithSkills()
        {
            int entityId = _entityManager.CreateEntity();
            var component = MythHunterSkillsComponent.CreateEmpty();
            _entityManager.AddComponent(entityId, component);
            return entityId;
        }

        #region Skill Learning Tests

        [Test]
        public void LearnSkill_NewSkill_Succeeds()
        {
            int entityId = CreateEntityWithSkills();

            bool result = _integration.LearnSkill(entityId, "skill_power_strike", 1, 0f);

            Assert.That(result, Is.True);
            Assert.That(_integration.HasSkill(entityId, "skill_power_strike"), Is.True);
            Assert.That(_integration.GetSkillLevel(entityId, "skill_power_strike"), Is.EqualTo(1));
        }

        [Test]
        public void LearnSkill_AlreadyLearned_ReturnsFalse()
        {
            int entityId = CreateEntityWithSkills();
            _integration.LearnSkill(entityId, "skill_power_strike", 1, 0f);

            bool result = _integration.LearnSkill(entityId, "skill_power_strike", 1, 0f);

            Assert.That(result, Is.False);
        }

        [Test]
        public void LearnSkill_PrerequisitesNotMet_ReturnsFalse()
        {
            int entityId = CreateEntityWithSkills();

            // Try to learn advanced skill without prerequisites
            bool result = _integration.LearnSkill(entityId, "skill_devastating_blow", 5, 0f);

            Assert.That(result, Is.False);
        }

        [Test]
        public void LearnSkill_WithCallback_TriggersCallback()
        {
            int entityId = CreateEntityWithSkills();
            bool callbackTriggered = false;
            string learnedSkillId = null;

            _integration.LearnSkill(entityId, "skill_power_strike", 1, 0f,
                onSkillLearned: (entId, skillId, level) =>
                {
                    callbackTriggered = true;
                    learnedSkillId = skillId;
                });

            Assert.That(callbackTriggered, Is.True);
            Assert.That(learnedSkillId, Is.EqualTo("skill_power_strike"));
        }

        #endregion

        #region Skill Level Up Tests

        [Test]
        public void IncreaseSkillLevel_LearnedSkill_Succeeds()
        {
            int entityId = CreateEntityWithSkills();
            _integration.LearnSkill(entityId, "skill_power_strike", 1, 0f);

            bool result = _integration.IncreaseSkillLevel(entityId, "skill_power_strike", 1f);

            Assert.That(result, Is.True);
            Assert.That(_integration.GetSkillLevel(entityId, "skill_power_strike"), Is.EqualTo(2));
        }

        [Test]
        public void IncreaseSkillLevel_UnlearnedSkill_ReturnsFalse()
        {
            int entityId = CreateEntityWithSkills();

            bool result = _integration.IncreaseSkillLevel(entityId, "skill_power_strike", 1f);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IncreaseSkillLevel_AtMaxLevel_ReturnsFalse()
        {
            int entityId = CreateEntityWithSkills();
            var component = MythHunterSkillsComponent.CreateEmpty();
            component.ProgressData.SetSkillLevel("skill_power_strike", 5); // Max level
            _entityManager.AddComponent(entityId, component);

            bool result = _integration.IncreaseSkillLevel(entityId, "skill_power_strike", 1f);

            Assert.That(result, Is.False);
        }

        #endregion

        #region Query Tests

        [Test]
        public void GetLearnedSkills_ReturnsAllLearned()
        {
            int entityId = CreateEntityWithSkills();
            _integration.LearnSkill(entityId, "skill_power_strike", 1, 0f);

            var learned = _integration.GetLearnedSkills(entityId);

            Assert.That(learned.Count, Is.EqualTo(1));
            Assert.That(learned, Contains.Item("skill_power_strike"));
        }

        [Test]
        public void CanLearnSkill_PrerequisitesMet_ReturnsTrue()
        {
            int entityId = CreateEntityWithSkills();
            var component = MythHunterSkillsComponent.CreateEmpty();
            component.ProgressData.SetSkillLevel("skill_power_strike", 3);
            _entityManager.AddComponent(entityId, component);

            bool canLearn = _integration.CanLearnSkill(entityId, "skill_devastating_blow", 10);

            Assert.That(canLearn, Is.True);
        }

        [Test]
        public void GetAvailableSkills_ReturnsOnlyAvailable()
        {
            int entityId = CreateEntityWithSkills();

            var available = _integration.GetAvailableSkills(entityId, 5);

            Assert.That(available.Count, Is.EqualTo(1)); // Only basic skill available
            Assert.That(available[0].Id, Is.EqualTo("skill_power_strike"));
        }

        #endregion

        #region Favorites Tests

        [Test]
        public void AddFavoriteSkill_AddsFavorite()
        {
            int entityId = CreateEntityWithSkills();
            _integration.LearnSkill(entityId, "skill_power_strike", 1, 0f);

            _integration.AddFavoriteSkill(entityId, "skill_power_strike");

            var favorites = _integration.GetFavoriteSkills(entityId);
            Assert.That(favorites, Contains.Item("skill_power_strike"));
        }

        [Test]
        public void RemoveFavoriteSkill_RemovesFavorite()
        {
            int entityId = CreateEntityWithSkills();
            _integration.AddFavoriteSkill(entityId, "skill_power_strike");
            _integration.RemoveFavoriteSkill(entityId, "skill_power_strike");

            var favorites = _integration.GetFavoriteSkills(entityId);
            Assert.That(favorites, Does.Not.Contain("skill_power_strike"));
        }

        #endregion

        #region Stat Bonuses Tests

        [Test]
        public void CalculateStatBonuses_ReturnsBonusesFromSkills()
        {
            int entityId = CreateEntityWithSkills();
            _integration.LearnSkill(entityId, "skill_power_strike", 1, 0f);
            _integration.IncreaseSkillLevel(entityId, "skill_power_strike", 1f); // Level 2

            var bonuses = _integration.CalculateStatBonuses(entityId);

            Assert.That(bonuses.ContainsKey("AttackPower"), Is.True);
            Assert.That(bonuses["AttackPower"], Is.EqualTo(10f)); // 5 per level * 2 levels
        }

        [Test]
        public void CalculateStatBonuses_MultipleSkills_SumsBonuses()
        {
            int entityId = CreateEntityWithSkills();
            var component = MythHunterSkillsComponent.CreateEmpty();
            component.ProgressData.SetSkillLevel("skill_power_strike", 3);
            component.ProgressData.SetSkillLevel("skill_devastating_blow", 2);
            _entityManager.AddComponent(entityId, component);

            var bonuses = _integration.CalculateStatBonuses(entityId);

            Assert.That(bonuses["AttackPower"], Is.EqualTo(15f)); // 5*3 from power strike
            Assert.That(bonuses["CriticalChance"], Is.EqualTo(4f)); // 2*2 from devastating blow
        }

        #endregion

        #region Component Creation Tests

        [Test]
        public void MythHunterSkillsComponent_CreateHero_CreatesEmpty()
        {
            var component = MythHunterSkillsComponent.CreateHero();

            Assert.That(component.ProgressData.SkillLevels, Is.Not.Null);
            Assert.That(component.TotalSkillPointsSpent, Is.EqualTo(0));
        }

        [Test]
        public void MythHunterSkillsComponent_CreateHero_WithStartingSkills()
        {
            var startingSkills = new List<string> { "skill_1", "skill_2" };
            var component = MythHunterSkillsComponent.CreateHero(startingSkills);

            Assert.That(component.ProgressData.GetSkillLevel("skill_1"), Is.EqualTo(1));
            Assert.That(component.ProgressData.GetSkillLevel("skill_2"), Is.EqualTo(1));
            Assert.That(component.TotalSkillPointsSpent, Is.EqualTo(2));
        }

        [Test]
        public void MythHunterSkillsComponent_CreateEnemy()
        {
            var skills = new Dictionary<string, int>
            {
                { "skill_1", 3 },
                { "skill_2", 5 }
            };
            var component = MythHunterSkillsComponent.CreateEnemy(skills, "tree_warrior");

            Assert.That(component.ProgressData.GetSkillLevel("skill_1"), Is.EqualTo(3));
            Assert.That(component.ProgressData.GetSkillLevel("skill_2"), Is.EqualTo(5));
            Assert.That(component.TotalSkillPointsSpent, Is.EqualTo(8)); // 3+5
            Assert.That(component.ActiveSkillTreeId, Is.EqualTo("tree_warrior"));
        }

        #endregion
    }
}
