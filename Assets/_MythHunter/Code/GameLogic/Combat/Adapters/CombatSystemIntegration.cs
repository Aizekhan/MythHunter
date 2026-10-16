// Assets/_MythHunter/Code/GameLogic/Combat/Adapters/CombatSystemIntegration.cs
using MythHunter.Components.Combat;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using RPGFramework.ECS;

namespace MythHunter.GameLogic.Combat.Adapters
{
    /// <summary>
    /// Integration helper for CombatSystem to work with both old and new components.
    /// Provides backward-compatible API during migration period.
    /// </summary>
    public class CombatSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly MythHunterDamageAdapter _damageAdapter;
        private readonly IMythLogger _logger;

        [Inject]
        public CombatSystemIntegration(
            IEntityManager entityManager,
            MythHunterDamageAdapter damageAdapter,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _damageAdapter = damageAdapter;
            _logger = logger;
        }

        #region Combat State Helpers

        /// <summary>
        /// Check if entity is in combat (supports both old and new components)
        /// </summary>
        public bool IsInCombat(int entityId)
        {
            // Try new component first
            if (_entityManager.TryGetComponent<CombatStateComponent>(entityId, out var combatState))
            {
                return combatState.IsInCombat;
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                return oldStats.IsInCombat;
            }

            return false;
        }

        /// <summary>
        /// Set combat state for entity
        /// </summary>
        public void SetCombatState(int entityId, bool isInCombat, int targetEntityId = -1)
        {
            // Update new component if exists
            if (_entityManager.TryGetComponent<CombatStateComponent>(entityId, out var combatState))
            {
                if (isInCombat)
                {
                    combatState.EnterCombat(targetEntityId);
                }
                else
                {
                    combatState.ExitCombat();
                }
                _entityManager.AddComponent(entityId, combatState);
            }

            // Update old component if exists (backward compatibility)
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                oldStats.IsInCombat = isInCombat;
                oldStats.CombatTargetEntityId = targetEntityId;
                _entityManager.AddComponent(entityId, oldStats);
            }
        }

        /// <summary>
        /// Get combat target entity ID
        /// </summary>
        public int GetCombatTarget(int entityId)
        {
            // Try new component first
            if (_entityManager.TryGetComponent<CombatStateComponent>(entityId, out var combatState))
            {
                return combatState.CombatTargetEntityId;
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                return oldStats.CombatTargetEntityId;
            }

            return -1;
        }

        /// <summary>
        /// Update last attack time for entity
        /// </summary>
        public void UpdateLastAttackTime(int entityId, float time)
        {
            // Update new component if exists
            if (_entityManager.TryGetComponent<CombatStateComponent>(entityId, out var combatState))
            {
                combatState.LastAttackTime = time;
                _entityManager.AddComponent(entityId, combatState);
            }

            // Update old component if exists
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                oldStats.LastAttackTime = time;
                _entityManager.AddComponent(entityId, oldStats);
            }
        }

        /// <summary>
        /// Check if entity can attack (based on attack speed)
        /// </summary>
        public bool CanAttack(int entityId, float currentTime)
        {
            float attackSpeed = GetAttackSpeed(entityId);

            // Try new component first
            if (_entityManager.TryGetComponent<CombatStateComponent>(entityId, out var combatState))
            {
                return combatState.CanAttack(currentTime, attackSpeed);
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                float attackInterval = 1.0f / attackSpeed;
                return (currentTime - oldStats.LastAttackTime) >= attackInterval;
            }

            return false;
        }

        #endregion

        #region Stat Helpers

        /// <summary>
        /// Get attack power for entity
        /// </summary>
        public float GetAttackPower(int entityId)
        {
            // Try new component first
            if (_entityManager.TryGetComponent<MythHunterStatsComponent>(entityId, out var stats))
            {
                return stats.Stats.GetFinalStat(CombatStatType.AttackPower);
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                return oldStats.AttackPower;
            }

            return 0f;
        }

        /// <summary>
        /// Get defense for entity
        /// </summary>
        public float GetDefense(int entityId)
        {
            // Try new component first
            if (_entityManager.TryGetComponent<MythHunterStatsComponent>(entityId, out var stats))
            {
                return stats.Stats.GetFinalStat(CombatStatType.Defense);
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                return oldStats.Defense;
            }

            return 0f;
        }

        /// <summary>
        /// Get attack speed for entity
        /// </summary>
        public float GetAttackSpeed(int entityId)
        {
            // Try new component first
            if (_entityManager.TryGetComponent<MythHunterStatsComponent>(entityId, out var stats))
            {
                return stats.Stats.GetFinalStat(CombatStatType.AttackSpeed);
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                return oldStats.AttackSpeed;
            }

            return 1.0f;
        }

        /// <summary>
        /// Get critical chance for entity
        /// </summary>
        public float GetCriticalChance(int entityId)
        {
            // Try new component first
            if (_entityManager.TryGetComponent<MythHunterStatsComponent>(entityId, out var stats))
            {
                return stats.Stats.GetFinalStat(CombatStatType.CriticalChance);
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                return oldStats.CriticalChance;
            }

            return 0.05f;
        }

        #endregion

        #region Resource Helpers

        /// <summary>
        /// Get current rage for entity
        /// </summary>
        public float GetRage(int entityId)
        {
            // Try new component first
            if (_entityManager.TryGetComponent<MythHunterResourceComponent>(entityId, out var resources))
            {
                return resources.RageCurrent;
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                return oldStats.Rage;
            }

            return 0f;
        }

        /// <summary>
        /// Add rage to entity
        /// </summary>
        public void AddRage(int entityId, float amount)
        {
            // Update new component if exists
            if (_entityManager.TryGetComponent<MythHunterResourceComponent>(entityId, out var resources))
            {
                resources.RageCurrent = System.Math.Min(resources.RageMax, resources.RageCurrent + amount);
                _entityManager.AddComponent(entityId, resources);
            }

            // Update old component if exists
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                oldStats.Rage = UnityEngine.Mathf.Min(oldStats.MaxRage, oldStats.Rage + amount);
                _entityManager.AddComponent(entityId, oldStats);
            }
        }

        /// <summary>
        /// Get current concentration for entity
        /// </summary>
        public float GetConcentration(int entityId)
        {
            // Try new component first
            if (_entityManager.TryGetComponent<MythHunterResourceComponent>(entityId, out var resources))
            {
                return resources.ConcentrationCurrent;
            }

            // Fallback to old component
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                return oldStats.Concentration;
            }

            return 0f;
        }

        /// <summary>
        /// Add concentration to entity
        /// </summary>
        public void AddConcentration(int entityId, float amount)
        {
            // Update new component if exists
            if (_entityManager.TryGetComponent<MythHunterResourceComponent>(entityId, out var resources))
            {
                resources.ConcentrationCurrent = System.Math.Min(resources.ConcentrationMax, resources.ConcentrationCurrent + amount);
                _entityManager.AddComponent(entityId, resources);
            }

            // Update old component if exists
            if (_entityManager.TryGetComponent<CombatStatsComponent>(entityId, out var oldStats))
            {
                oldStats.Concentration = UnityEngine.Mathf.Min(oldStats.MaxConcentration, oldStats.Concentration + amount);
                _entityManager.AddComponent(entityId, oldStats);
            }
        }

        /// <summary>
        /// Check if entity is alive
        /// </summary>
        public bool IsAlive(int entityId)
        {
            // Try new component first
            if (_entityManager.TryGetComponent<MythHunterResourceComponent>(entityId, out var resources))
            {
                return resources.IsAlive();
            }

            // Fallback to HealthComponent (old system)
            if (_entityManager.TryGetComponent<HealthComponent>(entityId, out var health))
            {
                return !health.IsDead;
            }

            return true; // Default to alive if no health component
        }

        #endregion

        #region Damage Integration

        /// <summary>
        /// Apply damage using Framework damage calculator (preferred method)
        /// Falls back to old method if new components not available
        /// </summary>
        public float ApplyDamage(int sourceEntityId, int targetEntityId, float damageMultiplier = 1.0f)
        {
            // Check if both entities have new components
            bool sourceHasNewComponents = _entityManager.HasComponent<MythHunterStatsComponent>(sourceEntityId);
            bool targetHasNewComponents = _entityManager.HasComponent<MythHunterStatsComponent>(targetEntityId) &&
                                          _entityManager.HasComponent<MythHunterResourceComponent>(targetEntityId);

            if (sourceHasNewComponents && targetHasNewComponents)
            {
                // Use new Framework-based damage calculation
                var result = _damageAdapter.ApplyPhysicalDamage(sourceEntityId, targetEntityId, damageMultiplier);
                return result.FinalDamage;
            }
            else
            {
                // Fallback to old damage calculation
                _logger.LogWarning(
                    $"Using old damage calculation for {sourceEntityId} -> {targetEntityId} (missing new components)",
                    "CombatIntegration");

                return ApplyDamageOldMethod(sourceEntityId, targetEntityId, damageMultiplier);
            }
        }

        /// <summary>
        /// Old damage calculation method (for backward compatibility)
        /// </summary>
        private float ApplyDamageOldMethod(int sourceEntityId, int targetEntityId, float damageMultiplier)
        {
            // This is a simplified version - real old method would be in CombatSystem
            float attackPower = GetAttackPower(sourceEntityId);
            float defense = GetDefense(targetEntityId);

            // Simple formula (not Framework formula)
            float damage = UnityEngine.Mathf.Max(1f, (attackPower - defense) * damageMultiplier);

            // Apply to health (if HealthComponent exists)
            if (_entityManager.TryGetComponent<HealthComponent>(targetEntityId, out var health))
            {
                health.CurrentHealth -= damage;
                _entityManager.AddComponent(targetEntityId, health);
            }

            return damage;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Check if entity has new Framework-based components
        /// </summary>
        public bool HasNewComponents(int entityId)
        {
            return _entityManager.HasComponent<MythHunterStatsComponent>(entityId) &&
                   _entityManager.HasComponent<MythHunterResourceComponent>(entityId) &&
                   _entityManager.HasComponent<CombatStateComponent>(entityId);
        }

        /// <summary>
        /// Check if entity has old components
        /// </summary>
        public bool HasOldComponents(int entityId)
        {
            return _entityManager.HasComponent<CombatStatsComponent>(entityId);
        }

        #endregion
    }
}
