// Шлях: Assets/_MythHunter/Code/Resources/Pool/GenericObjectPool.cs
using System;
using System.Collections.Generic;
using MythHunter.Utils.Logging;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Універсальний пул для об'єктів будь-якого типу
    /// </summary>
    public class GenericObjectPool<T> : BaseObjectPool, IObjectPool<T> where T : class
    {
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _maxSize;

        /// <summary>
        /// Кількість неактивних об'єктів у пулі
        /// </summary>
        public override int CountInactive => _pool.Count;

        /// <summary>
        /// Загальний розмір пулу
        /// </summary>
        public int TotalSize => _activeCount + _pool.Count;

        /// <summary>
        /// Створює новий пул об'єктів
        /// </summary>
        public GenericObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int initialSize = 10,
            int maxSize = 100,
            IMythLogger logger = null,
            string poolName = null) : base(logger, poolName ?? typeof(T).Name)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _maxSize = maxSize;

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
                var obj = _createFunc();
                _pool.Push(obj);
            }

            LogTrace($"Pre-warmed pool with {count} objects");
        }

        /// <summary>
        /// Отримує об'єкт з пулу
        /// </summary>
        public T Get()
        {
            T obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Pop();
                LogTrace($"Got object from pool, remaining: {_pool.Count}");
            }
            else
            {
                obj = _createFunc();
                LogTrace($"Created new object for pool, pool was empty");
            }

            _onGet?.Invoke(obj);
            _activeCount++;

            return obj;
        }

        /// <summary>
        /// Повертає об'єкт у пул
        /// </summary>
        public void Release(T obj)
        {
            if (obj == null)
                return;

            _onRelease?.Invoke(obj);

            if (_pool.Count < _maxSize)
            {
                _pool.Push(obj);
                LogTrace($"Released object to pool, now contains: {_pool.Count}");
            }
            else
            {
                _onDestroy?.Invoke(obj);
                LogTrace($"Destroyed object from pool, pool is full ({_maxSize})");
            }

            _activeCount--;
        }

        /// <summary>
        /// Реалізація повернення об'єкту для базового класу
        /// </summary>
        protected override void ReturnObjectInternal(UnityEngine.Object obj)
        {
            if (obj is T typedObj)
            {
                Release(typedObj);
            }
            else
            {
                LogWarning($"Cannot return object of type {obj.GetType().Name} to pool of type {typeof(T).Name}");
            }
        }

        /// <summary>
        /// Очищає пул, видаляючи всі об'єкти
        /// </summary>
        public override void Clear()
        {
            if (_onDestroy != null)
            {
                foreach (var obj in _pool)
                {
                    _onDestroy(obj);
                }
            }

            _pool.Clear();
            _activeCount = 0;

            LogInfo("Pool cleared");
        }

        /// <summary>
        /// Змінює розмір пулу
        /// </summary>
        public void Resize(int newSize)
        {
            if (newSize < 0)
            {
                throw new ArgumentException("Pool size cannot be negative", nameof(newSize));
            }

            // Якщо новий розмір менший за поточний, видаляємо зайві об'єкти
            while (_pool.Count > newSize)
            {
                var obj = _pool.Pop();
                _onDestroy?.Invoke(obj);
            }

            // Якщо новий розмір більший за поточний, додаємо нові об'єкти
            while (_pool.Count < newSize)
            {
                var obj = _createFunc();
                _pool.Push(obj);
            }

            LogInfo($"Resized pool to {newSize} objects");
        }
    }
}
