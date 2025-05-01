using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Реалізація пулу подій з кешуванням та аналітикою використання
    /// </summary>
    public class EventPool : IEventPool
    {
        private readonly Dictionary<Type, Queue<object>> _pools = new Dictionary<Type, Queue<object>>();
        private readonly int _maxPoolSize;
        private readonly IMythLogger _logger;

        // Аналітика використання пулу
        private readonly Dictionary<Type, EventPoolStatistics> _statistics = new Dictionary<Type, EventPoolStatistics>();

        // Механізм очищення пулу через "сміттєзбірник" подій
        private readonly Dictionary<Type, DateTime> _lastAccessTime = new Dictionary<Type, DateTime>();
        private readonly TimeSpan _poolCleanupInterval = TimeSpan.FromMinutes(5);
        private DateTime _lastCleanupTime = DateTime.Now;

        public EventPool(int maxPoolSize = 100, IMythLogger logger = null)
        {
            _maxPoolSize = maxPoolSize;
            _logger = logger;
        }

        /// <summary>
        /// Отримує об'єкт події з пулу або створює новий
        /// </summary>
        public T Get<T>() where T : struct, IEvent
        {
            Type eventType = typeof(T);

            // Оновлюємо час останнього доступу
            _lastAccessTime[eventType] = DateTime.Now;

            // Створюємо чергу для типу, якщо вона не існує
            if (!_pools.TryGetValue(eventType, out var pool))
            {
                pool = new Queue<object>();
                _pools[eventType] = pool;

                // Ініціалізуємо статистику
                _statistics[eventType] = new EventPoolStatistics
                {
                    EventType = eventType.Name,
                    CreationTime = DateTime.Now
                };
            }

            T eventObject;

            // Повертаємо об'єкт з пулу або створюємо новий
            if (pool.Count > 0)
            {
                eventObject = (T)pool.Dequeue();

                // Оновлюємо статистику
                if (_statistics.TryGetValue(eventType, out var stats))
                {
                    stats.ReusedCount++;
                }
            }
            else
            {
                eventObject = new T();

                // Оновлюємо статистику
                if (_statistics.TryGetValue(eventType, out var stats))
                {
                    stats.CreatedCount++;
                }
            }

            // Очищення кешу, якщо необхідно
            CheckPoolCleanup();

            return eventObject;
        }

        /// <summary>
        /// Повертає об'єкт події в пул
        /// </summary>
        public void Return<T>(T eventObject) where T : struct, IEvent
        {
            Type eventType = typeof(T);

            // Оновлюємо час останнього доступу
            _lastAccessTime[eventType] = DateTime.Now;

            // Створюємо чергу для типу, якщо вона не існує
            if (!_pools.TryGetValue(eventType, out var pool))
            {
                pool = new Queue<object>();
                _pools[eventType] = pool;

                // Ініціалізуємо статистику
                _statistics[eventType] = new EventPoolStatistics
                {
                    EventType = eventType.Name,
                    CreationTime = DateTime.Now
                };
            }

            // Додаємо об'єкт до пулу, якщо пул не переповнений
            if (pool.Count < _maxPoolSize)
            {
                pool.Enqueue(eventObject);

                // Оновлюємо статистику
                if (_statistics.TryGetValue(eventType, out var stats))
                {
                    stats.ReturnedCount++;
                }
            }
            else
            {
                // Оновлюємо статистику
                if (_statistics.TryGetValue(eventType, out var stats))
                {
                    stats.DiscardedCount++;
                }
            }
        }

        /// <summary>
        /// Очищає пул
        /// </summary>
        public void Clear()
        {
            _pools.Clear();
            _lastAccessTime.Clear();

            // Зберігаємо статистику, але скидаємо лічильники
            foreach (var stats in _statistics.Values)
            {
                stats.ReusedCount = 0;
                stats.CreatedCount = 0;
                stats.ReturnedCount = 0;
                stats.DiscardedCount = 0;
            }

            _lastCleanupTime = DateTime.Now;
        }

        /// <summary>
        /// Перевіряє необхідність очищення пулу
        /// </summary>
        private void CheckPoolCleanup()
        {
            var now = DateTime.Now;

            // Очищаємо пул кожні N хвилин
            if (now - _lastCleanupTime > _poolCleanupInterval)
            {
                CleanupUnusedPools();
                _lastCleanupTime = now;
            }
        }

        /// <summary>
        /// Очищає невикористовувані пули
        /// </summary>
        private void CleanupUnusedPools()
        {
            var now = DateTime.Now;
            var typesToRemove = new List<Type>();

            // Знаходимо пули, які не використовувались протягом інтервалу
            foreach (var pair in _lastAccessTime)
            {
                var type = pair.Key;
                var lastAccess = pair.Value;

                if (now - lastAccess > _poolCleanupInterval * 2) // Подвійний інтервал
                {
                    typesToRemove.Add(type);
                }
            }

            // Видаляємо невикористовувані пули
            foreach (var type in typesToRemove)
            {
                _pools.Remove(type);
                _lastAccessTime.Remove(type);

                if (_logger != null)
                {
                    _logger.LogDebug($"Cleaned up unused event pool for type {type.Name}", "EventPool");
                }
            }
        }

        /// <summary>
        /// Отримує статистику використання пулу
        /// </summary>
        public Dictionary<string, EventPoolStatistics> GetStatistics()
        {
            // Оновлюємо актуальні дані для кожного пулу
            foreach (var pair in _pools)
            {
                Type eventType = pair.Key;
                var pool = pair.Value;

                if (_statistics.TryGetValue(eventType, out var stats))
                {
                    stats.PoolSize = pool.Count;
                }
            }

            return _statistics.ToDictionary(
                pair => pair.Key.Name,
                pair => pair.Value
            );
        }
    }

    /// <summary>
    /// Статистика використання пулу подій
    /// </summary>
    public class EventPoolStatistics
    {
        public string EventType
        {
            get; set;
        }
        public DateTime CreationTime
        {
            get; set;
        }
        public int PoolSize
        {
            get; set;
        }
        public long CreatedCount
        {
            get; set;
        }
        public long ReusedCount
        {
            get; set;
        }
        public long ReturnedCount
        {
            get; set;
        }
        public long DiscardedCount
        {
            get; set;
        }

        public override string ToString()
        {
            return $"EventType: {EventType}, Size: {PoolSize}, Created: {CreatedCount}, Reused: {ReusedCount}, " +
                   $"Returned: {ReturnedCount}, Discarded: {DiscardedCount}";
        }
    }
}
