// Assets/_MythHunter/Code/Systems/Gameplay/Loot/LootDropSystem.cs

using System;
using RPGFramework.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using MythHunter.Systems.Phase;
using MythHunter.GameLogic.Loot.Adapters;
using MythHunter.GameLogic.Loot.Components;
using MythHunter.Components.Movement;
using RPGFramework.ECS;
using UnityEngine;

namespace MythHunter.Systems.Loot
{
    /// <summary>
    /// System responsible for handling loot drops from entities.
    /// Subscribes to EntityDeathEvent and generates loot when entities with loot components die.
    ///
    /// Pattern:
    /// - Separated concern from CombatSystem (follows EPIC 12 architecture)
    /// - Uses LootSystemIntegration helper for loot generation
    /// - Requires PositionComponent to determine drop location
    /// - Integrates with Phase 1 Item system for item entity creation
    /// </summary>
    public class LootDropSystem : SystemBase, IEventSubscriber
    {
        private readonly IEntityManager _entityManager;
        private readonly LootSystemIntegration _lootIntegration;
        private readonly IPhaseSystem _phaseSystem;

        private const string LogCategory = "LootDropSystem";

        // Loot drop offset (slightly randomized to prevent stacking)
        private const float DROP_OFFSET_RADIUS = 0.5f;

        [Inject]
        public LootDropSystem(
            IEntityManager entityManager,
            LootSystemIntegration lootIntegration,
            IEventBus eventBus,
            IMythLogger logger,
            IPhaseSystem phaseSystem)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;
            _lootIntegration = lootIntegration;
            _phaseSystem = phaseSystem;
        }

        public override void Initialize()
        {
            base.Initialize();
            _logger.LogInfo("LootDropSystem initialized", LogCategory);
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<EntityDeathEvent>(OnEntityDeath);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<EntityDeathEvent>(OnEntityDeath);
        }

        /// <summary>
        /// Handles entity death event and generates loot if entity has loot component.
        /// </summary>
        private void OnEntityDeath(EntityDeathEvent evt)
        {
            int entityId = evt.EntityId;

            // Check if entity has loot table configured
            if (!_lootIntegration.HasLootTable(entityId))
            {
                _logger.LogDebug($"Entity {entityId} has no loot table, skipping loot generation", LogCategory);
                return;
            }

            // Check if loot already generated (for one-time sources like chests)
            if (_lootIntegration.IsLootGenerated(entityId))
            {
                _logger.LogDebug($"Loot already generated for entity {entityId}, skipping", LogCategory);
                return;
            }

            // Get entity position for loot drop
            if (!_entityManager.TryGetComponent<PositionComponent>(entityId, out var positionComponent))
            {
                _logger.LogWarning($"Entity {entityId} has loot table but no PositionComponent, cannot drop loot", LogCategory);
                return;
            }

            Vector3 dropPosition = positionComponent.Position;

            // Add random offset to prevent item stacking
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * DROP_OFFSET_RADIUS;
            dropPosition += new Vector3(randomOffset.x, 0f, randomOffset.y);

            // Generate loot and create item entities
            try
            {
                var itemEntityIds = _lootIntegration.GenerateLootAndCreateItems(entityId, dropPosition);

                if (itemEntityIds.Count > 0)
                {
                    string lootTableId = _lootIntegration.GetLootTableId(entityId);
                    int entityLevel = _lootIntegration.GetEntityLevel(entityId);

                    _logger.LogInfo(
                        $"Dropped {itemEntityIds.Count} items from entity {entityId} " +
                        $"(table='{lootTableId}', level={entityLevel}) at position {dropPosition}",
                        LogCategory
                    );

                    // Publish loot dropped event for UI/audio feedback
                    Publish(new LootDroppedEvent
                    {
                        SourceEntityId = entityId,
                        KillerEntityId = evt.KillerEntityId,
                        ItemEntityIds = itemEntityIds.ToArray(),
                        DropPosition = dropPosition,
                        LootTableId = lootTableId,
                        Timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogDebug($"No loot dropped from entity {entityId} (empty loot roll)", LogCategory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to generate loot for entity {entityId}: {ex.Message}", LogCategory);
            }
        }

        public override void Update(float deltaTime)
        {
            // No per-frame updates needed - event-driven system
        }

        public override void Dispose()
        {
            _logger.LogInfo("LootDropSystem disposed", LogCategory);
            base.Dispose();
        }
    }

    /// <summary>
    /// Event published when loot is dropped from an entity.
    /// Used for UI notifications, audio feedback, visual effects, etc.
    /// </summary>
    public struct LootDroppedEvent : IEvent
    {
        /// <summary>
        /// Entity that dropped the loot (enemy, chest, boss).
        /// </summary>
        public int SourceEntityId;

        /// <summary>
        /// Entity that killed the source (for kill credit, luck modifiers).
        /// </summary>
        public int KillerEntityId;

        /// <summary>
        /// Item entity IDs that were created for the loot drops.
        /// </summary>
        public int[] ItemEntityIds;

        /// <summary>
        /// World position where loot was dropped.
        /// </summary>
        public Vector3 DropPosition;

        /// <summary>
        /// Loot table ID that was used for generation.
        /// </summary>
        public string LootTableId;

        /// <summary>
        /// Timestamp when loot was dropped.
        /// </summary>
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
