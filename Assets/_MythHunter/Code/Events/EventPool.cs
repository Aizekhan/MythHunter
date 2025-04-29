using System;
using System.Collections.Generic;

namespace MythHunter.Events
{
    /// <summary>
    /// Реалізація пулу подій для оптимізації створення/знищення подій
    /// </summary>
    public class EventPool : IEventPool
    {
        // Словник пулів для різних типів подій
        private readonly Dictionary<Type, Queue<object>> _pools = new Dictionary<Type, Queue<object>>();

        // Максимальний розмір пулу для кожного типу
        private readonly int _maxPoolSize;

        public EventPool(int maxPoolSize = 100)
        {
            _maxPoolSize = maxPoolSize;
        }

        /// <summary>
        /// Отримує об'єкт події з пулу або створює новий
        /// </summary>
        public T Get<T>() where T : struct, IEvent
        {
            Type eventType = typeof(T);

            // Створюємо чергу для типу, якщо вона не існує
            if (!_pools.TryGetValue(eventType, out var pool))
            {
                pool = new Queue<object>();
                _pools[eventType] = pool;
            }

            // Повертаємо об'єкт з пулу або створюємо новий
            if (pool.Count > 0)
            {
                return (T)pool.Dequeue();
            }

            return new T();
        }

        /// <summary>
        /// Повертає об'єкт події в пул
        /// </summary>
        public void Return<T>(T eventObject) where T : struct, IEvent
        {
            Type eventType = typeof(T);

            // Створюємо чергу для типу, якщо вона не існує
            if (!_pools.TryGetValue(eventType, out var pool))
            {
                pool = new Queue<object>();
                _pools[eventType] = pool;
            }

            // Додаємо об'єкт до пулу, якщо пул не переповнений
            if (pool.Count < _maxPoolSize)
            {
                pool.Enqueue(eventObject);
            }
        }

        /// <summary>
        /// Очищає пул
        /// </summary>
        public void Clear()
        {
            _pools.Clear();
        }
    }
}
