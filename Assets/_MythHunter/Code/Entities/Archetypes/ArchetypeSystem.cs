using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Utils.Logging;

namespace MythHunter.Entities
{
    /// <summary>
    /// Комплексна система для керування архетипами сутностей
    /// </summary>
    public class ArchetypeSystem : Core.ECS.SystemBase, IEventSubscriber
    {
        private readonly IEntityManager _entityManager;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        // Реєстр архетипів
        private readonly EntityArchetypeRegistry _archetypeRegistry;

        // Шаблони архетипів
        private readonly Archetypes.ArchetypeTemplateRegistry _templateRegistry;

        // Кеш зв'язків сутностей з архетипами
        private readonly Dictionary<int, string> _entityToArchetype = new Dictionary<int, string>();
        private readonly Dictionary<string, HashSet<int>> _archetypeToEntities = new Dictionary<string, HashSet<int>>();

        // Кеш компонентів
        private readonly ComponentCacheRegistry _componentCacheRegistry;

        [Inject]
        public ArchetypeSystem(
            IEntityManager entityManager,
            IEventBus eventBus,
            IMythLogger logger,
            ComponentCacheRegistry componentCacheRegistry)
        {
            _entityManager = entityManager;
            _eventBus = eventBus;
            _logger = logger;
            _componentCacheRegistry = componentCacheRegistry;

            // Створюємо реєстри
            _archetypeRegistry = new EntityArchetypeRegistry(entityManager);
            _templateRegistry = new Archetypes.ArchetypeTemplateRegistry(entityManager, logger);
        }

        public override void Initialize()
        {
            // Реєструємо базові архетипи
            RegisterBaseArchetypes();

            // Підписуємось на події
            SubscribeToEvents();

            _logger.LogInfo("ArchetypeSystem initialized", "Entity");
        }

        /// <summary>
        /// Реєструє базові архетипи
        /// </summary>
        private void RegisterBaseArchetypes()
        {
            // Реєструємо архетипи в реєстрі
            _archetypeRegistry.RegisterArchetype(new Archetypes.CharacterArchetype());
            _archetypeRegistry.RegisterArchetype(new Archetypes.EnemyArchetype());

            _logger.LogInfo("Base archetypes registered", "Entity");
        }

        public void SubscribeToEvents()
        {
            // Підписуємось на події створення та знищення сутностей
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
            if (!string.IsNullOrEmpty(evt.ArchetypeId))
            {
                RegisterEntityArchetype(evt.EntityId, evt.ArchetypeId);
            }
        }

        private void OnEntityDestroyed(Events.Domain.EntityDestroyedEvent evt)
        {
            UnregisterEntityArchetype(evt.EntityId);
        }

        /// <summary>
        /// Створює сутність за архетипом
        /// </summary>
        public int CreateEntityFromArchetype(string archetypeId)
        {
            // Спочатку перевіряємо шаблони
            if (_templateRegistry.HasTemplate(archetypeId))
            {
                int entityId = _templateRegistry.CreateEntityFromTemplate(archetypeId);
                RegisterEntityArchetype(entityId, archetypeId);

                // Публікуємо подію створення сутності
                _eventBus.Publish(new Events.Domain.EntityCreatedEvent
                {
                    EntityId = entityId,
                    ArchetypeId = archetypeId,
                    Timestamp = DateTime.UtcNow
                });

                return entityId;
            }

            // Якщо шаблону немає, перевіряємо реєстр архетипів
            var archetype = _archetypeRegistry.GetArchetype(archetypeId);
            if (archetype != null)
            {
                int entityId = archetype.CreateEntity(_entityManager);
                RegisterEntityArchetype(entityId, archetypeId);

                // Публікуємо подію створення сутності
                _eventBus.Publish(new Events.Domain.EntityCreatedEvent
                {
                    EntityId = entityId,
                    ArchetypeId = archetypeId,
                    Timestamp = DateTime.UtcNow
                });

                return entityId;
            }

            _logger.LogWarning($"Archetype '{archetypeId}' not found", "Entity");
            return -1;
        }

        /// <summary>
        /// Створює сутність за архетипом з параметрами
        /// </summary>
        public int CreateEntityFromArchetype(string archetypeId, Dictionary<Type, object> parameters)
        {
            // Створюємо через шаблон з параметрами, якщо доступно
            if (_templateRegistry.HasTemplate(archetypeId))
            {
                int entityId = _templateRegistry.CreateEntityFromTemplate(archetypeId, parameters);
                RegisterEntityArchetype(entityId, archetypeId);

                // Публікуємо подію створення сутності
                _eventBus.Publish(new Events.Domain.EntityCreatedEvent
                {
                    EntityId = entityId,
                    ArchetypeId = archetypeId,
                    Timestamp = DateTime.UtcNow
                });

                return entityId;
            }

            // Альтернативний шлях через стандартний реєстр
            var archetype = _archetypeRegistry.GetArchetype(archetypeId);
            if (archetype != null)
            {
                int entityId = archetype.CreateEntity(_entityManager);

                // Додаємо компоненти з параметрів
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        var componentType = parameter.Key;
                        var component = parameter.Value;

                        // Використовуємо рефлексію для виклику AddComponent з правильним типом
                        var method = typeof(IEntityManager).GetMethod("AddComponent")
                            .MakeGenericMethod(componentType);
                        method.Invoke(_entityManager, new object[] { entityId, component });
                    }
                }

                RegisterEntityArchetype(entityId, archetypeId);

                // Публікуємо подію створення сутності
                _eventBus.Publish(new Events.Domain.EntityCreatedEvent
                {
                    EntityId = entityId,
                    ArchetypeId = archetypeId,
                    Timestamp = DateTime.UtcNow
                });

                return entityId;
            }

            _logger.LogWarning($"Archetype '{archetypeId}' not found", "Entity");
            return -1;
        }

        /// <summary>
        /// Знищує сутність
        /// </summary>
        public void DestroyEntity(int entityId)
        {
            if (!_entityManager.HasComponent<Components.Core.IdComponent>(entityId))
            {
                _logger.LogWarning($"Entity {entityId} does not exist", "Entity");
                return;
            }

            // Публікуємо подію знищення сутності
            _eventBus.Publish(new Events.Domain.EntityDestroyedEvent
            {
                EntityId = entityId,
                Timestamp = DateTime.UtcNow
            });

            // Видаляємо з кешу архетипів
            UnregisterEntityArchetype(entityId);

            // Знищуємо сутність
            _entityManager.DestroyEntity(entityId);
        }

        /// <summary>
        /// Реєструє архетип сутності
        /// </summary>
        private void RegisterEntityArchetype(int entityId, string archetypeId)
        {
            // Видаляємо з попереднього архетипу, якщо був
            if (_entityToArchetype.TryGetValue(entityId, out var oldArchetypeId))
            {
                if (_archetypeToEntities.TryGetValue(oldArchetypeId, out var entities))
                {
                    entities.Remove(entityId);
                }
            }

            // Записуємо у новий архетип
            _entityToArchetype[entityId] = archetypeId;

            if (!_archetypeToEntities.TryGetValue(archetypeId, out var archetypeEntities))
            {
                archetypeEntities = new HashSet<int>();
                _archetypeToEntities[archetypeId] = archetypeEntities;
            }

            archetypeEntities.Add(entityId);
        }

        /// <summary>
        /// Видаляє реєстрацію архетипу для сутності
        /// </summary>
        private void UnregisterEntityArchetype(int entityId)
        {
            if (_entityToArchetype.TryGetValue(entityId, out var archetypeId))
            {
                if (_archetypeToEntities.TryGetValue(archetypeId, out var entities))
                {
                    entities.Remove(entityId);
                }

                _entityToArchetype.Remove(entityId);
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
        /// Отримує всі сутності вказаного архетипу
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
        /// Перевіряє, чи відповідає сутність вказаному архетипу
        /// </summary>
        public bool IsEntityOfArchetype(int entityId, string archetypeId)
        {
            return _entityToArchetype.TryGetValue(entityId, out var currentArchetypeId) &&
                   currentArchetypeId == archetypeId;
        }

        /// <summary>
        /// Оновлює кеші компонентів
        /// </summary>
        public override void Update(float deltaTime)
        {
            // Оновлюємо кеші компонентів один раз на кадр
            _componentCacheRegistry.UpdateAllCaches();
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _entityToArchetype.Clear();
            _archetypeToEntities.Clear();
        }
    }
}
