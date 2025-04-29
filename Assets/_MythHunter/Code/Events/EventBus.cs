using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Реалізація шини подій з підтримкою синхронної та асинхронної обробки
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<SyncEventHandler>> _syncHandlers = new Dictionary<Type, List<SyncEventHandler>>();
        private readonly Dictionary<Type, List<AsyncEventHandler>> _asyncHandlers = new Dictionary<Type, List<AsyncEventHandler>>();
        private readonly IEventPool _eventPool;
        private readonly IMythLogger _logger;

        // Клас обгортки для синхронного обробника подій
        private class SyncEventHandler
        {
            public Delegate Handler
            {
                get;
            }
            public EventPriority Priority
            {
                get;
            }

            public SyncEventHandler(Delegate handler, EventPriority priority)
            {
                Handler = handler;
                Priority = priority;
            }
        }

        // Клас обгортки для асинхронного обробника подій
        private class AsyncEventHandler
        {
            public Delegate Handler
            {
                get;
            }
            public EventPriority Priority
            {
                get;
            }

            public AsyncEventHandler(Delegate handler, EventPriority priority)
            {
                Handler = handler;
                Priority = priority;
            }
        }

        public EventBus(IEventPool eventPool, IMythLogger logger)
        {
            _eventPool = eventPool;
            _logger = logger;
        }

        #region Sync Methods

        /// <summary>
        /// Підписується на синхронну обробку події
        /// </summary>
        public void Subscribe<TEvent>(Action<TEvent> handler, EventPriority priority = EventPriority.Normal)
            where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);

            if (!_syncHandlers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<SyncEventHandler>();
                _syncHandlers[eventType] = handlers;
            }

            handlers.Add(new SyncEventHandler(handler, priority));

            // Сортуємо обробники за пріоритетом (високий пріоритет першим)
            _syncHandlers[eventType] = handlers.OrderByDescending(h => h.Priority).ToList();

            _logger.LogDebug($"Subscribed to event {eventType.Name} with priority {priority}");
        }

        /// <summary>
        /// Відписується від синхронної обробки події
        /// </summary>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);

            if (!_syncHandlers.TryGetValue(eventType, out var handlers))
                return;

            // Знаходимо і видаляємо обробник
            handlers.RemoveAll(h => h.Handler.Equals(handler));

            if (handlers.Count == 0)
                _syncHandlers.Remove(eventType);

            _logger.LogDebug($"Unsubscribed from event {eventType.Name}");
        }

        /// <summary>
        /// Публікує подію для синхронної обробки
        /// </summary>
        public void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);

            _logger.LogDebug($"Publishing event {eventType.Name} with ID {eventData.GetEventId()} " +
                           $"and priority {eventData.GetPriority()}");

            if (_syncHandlers.TryGetValue(eventType, out var handlers))
            {
                // Виконуємо обробники в порядку пріоритету
                foreach (var handler in handlers)
                {
                    try
                    {
                        ((Action<TEvent>)handler.Handler).Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error handling event {eventType.Name}: {ex.Message}",
                                       "EventBus", ex);
                    }
                }
            }

            // Повертаємо подію в пул
            _eventPool.Return(eventData);
        }

        #endregion

        #region Async Methods

        /// <summary>
        /// Підписується на асинхронну обробку події
        /// </summary>
        public void SubscribeAsync<TEvent>(Func<TEvent, UniTask> handler, EventPriority priority = EventPriority.Normal)
            where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);

            if (!_asyncHandlers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<AsyncEventHandler>();
                _asyncHandlers[eventType] = handlers;
            }

            handlers.Add(new AsyncEventHandler(handler, priority));

            // Сортуємо обробники за пріоритетом (високий пріоритет першим)
            _asyncHandlers[eventType] = handlers.OrderByDescending(h => h.Priority).ToList();

            _logger.LogDebug($"Subscribed async to event {eventType.Name} with priority {priority}");
        }

        /// <summary>
        /// Відписується від асинхронної обробки події
        /// </summary>
        public void UnsubscribeAsync<TEvent>(Func<TEvent, UniTask> handler) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);

            if (!_asyncHandlers.TryGetValue(eventType, out var handlers))
                return;

            // Знаходимо і видаляємо обробник
            handlers.RemoveAll(h => h.Handler.Equals(handler));

            if (handlers.Count == 0)
                _asyncHandlers.Remove(eventType);

            _logger.LogDebug($"Unsubscribed async from event {eventType.Name}");
        }

        /// <summary>
        /// Публікує подію для асинхронної обробки
        /// </summary>
        public async UniTask PublishAsync<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);

            _logger.LogDebug($"Publishing async event {eventType.Name} with ID {eventData.GetEventId()} " +
                           $"and priority {eventData.GetPriority()}");

            if (_asyncHandlers.TryGetValue(eventType, out var handlers))
            {
                // Список задач для очікування
                List<UniTask> tasks = new List<UniTask>();

                // Запускаємо обробники в порядку пріоритету
                foreach (var handler in handlers)
                {
                    try
                    {
                        tasks.Add(((Func<TEvent, UniTask>)handler.Handler).Invoke(eventData));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error handling async event {eventType.Name}: {ex.Message}",
                                       "EventBus", ex);
                    }
                }

                // Чекаємо завершення всіх задач
                await UniTask.WhenAll(tasks);
            }

            // Повертаємо подію в пул
            _eventPool.Return(eventData);
        }

        #endregion

        /// <summary>
        /// Очищає всі підписки
        /// </summary>
        public void Clear()
        {
            _syncHandlers.Clear();
            _asyncHandlers.Clear();
            _logger.LogInfo("EventBus cleared");
        }
    }
}
