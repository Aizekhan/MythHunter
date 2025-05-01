// Шлях: Assets/_MythHunter/Code/Events/PrioritizedEventPool.cs

using System;
using System.Collections.Generic;

namespace MythHunter.Events
{
    /// <summary>
    /// Реалізація пулу подій з підтримкою пріоритетів
    /// </summary>
    public class PrioritizedEventPool : IEventPool
    {
        // Словник пулів для різних типів подій
        private readonly Dictionary<Type, Queue<object>> _pools = new Dictionary<Type, Queue<object>>();

        // Словник пріоритетів для типів подій
        private readonly Dictionary<Type, EventPriority> _priorities = new Dictionary<Type, EventPriority>();

        // Максимальний розмір пулу для кожного типу
        private readonly int _maxPoolSize;

        public PrioritizedEventPool(int maxPoolSize = 100)
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

            // Створюємо нову подію
            var newEvent = new T();

            // Запам'ятовуємо пріоритет, якщо він ще не зареєстрований
            if (!_priorities.ContainsKey(eventType))
            {
                _priorities[eventType] = newEvent.GetPriority();
            }

            return newEvent;
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
        /// Отримує пріоритет для типу події
        /// </summary>
        public EventPriority GetPriority<T>() where T : struct, IEvent
        {
            Type eventType = typeof(T);

            if (_priorities.TryGetValue(eventType, out var priority))
            {
                return priority;
            }

            // Якщо пріоритет не зареєстрований, створюємо тимчасову подію для отримання пріоритету
            var tempEvent = new T();
            var eventPriority = tempEvent.GetPriority();
            _priorities[eventType] = eventPriority;

            return eventPriority;
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
