// Файл: Assets/_MythHunter/Code/Core/ECS/ComponentCacheRegistry.cs

using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Entities;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Оптимізований глобальний реєстр кешів компонентів з підтримкою фаз
    /// </summary>
    public class ComponentCacheRegistry : IComponentCacheRegistry, IEventSubscriber
    {
        private readonly Dictionary<Type, object> _caches = new Dictionary<Type, object>();
        private readonly IEntityManager _entityManager;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        // Налаштування автооновлення
        private bool _autoUpdate = true;
        private bool _isSubscribed = false;

        // Налаштування оновлення за фазами
        private readonly Dictionary<GamePhase, HashSet<Type>> _phaseComponentMapping = new Dictionary<GamePhase, HashSet<Type>>();
        private GamePhase _currentPhase = GamePhase.None;

        // Лічильник кадрів для періодичного оновлення
        private int _frameCounter = 0;
        private int _updateInterval = 5; // Оновлення кожні 5 кадрів

        // Статистика
        private int _totalCacheCount = 0;
        private int _totalUpdateCount = 0;
        private int _totalHitCount = 0;
        private int _totalMissCount = 0;

        // Автоматично створюваний кеш для компонентів
        private readonly HashSet<Type> _autoCreateTypes = new HashSet<Type>();

        [Inject]
        public ComponentCacheRegistry(IEntityManager entityManager, IEventBus eventBus, IMythLogger logger)
        {
            _entityManager = entityManager;
            _eventBus = eventBus;
            _logger = logger;

            // Ініціалізація мапінгу фаз
            foreach (GamePhase phase in Enum.GetValues(typeof(GamePhase)))
            {
                _phaseComponentMapping[phase] = new HashSet<Type>();
            }

            // Підписуємося на події
            SubscribeToEvents();

            // Реєструємо компоненти для автоматичного кешування
            RegisterAutoCreateComponents();
        }

        /// <summary>
        /// Реєструє компоненти для автоматичного кешування
        /// </summary>
        private void RegisterAutoCreateComponents()
        {
            // Додаємо базові компоненти, які найчастіше використовуються
            RegisterTypeForAutoCreate<Components.Core.NameComponent>();
            RegisterTypeForAutoCreate<Components.Core.DescriptionComponent>();
            RegisterTypeForAutoCreate<Components.Core.ValueComponent>();

            // Додаємо компоненти для всіх фаз
            // ...

            _logger.LogInfo($"Registered {_autoCreateTypes.Count} component types for auto-creation", "ECS");
        }

        /// <summary>
        /// Реєструє тип компонента для автоматичного створення кешу
        /// </summary>
        public void RegisterTypeForAutoCreate<T>() where T : struct, IComponent
        {
            _autoCreateTypes.Add(typeof(T));

            // Якщо кеш ще не створено, створюємо його
            if (!_caches.ContainsKey(typeof(T)))
            {
                var cache = new ComponentCache<T>(_entityManager);
                _caches[typeof(T)] = cache;
                _totalCacheCount++;
            }
        }

        /// <summary>
        /// Реєструє тип компонента для конкретної фази
        /// </summary>
        public void RegisterComponentForPhase<T>(GamePhase phase) where T : struct, IComponent
        {
            Type componentType = typeof(T);

            if (!_phaseComponentMapping.ContainsKey(phase))
            {
                _phaseComponentMapping[phase] = new HashSet<Type>();
            }

            _phaseComponentMapping[phase].Add(componentType);
            _logger.LogDebug($"Registered component {componentType.Name} for phase {phase}", "ECS");
        }

        /// <summary>
        /// Отримує або створює кеш для вказаного типу компонента
        /// </summary>
        public ComponentCache<T> GetCache<T>() where T : struct, IComponent
        {
            Type componentType = typeof(T);

            if (_caches.TryGetValue(componentType, out var cache))
            {
                return (ComponentCache<T>)cache;
            }

            var newCache = new ComponentCache<T>(_entityManager);
            _caches[componentType] = newCache;
            _totalCacheCount++;

            if (_autoUpdate)
            {
                newCache.Update();
                _totalUpdateCount++;
            }

            _logger.LogDebug($"Created component cache for type {componentType.Name}", "ECS");

            return newCache;
        }

        /// <summary>
        /// Оновлює кеш для вказаного типу компонента
        /// </summary>
        public void UpdateCache<T>() where T : struct, IComponent
        {
            var cache = GetCache<T>();
            cache.Update();
            _totalUpdateCount++;
        }

        /// <summary>
        /// Встановлює режим автооновлення кешів
        /// </summary>
        public void SetAutoUpdate(bool value)
        {
            _autoUpdate = value;
            _logger.LogInfo($"Auto-update mode set to {value}", "ECS");
        }

        /// <summary>
        /// Встановлює інтервал оновлення кешів (у кадрах)
        /// </summary>
        public void SetUpdateInterval(int frames)
        {
            if (frames <= 0)
            {
                _logger.LogWarning("Update interval must be positive", "ECS");
                return;
            }

            _updateInterval = frames;
            _logger.LogInfo($"Update interval set to {frames} frames", "ECS");
        }

        /// <summary>
        /// Оновлює всі кеші
        /// </summary>
        public void UpdateAllCaches()
        {
            foreach (var cache in _caches.Values)
            {
                var updateMethod = cache.GetType().GetMethod("Update");
                updateMethod?.Invoke(cache, null);
                _totalUpdateCount++;
            }

            _logger.LogTrace($"Updated {_caches.Count} component caches", "ECS");
        }

        /// <summary>
        /// Оновлює кеші компонентів, які активні в поточній фазі
        /// </summary>
        public void UpdateCachesForCurrentPhase()
        {
            if (_currentPhase == GamePhase.None)
                return;

            if (!_phaseComponentMapping.TryGetValue(_currentPhase, out var componentTypes))
                return;

            int updatedCount = 0;

            foreach (var type in componentTypes)
            {
                if (_caches.TryGetValue(type, out var cache))
                {
                    var updateMethod = cache.GetType().GetMethod("Update");
                    updateMethod?.Invoke(cache, null);
                    _totalUpdateCount++;
                    updatedCount++;
                }
            }

            _logger.LogTrace($"Updated {updatedCount} component caches for phase {_currentPhase}", "ECS");
        }

        /// <summary>
        /// Оновлює кеші при зміні кадру (викликається з SystemRegistry)
        /// </summary>
        public void Update()
        {
            if (!_autoUpdate)
                return;

            _frameCounter++;

            if (_frameCounter >= _updateInterval)
            {
                _frameCounter = 0;

                // Оновлюємо кеші для поточної фази
                UpdateCachesForCurrentPhase();

                // Оновлюємо кеші для автоматичного створення
                foreach (var type in _autoCreateTypes)
                {
                    if (_caches.TryGetValue(type, out var cache))
                    {
                        var updateMethod = cache.GetType().GetMethod("Update");
                        updateMethod?.Invoke(cache, null);
                        _totalUpdateCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Очищає всі кеші
        /// </summary>
        public void ClearAllCaches()
        {
            _caches.Clear();
            _totalCacheCount = 0;
            _logger.LogInfo("Cleared all component caches", "ECS");
        }

        /// <summary>
        /// Отримує статистику кешування
        /// </summary>
        public Dictionary<string, CacheStatistics> GetCacheStatistics()
        {
            var result = new Dictionary<string, CacheStatistics>();

            foreach (var pair in _caches)
            {
                var cacheType = pair.Key;
                var cache = pair.Value;

                // Викликаємо метод GetStatistics через рефлексію
                var statsMethod = cache.GetType().GetMethod("GetStatistics");
                if (statsMethod != null)
                {
                    var stats = (CacheStatistics)statsMethod.Invoke(cache, null);
                    result[cacheType.Name] = stats;

                    // Оновлення загальної статистики
                    _totalHitCount += stats.HitCount;
                    _totalMissCount += stats.MissCount;
                }
            }

            // Додаємо загальну статистику
            result["Total"] = new CacheStatistics
            {
                ComponentType = "All Components",
                CachedCount = _totalCacheCount,
                UpdateCount = _totalUpdateCount,
                HitCount = _totalHitCount,
                MissCount = _totalMissCount,
                HitRatio = _totalHitCount + _totalMissCount > 0 ? (float)_totalHitCount / (_totalHitCount + _totalMissCount) : 0
            };

            return result;
        }

        #region Event Handling

        /// <summary>
        /// Підписується на події
        /// </summary>
        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
            _eventBus.Subscribe<EntityCreatedEvent>(OnEntityCreated);
            _eventBus.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);

            _isSubscribed = true;
            _logger.LogDebug("ComponentCacheRegistry subscribed to events", "ECS");
        }

        /// <summary>
        /// Відписується від подій
        /// </summary>
        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<PhaseChangedEvent>(OnPhaseChanged);
            _eventBus.Unsubscribe<EntityCreatedEvent>(OnEntityCreated);
            _eventBus.Unsubscribe<EntityDestroyedEvent>(OnEntityDestroyed);

            _isSubscribed = false;
            _logger.LogDebug("ComponentCacheRegistry unsubscribed from events", "ECS");
        }

        /// <summary>
        /// Обробляє подію зміни фази
        /// </summary>
        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            _currentPhase = evt.CurrentPhase;
            _logger.LogDebug($"ComponentCacheRegistry phase changed to {_currentPhase}", "ECS");

            // Оновлюємо кеші для нової фази
            UpdateCachesForCurrentPhase();
        }

        /// <summary>
        /// Обробляє подію створення сутності
        /// </summary>
        private void OnEntityCreated(EntityCreatedEvent evt)
        {
            // Можливо, потрібно оновити кеші при створенні сутності певного типу
            if (!string.IsNullOrEmpty(evt.ArchetypeId))
            {
                // Тут можна додати логіку оновлення кешів для архетипу
                _logger.LogTrace($"Entity {evt.EntityId} created with archetype {evt.ArchetypeId}", "ECS");
            }
        }

        /// <summary>
        /// Обробляє подію знищення сутності
        /// </summary>
        private void OnEntityDestroyed(EntityDestroyedEvent evt)
        {
            // Потрібно видалити сутність з усіх кешів
            foreach (var cache in _caches.Values)
            {
                var removeMethod = cache.GetType().GetMethod("Remove");
                if (removeMethod != null)
                {
                    removeMethod.Invoke(cache, new object[] { evt.EntityId });
                }
            }

            _logger.LogTrace($"Entity {evt.EntityId} removed from caches", "ECS");
        }

        #endregion
    }
}
