// Шлях: Assets/_MythHunter/Code/Resources/Pool/GameObjectPool.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using MythHunter.Utils.Logging;
using RPGFramework.Pooling;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Unity GameObject pool adapter using Framework ObjectPool internally.
    /// Handles Unity-specific GameObject lifecycle (Transform parenting, SetActive, naming).
    /// </summary>
    public class GameObjectPool : BaseObjectPool, IObjectPool<GameObject>
    {
        private readonly GameObject _prefab;
        private readonly Transform _poolParent;
        private readonly HashSet<GameObject> _activeObjects;
        private readonly RPGFramework.Pooling.ObjectPool<GameObject> _pool;
        private int _instanceCounter;

        /// <summary>
        /// Кількість неактивних об'єктів у пулі
        /// </summary>
        public override int CountInactive => _pool.CountInactive;

        public GameObjectPool(
            GameObject prefab,
            int initialSize = 10,
            Transform parent = null,
            Action<GameObject> onGet = null,
            Action<GameObject> onRelease = null,
            IMythLogger logger = null) : base(logger, prefab?.name ?? "UnnamedGameObjectPool")
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            _prefab = prefab;
            _activeObjects = new HashSet<GameObject>();
            _instanceCounter = 0;

            // Створення контейнера для об'єктів
            if (parent == null)
            {
                var poolContainer = new GameObject($"Pool_{_poolName}");
                UnityEngine.Object.DontDestroyOnLoad(poolContainer);
                _poolParent = poolContainer.transform;
            }
            else
            {
                _poolParent = parent;
            }

            // Create Framework pool with Unity-specific lifecycle callbacks
            _pool = new RPGFramework.Pooling.ObjectPool<GameObject>(
                createFunc: CreateGameObject,
                onGet: obj =>
                {
                    obj.SetActive(true);
                    obj.transform.SetParent(null);
                    onGet?.Invoke(obj);
                },
                onRelease: obj =>
                {
                    onRelease?.Invoke(obj);
                    obj.SetActive(false);
                    obj.transform.SetParent(_poolParent);
                },
                onDestroy: obj =>
                {
                    if (obj != null)
                        UnityEngine.Object.Destroy(obj);
                },
                initialSize: initialSize,
                maxSize: 100,
                logger: new FrameworkLoggerAdapter(logger),
                poolName: prefab.name
            );
        }

        private GameObject CreateGameObject()
        {
            GameObject obj = UnityEngine.Object.Instantiate(_prefab, _poolParent);
            obj.name = $"{_prefab.name}_Pooled_{_instanceCounter++}";
            obj.SetActive(false);
            return obj;
        }

        /// <summary>
        /// Попереднє створення об'єктів у пулі
        /// </summary>
        public void PreWarm(int count)
        {
            // Framework pool already handles pre-warming in constructor
            LogTrace($"Pool pre-warmed with {count} objects");
        }

        /// <summary>
        /// Отримує об'єкт з пулу
        /// </summary>
        public GameObject Get()
        {
            var obj = _pool.Get();
            _activeObjects.Add(obj);
            _activeCount++;
            return obj;
        }

        /// <summary>
        /// Повертає об'єкт у пул
        /// </summary>
        public void Release(GameObject obj)
        {
            if (obj == null)
            {
                LogWarning("Attempted to release null GameObject");
                return;
            }

            if (!_activeObjects.Contains(obj))
            {
                LogWarning($"Object {obj.name} was not created from this pool");
                return;
            }

            _activeObjects.Remove(obj);
            _activeCount--;
            _pool.Release(obj);
        }

        /// <summary>
        /// Реалізація повернення об'єкту для базового класу
        /// </summary>
        protected override void ReturnObjectInternal(UnityEngine.Object obj)
        {
            if (obj is GameObject gameObj)
            {
                Release(gameObj);
            }
            else
            {
                LogWarning($"Cannot return object of type {obj.GetType().Name} to GameObject pool");
            }
        }

        /// <summary>
        /// Очищає пул, знищуючи всі об'єкти
        /// </summary>
        public override void Clear()
        {
            // Destroy active objects (not managed by Framework pool)
            foreach (var obj in _activeObjects)
            {
                if (obj != null)
                    UnityEngine.Object.Destroy(obj);
            }
            _activeObjects.Clear();
            _activeCount = 0;

            // Clear Framework pool (inactive objects)
            _pool.Clear();

            LogInfo("Pool cleared");
        }

        /// <summary>
        /// Вивільняє всі зайві об'єкти, залишаючи лише вказану кількість
        /// </summary>
        public void Trim(int count)
        {
            if (count < 0)
                count = 0;

            // Resize Framework pool to target size
            _pool.Resize(count);

            LogInfo($"Trimmed pool to {_pool.CountInactive} inactive objects");
        }

        /// <summary>
        /// Adapter class to bridge MythHunter logging to Framework logging.
        /// </summary>
        private class FrameworkLoggerAdapter : RPGFramework.Logging.ILogger
        {
            private readonly IMythLogger _mythLogger;

            public FrameworkLoggerAdapter(IMythLogger mythLogger)
            {
                _mythLogger = mythLogger;
            }

            public void Log(RPGFramework.Logging.LogSeverity severity, string message, string category = null, Exception exception = null)
            {
                switch (severity)
                {
                    case RPGFramework.Logging.LogSeverity.Trace:
                        _mythLogger?.LogTrace(message, category ?? "Pool");
                        break;
                    case RPGFramework.Logging.LogSeverity.Debug:
                    case RPGFramework.Logging.LogSeverity.Information:
                        _mythLogger?.LogInfo(message, category ?? "Pool");
                        break;
                    case RPGFramework.Logging.LogSeverity.Warning:
                        _mythLogger?.LogWarning(message, category ?? "Pool");
                        break;
                    case RPGFramework.Logging.LogSeverity.Error:
                    case RPGFramework.Logging.LogSeverity.Critical:
                        _mythLogger?.LogError(message, category ?? "Pool");
                        break;
                }
            }
        }
    }
}
