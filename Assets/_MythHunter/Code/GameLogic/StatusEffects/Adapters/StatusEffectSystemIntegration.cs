// Assets/_MythHunter/Code/GameLogic/StatusEffects/Adapters/StatusEffectSystemIntegration.cs

using System;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.ECS;
using RPGFramework.StatusEffects;
using RPGFramework.DI;
using MythHunter.Utils.Logging;
using MythHunter.GameLogic.StatusEffects.Components;

namespace MythHunter.GameLogic.StatusEffects.Adapters
{
    /// <summary>
    /// Integration helper for status effect operations.
    /// Bridges Framework StatusEffect System (RPGFramework.StatusEffects.Runtime) with MythHunter game layer.
    ///
    /// Pattern (from EPIC 15 Phase 2):
    /// - Greenfield integration (no legacy status effect code)
    /// - Uses MythHunterStatusEffectsComponent for game-specific metadata
    /// - Manages effects via Framework IStatusEffectManager
    /// - Synchronizes Framework ActiveEffectsData with game-layer metadata
    ///
    /// Key Responsibilities:
    /// - Apply/remove effects with immunity checks
    /// - Synchronize metadata when effects change
    /// - Query entities with specific effects
    /// - Bridge Framework events with MythHunter systems (VFX, Audio, UI)
    /// </summary>
    public class StatusEffectSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly IStatusEffectManager _statusEffectManager;
        private readonly IMythLogger _logger;

        private const string LogCategory = "StatusEffectIntegration";

        [Inject]
        public StatusEffectSystemIntegration(
            IEntityManager entityManager,
            IStatusEffectManager statusEffectManager,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _statusEffectManager = statusEffectManager;
            _logger = logger;
        }

        // ============================================================
        // EFFECT APPLICATION OPERATIONS
        // ============================================================

        /// <summary>
        /// Applies status effect to entity with MythHunter-specific metadata.
        /// Checks immunity and max effects limit before applying.
        /// Synchronizes metadata after application.
        /// </summary>
        /// <param name="entityId">Target entity</param>
        /// <param name="effectId">Effect ID to apply</param>
        /// <param name="sourceId">Source entity/ability that applied effect</param>
        /// <param name="currentTime">Current game time</param>
        /// <param name="metadata">Game-specific metadata (visuals, sounds, etc.)</param>
        /// <returns>True if applied successfully</returns>
        public bool ApplyEffect(
            int entityId,
            string effectId,
            string sourceId,
            float currentTime,
            EffectMetadata metadata)
        {
            // Ensure MythHunter component exists
            EnsureComponentExists(entityId);

            // Get MythHunter component
            if (!_entityManager.TryGetComponent<MythHunterStatusEffectsComponent>(entityId, out var mythComponent))
            {
                _logger.LogWarning($"Failed to get MythHunter component for entity {entityId}", LogCategory);
                return false;
            }

            // Check immunity
            if (mythComponent.IsImmuneTo(effectId))
            {
                _logger.LogInfo($"Entity {entityId} is immune to effect '{effectId}'", LogCategory);
                return false;
            }

            // Check max effects limit
            var currentEffects = _statusEffectManager.GetActiveEffects(entityId);
            if (!mythComponent.CanAcceptMoreEffects(currentEffects.Count))
            {
                _logger.LogWarning($"Entity {entityId} has reached max active effects ({mythComponent.MaxActiveEffects})", LogCategory);
                return false;
            }

            // Apply effect via Framework
            bool applied = _statusEffectManager.ApplyEffect(entityId, effectId, sourceId, currentTime);

            if (applied)
            {
                // Add metadata
                mythComponent.AddEffectMetadata(metadata);
                _entityManager.AddComponent(entityId, mythComponent);

                _logger.LogInfo($"Applied effect '{effectId}' to entity {entityId} from source '{sourceId}'", LogCategory);
            }

            return applied;
        }

        /// <summary>
        /// Removes specific effect from entity.
        /// Cleans up metadata after removal.
        /// </summary>
        public bool RemoveEffect(int entityId, string effectId)
        {
            bool removed = _statusEffectManager.RemoveEffect(entityId, effectId);

            if (removed && _entityManager.TryGetComponent<MythHunterStatusEffectsComponent>(entityId, out var mythComponent))
            {
                mythComponent.RemoveEffectMetadata(effectId);
                _entityManager.AddComponent(entityId, mythComponent);

                _logger.LogInfo($"Removed effect '{effectId}' from entity {entityId}", LogCategory);
            }

            return removed;
        }

        /// <summary>
        /// Removes all effects from entity.
        /// Clears all metadata.
        /// </summary>
        public void RemoveAllEffects(int entityId)
        {
            _statusEffectManager.RemoveAllEffects(entityId);

            if (_entityManager.TryGetComponent<MythHunterStatusEffectsComponent>(entityId, out var mythComponent))
            {
                mythComponent.ClearAll();
                _entityManager.AddComponent(entityId, mythComponent);

                _logger.LogInfo($"Removed all effects from entity {entityId}", LogCategory);
            }
        }

        // ============================================================
        // EFFECT UPDATE OPERATIONS
        // ============================================================

        /// <summary>
        /// Updates all effects for entity (reduces durations, processes ticks).
        /// Should be called every frame/tick for entities with effects.
        /// Automatically removes expired effects and their metadata.
        /// </summary>
        public void UpdateEffects(int entityId, float deltaTime)
        {
            // Get effects before update
            var effectsBefore = _statusEffectManager.GetActiveEffects(entityId).Select(e => e.EffectId).ToHashSet();

            // Update via Framework
            _statusEffectManager.UpdateEffects(entityId, deltaTime);

            // Get effects after update
            var effectsAfter = _statusEffectManager.GetActiveEffects(entityId).Select(e => e.EffectId).ToHashSet();

            // Find expired effects (were active before, not active now)
            var expiredEffects = effectsBefore.Except(effectsAfter).ToList();

            // Clean up metadata for expired effects
            if (expiredEffects.Count > 0 && _entityManager.TryGetComponent<MythHunterStatusEffectsComponent>(entityId, out var mythComponent))
            {
                foreach (var effectId in expiredEffects)
                {
                    mythComponent.RemoveEffectMetadata(effectId);
                    _logger.LogInfo($"Effect '{effectId}' expired on entity {entityId}", LogCategory);
                }
                _entityManager.AddComponent(entityId, mythComponent);
            }
        }

        // ============================================================
        // IMMUNITY MANAGEMENT
        // ============================================================

        /// <summary>
        /// Adds immunity to specific effect type.
        /// Optionally removes existing effect if already applied.
        /// </summary>
        public void AddImmunity(int entityId, string effectId, bool removeIfActive = true)
        {
            EnsureComponentExists(entityId);

            if (_entityManager.TryGetComponent<MythHunterStatusEffectsComponent>(entityId, out var mythComponent))
            {
                mythComponent.AddImmunity(effectId);
                _entityManager.AddComponent(entityId, mythComponent);

                if (removeIfActive && _statusEffectManager.HasEffect(entityId, effectId))
                {
                    RemoveEffect(entityId, effectId);
                }

                _logger.LogInfo($"Added immunity to '{effectId}' for entity {entityId}", LogCategory);
            }
        }

        /// <summary>
        /// Removes immunity to specific effect type.
        /// </summary>
        public bool RemoveImmunity(int entityId, string effectId)
        {
            if (_entityManager.TryGetComponent<MythHunterStatusEffectsComponent>(entityId, out var mythComponent))
            {
                bool removed = mythComponent.RemoveImmunity(effectId);
                if (removed)
                {
                    _entityManager.AddComponent(entityId, mythComponent);
                    _logger.LogInfo($"Removed immunity to '{effectId}' for entity {entityId}", LogCategory);
                }
                return removed;
            }
            return false;
        }

        // ============================================================
        // QUERY OPERATIONS
        // ============================================================

        /// <summary>
        /// Checks if entity has specific effect active.
        /// </summary>
        public bool HasEffect(int entityId, string effectId)
        {
            return _statusEffectManager.HasEffect(entityId, effectId);
        }

        /// <summary>
        /// Gets all active effects for entity (Framework data).
        /// </summary>
        public IReadOnlyList<IStatusEffectInstance> GetActiveEffects(int entityId)
        {
            return _statusEffectManager.GetActiveEffects(entityId);
        }

        /// <summary>
        /// Gets metadata for specific effect on entity.
        /// </summary>
        public bool TryGetMetadata(int entityId, string effectId, out EffectMetadata metadata)
        {
            metadata = default;
            return _entityManager.TryGetComponent<MythHunterStatusEffectsComponent>(entityId, out var mythComponent)
                && mythComponent.TryGetMetadata(effectId, out metadata);
        }

        /// <summary>
        /// Gets all effect metadata ordered by display priority.
        /// Useful for UI rendering.
        /// </summary>
        public IEnumerable<EffectMetadata> GetOrderedMetadata(int entityId)
        {
            if (_entityManager.TryGetComponent<MythHunterStatusEffectsComponent>(entityId, out var mythComponent))
            {
                return mythComponent.GetOrderedMetadata();
            }
            return Enumerable.Empty<EffectMetadata>();
        }

        /// <summary>
        /// Gets effects that should show visual effects.
        /// </summary>
        public IEnumerable<EffectMetadata> GetVisualEffects(int entityId)
        {
            if (_entityManager.TryGetComponent<MythHunterStatusEffectsComponent>(entityId, out var mythComponent))
            {
                return mythComponent.GetVisualEffects();
            }
            return Enumerable.Empty<EffectMetadata>();
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>
        /// Ensures both Framework and MythHunter components exist on entity.
        /// Creates them if missing.
        /// </summary>
        private void EnsureComponentExists(int entityId)
        {
            // Ensure Framework component
            if (!_entityManager.TryGetComponent<ActiveEffectsData>(entityId, out var _))
            {
                _entityManager.AddComponent(entityId, ActiveEffectsData.CreateEmpty());
            }

            // Ensure MythHunter component
            if (!_entityManager.TryGetComponent<MythHunterStatusEffectsComponent>(entityId, out var _))
            {
                _entityManager.AddComponent(entityId, MythHunterStatusEffectsComponent.CreateEmpty());
            }
        }
    }
}
