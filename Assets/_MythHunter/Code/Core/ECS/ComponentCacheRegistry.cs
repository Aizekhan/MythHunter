using System;
using System.Collections.Generic;
using MythHunter.Entities;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Глобальний реєстр кешів компонентів
    /// </summary>
    public class ComponentCacheRegistry : IComponentCacheRegistry
    {
        private readonly Dictionary<Type, object> _caches = new Dictionary<Type, object>();
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;
        private bool _autoUpdate = true;

        // Статистика
        private int _totalCacheCount = 0;
        private int _totalUpdateCount = 0;
        private int _totalHitCount = 0;
        private int _totalMissCount = 0;

        public ComponentCacheRegistry(IEntityManager entityManager, IMythLogger logger)
        {
            _entityManager = entityManager;
            _logger = logger;
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
        public void SetAutoUpdate(bool value)
        {
            _autoUpdate = value;
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
    }
}
