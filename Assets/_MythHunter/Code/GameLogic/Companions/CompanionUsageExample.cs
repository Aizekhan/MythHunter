using RPGFramework.Companions.Runtime;
using System;

namespace MythHunter.GameLogic.Companions
{
    /// <summary>
    /// Example usage of the Companion System in MythHunter.
    /// Demonstrates how to create, manage, and interact with companions.
    /// </summary>
    public class CompanionUsageExample
    {
        private ICompanionManager _companionManager;

        public CompanionUsageExample()
        {
            _companionManager = new CompanionManager();
        }

        /// <summary>
        /// Example: Player acquires their first companion (a wolf).
        /// </summary>
        public void ExampleAcquireWolf(int playerId)
        {
            // Create a wolf companion
            var wolf = MythHunterCompanions.CreateWolf(_companionManager, playerId, "Fenrir");

            Console.WriteLine($"Acquired companion: {wolf.Name} (Level {wolf.Level})");
            Console.WriteLine($"Type: {wolf.CompanionType}");
            Console.WriteLine($"Loyalty: {wolf.Loyalty}");

            // Summon the wolf
            wolf.Summon();
            Console.WriteLine($"Wolf summoned! Current state: {wolf.CurrentState}");

            // Give the wolf a command
            wolf.IssueCommand(CompanionCommand.Follow);
            Console.WriteLine($"Wolf is now following you.");
        }

        /// <summary>
        /// Example: Player mounts a horse for fast travel.
        /// </summary>
        public void ExampleMountHorse(int playerId)
        {
            // Create a horse
            var horse = MythHunterCompanions.CreateHorse(_companionManager, playerId, "Shadowfax");

            Console.WriteLine($"Acquired mount: {horse.Name}");

            // Get horse stats
            var stats = _companionManager.GetCompanionStats(horse.CompanionId);
            Console.WriteLine($"Speed: {stats.Speed}x");
            Console.WriteLine($"Health: {stats.CurrentHealth}/{stats.MaxHealth}");

            // Summon and mount
            horse.Summon();
            Console.WriteLine("You mount the horse. Travel speed increased!");
        }

        /// <summary>
        /// Example: Player feeds their companion to maintain happiness and loyalty.
        /// </summary>
        public void ExampleFeedCompanion(string companionId)
        {
            var companion = _companionManager.GetCompanion(companionId);
            var stats = _companionManager.GetCompanionStats(companionId);

            Console.WriteLine($"Before feeding:");
            Console.WriteLine($"  Hunger: {stats.Hunger}/100");
            Console.WriteLine($"  Happiness: {stats.Happiness}/100");
            Console.WriteLine($"  Loyalty: {companion.Loyalty}");

            // Feed the companion
            bool fed = _companionManager.FeedCompanion(companionId, "meat_001");

            if (fed)
            {
                Console.WriteLine($"\nAfter feeding:");
                Console.WriteLine($"  Hunger: {stats.Hunger}/100");
                Console.WriteLine($"  Happiness: {stats.Happiness}/100");
                Console.WriteLine($"  Loyalty: {companion.Loyalty}");
            }
        }

        /// <summary>
        /// Example: Player summons a Fire Elemental for a tough battle.
        /// </summary>
        public void ExampleSummonFireElemental(int playerId)
        {
            // Create fire elemental
            var elemental = MythHunterCompanions.CreateFireElemental(_companionManager, playerId, "Ignis");

            Console.WriteLine($"Summoned: {elemental.Name}");

            // Get elemental stats
            var stats = _companionManager.GetCompanionStats(elemental.CompanionId);
            Console.WriteLine($"Attack Power: {stats.Attack}");
            Console.WriteLine($"Defense: {stats.Defense}");

            // Summon and command to attack
            elemental.Summon();
            elemental.IssueCommand(CompanionCommand.Attack);

            Console.WriteLine("Fire Elemental joins the battle!");
        }

        /// <summary>
        /// Example: Player uses their raven familiar to scout an area.
        /// </summary>
        public void ExampleRavenScout(int playerId)
        {
            // Create raven familiar
            var raven = MythHunterCompanions.CreateRaven(_companionManager, playerId, "Huginn");

            Console.WriteLine($"Familiar bound: {raven.Name}");

            // Get the scout ability
            var scoutAbility = MythHunterCompanions.Abilities.CreateRavenScout();

            // Summon the raven
            raven.Summon();

            // Use the scout ability
            if (scoutAbility.CanUse())
            {
                bool success = scoutAbility.Use(raven);
                if (success)
                {
                    Console.WriteLine($"{raven.Name} scouts the area!");
                    Console.WriteLine($"Ability on cooldown for {scoutAbility.Cooldown} seconds.");
                }
            }
            else
            {
                Console.WriteLine($"Scout ability on cooldown. {scoutAbility.RemainingCooldown}s remaining.");
            }
        }

        /// <summary>
        /// Example: Combat scenario with a wolf companion.
        /// </summary>
        public void ExampleCombatWithWolf(string wolfId, int enemyId)
        {
            var wolf = _companionManager.GetCompanion(wolfId);
            var stats = _companionManager.GetCompanionStats(wolfId);
            var behavior = _companionManager.GetCompanionBehavior(wolfId);

            Console.WriteLine($"{wolf.Name} enters combat!");

            // Command wolf to attack
            behavior.AttackTarget(enemyId);
            Console.WriteLine($"  State: {wolf.CurrentState}");
            Console.WriteLine($"  Target: Enemy #{behavior.GetCurrentTarget()}");

            // Wolf takes damage
            stats.TakeDamage(30);
            Console.WriteLine($"  {wolf.Name} takes 30 damage! HP: {stats.CurrentHealth}/{stats.MaxHealth}");

            // Use wolf's howl ability
            var howlAbility = MythHunterCompanions.Abilities.CreateWolfHowl();
            if (wolf.Level >= 3 && howlAbility.CanUse())
            {
                howlAbility.Use(wolf, enemyId);
                Console.WriteLine($"  {wolf.Name} howls, intimidating the enemy!");
            }
        }

        /// <summary>
        /// Example: Companion leveling up through experience gain.
        /// </summary>
        public void ExampleCompanionLevelUp(string companionId)
        {
            var companion = _companionManager.GetCompanion(companionId);
            var stats = _companionManager.GetCompanionStats(companionId);

            Console.WriteLine($"{companion.Name} - Level {companion.Level}");
            Console.WriteLine($"Experience: {stats.Experience}/{stats.ExperienceToNextLevel}");

            // Gain experience from combat
            stats.GainExperience(120);

            if (stats.Experience >= stats.ExperienceToNextLevel)
            {
                // Level up!
                if (stats is CompanionStats concreteStats && companion is Companion concreteCompanion)
                {
                    int oldLevel = companion.Level;
                    concreteCompanion.LevelUp();
                    concreteStats.LevelUp(companion.Level);

                    Console.WriteLine($"\n*** LEVEL UP! ***");
                    Console.WriteLine($"{companion.Name} is now level {companion.Level}!");
                    Console.WriteLine($"Stats increased:");
                    Console.WriteLine($"  Max HP: {stats.MaxHealth}");
                    Console.WriteLine($"  Attack: {stats.Attack}");
                    Console.WriteLine($"  Defense: {stats.Defense}");
                }
            }
        }

        /// <summary>
        /// Example: Managing multiple companions.
        /// </summary>
        public void ExampleManageMultipleCompanions(int playerId)
        {
            // Create multiple companions
            var wolf = MythHunterCompanions.CreateWolf(_companionManager, playerId);
            var horse = MythHunterCompanions.CreateHorse(_companionManager, playerId);
            var cat = MythHunterCompanions.CreateCat(_companionManager, playerId);

            // Get all player's companions
            var playerCompanions = _companionManager.GetCompanionsByOwner(playerId);

            Console.WriteLine($"Player's Companions ({playerCompanions.Count}):");
            foreach (var companion in playerCompanions)
            {
                Console.WriteLine($"  - {companion.Name} ({companion.CompanionType})");
                Console.WriteLine($"    Level: {companion.Level}, Loyalty: {companion.Loyalty}");
                Console.WriteLine($"    Active: {companion.IsActive}");
            }

            // Summon the wolf for combat
            wolf.Summon();
            Console.WriteLine($"\n{wolf.Name} has been summoned!");

            // Check active companions
            var activeCompanions = _companionManager.ActiveCompanions;
            Console.WriteLine($"\nActive Companions: {activeCompanions.Count}");
        }

        /// <summary>
        /// Example: Game loop update for companions.
        /// This would be called every frame/tick.
        /// </summary>
        public void ExampleGameLoopUpdate(float deltaTime)
        {
            // Update all companions (regeneration, hunger decay, AI behavior)
            _companionManager.UpdateCompanions(deltaTime);

            // Check if any companions need care
            foreach (var companion in _companionManager.ActiveCompanions)
            {
                var stats = _companionManager.GetCompanionStats(companion.CompanionId);

                // Warn if companion is hungry
                if (stats.Hunger < 30)
                {
                    Console.WriteLine($"Warning: {companion.Name} is hungry! (Hunger: {stats.Hunger}/100)");
                }

                // Warn if companion is unhappy
                if (stats.Happiness < 30)
                {
                    Console.WriteLine($"Warning: {companion.Name} is unhappy! (Happiness: {stats.Happiness}/100)");
                }

                // Check if companion needs healing
                if (stats.CurrentHealth < stats.MaxHealth * 0.3f)
                {
                    Console.WriteLine($"Warning: {companion.Name} is low on health! ({stats.CurrentHealth}/{stats.MaxHealth})");
                }
            }
        }
    }
}
