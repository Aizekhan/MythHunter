// Assets/_MythHunter/Code/GameLogic/Abilities/Adapters/AbilitySystemIntegration.cs

using System;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.ECS;
using RPGFramework.Abilities;
using RPGFramework.DI;
using MythHunter.Utils.Logging;
using MythHunter.GameLogic.Abilities.Components;

namespace MythHunter.GameLogic.Abilities.Adapters
{
    /// <summary>
    /// Integration helper for ability execution operations.
    /// Bridges Framework Ability System (RPGFramework.Abilities.Runtime) with MythHunter game layer.
    ///
    /// Pattern (from EPIC 13 Phase 2):
    /// - Greenfield integration (no legacy ability code to migrate)
    /// - Uses MythHunterAbilityComponent for per-entity ability state
    /// - Executes abilities via Framework IAbilityExecutor
    /// - Manages cooldowns, casting, charges at component level
    ///
    /// Key Responsibilities:
    /// - Execute abilities with resource validation
    /// - Manage cooldown state on components
    /// - Query entities with specific abilities
    /// - Bridge Framework callbacks with MythHunter game systems (Combat, Stats, etc.)
    /// </summary>
    public class AbilitySystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly IAbilityExecutor _abilityExecutor;
        private readonly IAbilityRegistry _abilityRegistry;
        private readonly IMythLogger _logger;

        private const string LogCategory = "AbilityIntegration";

        [Inject]
        public AbilitySystemIntegration(
            IEntityManager entityManager,
            IAbilityExecutor abilityExecutor,
            IAbilityRegistry abilityRegistry,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _abilityExecutor = abilityExecutor;
            _abilityRegistry = abilityRegistry;
            _logger = logger;
        }

        // ============================================================
        // ABILITY EXECUTION OPERATIONS
        // ============================================================

        /// <summary>
        /// Executes ability for entity using MythHunterAbilityComponent.
        /// Validates cooldown, charges, and casting state before execution.
        /// Updates component state after execution (cooldown, charges).
        /// </summary>
        /// <param name="casterEntityId">Entity using ability</param>
        /// <param name="abilityId">Ability ID to execute</param>
        /// <param name="targetEntityId">Target entity (-1 for self/none)</param>
        /// <param name="currentTime">Current game time</param>
        /// <param name="resourceCheck">Callback to check resources (entityId, resourceType, amount) -> hasEnough</param>
        /// <param name="resourceConsume">Callback to consume resources (entityId, resourceType, amount)</param>
        /// <param name="applyEffect">Callback to apply ability effects (casterEntityId, targetEntityId, abilityId)</param>
        /// <returns>Execution result</returns>
        public AbilityExecutionResult ExecuteAbility(
            int casterEntityId,
            string abilityId,
            int targetEntityId,
            float currentTime,
            Func<int, string, float, bool> resourceCheck,
            Action<int, string, float> resourceConsume,
            Action<int, int, string> applyEffect)
        {
            // Get ability component
            if (!TryGetAbilityComponent(casterEntityId, out var abilityComponent))
            {
                _logger.LogWarning($"Entity {casterEntityId} has no ability component", LogCategory);
                return AbilityExecutionResult.CreateFailure(abilityId, casterEntityId, "No ability component", currentTime);
            }

            // Check if entity has this ability
            if (!abilityComponent.HasAbility(abilityId))
            {
                _logger.LogWarning($"Entity {casterEntityId} does not have ability '{abilityId}'", LogCategory);
                return AbilityExecutionResult.CreateFailure(abilityId, casterEntityId, "Ability not learned", currentTime);
            }

            // Get ability instance data
            if (!abilityComponent.TryGetAbility(abilityId, out var abilityData))
            {
                _logger.LogWarning($"Failed to get ability data for '{abilityId}' on entity {casterEntityId}", LogCategory);
                return AbilityExecutionResult.CreateFailure(abilityId, casterEntityId, "Ability data not found", currentTime);
            }

            // Check if ability can be used (cooldown, charges, casting)
            if (!CanExecuteAbility(casterEntityId, abilityId, currentTime))
            {
                string reason = GetCannotUseReason(abilityComponent, abilityId, currentTime);
                _logger.LogDebug($"Entity {casterEntityId} cannot use ability '{abilityId}': {reason}", LogCategory);
                return AbilityExecutionResult.CreateFailure(abilityId, casterEntityId, reason, currentTime);
            }

            // Get ability definition from registry
            IAbility ability = _abilityRegistry.GetAbility(abilityId);
            if (ability == null)
            {
                _logger.LogWarning($"Ability '{abilityId}' not found in registry", LogCategory);
                return AbilityExecutionResult.CreateFailure(abilityId, casterEntityId, "Ability not registered", currentTime);
            }

            // Execute via Framework system (with callbacks for resource checks and effects)
            AbilityExecutionResult result = _abilityExecutor.ExecuteAbility(
                abilityId,
                casterEntityId,
                targetEntityId,
                currentTime,
                resourceCheck,
                resourceConsume,
                applyEffect
            );

            // Update component state if execution succeeded
            if (result.Success)
            {
                UpdateComponentAfterExecution(casterEntityId, abilityId, ability, currentTime);

                _logger.LogInfo(
                    $"Entity {casterEntityId} executed ability '{abilityId}' " +
                    $"(target={targetEntityId}, level={abilityData.Level})",
                    LogCategory
                );
            }
            else
            {
                _logger.LogDebug(
                    $"Ability execution failed for entity {casterEntityId}: {result.FailureReason}",
                    LogCategory
                );
            }

            return result;
        }

        /// <summary>
        /// Checks if entity can execute ability (without actually executing it).
        /// Useful for UI ability button highlighting, AI decision making.
        /// </summary>
        /// <param name="casterEntityId">Entity that would use ability</param>
        /// <param name="abilityId">Ability ID to check</param>
        /// <param name="currentTime">Current game time</param>
        /// <returns>True if ability can be executed right now</returns>
        public bool CanExecuteAbility(int casterEntityId, string abilityId, float currentTime)
        {
            if (!TryGetAbilityComponent(casterEntityId, out var component))
                return false;

            if (!component.TryGetAbility(abilityId, out var data))
                return false;

            // Check cooldown, charges, and casting state
            return !data.CooldownState.IsOnCooldown(currentTime)
                && (data.RemainingCharges == -1 || data.RemainingCharges > 0)
                && !component.IsCastingAny();
        }

        /// <summary>
        /// Cancels active cast for entity.
        /// Clears CurrentlyCasting.
        /// </summary>
        /// <param name="entityId">Entity whose cast should be canceled</param>
        public void CancelCast(int entityId)
        {
            if (!TryGetAbilityComponent(entityId, out var component))
            {
                _logger.LogWarning($"Cannot cancel cast - entity {entityId} has no ability component", LogCategory);
                return;
            }

            if (!component.IsCastingAny())
            {
                _logger.LogDebug($"Entity {entityId} is not casting", LogCategory);
                return;
            }

            string abilityId = component.CurrentlyCasting;
            component.CurrentlyCasting = null;

            // Update ability data to stop casting
            if (component.TryGetAbility(abilityId, out var data))
            {
                data.IsCasting = false;
                data.CastStartTime = 0f;
                component.UpdateAbility(abilityId, data);
            }

            _entityManager.AddComponent(entityId, component);

            _logger.LogInfo($"Canceled cast for entity {entityId} ability '{abilityId}'", LogCategory);
        }

        // ============================================================
        // COMPONENT MANAGEMENT OPERATIONS
        // ============================================================

        /// <summary>
        /// Adds ability to entity.
        /// Creates MythHunterAbilityComponent if entity doesn't have one.
        /// </summary>
        /// <param name="entityId">Entity to add ability to</param>
        /// <param name="abilityId">Ability ID from registry</param>
        /// <param name="level">Initial ability level</param>
        /// <param name="maxCharges">Max charges for charge-based abilities (-1 for unlimited)</param>
        /// <returns>True if ability was added successfully</returns>
        public bool AddAbilityToEntity(int entityId, string abilityId, int level = 1, int maxCharges = -1)
        {
            // Verify ability exists in registry
            if (!_abilityRegistry.HasAbility(abilityId))
            {
                _logger.LogWarning($"Cannot add ability '{abilityId}' - not registered", LogCategory);
                return false;
            }

            // Get or create component
            MythHunterAbilityComponent component;
            if (!TryGetAbilityComponent(entityId, out component))
            {
                component = MythHunterAbilityComponent.CreateEmpty();
            }

            // Check if entity already has this ability
            if (component.HasAbility(abilityId))
            {
                _logger.LogDebug($"Entity {entityId} already has ability '{abilityId}'", LogCategory);
                return false;
            }

            // Add ability to component
            component.AddAbility(abilityId, level, maxCharges);
            _entityManager.AddComponent(entityId, component);

            _logger.LogInfo($"Added ability '{abilityId}' (level {level}) to entity {entityId}", LogCategory);
            return true;
        }

        /// <summary>
        /// Removes ability from entity.
        /// </summary>
        /// <param name="entityId">Entity to remove ability from</param>
        /// <param name="abilityId">Ability ID to remove</param>
        /// <returns>True if ability was removed</returns>
        public bool RemoveAbilityFromEntity(int entityId, string abilityId)
        {
            if (!TryGetAbilityComponent(entityId, out var component))
            {
                _logger.LogDebug($"Entity {entityId} has no ability component", LogCategory);
                return false;
            }

            if (!component.RemoveAbility(abilityId))
            {
                _logger.LogDebug($"Entity {entityId} does not have ability '{abilityId}' to remove", LogCategory);
                return false;
            }

            _entityManager.AddComponent(entityId, component);
            _logger.LogInfo($"Removed ability '{abilityId}' from entity {entityId}", LogCategory);

            return true;
        }

        /// <summary>
        /// Levels up ability for entity (increases ability level).
        /// </summary>
        /// <param name="entityId">Entity whose ability to level up</param>
        /// <param name="abilityId">Ability ID to level up</param>
        /// <returns>True if ability was leveled up</returns>
        public bool LevelUpAbility(int entityId, string abilityId)
        {
            if (!TryGetAbilityComponent(entityId, out var component))
            {
                _logger.LogWarning($"Cannot level up ability - entity {entityId} has no ability component", LogCategory);
                return false;
            }

            if (!component.TryGetAbility(abilityId, out var data))
            {
                _logger.LogWarning($"Cannot level up ability - entity {entityId} has no ability '{abilityId}'", LogCategory);
                return false;
            }

            data.Level++;
            component.UpdateAbility(abilityId, data);
            _entityManager.AddComponent(entityId, component);

            _logger.LogInfo($"Leveled up ability '{abilityId}' to level {data.Level} for entity {entityId}", LogCategory);
            return true;
        }

        // ============================================================
        // QUERY OPERATIONS
        // ============================================================

        /// <summary>
        /// Checks if entity has specific ability.
        /// </summary>
        /// <param name="entityId">Entity to check</param>
        /// <param name="abilityId">Ability ID to look for</param>
        /// <returns>True if entity has this ability</returns>
        public bool HasAbility(int entityId, string abilityId)
        {
            return TryGetAbilityComponent(entityId, out var component) &&
                   component.HasAbility(abilityId);
        }

        /// <summary>
        /// Gets all ability IDs for entity.
        /// </summary>
        /// <param name="entityId">Entity to query</param>
        /// <returns>List of ability IDs</returns>
        public List<string> GetAbilityIds(int entityId)
        {
            if (!TryGetAbilityComponent(entityId, out var component))
                return new List<string>();

            return component.GetAbilityIds().ToList();
        }

        /// <summary>
        /// Gets all entities that have specific ability.
        /// </summary>
        /// <param name="abilityId">Ability ID to search for</param>
        /// <returns>List of entity IDs</returns>
        public IEnumerable<int> GetEntitiesWithAbility(string abilityId)
        {
            return _entityManager.GetEntitiesWith<MythHunterAbilityComponent>()
                .Where(entityId => HasAbility(entityId, abilityId));
        }

        /// <summary>
        /// Gets remaining cooldown time for ability.
        /// </summary>
        /// <param name="entityId">Entity to check</param>
        /// <param name="abilityId">Ability ID</param>
        /// <param name="currentTime">Current game time</param>
        /// <returns>Remaining cooldown in seconds (0 if ready)</returns>
        public float GetRemainingCooldown(int entityId, string abilityId, float currentTime)
        {
            if (!TryGetAbilityComponent(entityId, out var component))
                return 0f;

            return component.GetRemainingCooldown(abilityId, currentTime);
        }

        /// <summary>
        /// Gets remaining charges for ability.
        /// </summary>
        /// <param name="entityId">Entity to check</param>
        /// <param name="abilityId">Ability ID</param>
        /// <returns>Remaining charges (-1 if unlimited, 0+ if charge-based)</returns>
        public int GetRemainingCharges(int entityId, string abilityId)
        {
            if (!TryGetAbilityComponent(entityId, out var component))
                return 0;

            if (!component.TryGetAbility(abilityId, out var data))
                return 0;

            return data.RemainingCharges;
        }

        /// <summary>
        /// Checks if ability is registered in registry.
        /// </summary>
        /// <param name="abilityId">Ability ID to check</param>
        /// <returns>True if ability is registered</returns>
        public bool IsAbilityRegistered(string abilityId)
        {
            return _abilityRegistry.HasAbility(abilityId);
        }

        /// <summary>
        /// Gets all registered ability IDs.
        /// </summary>
        /// <returns>List of all ability IDs in registry</returns>
        public IEnumerable<string> GetAllAbilityIds()
        {
            return _abilityRegistry.GetAllAbilities().Select(a => a.AbilityId);
        }

        // ============================================================
        // PRIVATE HELPERS
        // ============================================================

        private bool TryGetAbilityComponent(int entityId, out MythHunterAbilityComponent component)
        {
            return _entityManager.TryGetComponent(entityId, out component);
        }

        private string GetCannotUseReason(MythHunterAbilityComponent component, string abilityId, float currentTime)
        {
            if (!component.HasAbility(abilityId))
                return "Ability not learned";

            if (!component.TryGetAbility(abilityId, out var data))
                return "Ability data not found";

            if (data.CooldownState.IsOnCooldown(currentTime))
                return $"On cooldown ({data.CooldownState.GetRemainingTime(currentTime):F1}s remaining)";

            if (data.RemainingCharges == 0)
                return "No charges remaining";

            if (component.IsCastingAny())
                return "Already casting another ability";

            return "Unknown reason";
        }

        private void UpdateComponentAfterExecution(int entityId, string abilityId, IAbility ability, float currentTime)
        {
            if (!TryGetAbilityComponent(entityId, out var component))
                return;

            if (!component.TryGetAbility(abilityId, out var data))
                return;

            // Start cooldown
            if (ability.Cooldown > 0f)
            {
                float modifiedCooldown = ability.Cooldown * data.CooldownModifier;
                data.CooldownState = new CooldownState(abilityId, modifiedCooldown, currentTime);
            }

            // Use charge (if charge-based)
            if (data.RemainingCharges > 0)
            {
                data.RemainingCharges--;
            }

            // Update component
            component.UpdateAbility(abilityId, data);
            _entityManager.AddComponent(entityId, component);
        }
    }
}
