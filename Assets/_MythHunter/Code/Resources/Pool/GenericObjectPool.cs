// Шлях: Assets/_MythHunter/Code/Resources/Pool/GenericObjectPool.cs

using System;
using System.Collections.Generic;
using MythHunter.Utils.Logging;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Універсальний пул для об'єктів будь-якого типу
    /// </summary>
    public class GenericObjectPool<T> where T : class
    {
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _initialSize;
        private readonly int _maxSize;
        private readonly IMythLogger _logger;
        private readonly string _poolName;

        private int _activeCount;

        /// <summary>
        /// Кількість активних об'єктів у пулі
        /// </summary>
        public int ActiveCount => _activeCount;

        /// <summary>
        /// Кількість неактивних об'єктів у пулі
        /// </summary>
        public int InactiveCount => _pool.Count;

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
            string poolName = null)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _initialSize = initialSize;
            _maxSize = maxSize;
            _logger = logger;
            _poolName = poolName ?? typeof(T).Name;

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

            _logger?.LogDebug($"Pre-warmed pool '{_poolName}' with {count} objects", "Pool");
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
                _logger?.LogTrace($"Got object from pool '{_poolName}', remaining: {_pool.Count}", "Pool");
            }
            else
            {
                obj = _createFunc();
                _logger?.LogTrace($"Created new object for pool '{_poolName}', pool was empty", "Pool");
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
                _logger?.LogTrace($"Released object to pool '{_poolName}', now contains: {_pool.Count}", "Pool");
            }
            else
            {
                _onDestroy?.Invoke(obj);
                _logger?.LogTrace($"Destroyed object from pool '{_poolName}', pool is full ({_maxSize})", "Pool");
            }

            _activeCount--;
        }

        /// <summary>
        /// Очищає пул, видаляючи всі об'єкти
        /// </summary>
        public void Clear()
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

            _logger?.LogInfo($"Cleared pool '{_poolName}'", "Pool");
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

            _logger?.LogInfo($"Resized pool '{_poolName}' to {newSize} objects", "Pool");
        }
    }
}
