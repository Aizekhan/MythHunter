// Шлях: Assets/_MythHunter/Code/Core/ECS/TypedComponentCache.cs

using System;
using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Реєстр компонентних кешів для швидкого доступу до компонентів різних типів
    /// </summary>
    public class ComponentCacheRegistry
    {
        private readonly Dictionary<Type, object> _caches = new Dictionary<Type, object>();
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

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

            _logger.LogDebug($"Created component cache for type {componentType.Name}", "ECS");

            return newCache;
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
            }

            _logger.LogTrace($"Updated {_caches.Count} component caches", "ECS");
        }

        /// <summary>
        /// Очищає всі кеші
        /// </summary>
        public void ClearAllCaches()
        {
            _caches.Clear();
            _logger.LogInfo("Cleared all component caches", "ECS");
        }
    }
}
