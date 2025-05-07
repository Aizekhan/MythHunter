// Шлях: Assets/_MythHunter/Code/Resources/Pool/OptimizedPoolManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Оптимізований менеджер пулів об'єктів з розширеними можливостями відстеження
    /// </summary>
    public class OptimizedPoolManager : IPoolManager
    {
        private readonly Dictionary<string, IObjectPool> _pools = new Dictionary<string, IObjectPool>();
        private readonly IMythLogger _logger;
        private readonly Dictionary<Type, Dictionary<string, IObjectPool>> _typedPools = new Dictionary<Type, Dictionary<string, IObjectPool>>();
        private readonly Dictionary<string, PoolStatistics> _statistics = new Dictionary<string, PoolStatistics>();

        // Відстеження активних об'єктів
        private readonly Dictionary<int, PooledObjectInfo> _activeObjects = new Dictionary<int, PooledObjectInfo>();
        private float _autoCleanupInterval = 60f; // Секунд
        private float _lastCleanupTime;

        // Нові поля для інтеграції з PooledObjectLifetimeTracker
        private PooledObjectLifetimeTracker _lifetimeTracker;
        private PoolMonitor _poolMonitor;

        /// <summary>
        /// Інформація про активний об'єкт пулу
        /// </summary>
        private struct PooledObjectInfo
        {
            public string PoolKey;
            public UnityEngine.Object Instance;
            public float ActivationTime;
        }

        [Inject]
        public OptimizedPoolManager(IMythLogger logger)
        {
            _logger = logger;
            _lastCleanupTime = Time.realtimeSinceStartup;

            // Ініціалізація системи відстеження життєвого циклу
            _lifetimeTracker = new PooledObjectLifetimeTracker(logger);
        }

        /// <summary>
        /// Встановлює посилання на PoolMonitor
        /// </summary>
        public void SetPoolMonitor(PoolMonitor poolMonitor)
        {
            _poolMonitor = poolMonitor;
            _logger.LogInfo("Pool monitor reference set", "Pool");
        }

        /// <summary>
        /// Отримує об'єкт з пулу
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

            // Перевірка наявності компонента PooledObject для GameObject
            if (instance is GameObject go)
            {
                var pooledObj = go.GetComponent<PooledObject>();
                if (pooledObj == null)
                {
                    pooledObj = go.AddComponent<PooledObject>();
                }
                pooledObj.Initialize(key, this);
            }

            // Відстеження активного об'єкта
            int instanceId = instance.GetInstanceID();
            _activeObjects[instanceId] = new PooledObjectInfo
            {
                PoolKey = key,
                Instance = instance,
                ActivationTime = Time.realtimeSinceStartup
            };

            // Оновлення статистики
            if (_statistics.TryGetValue(key, out var stats))
            {
                stats.ActiveCount++;
                stats.TotalGetCount++;
            }

            // Відстеження життєвого циклу об'єкта
            _lifetimeTracker?.TrackActivation(key, instance);

            // Інтеграція з PoolMonitor для сценозалежних пулів
            if (_poolMonitor != null && instance is GameObject gameObj)
            {
                _poolMonitor.RegisterSceneDependentPool(key, gameObj.scene.name);
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

            // Відстеження деактивації об'єкта
            _lifetimeTracker?.TrackDeactivation(key, instance);

            // Видалення з відстеження активних об'єктів
            int instanceId = instance.GetInstanceID();
            _activeObjects.Remove(instanceId);

            // Повертаємо у пул через IObjectPool.Return
            pool.ReturnObject(instance);

            // Оновлення статистики
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

            if (typeof(T) == typeof(GameObject))
            {
                // Контейнер для об'єктів пулу
                var poolParent = new GameObject($"Pool_{key}");
                UnityEngine.Object.DontDestroyOnLoad(poolParent);

                var goPool = new GameObjectPool(prefab as GameObject, initialSize, poolParent.transform, null, null, _logger);
                pool = goPool;
            }
            else
            {
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
                pool = new ObjectPoolAdapter<T>(objPool);
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

            // Інформуємо PoolMonitor про створення пулу
            if (_poolMonitor != null && typeof(T) == typeof(GameObject))
            {
                // Визначаємо поточну сцену
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                _poolMonitor.RegisterSceneDependentPool(key, currentScene);
            }

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

            // Видаляємо активні об'єкти цього пулу
            var keysToRemove = _activeObjects.Where(kvp => kvp.Value.PoolKey == key)
                .Select(kvp => kvp.Key).ToList();
            foreach (var k in keysToRemove)
            {
                _activeObjects.Remove(k);
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
            _activeObjects.Clear();

            _logger.LogInfo("Cleared all pools", "Pool");
        }

        /// <summary>
        /// Отримує статистику використання пулів
        /// </summary>
        public Dictionary<string, PoolStatistics> GetAllPoolsStatistics()
        {
            // Оновлюємо актуальні дані для кожного пулу
            foreach (var pair in _pools)
            {
                string key = pair.Key;
                var pool = pair.Value;

                if (_statistics.TryGetValue(key, out var stats))
                {
                    stats.InactiveCount = pool.CountInactive;
                    stats.ActiveCount = pool.CountActive;
                    stats.TotalSize = pool.CountActive + pool.CountInactive;
                }
            }

            return new Dictionary<string, PoolStatistics>(_statistics);
        }

        /// <summary>
        /// Перевіряє наявність потенційних витоків об'єктів
        /// </summary>
        public void CheckForLeaks()
        {
            // Перевірка об'єктів, які активні занадто довго
            float currentTime = Time.realtimeSinceStartup;
            var suspiciousObjects = _activeObjects
                .Where(kvp => currentTime - kvp.Value.ActivationTime > 300f) // 5+ хвилин активності
                .Take(10) // Обмеження логу
                .ToList();

            if (suspiciousObjects.Count > 0)
            {
                _logger.LogWarning($"Found {suspiciousObjects.Count} objects potentially leaked:", "Pool");
                foreach (var obj in suspiciousObjects)
                {
                    var info = obj.Value;
                    _logger.LogWarning($"- Object ID: {obj.Key}, Pool: {info.PoolKey}, " +
                        $"Active for: {currentTime - info.ActivationTime:F1} seconds", "Pool");
                }
            }

            // Перевірка витоків через систему відстеження життєвого циклу
            if (_lifetimeTracker != null)
            {
                var potentialLeaks = _lifetimeTracker.DetectPotentialLeaks();
                if (potentialLeaks.Count > 0)
                {
                    _logger.LogWarning($"Detected {potentialLeaks.Count} potential leaks via lifetime tracking", "Pool");
                    // Деталі в зовнішній системі
                }
            }

            // Автоматичне очищення, якщо пройшов інтервал
            if (currentTime - _lastCleanupTime > _autoCleanupInterval)
            {
                TrimExcessObjects();
                _lastCleanupTime = currentTime;
            }
        }

        /// <summary>
        /// Видаляє зайві неактивні об'єкти з пулів
        /// </summary>
        public void TrimExcessObjects(int maxInactivePerPool = 20)
        {
            foreach (var pair in _pools)
            {
                string key = pair.Key;
                var pool = pair.Value;

                // Для GameObjectPool використовуємо спеціальний метод Trim
                if (pool is GameObjectPool gameObjectPool && pool.CountInactive > maxInactivePerPool)
                {
                    gameObjectPool.Trim(maxInactivePerPool);
                    _logger.LogInfo($"Trimmed pool '{key}' to {maxInactivePerPool} inactive objects", "Pool");
                }
            }
        }

        /// <summary>
        /// Отримує загальну кількість активних об'єктів у всіх пулах
        /// </summary>
        public int GetTotalActiveObjects()
        {
            return _activeObjects.Count;
        }

        /// <summary>
        /// Встановлює інтервал для автоматичного очищення пулів
        /// </summary>
        public void SetAutoCleanupInterval(float seconds)
        {
            _autoCleanupInterval = Mathf.Max(10f, seconds);
            _logger.LogInfo($"Auto cleanup interval set to {_autoCleanupInterval} seconds", "Pool");
        }

        /// <summary>
        /// Отримує систему відстеження життєвого циклу об'єктів
        /// </summary>
        public PooledObjectLifetimeTracker GetLifetimeTracker()
        {
            return _lifetimeTracker;
        }

        /// <summary>
        /// Отримує статистику про час життя об'єктів
        /// </summary>
        public Dictionary<string, List<PooledObjectLifetimeTracker.ObjectLifetimeInfo>> GetLifetimeStatistics()
        {
            return _lifetimeTracker?.GetLifetimeStatistics() ?? new Dictionary<string, List<PooledObjectLifetimeTracker.ObjectLifetimeInfo>>();
        }

        /// <summary>
        /// Реєструє зв'язок пулу із сценою
        /// </summary>
        public void RegisterSceneAssociation(string poolKey, string sceneName)
        {
            if (_poolMonitor != null)
            {
                _poolMonitor.RegisterSceneDependentPool(poolKey, sceneName);
                _logger.LogInfo($"Associated pool '{poolKey}' with scene '{sceneName}'", "Pool");
            }
        }

        public void UpdateSceneAssociations()
        {
            if (_poolMonitor == null)
                return;

            // Отримуємо поточні сцени
            var sceneNames = new List<string>();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                sceneNames.Add(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name);
            }

            // Оновлюємо статистику
            foreach (var pair in _statistics)
            {
                string poolKey = pair.Key;
                var stats = pair.Value;

                // Оновлюємо інформацію про сцену на основі активних GameObject
                foreach (var obj in _activeObjects.Values)
                {
                    if (obj.PoolKey == poolKey && obj.Instance is GameObject go)
                    {
                        stats.SceneName = go.scene.name;
                        break;
                    }
                }

                // Якщо пул асоційований з поточною сценою, реєструємо його
                if (!string.IsNullOrEmpty(stats.SceneName) && sceneNames.Contains(stats.SceneName))
                {
                    _poolMonitor?.RegisterSceneDependentPool(poolKey, stats.SceneName);
                }
            }
        }

        public PoolMonitor GetPoolMonitor()
        {
            return _poolMonitor;
        }
    }
}
