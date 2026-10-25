// Шлях: Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeSystem.cs
using System;
using System.Collections.Generic;
using RPGFramework.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Archetype; // EPIC 08 Phase 2
using MythHunter.Utils.Logging;

namespace MythHunter.Entities.Archetypes
{
    /// <summary>
    /// Система для керування архетипами сутностей
    /// </summary>
    public class ArchetypeSystem : Core.ECS.SystemBase, IEventSubscriber, IArchetypeSystem
    {
        private readonly IArchetypeRegistry _archetypeRegistry;
        private readonly IArchetypeDetector _archetypeDetector;
        private readonly IArchetypeTemplateRegistry _templateRegistry;

        [Inject]
        public ArchetypeSystem(
            IArchetypeRegistry archetypeRegistry,
            IArchetypeDetector archetypeDetector,
            IArchetypeTemplateRegistry templateRegistry,
            IMythLogger logger,
            IEventBus eventBus)
            : base(logger, eventBus)
        {
            _archetypeRegistry = archetypeRegistry;
            _archetypeDetector = archetypeDetector;
            _templateRegistry = templateRegistry;
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<EntityCreatedEvent>(OnEntityCreated);
            Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);

            // EPIC 08 Phase 2: Event-driven entity creation
            Subscribe<CreateEntityFromArchetypeRequestEvent>(OnCreateEntityFromArchetypeRequest);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<EntityCreatedEvent>(OnEntityCreated);
            Unsubscribe<EntityDestroyedEvent>(OnEntityDestroyed);

            // EPIC 08 Phase 2: Event-driven entity creation
            Unsubscribe<CreateEntityFromArchetypeRequestEvent>(OnCreateEntityFromArchetypeRequest);
        }

        private void OnEntityCreated(EntityCreatedEvent evt)
        {
            // При створенні сутності перевіряємо, до якого архетипу вона належить
            string archetypeId = evt.ArchetypeId;

            if (!string.IsNullOrEmpty(archetypeId))
            {
                _archetypeRegistry.RegisterEntityArchetype(evt.EntityId, archetypeId);
            }
            else
            {
                // Якщо архетип не вказано, пробуємо визначити його
                _archetypeDetector.DetectAndRegisterArchetype(evt.EntityId);
            }
        }

        private void OnEntityDestroyed(EntityDestroyedEvent evt)
        {
            // При видаленні сутності видаляємо її з кешу архетипів
            _archetypeRegistry.UnregisterEntityArchetype(evt.EntityId);
        }

        /// <summary>
        /// EPIC 08 Phase 2: Event-driven entity creation handler
        /// Listens to CreateEntityFromArchetypeRequestEvent and creates entity
        /// </summary>
        private void OnCreateEntityFromArchetypeRequest(CreateEntityFromArchetypeRequestEvent request)
        {
            try
            {
                _logger.LogInfo($"Creating entity from archetype: {request.ArchetypeId} (Request: {request.RequestId})", "ArchetypeSystem");

                // Create entity using existing method
                var overrides = request.AdditionalData as Dictionary<Type, object>;
                int entityId = CreateEntityFromArchetype(request.ArchetypeId, overrides);

                // Publish success response
                _eventBus.Publish(new EntityFromArchetypeCreatedEvent
                {
                    ArchetypeId = request.ArchetypeId,
                    EntityId = entityId,
                    RequestId = request.RequestId,
                    PlayerIndex = request.PlayerIndex,
                    Success = entityId >= 0,
                    ErrorMessage = entityId < 0 ? $"Failed to create entity from archetype '{request.ArchetypeId}'" : null,
                    Timestamp = DateTime.UtcNow
                });

                if (entityId >= 0)
                {
                    _logger.LogInfo($"Entity {entityId} created from archetype {request.ArchetypeId} (Request: {request.RequestId})", "ArchetypeSystem");
                }
                else
                {
                    _logger.LogError($"Failed to create entity from archetype {request.ArchetypeId} (Request: {request.RequestId})", "ArchetypeSystem");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception creating entity from archetype {request.ArchetypeId}: {ex.Message}", "ArchetypeSystem", ex);

                // Publish failure response
                _eventBus.Publish(new EntityFromArchetypeCreatedEvent
                {
                    ArchetypeId = request.ArchetypeId,
                    EntityId = -1,
                    RequestId = request.RequestId,
                    PlayerIndex = request.PlayerIndex,
                    Success = false,
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Створює сутність на основі шаблону архетипу
        /// </summary>
        public int CreateEntityFromArchetype(string archetypeId, Dictionary<Type, object> overrides = null)
        {
            int entityId = _templateRegistry.CreateEntityFromTemplate(archetypeId, overrides);

            if (entityId >= 0)
            {
                _archetypeRegistry.RegisterEntityArchetype(entityId, archetypeId);

                // Публікуємо подію створення сутності
                _eventBus.Publish(new EntityCreatedEvent
                {
                    EntityId = entityId,
                    ArchetypeId = archetypeId,
                    Timestamp = DateTime.Now
                });
            }

            return entityId;
        }

        /// <summary>
        /// Отримує архетип сутності
        /// </summary>
        public string GetEntityArchetype(int entityId)
        {
            return _archetypeRegistry.GetEntityArchetype(entityId);
        }

        /// <summary>
        /// Отримує всі сутності заданого архетипу
        /// </summary>
        public List<int> GetEntitiesByArchetype(string archetypeId)
        {
            return _archetypeRegistry.GetEntitiesByArchetype(archetypeId);
        }

        /// <summary>
        /// Перевіряє, чи відповідає сутність заданому архетипу
        /// </summary>
        public bool IsEntityOfArchetype(int entityId, string archetypeId)
        {
            return _archetypeRegistry.IsEntityOfArchetype(entityId, archetypeId);
        }

        /// <summary>
        /// Реєструє архетип сутності
        /// </summary>
        public void RegisterEntityArchetype(int entityId, string archetypeId)
        {
            _archetypeRegistry.RegisterEntityArchetype(entityId, archetypeId);
        }

        public override void Update(float deltaTime)
        {
            // Тут може бути логіка періодичного оновлення
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _archetypeRegistry.Clear();
            _logger.LogInfo("ArchetypeSystem disposed", "Entity");
        }
    }
}
