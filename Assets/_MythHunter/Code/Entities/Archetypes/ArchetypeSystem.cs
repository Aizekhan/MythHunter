// Шлях: Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeSystem.cs

using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Utils.Logging;

namespace MythHunter.Entities.Archetypes
{
    /// <summary>
    /// Система для керування архетипами сутностей
    /// </summary>
    public class ArchetypeSystem : SystemBase, IEventSubscriber
    {
        private readonly IEntityManager _entityManager;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly Dictionary<int, string> _entityToArchetype = new Dictionary<int, string>();
        private readonly Dictionary<string, List<int>> _archetypeToEntities = new Dictionary<string, List<int>>();

        [Inject]
        public ArchetypeSystem(
            IEntityManager entityManager,
            IEventBus eventBus,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _eventBus = eventBus;
            _logger = logger;
        }

        public override void Initialize()
        {
            SubscribeToEvents();
            _logger.LogInfo("ArchetypeSystem initialized", "Entity");
        }

        public void SubscribeToEvents()
        {
            // Підписуємося на події створення та видалення сутностей
            // Це дозволить нам відслідковувати архетипи сутностей

            // Припустимо, існують такі події:
            _eventBus.Subscribe<Events.Domain.EntityCreatedEvent>(OnEntityCreated);
            _eventBus.Subscribe<Events.Domain.EntityDestroyedEvent>(OnEntityDestroyed);
        }

        public void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<Events.Domain.EntityCreatedEvent>(OnEntityCreated);
            _eventBus.Unsubscribe<Events.Domain.EntityDestroyedEvent>(OnEntityDestroyed);
        }

        private void OnEntityCreated(Events.Domain.EntityCreatedEvent evt)
        {
            // При створенні сутності перевіряємо, до якого архетипу вона належить
            string archetypeId = evt.ArchetypeId;

            if (!string.IsNullOrEmpty(archetypeId))
            {
                RegisterEntityArchetype(evt.EntityId, archetypeId);
            }
        }

        private void OnEntityDestroyed(Events.Domain.EntityDestroyedEvent evt)
        {
            // При видаленні сутності видаляємо її з кешу архетипів
            UnregisterEntityArchetype(evt.EntityId);
        }

        /// <summary>
        /// Реєструє архетип сутності у кеші
        /// </summary>
        public void RegisterEntityArchetype(int entityId, string archetypeId)
        {
            if (string.IsNullOrEmpty(archetypeId))
            {
                _logger.LogWarning($"Cannot register entity {entityId} with empty archetype ID", "Entity");
                return;
            }

            // Видаляємо сутність з попереднього архетипу, якщо вона була зареєстрована
            if (_entityToArchetype.TryGetValue(entityId, out var oldArchetypeId))
            {
                if (_archetypeToEntities.TryGetValue(oldArchetypeId, out var entities))
                {
                    entities.Remove(entityId);
                }
            }

            // Реєструємо сутність у новому архетипі
            _entityToArchetype[entityId] = archetypeId;

            if (!_archetypeToEntities.TryGetValue(archetypeId, out var archetypeEntities))
            {
                archetypeEntities = new List<int>();
                _archetypeToEntities[archetypeId] = archetypeEntities;
            }

            archetypeEntities.Add(entityId);
            _logger.LogDebug($"Registered entity {entityId} with archetype '{archetypeId}'", "Entity");
        }

        /// <summary>
        /// Видаляє реєстрацію архетипу сутності
        /// </summary>
        public void UnregisterEntityArchetype(int entityId)
        {
            if (_entityToArchetype.TryGetValue(entityId, out var archetypeId))
            {
                if (_archetypeToEntities.TryGetValue(archetypeId, out var entities))
                {
                    entities.Remove(entityId);
                }

                _entityToArchetype.Remove(entityId);
                _logger.LogDebug($"Unregistered entity {entityId} from archetype '{archetypeId}'", "Entity");
            }
        }

        /// <summary>
        /// Отримує архетип сутності
        /// </summary>
        public string GetEntityArchetype(int entityId)
        {
            return _entityToArchetype.TryGetValue(entityId, out var archetypeId)
                ? archetypeId
                : null;
        }

        /// <summary>
        /// Отримує всі сутності заданого архетипу
        /// </summary>
        public List<int> GetEntitiesByArchetype(string archetypeId)
        {
            if (_archetypeToEntities.TryGetValue(archetypeId, out var entities))
            {
                return new List<int>(entities);
            }

            return new List<int>();
        }

        /// <summary>
        /// Перевіряє, чи відповідає сутність заданому архетипу
        /// </summary>
        public bool IsEntityOfArchetype(int entityId, string archetypeId)
        {
            return _entityToArchetype.TryGetValue(entityId, out var currentArchetypeId) &&
                   currentArchetypeId == archetypeId;
        }

        public override void Update(float deltaTime)
        {
            // Тут можна додати логіку для періодичного оновлення кешу архетипів
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _entityToArchetype.Clear();
            _archetypeToEntities.Clear();
            _logger.LogInfo("ArchetypeSystem disposed", "Entity");
        }
    }
}
