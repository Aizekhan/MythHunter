// Шлях: Assets/_MythHunter/Code/Events/EventBus.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Реалізація шини подій з підтримкою синхронної та асинхронної обробки
    /// та оптимізованою обробкою пріоритетів
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<SyncEventHandler>> _syncHandlers = new Dictionary<Type, List<SyncEventHandler>>();
        private readonly Dictionary<Type, List<AsyncEventHandler>> _asyncHandlers = new Dictionary<Type, List<AsyncEventHandler>>();
        // Додаємо делегат для обробки подій різних типів
        private readonly Dictionary<Type, Action<IEvent>> _eventProcessors = new Dictionary<Type, Action<IEvent>>();
        private readonly Dictionary<Type, Func<IEvent, UniTask>> _asyncDispatchers = new();

        private readonly IEventPool _eventPool;
        private readonly IMythLogger _logger;

        // Черги для пріоритетної обробки подій
        private readonly Queue<EventQueueItem> _highPriorityQueue = new Queue<EventQueueItem>();
        private readonly Queue<EventQueueItem> _normalPriorityQueue = new Queue<EventQueueItem>();
        private readonly Queue<EventQueueItem> _lowPriorityQueue = new Queue<EventQueueItem>();

        // Управління асинхронною обробкою
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isProcessing;
        private readonly object _syncLock = new object();


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

        // Структура елемента черги подій
        private struct EventQueueItem
        {
            public IEvent Event;
            public Type EventType;  // Додали це поле
            public bool IsAsync;
            public EventPriority Priority;
        }

        [Inject]
        public EventBus(IEventPool eventPool, IMythLogger logger)
        {
            _eventPool = eventPool;
            _logger = logger;

            // Ініціалізація асинхронної обробки
            _cancellationTokenSource = new CancellationTokenSource();
            StartProcessingAsync().Forget();
            RegisterEventProcessors();
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

            // Для високопріоритетних подій виконуємо обробку відразу
            if (eventData.GetPriority() == EventPriority.Critical)
            {
                ProcessEventImmediately(eventData);
            }
            else
            {
                // Інакше додаємо до відповідної черги за пріоритетом
                Enqueue(eventData);
            }
        }

        /// <summary>
        /// Негайно обробляє подію, минаючи чергу
        /// </summary>
        private void ProcessEventImmediately<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);

            // Сповіщаємо дебаг-обробники
            DebugEventMiddleware.NotifyHandlers(eventData);

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
            var type = typeof(TEvent);

            if (!_asyncHandlers.TryGetValue(type, out var handlers))
            {
                handlers = new List<AsyncEventHandler>();
                _asyncHandlers[type] = handlers;
            }

            handlers.Add(new AsyncEventHandler(handler, priority));
            _asyncHandlers[type] = handlers.OrderByDescending(h => h.Priority).ToList();

            // 🧠 Додаємо в кеш універсальну обгортку
            _asyncDispatchers[type] = async (evt) => await handler((TEvent)evt);

            _logger.LogDebug($"Subscribed async to event {type.Name} with priority {priority}");
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

            // Для критичних подій виконуємо обробку відразу
            if (eventData.GetPriority() == EventPriority.Critical)
            {
                await ProcessEventImmediatelyAsync(eventData);
            }
            else
            {
                // Додаємо до черги асинхронних подій
                EnqueueAsync(eventData);
            }
        }

        /// <summary>
        /// Негайно обробляє асинхронну подію, минаючи чергу
        /// </summary>
        private async UniTask ProcessEventImmediatelyAsync<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);

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

        #region Queue Methods

        /// <summary>
        /// Додає подію в чергу для синхронної обробки
        /// </summary>
        private void Enqueue<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            var priority = eventData.GetPriority();

            lock (_syncLock)
            {
                GetQueueForPriority(priority).Enqueue(new EventQueueItem
                {
                    Event = eventData,
                    EventType = typeof(TEvent),  // Додали типізацію
                    IsAsync = false,
                    Priority = priority
                });
            }

            _logger.LogDebug($"Enqueued event {typeof(TEvent).Name} with ID {eventData.GetEventId()} and priority {priority}");
        }

        /// <summary>
        /// Додає подію в чергу для асинхронної обробки
        /// </summary>
        private void EnqueueAsync<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            var priority = eventData.GetPriority();

            lock (_syncLock)
            {
                GetQueueForPriority(priority).Enqueue(new EventQueueItem
                {
                    Event = eventData,
                    EventType = typeof(TEvent),  // Додали типізацію
                    IsAsync = true,
                    Priority = priority
                });
            }

            _logger.LogDebug($"Enqueued async event {typeof(TEvent).Name} with ID {eventData.GetEventId()} and priority {priority}");
        }

        /// <summary>
        /// Асинхронно обробляє чергу подій
        /// </summary>
        private async UniTaskVoid StartProcessingAsync()
        {
            _isProcessing = true;

            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    bool processedAny = false;

                    // Обробляємо події високого пріоритету
                    processedAny |= await ProcessQueueAsync(_highPriorityQueue);

                    // Якщо є ще час, обробляємо події нормального пріоритету
                    if (!processedAny)
                        processedAny |= await ProcessQueueAsync(_normalPriorityQueue);

                    // Якщо є ще час, обробляємо події низького пріоритету
                    if (!processedAny)
                        processedAny |= await ProcessQueueAsync(_lowPriorityQueue);

                    // Якщо не було оброблено жодної події, чекаємо перед наступною спробою
                    if (!processedAny)
                    {
                        await UniTask.Delay(10, cancellationToken: _cancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Обробка скасування операції
                _logger.LogInfo("Async event processing cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in async event processing: {ex.Message}", "EventBus", ex);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        /// <summary>
        /// Обробляє одну подію з черги
        /// </summary>
        // Модифікація методу ProcessQueueAsync - видаляємо рефлексію
        private async UniTask<bool> ProcessQueueAsync(Queue<EventQueueItem> queue)
        {
            EventQueueItem item;

            lock (_syncLock)
            {
                if (queue.Count == 0)
                    return false;

                item = queue.Dequeue();
            }

            try
            {
                if (item.IsAsync)
                {
                    // Обробка асинхронних подій через кешований делегат
                    if (_asyncDispatchers.TryGetValue(item.EventType, out var dispatcher))
                    {
                        await dispatcher(item.Event);
                    }
                    else
                    {
                        _logger.LogWarning($"No async dispatcher found for {item.EventType.Name}");
                    }
                }
                else
                {
                    // Обробка синхронних подій через зареєстрований процесор
                    if (_eventProcessors.TryGetValue(item.EventType, out var processor))
                    {
                        processor(item.Event);
                    }
                    else
                    {
                        _logger.LogWarning($"No processor registered for event type: {item.EventType.Name}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing event of type {item.EventType.Name}: {ex.Message}", "EventBus", ex);
                return true; // подію вважаємо "обробленою", щоб не блокувати чергу
            }
        }

        /// <summary>
        /// Повертає чергу для вказаного пріоритету
        /// </summary>
        private Queue<EventQueueItem> GetQueueForPriority(EventPriority priority)
        {
            switch (priority)
            {
                case EventPriority.Critical:
                case EventPriority.High:
                    return _highPriorityQueue;
                case EventPriority.Normal:
                    return _normalPriorityQueue;
                case EventPriority.Low:
                default:
                    return _lowPriorityQueue;
            }
        }

        // Метод для реєстрації обробника подій конкретного типу
        private void RegisterEventProcessor<T>(Action<T> processor) where T : struct, IEvent
        {
            _eventProcessors[typeof(T)] = (evt) => processor((T)evt);
        }
       
        private void RegisterEventProcessors()
        {
            // Використовуємо метод для реєстрації обробників
            RegisterEventProcessor<GameStartedEvent>((evt) => ProcessEventImmediately(evt));
            RegisterEventProcessor<GamePausedEvent>((evt) => ProcessEventImmediately(evt));
            RegisterEventProcessor<GameEndedEvent>((evt) => ProcessEventImmediately(evt));
            // Додайте інші типи подій за потреби
        }
        #endregion

        /// <summary>
        /// Очищає всі підписки
        /// </summary>
        public void Clear()
        {
            lock (_syncLock)
            {
                _syncHandlers.Clear();
                _asyncHandlers.Clear();
                _highPriorityQueue.Clear();
                _normalPriorityQueue.Clear();
                _lowPriorityQueue.Clear();
            }

            _logger.LogInfo("EventBus cleared");
        }

        /// <summary>
        /// Звільняє ресурси
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            Clear();
        }
    }
}
