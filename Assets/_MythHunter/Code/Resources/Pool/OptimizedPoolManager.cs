using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;
using UnityEngine.Pool;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Оптимізований менеджер пулів об'єктів
    /// </summary>
    public class OptimizedPoolManager : IPoolManager
    {
        private readonly Dictionary<string, IObjectPool> _pools = new Dictionary<string, IObjectPool>();
        private readonly IMythLogger _logger;

        // Кеш типів для швидшого доступу
        private readonly Dictionary<Type, Dictionary<string, IObjectPool>> _typedPools = new Dictionary<Type, Dictionary<string, IObjectPool>>();

        // Статистика використання пулів
        private readonly Dictionary<string, PoolStatistics> _statistics = new Dictionary<string, PoolStatistics>();

        [Inject]
        public OptimizedPoolManager(IMythLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Отримує об'єкт з пула
        /// </summary>
        public T GetFromPool<T>(string key) where T : UnityEngine.Object
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                _logger.LogWarning($"Pool with key '{key}' does not exist", "Pool");
                return null;
            }

            if (!(pool is IObjectPool<T> typedPool))
            {
                _logger.LogError($"Pool with key '{key}' exists but is not of type {typeof(T).Name}", "Pool");
                return null;
            }

            var instance = typedPool.Get();

            // Оновлюємо статистику
            if (_statistics.TryGetValue(key, out var stats))
            {
                stats.ActiveCount++;
                stats.TotalGetCount++;
            }

            return instance;
        }

        /// <summary>
        /// Повертає об'єкт у пул
        /// </summary>
        public void ReturnToPool(string key, UnityEngine.Object instance)
        {
            if (instance == null)
                return;

            if (!_pools.TryGetValue(key, out var pool))
            {
                _logger.LogWarning($"Pool with key '{key}' does not exist", "Pool");
                return;
            }

            // Повертаємо у пул через IObjectPool.Return
            pool.ReturnObject(instance);

            // Оновлюємо статистику
            if (_statistics.TryGetValue(key, out var stats))
            {
                stats.ActiveCount--;
                stats.TotalReturnCount++;
            }
        }

        /// <summary>
        /// Створює новий пул для вказаного типу
        /// </summary>
        public void CreatePool<T>(string key, T prefab, int initialSize) where T : UnityEngine.Object
        {
            if (_pools.ContainsKey(key))
            {
                _logger.LogWarning($"Pool with key '{key}' already exists", "Pool");
                return;
            }

            IObjectPool pool;

            // Створюємо відповідний пул залежно від типу
            if (typeof(T) == typeof(GameObject))
            {
                // Контейнер для об'єктів пулу
                var poolParent = new GameObject($"Pool_{key}");
                UnityEngine.Object.DontDestroyOnLoad(poolParent);

                // Явно приводимо до IObjectPool після створення
                var goPool = new GameObjectPool(prefab as GameObject, initialSize, poolParent.transform, null, null, _logger);
                pool = goPool; // Явне приведення типу, якщо GameObjectPool реалізує IObjectPool
            }
            else
            {
                // Для будь-яких інших типів використовуємо GenericObjectPool, адаптований до IObjectPool
                var objPool = new GenericObjectPool<T>(
     () => UnityEngine.Object.Instantiate(prefab),
     null,
     null,
     null,
     initialSize,
     100,
     _logger,
     key
 );
                // Тут потрібен адаптер або переконатися, що GenericObjectPool реалізує IObjectPool
                pool = new ObjectPoolAdapter<T>(objPool); // Створюємо адаптер для GenericObjectPool
            }

            // Додаємо пул до словників
            _pools[key] = pool;


            // Додаємо в кеш за типом
            var type = typeof(T);
            if (!_typedPools.TryGetValue(type, out var typePools))
            {
                typePools = new Dictionary<string, IObjectPool>();
                _typedPools[type] = typePools;
            }

            typePools[key] = pool;

            // Ініціалізуємо статистику
            _statistics[key] = new PoolStatistics
            {
                PoolType = type.Name,
                CreationTime = DateTime.Now,
                InitialSize = initialSize,
            };

            _logger.LogInfo($"Created pool '{key}' for type {typeof(T).Name} with initial size {initialSize}", "Pool");
        }

        /// <summary>
        /// Очищає пул
        /// </summary>
        public void ClearPool(string key)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                _logger.LogWarning($"Pool with key '{key}' does not exist", "Pool");
                return;
            }

            pool.Clear();
            _pools.Remove(key);

            // Видаляємо з кешу за типом
            foreach (var typeDict in _typedPools.Values)
            {
                typeDict.Remove(key);
            }

            // Видаляємо статистику
            _statistics.Remove(key);

            _logger.LogInfo($"Cleared and removed pool '{key}'", "Pool");
        }

        /// <summary>
        /// Очищає всі пули
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }

            _pools.Clear();
            _typedPools.Clear();
            _statistics.Clear();

            _logger.LogInfo("Cleared all pools", "Pool");
        }

        /// <summary>
        /// Отримує статистику про використання пулів
        /// </summary>
        public Dictionary<string, PoolStatistics> GetPoolStatistics()
        {
            // Оновлюємо актуальні дані для кожного пулу
            foreach (var pair in _pools)
            {
                string key = pair.Key;
                var pool = pair.Value;

                if (_statistics.TryGetValue(key, out var stats))
                {
                    stats.InactiveCount = pool.CountInactive;
                    stats.TotalSize = pool.CountActive + pool.CountInactive;
                }
            }

            return new Dictionary<string, PoolStatistics>(_statistics);
        }

        /// <summary>
        /// Змінює розмір пулу
        /// </summary>
        public void ResizePool(string key, int newSize)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                _logger.LogWarning($"Pool with key '{key}' does not exist", "Pool");
                return;
            }

            // Лише для GameObjectPool поки що підтримується
            if (pool is GameObjectPool gameObjectPool)
            {
                gameObjectPool.Resize(newSize);
                _logger.LogInfo($"Resized pool '{key}' to {newSize}", "Pool");

                // Оновлюємо статистику
                if (_statistics.TryGetValue(key, out var stats))
                {
                    stats.InitialSize = newSize;
                }
            }
            else
            {
                _logger.LogWarning($"Resizing not supported for pool type {pool.GetType().Name}", "Pool");
            }
        }

        /// <summary>
        /// Прерогрів пулу - створення додаткових об'єктів
        /// </summary>
        public void WarmupPool(string key, int count)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                _logger.LogWarning($"Pool with key '{key}' does not exist", "Pool");
                return;
            }

            // Для GameObject пулів можемо використати їх внутрішній метод
            if (pool is GameObjectPool gameObjectPool)
            {
                gameObjectPool.Prewarm(count);
                _logger.LogInfo($"Warmed up pool '{key}' with {count} additional objects", "Pool");
            }
            else
            {
                _logger.LogWarning($"Warmup not directly supported for pool type {pool.GetType().Name}", "Pool");
            }
        }
    }

    /// <summary>
    /// Статистика використання пулу
    /// </summary>
    public class PoolStatistics
    {
        public string PoolType
        {
            get; set;
        }
        public DateTime CreationTime
        {
            get; set;
        }
        public int InitialSize
        {
            get; set;
        }
        public int ActiveCount
        {
            get; set;
        }
        public int InactiveCount
        {
            get; set;
        }
        public int TotalSize
        {
            get; set;
        }
        public long TotalGetCount
        {
            get; set;
        }
        public long TotalReturnCount
        {
            get; set;
        }

        public override string ToString()
        {
            return $"Pool: {PoolType}, Active: {ActiveCount}, Inactive: {InactiveCount}, Total: {TotalSize}, Get: {TotalGetCount}, Return: {TotalReturnCount}";
        }
    }

    /// <summary>
    /// Розширений інтерфейс для всіх об'єктних пулів
    /// </summary>
  

    /// <summary>
    /// Розширення GameObject пулу для підтримки додаткових функцій
    /// </summary>
    public static class GameObjectPoolExtensions
    {
        public static void Resize(this GameObjectPool pool, int newSize)
        {
            // Адаптер для виклику внутрішнього методу Trim
            pool.Trim(newSize);
        }

        public static void Prewarm(this GameObjectPool pool, int count)
        {
            // Симуляція прерогріву через отримання та повернення об'єктів
            var objects = new List<GameObject>();

            for (int i = 0; i < count; i++)
            {
                var obj = pool.Get();
                objects.Add(obj);
            }

            foreach (var obj in objects)
            {
                pool.Release(obj);
            }
        }
    }
}
