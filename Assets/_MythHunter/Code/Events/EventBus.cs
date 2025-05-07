// Шлях: Assets/_MythHunter/Code/Events/EventBus.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using MythHunter.Utils.Extensions;

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
            if (handler == null)
            {
                _logger.LogWarning($"Trying to subscribe null handler for event {typeof(TEvent).Name}", "EventBus");
                return;
            }

            Type eventType = typeof(TEvent);

            if (!_syncHandlers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<SyncEventHandler>();
                _syncHandlers[eventType] = handlers;
            }

            handlers.Add(new SyncEventHandler(handler, priority));

            // Сортуємо обробники за пріоритетом (високий пріоритет першим)
            _syncHandlers[eventType] = handlers.OrderByDescending(h => h.Priority).ToList();

            // Оновлюємо кешований процесор для цього типу події
            // Зберігаємо оптимізований делегат, який викликає всі обробники за пріоритетами
            RegisterProcessor(eventType);

            _logger.LogDebug($"Subscribed to event {eventType.Name} with priority {priority}");
        }

        // Оновити кеш процесорів для типу події
        private void RegisterProcessor(Type eventType)
        {
            // Створюємо динамічний делегат, який викликатиме всі обробники за порядком пріоритету
            _eventProcessors[eventType] = (evt) =>
            {
                if (_syncHandlers.TryGetValue(eventType, out var handlersToCall))
                {
                    foreach (var handler in handlersToCall)
                    {
                        try
                        {
                            // Викликаємо обробник через Delegate.DynamicInvoke для уникнення рефлексії на гарячому шляху
                            handler.Handler.DynamicInvoke(evt);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error handling event {eventType.Name}: {ex.Message}", "EventBus", ex);
                        }
                    }
                }
            };
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
            // Перевірка на дефолтне значення структури (не є null, оскільки це структура, але може бути default)
            if (EqualityComparer<TEvent>.Default.Equals(eventData, default))
            {
                _logger.LogWarning($"Trying to publish default event of type {typeof(TEvent).Name}", "EventBus");
                return;
            }
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
            // Перевірка на default значення
            if (EqualityComparer<TEvent>.Default.Equals(eventData, default))
            {
                _logger.LogWarning($"Trying to publish default async event of type {typeof(TEvent).Name}", "EventBus");
                return;
            }
            Type eventType = typeof(TEvent);

            _logger.LogDebug($"Publishing async event {eventType.Name} with ID {eventData.GetEventId()} " +
                           $"and priority {eventData.GetPriority()}");
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError($"Error publishing async event {eventType.Name}: {ex.Message}", "EventBus", ex);
            }
         }

        /// <summary>
        /// Негайно обробляє асинхронну подію, минаючи чергу
        /// </summary>
        private async UniTask ProcessEventImmediatelyAsync<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);

            // Сповіщаємо дебаг-обробники
            try
            {
                DebugEventMiddleware.NotifyHandlers(eventData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error in debug handlers for event {eventType.Name}: {ex.Message}", "EventBus");
                // Продовжуємо обробку, оскільки дебаг-обробники не критичні
            }

            if (_asyncHandlers.TryGetValue(eventType, out var handlers))
            {
                // Список задач для очікування
                List<UniTask> tasks = new List<UniTask>();
                List<Exception> exceptions = new List<Exception>();

                // Запускаємо обробники в порядку пріоритету
                foreach (var handler in handlers)
                {
                    try
                    {
                        // Використовуємо SafeFireAndForget замість простого додавання до списку
                        var task = ((Func<TEvent, UniTask>)handler.Handler).Invoke(eventData);
                        tasks.Add(task);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                        _logger.LogError($"Error starting async handler for event {eventType.Name}: {ex.Message}", "EventBus", ex);
                    }
                }

                // Безпечне очікування всіх задач
                try
                {
                    // Використання WithCancellation для можливості скасування за потреби
                    await UniTask.WhenAll(tasks).AttachExceptionHandler(ex =>
                    {
                        exceptions.Add(ex);
                        _logger.LogError($"Error in async event handler: {ex.Message}", "EventBus", ex);
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error waiting for async handlers of event {eventType.Name}: {ex.Message}", "EventBus", ex);
                }

                // Логування всіх помилок
                if (exceptions.Count > 0)
                {
                    _logger.LogWarning($"Event {eventType.Name} processed with {exceptions.Count} errors", "EventBus");
                }
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
                    // Для асинхронних подій
                    if (_asyncHandlers.TryGetValue(item.EventType, out var asyncHandlers))
                    {
                        var tasks = new List<UniTask>();
                        foreach (var handler in asyncHandlers)
                        {
                            try
                            {
                                // Використовуємо безпечне виконання асинхронних обробників
                                UniTask task = (UniTask)handler.Handler.DynamicInvoke(item.Event);
                                tasks.Add(task);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error in async handler for {item.EventType.Name}: {ex.Message}", "EventBus", ex);
                            }
                        }

                        try
                        {
                            // Безпечне очікування всіх задач
                            await UniTask.WhenAll(tasks);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error awaiting handlers for {item.EventType.Name}: {ex.Message}", "EventBus", ex);
                        }
                    }
                }
                else
                {
                    // Для синхронних подій використовуємо кешований процесор
                    if (_eventProcessors.TryGetValue(item.EventType, out var processor))
                    {
                        processor(item.Event);
                    }
                    else
                    {
                        _logger.LogWarning($"No processor registered for event type: {item.EventType.Name}", "EventBus");
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
        // Запасний метод обробки, якщо немає кешованого делегата
        private void ProcessEventFallback(IEvent eventData, Type eventType)
        {
            // Використовуємо рефлексію лише у крайньому випадку
            if (_syncHandlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        var method = handler.Handler.GetType().GetMethod("Invoke");
                        method.Invoke(handler.Handler, new object[] { eventData });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in fallback event handling: {ex.Message}", "EventBus");
                    }
                }
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
        // Викликати в конструкторі або в окремому методі
        private void RegisterCommonEventProcessors()
        {
            // Реєструємо найчастіші типи подій
            RegisterEventProcessor<GameStartedEvent>((evt) => ProcessEventImmediately(evt));
            RegisterEventProcessor<GamePausedEvent>((evt) => ProcessEventImmediately(evt));
            RegisterEventProcessor<GameEndedEvent>((evt) => ProcessEventImmediately(evt));
            RegisterEventProcessor<PhaseChangedEvent>((evt) => ProcessEventImmediately(evt));
            // Додайте інші часто використовувані типи
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
