// ObjectPool.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Універсальний пул для об'єктів UnityEngine.Object
    /// </summary>
    public class ObjectPool : IObjectPool<UnityEngine.Object>
    {
        private readonly UnityEngine.Object _prefab;
        private readonly Stack<UnityEngine.Object> _inactiveObjects;
        private readonly HashSet<UnityEngine.Object> _activeObjects;
        private readonly Transform _parent;
        [Inject] private IMythLogger _logger; // Додаємо логер через DI
        /// <summary>
        /// Кількість активних об'єктів у пулі
        /// </summary>
        public int CountActive => _activeObjects.Count;

        /// <summary>
        /// Кількість неактивних об'єктів у пулі
        /// </summary>
        public int CountInactive => _inactiveObjects.Count;

        /// <summary>
        /// Створює новий пул об'єктів
        /// </summary>
        public ObjectPool(UnityEngine.Object prefab, int initialSize = 10, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            _inactiveObjects = new Stack<UnityEngine.Object>(initialSize);
            _activeObjects = new HashSet<UnityEngine.Object>();

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
                UnityEngine.Object obj = null;

                if (_prefab is GameObject prefabGO)
                {
                    obj = UnityEngine.Object.Instantiate(prefabGO, _parent);
                    (obj as GameObject).SetActive(false);
                }
                else
                {
                    obj = UnityEngine.Object.Instantiate(_prefab);
                }

                _inactiveObjects.Push(obj);
            }
        }

        /// <summary>
        /// Отримати об'єкт з пулу
        /// </summary>
        public UnityEngine.Object Get()
        {
            UnityEngine.Object obj;

            if (_inactiveObjects.Count > 0)
            {
                // Використовуємо існуючий об'єкт із пулу
                obj = _inactiveObjects.Pop();
            }
            else
            {
                // Створюємо новий об'єкт, якщо пул порожній
                if (_prefab is GameObject prefabGO)
                {
                    obj = UnityEngine.Object.Instantiate(prefabGO, _parent);
                }
                else
                {
                    obj = UnityEngine.Object.Instantiate(_prefab);
                }
            }

            // Активуємо GameObject, якщо це можливо
            if (obj is GameObject gameObject)
            {
                gameObject.SetActive(true);
            }

            _activeObjects.Add(obj);
            return obj;
        }

        /// <summary>
        /// Повернути об'єкт у пул
        /// </summary>
        public void Return(UnityEngine.Object obj)
        {
            if (obj == null)
                return;

            if (!_activeObjects.Remove(obj))
            {
                _logger.LogWarning($"Trying to return an object that wasn't created from this pool");
                return;
            }

            // Деактивуємо GameObject, якщо це можливо
            if (obj is GameObject gameObject)
            {
                gameObject.SetActive(false);
            }

            _inactiveObjects.Push(obj);
        }

        /// <summary>
        /// Очистити пул, видаливши всі об'єкти
        /// </summary>
        public void Clear()
        {
            // Видаляємо всі активні об'єкти
            foreach (var obj in _activeObjects)
            {
                UnityEngine.Object.Destroy(obj);
            }
            _activeObjects.Clear();

            // Видаляємо всі неактивні об'єкти
            while (_inactiveObjects.Count > 0)
            {
                var obj = _inactiveObjects.Pop();
                UnityEngine.Object.Destroy(obj);
            }
        }
    }
}
