// Assets/_MythHunter/Code/GameLogic/Combat/Adapters/MythHunterDamageAdapter.cs
using MythHunter.Components.Combat;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using RPGFramework.Combat;
using RPGFramework.ECS;

namespace MythHunter.GameLogic.Combat.Adapters
{
    /// <summary>
    /// Adapter for calculating damage using Framework DamageCalculator.
    /// Bridges MythHunter combat system with RPGFramework damage calculation.
    /// </summary>
    public class MythHunterDamageAdapter
    {
        private readonly IDamageCalculator _damageCalculator;
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        [Inject]
        public MythHunterDamageAdapter(
            IDamageCalculator damageCalculator,
            IEntityManager entityManager,
            IMythLogger logger)
        {
            _damageCalculator = damageCalculator;
            _entityManager = entityManager;
            _logger = logger;
        }

        /// <summary>
        /// Calculate and apply physical damage from attacker to target
        /// </summary>
        /// <param name="attackerEntityId">Entity dealing damage</param>
        /// <param name="targetEntityId">Entity receiving damage</param>
        /// <param name="damageMultiplier">Optional damage multiplier (e.g., from abilities)</param>
        /// <returns>Damage calculation result with details</returns>
        public DamageResult ApplyPhysicalDamage(
            int attackerEntityId,
            int targetEntityId,
            float damageMultiplier = 1.0f)
        {
            return ApplyDamage(attackerEntityId, targetEntityId, DamageType.Physical, damageMultiplier);
        }

        /// <summary>
        /// Calculate and apply magical damage from attacker to target
        /// </summary>
        public DamageResult ApplyMagicalDamage(
            int attackerEntityId,
            int targetEntityId,
            float damageMultiplier = 1.0f)
        {
            return ApplyDamage(attackerEntityId, targetEntityId, DamageType.Magical, damageMultiplier);
        }

        /// <summary>
        /// Calculate and apply damage from attacker to target
        /// </summary>
        /// <param name="attackerEntityId">Entity dealing damage</param>
        /// <param name="targetEntityId">Entity receiving damage</param>
        /// <param name="damageType">Type of damage (Physical, Magical, Pure)</param>
        /// <param name="damageMultiplier">Damage multiplier (default 1.0)</param>
        /// <param name="armorPenetration">Armor penetration percentage (default 0)</param>
        /// <returns>Damage calculation result</returns>
        public DamageResult ApplyDamage(
            int attackerEntityId,
            int targetEntityId,
            DamageType damageType,
            float damageMultiplier = 1.0f,
            float armorPenetration = 0f)
        {
            // Validate entities exist
            if (!_entityManager.EntityExists(attackerEntityId))
            {
                _logger.LogWarning($"Attacker entity {attackerEntityId} does not exist", "DamageAdapter");
                return default;
            }

            if (!_entityManager.EntityExists(targetEntityId))
            {
                _logger.LogWarning($"Target entity {targetEntityId} does not exist", "DamageAdapter");
                return default;
            }

            // Get attacker stats
            if (!_entityManager.TryGetComponent<MythHunterStatsComponent>(attackerEntityId, out var attackerStats))
            {
                _logger.LogWarning($"Attacker {attackerEntityId} missing MythHunterStatsComponent", "DamageAdapter");
                return default;
            }

            // Get target stats and resources
            if (!_entityManager.TryGetComponent<MythHunterStatsComponent>(targetEntityId, out var targetStats))
            {
                _logger.LogWarning($"Target {targetEntityId} missing MythHunterStatsComponent", "DamageAdapter");
                return default;
            }

            if (!_entityManager.TryGetComponent<MythHunterResourceComponent>(targetEntityId, out var targetResources))
            {
                _logger.LogWarning($"Target {targetEntityId} missing MythHunterResourceComponent", "DamageAdapter");
                return default;
            }

            // Extract combat stats using Framework StatContainer
            float baseDamage = attackerStats.Stats.GetFinalStat(CombatStatType.AttackPower);
            float critChance = attackerStats.Stats.GetFinalStat(CombatStatType.CriticalChance);
            float critMultiplier = attackerStats.Stats.GetFinalStat(CombatStatType.CriticalMultiplier);
            float defense = targetStats.Stats.GetFinalStat(CombatStatType.Defense);

            // Calculate damage using Framework DamageCalculator
            DamageResult result = _damageCalculator.CalculateDamage(
                baseDamage,
                defense,
                damageType,
                damageMultiplier,
                armorPenetration,
                critChance,
                critMultiplier
            );

            // Apply damage to target health
            float currentHealth = targetResources.Resources.GetCurrent(MythHunterResourceType.Health);
            float newHealth = currentHealth - result.FinalDamage;

            // Update health (clamped by ResourceComponent)
            targetResources.Resources.SetCurrent(MythHunterResourceType.Health, newHealth);
            _entityManager.SetComponent(targetEntityId, targetResources);

            // Log damage details
            _logger.LogInfo(
                $"Damage: {attackerEntityId} → {targetEntityId}: " +
                $"{result.FinalDamage:F1} ({damageType})" +
                $"{(result.IsCritical ? " CRIT!" : "")} " +
                $"[HP: {currentHealth:F1} → {newHealth:F1}]",
                "Combat");

            return result;
        }

        /// <summary>
        /// Check if target would die from specified damage (without applying it)
        /// </summary>
        public bool WouldKill(int targetEntityId, float damage)
        {
            if (!_entityManager.TryGetComponent<MythHunterResourceComponent>(targetEntityId, out var resources))
                return false;

            float currentHealth = resources.Resources.GetCurrent(MythHunterResourceType.Health);
            return damage >= currentHealth;
        }

        /// <summary>
        /// Get effective damage reduction from defense (percentage)
        /// </summary>
        public float GetDamageReduction(int entityId, DamageType damageType)
        {
            if (!_entityManager.TryGetComponent<MythHunterStatsComponent>(entityId, out var stats))
                return 0f;

            float defense = stats.Stats.GetFinalStat(CombatStatType.Defense);

            // Framework damage formula: damage * (100 / (100 + defense))
            // Reduction: 1 - (100 / (100 + defense)) = defense / (100 + defense)
            return defense / (100f + defense);
        }
    }
}
