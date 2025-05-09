// Шлях: Assets/_MythHunter/Code/Resources/Pool/GameObjectPool.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using MythHunter.Utils.Logging;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Оптимізований пул для GameObject з розширеною функціональністю
    /// </summary>
    public class GameObjectPool : BaseObjectPool, IObjectPool<GameObject>
    {
        private readonly GameObject _prefab;
        private readonly Stack<GameObject> _inactiveObjects;
        private readonly HashSet<GameObject> _activeObjects;
        private readonly Transform _poolParent;
        private readonly Action<GameObject> _onGet;
        private readonly Action<GameObject> _onRelease;

        /// <summary>
        /// Кількість неактивних об'єктів у пулі
        /// </summary>
        public override int CountInactive => _inactiveObjects.Count;

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
            _inactiveObjects = new Stack<GameObject>(initialSize);
            _activeObjects = new HashSet<GameObject>();
            _onGet = onGet;
            _onRelease = onRelease;

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
        public void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject obj = UnityEngine.Object.Instantiate(_prefab, _poolParent);
                obj.name = $"{_prefab.name}_Pooled_{i}";
                obj.SetActive(false);
                _inactiveObjects.Push(obj);
            }

            LogTrace($"Pre-warmed pool with {count} objects");
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
                LogTrace($"Got object from pool, remaining: {_inactiveObjects.Count}");
            }
            else
            {
                obj = UnityEngine.Object.Instantiate(_prefab);
                obj.name = $"{_prefab.name}_Pooled_{_activeObjects.Count}";
                LogTrace($"Created new object for pool, pool was empty");
            }

            obj.SetActive(true);
            _activeObjects.Add(obj);
            _activeCount++;
            _onGet?.Invoke(obj);

            return obj;
        }

        /// <summary>
        /// Повертає об'єкт у пул
        /// </summary>
        public void Release(GameObject obj)
        {
            if (obj == null)
                return;

            if (!_activeObjects.Contains(obj))
            {
                LogWarning($"Object {obj.name} was not created from this pool");
                return;
            }

            _onRelease?.Invoke(obj);
            obj.SetActive(false);
            obj.transform.SetParent(_poolParent);

            _activeObjects.Remove(obj);
            _inactiveObjects.Push(obj);
            _activeCount--;

            LogTrace($"Returned object to pool, now contains: {_inactiveObjects.Count}");
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
            // Знищуємо всі активні об'єкти
            foreach (var obj in _activeObjects)
            {
                if (obj != null)
                    UnityEngine.Object.Destroy(obj);
            }
            _activeObjects.Clear();
            _activeCount = 0;

            // Знищуємо всі неактивні об'єкти
            while (_inactiveObjects.Count > 0)
            {
                var obj = _inactiveObjects.Pop();
                if (obj != null)
                    UnityEngine.Object.Destroy(obj);
            }

            LogInfo("Pool cleared");
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

            LogInfo($"Trimmed pool to {_inactiveObjects.Count} inactive objects");
        }
    }
}
