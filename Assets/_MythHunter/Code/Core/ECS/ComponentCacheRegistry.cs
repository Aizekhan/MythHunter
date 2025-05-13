// Файл: Assets/_MythHunter/Code/Core/ECS/ComponentCacheRegistry.cs

using System;
using System.Collections.Generic;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Entities;
using MythHunter.Events.Domain;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Оптимізований глобальний реєстр кешів компонентів з підтримкою фаз
    /// </summary>
    public class ComponentCacheRegistry : IComponentCacheRegistry, IDisposable
    {
        private readonly Dictionary<Type, object> _caches = new Dictionary<Type, object>();
        private readonly IEntityManager _entityManager;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly IPhaseProvider _phaseProvider;

        // Налаштування автооновлення
        private bool _autoUpdate = true;

        // Налаштування оновлення за фазами
        private readonly Dictionary<string, HashSet<Type>> _phaseComponentMapping = new Dictionary<string, HashSet<Type>>();
        private string _currentPhase = string.Empty;

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
        public ComponentCacheRegistry(
            IEntityManager entityManager,
            IEventBus eventBus,
            IMythLogger logger,
            IPhaseProvider phaseProvider)
        {
            _entityManager = entityManager;
            _eventBus = eventBus;
            _logger = logger;
            _phaseProvider = phaseProvider;

            // Підписуємося на зміну фази через провайдер
            _phaseProvider.SubscribeToPhaseChange(OnPhaseChanged);

            // Підписуємося на події сутностей
            SubscribeToEntityEvents();

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
        public void RegisterComponentForPhase<T>(string phaseId) where T : struct, IComponent
        {
            Type componentType = typeof(T);

            if (!_phaseComponentMapping.ContainsKey(phaseId))
            {
                _phaseComponentMapping[phaseId] = new HashSet<Type>();
            }

            _phaseComponentMapping[phaseId].Add(componentType);
            _logger.LogDebug($"Registered component {componentType.Name} for phase {phaseId}", "ECS");
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
            if (string.IsNullOrEmpty(_currentPhase))
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
        /// Підписується на події сутностей
        /// </summary>
        private void SubscribeToEntityEvents()
        {
            _eventBus.Subscribe<EntityCreatedEvent>(OnEntityCreated);
            _eventBus.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
            _logger.LogDebug("ComponentCacheRegistry subscribed to entity events", "ECS");
        }

        /// <summary>
        /// Відписується від подій сутностей
        /// </summary>
        private void UnsubscribeFromEntityEvents()
        {
            _eventBus.Unsubscribe<EntityCreatedEvent>(OnEntityCreated);
            _eventBus.Unsubscribe<EntityDestroyedEvent>(OnEntityDestroyed);
            _logger.LogDebug("ComponentCacheRegistry unsubscribed from entity events", "ECS");
        }

        /// <summary>
        /// Обробляє подію зміни фази
        /// </summary>
        private void OnPhaseChanged(string previousPhase, string currentPhase)
        {
            _currentPhase = currentPhase;
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

        public void Dispose()
        {
            // Відписуємося від подій
            _phaseProvider.UnsubscribeFromPhaseChange(OnPhaseChanged);
            UnsubscribeFromEntityEvents();

            // Очищаємо ресурси
            ClearAllCaches();
        }
    }
}
