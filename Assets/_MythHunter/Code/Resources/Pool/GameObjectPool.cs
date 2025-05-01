// Шлях: Assets/_MythHunter/Code/Resources/Pool/GameObjectPool.cs

using System;
using System.Collections.Generic;
using UnityEngine;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Спеціалізований пул для GameObject з підтримкою префабів
    /// </summary>
    public class GameObjectPool : IObjectPool<GameObject>
    {
        private readonly GameObject _prefab;
        private readonly Stack<GameObject> _inactiveObjects;
        private readonly HashSet<GameObject> _activeObjects;
        private readonly Transform _poolParent;
        private readonly Action<GameObject> _onGet;
        private readonly Action<GameObject> _onRelease;
        private readonly IMythLogger _logger;
        private readonly string _poolName;

        /// <summary>
        /// Кількість активних об'єктів у пулі
        /// </summary>
        public int CountActive => _activeObjects.Count;

        /// <summary>
        /// Кількість неактивних об'єктів у пулі
        /// </summary>
        public int CountInactive => _inactiveObjects.Count;

        [Inject]
        public GameObjectPool(
            GameObject prefab,
            int initialSize = 10,
            Transform parent = null,
            Action<GameObject> onGet = null,
            Action<GameObject> onRelease = null,
            IMythLogger logger = null,
            string poolName = null)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            _prefab = prefab;
            _inactiveObjects = new Stack<GameObject>(initialSize);
            _activeObjects = new HashSet<GameObject>();
            _onGet = onGet;
            _onRelease = onRelease;
            _logger = logger;
            _poolName = poolName ?? prefab.name;

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

            // Попереднє створення об'єктів
            PreWarm(initialSize);
        }

        /// <summary>
        /// Попереднє створення об'єктів у пулі
        /// </summary>
        private void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject obj = UnityEngine.Object.Instantiate(_prefab, _poolParent);
                obj.name = $"{_prefab.name}_Pooled_{i}";
                obj.SetActive(false);
                _inactiveObjects.Push(obj);
            }

            _logger?.LogDebug($"Pre-warmed pool '{_poolName}' with {count} objects", "Pool");
        }

        /// <summary>
        /// Отримує об'єкт з пулу
        /// </summary>
        public GameObject Get()
        {
            GameObject obj;

            if (_inactiveObjects.Count > 0)
            {
                obj = _inactiveObjects.Pop();
                obj.transform.SetParent(null);
                _logger?.LogTrace($"Got object from pool '{_poolName}', remaining: {_inactiveObjects.Count}", "Pool");
            }
            else
            {
                obj = UnityEngine.Object.Instantiate(_prefab);
                obj.name = $"{_prefab.name}_Pooled_{_activeObjects.Count}";
                _logger?.LogTrace($"Created new object for pool '{_poolName}', pool was empty", "Pool");
            }

            obj.SetActive(true);
            _activeObjects.Add(obj);
            _onGet?.Invoke(obj);

            return obj;
        }

        /// <summary>
        /// Повертає об'єкт у пул
        /// </summary>
        public void Return(GameObject obj)
        {
            if (obj == null)
                return;

            if (!_activeObjects.Contains(obj))
            {
                _logger?.LogWarning($"Object {obj.name} was not created from this pool '{_poolName}'", "Pool");
                return;
            }

            _onRelease?.Invoke(obj);
            obj.SetActive(false);
            obj.transform.SetParent(_poolParent);

            _activeObjects.Remove(obj);
            _inactiveObjects.Push(obj);

            _logger?.LogTrace($"Returned object to pool '{_poolName}', now contains: {_inactiveObjects.Count}", "Pool");
        }

        /// <summary>
        /// Очищає пул, знищуючи всі об'єкти
        /// </summary>
        public void Clear()
        {
            // Знищуємо всі активні об'єкти
            foreach (var obj in _activeObjects)
            {
                if (obj != null)
                    UnityEngine.Object.Destroy(obj);
            }
            _activeObjects.Clear();

            // Знищуємо всі неактивні об'єкти
            while (_inactiveObjects.Count > 0)
            {
                var obj = _inactiveObjects.Pop();
                if (obj != null)
                    UnityEngine.Object.Destroy(obj);
            }

            _logger?.LogInfo($"Cleared pool '{_poolName}'", "Pool");
        }

        /// <summary>
        /// Вивільняє всі зайві об'єкти, залишаючи лише вказану кількість
        /// </summary>
        public void Trim(int count)
        {
            if (count < 0)
                count = 0;

            while (_inactiveObjects.Count > count)
            {
                var obj = _inactiveObjects.Pop();
                if (obj != null)
                    UnityEngine.Object.Destroy(obj);
            }

            _logger?.LogInfo($"Trimmed pool '{_poolName}' to {_inactiveObjects.Count} inactive objects", "Pool");
        }
    }
}
