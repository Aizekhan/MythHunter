// Шлях: Assets/_MythHunter/Code/Events/EventBus.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using MythHunter.Utils.Extensions;
using MythHunter.Events.Domain.Lobby;

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
        private readonly Dictionary<Type, Func<IEvent, UniTask>> _asyncDispatchers = new Dictionary<Type, Func<IEvent, UniTask>>();

        // Додайте кешування подій без обробників
        private readonly Dictionary<Type, List<IEvent>> _pendingEvents = new Dictionary<Type, List<IEvent>>();
        private readonly HashSet<Type> _monitoredEventTypes = new HashSet<Type>();

        // EPIC 08: Cache MethodInfo for reflection calls to avoid repeated GetMethod() calls
        private readonly Dictionary<Type, System.Reflection.MethodInfo> _returnToPoolMethods = new Dictionary<Type, System.Reflection.MethodInfo>();
        private readonly Dictionary<Type, Action<object, IEvent>> _compiledSyncHandlers = new Dictionary<Type, Action<object, IEvent>>();
        private readonly Dictionary<Type, Func<object, IEvent, UniTask>> _compiledAsyncHandlers = new Dictionary<Type, Func<object, IEvent, UniTask>>();

        // EPIC 08 Phase 3: Memory leak prevention - limit pending events
        private const int MAX_PENDING_EVENTS = 10000;

        private readonly IEventPool _eventPool;
        private readonly IMythLogger _logger;

        // Черги для пріоритетної обробки подій
        private readonly Queue<EventQueueItem> _highPriorityQueue = new Queue<EventQueueItem>();
        private readonly Queue<EventQueueItem> _normalPriorityQueue = new Queue<EventQueueItem>();
        private readonly Queue<EventQueueItem> _lowPriorityQueue = new Queue<EventQueueItem>();

        // Управління асинхронною обробкою
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isProcessing;
        public bool IsProcessing => _isProcessing;
        private readonly object _syncLock = new object();

        private static readonly HashSet<Type> _logExcludedEvents = new(){
                                                        typeof(SelectionTimerUpdatedEvent),
                                                        // typeof(AnotherSpamEvent),
                                                                    };
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
            public Type EventType;
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
            // EPIC 08: Now accepts external cancellation token
            StartProcessingAsync(CancellationToken.None).Forget();
            RegisterEventProcessors();
        }

        #region Sync Methods

        /// <summary>
        /// Підписується на синхронну обробку події
        /// EPIC 08 Phase 3: Added duplicate subscription detection
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

            // Додаємо обробник в список
            if (!_syncHandlers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<SyncEventHandler>();
                _syncHandlers[eventType] = handlers;
            }

            // EPIC 08 Phase 3: Check for duplicate subscription
            bool isDuplicate = handlers.Any(h => h.Handler.Equals(handler));
            if (isDuplicate)
            {
                _logger.LogWarning($"Handler already subscribed to {eventType.Name}. Skipping duplicate subscription.", "EventBus");
                return;
            }

            handlers.Add(new SyncEventHandler(handler, priority));

            // Сортуємо обробники за пріоритетом (високий пріоритет першим)
            _syncHandlers[eventType] = handlers.OrderByDescending(h => h.Priority).ToList();

            // Оновлюємо кешований процесор для цього типу події
            RegisterProcessor(eventType);

            _logger.LogDebug($"Subscribed to event {eventType.Name} with priority {priority}");

            // Перевірка відкладених подій для нового підписника
            if (_pendingEvents.TryGetValue(eventType, out var pendingEvents) && pendingEvents.Count > 0)
            {
                _logger.LogInfo($"Processing {pendingEvents.Count} pending events of type {eventType.Name}", "EventBus");

                foreach (var evt in pendingEvents.ToList())
                {
                    if (evt is TEvent typedEvent)
                    {
                        try
                        {
                            handler(typedEvent);
                            pendingEvents.Remove(evt);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing pending event {typedEvent.GetEventId()}: {ex.Message}", "EventBus", ex);
                        }
                    }
                }
            }
        }

        // Оновити кеш процесорів для типу події
        // EPIC 08 Optimization: Use compiled delegates instead of DynamicInvoke
        private void RegisterProcessor(Type eventType)
        {
            // Create compiled delegate that invokes handlers without DynamicInvoke
            _eventProcessors[eventType] = (evt) =>
            {
                if (_syncHandlers.TryGetValue(eventType, out var handlersToCall))
                {
                    foreach (var handler in handlersToCall)
                    {
                        try
                        {
                            // Use direct invocation through typed Action<T>
                            // This is faster than DynamicInvoke as it avoids boxing and reflection overhead
                            InvokeSyncHandler(handler.Handler, evt, eventType);
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
        /// EPIC 08: Optimized sync handler invocation without DynamicInvoke
        /// Uses compiled expression trees for 10-20x performance improvement
        /// </summary>
        private void InvokeSyncHandler(Delegate handler, IEvent evt, Type eventType)
        {
            // Get or create compiled delegate for this event type
            if (!_compiledSyncHandlers.TryGetValue(eventType, out var compiledHandler))
            {
                // Create a compiled delegate using expression trees (one-time cost)
                var eventParam = System.Linq.Expressions.Expression.Parameter(typeof(IEvent), "evt");
                var handlerParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "handler");

                var convertedEvent = System.Linq.Expressions.Expression.Convert(eventParam, eventType);
                var convertedHandler = System.Linq.Expressions.Expression.Convert(handlerParam, handler.GetType());

                var invokeMethod = handler.GetType().GetMethod("Invoke");
                var invokeCall = System.Linq.Expressions.Expression.Call(convertedHandler, invokeMethod, convertedEvent);

                compiledHandler = System.Linq.Expressions.Expression.Lambda<Action<object, IEvent>>(
                    invokeCall, handlerParam, eventParam).Compile();

                _compiledSyncHandlers[eventType] = compiledHandler;
            }

            // Invoke the compiled delegate (very fast, no reflection)
            compiledHandler(handler, evt);
        }

        /// <summary>
        /// EPIC 08: Optimized async handler invocation without DynamicInvoke
        /// Uses compiled expression trees for 10-20x performance improvement
        /// </summary>
        private UniTask InvokeAsyncHandler(Delegate handler, IEvent evt, Type eventType)
        {
            // Get or create compiled delegate for this event type
            if (!_compiledAsyncHandlers.TryGetValue(eventType, out var compiledHandler))
            {
                // Create a compiled delegate using expression trees (one-time cost)
                var eventParam = System.Linq.Expressions.Expression.Parameter(typeof(IEvent), "evt");
                var handlerParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "handler");

                var convertedEvent = System.Linq.Expressions.Expression.Convert(eventParam, eventType);
                var convertedHandler = System.Linq.Expressions.Expression.Convert(handlerParam, handler.GetType());

                var invokeMethod = handler.GetType().GetMethod("Invoke");
                var invokeCall = System.Linq.Expressions.Expression.Call(convertedHandler, invokeMethod, convertedEvent);

                compiledHandler = System.Linq.Expressions.Expression.Lambda<Func<object, IEvent, UniTask>>(
                    invokeCall, handlerParam, eventParam).Compile();

                _compiledAsyncHandlers[eventType] = compiledHandler;
            }

            // Invoke the compiled delegate (very fast, no reflection)
            return compiledHandler(handler, evt);
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
        // Метод публікації з кешуванням подій без обробників
        public virtual void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            // Перевірка на дефолтне значення структури
            if (EqualityComparer<TEvent>.Default.Equals(eventData, default))
            {
                _logger.LogWarning($"Trying to publish default event of type {typeof(TEvent).Name}", "EventBus");
                return;
            }

            Type eventType = typeof(TEvent);

            // Перевірка наявності обробників для цього типу подій
            bool hasHandlers = HasHandlersForEvent<TEvent>();

            if (!hasHandlers && _monitoredEventTypes.Contains(eventType))
            {
                // Якщо немає обробників, але тип події моніториться - зберігаємо для відкладеної обробки
                if (!_pendingEvents.TryGetValue(eventType, out var events))
                {
                    events = new List<IEvent>();
                    _pendingEvents[eventType] = events;
                }

                // EPIC 08 Phase 3: Prevent memory leak by limiting pending events
                if (events.Count >= MAX_PENDING_EVENTS)
                {
                    _logger.LogWarning($"Pending events limit ({MAX_PENDING_EVENTS}) reached for {eventType.Name}. Dropping oldest event.", "EventBus");
                    events.RemoveAt(0);
                }

                events.Add(eventData);
                _logger.LogDebug($"Event {eventType.Name} with ID {eventData.GetEventId()} cached until handlers are registered ({events.Count}/{MAX_PENDING_EVENTS})", "EventBus");
                return;
            }

            if (!_logExcludedEvents.Contains(typeof(TEvent)))
            {
                _logger.LogDebug($"Publishing event {eventType.Name} with ID {eventData.GetEventId()} and priority {eventData.GetPriority()}");
            }

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
                        _logger.LogError($"Error handling event {eventType.Name}: {ex.Message}", "EventBus", ex);
                    }
                }
            }

            // Повертаємо подію в пул (виправлення помилки CS0453)
            if (!EqualityComparer<TEvent>.Default.Equals(eventData, default))
            {
                _eventPool.Return(eventData);
            }
        }

        #endregion

        #region Async Methods

        /// <summary>
        /// Підписується на асинхронну обробку події
        /// EPIC 08 Phase 3: Added duplicate subscription detection
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

            // EPIC 08 Phase 3: Check for duplicate subscription
            bool isDuplicate = handlers.Any(h => h.Handler.Equals(handler));
            if (isDuplicate)
            {
                _logger.LogWarning($"Async handler already subscribed to {type.Name}. Skipping duplicate subscription.", "EventBus");
                return;
            }

            handlers.Add(new AsyncEventHandler(handler, priority));
            _asyncHandlers[type] = handlers.OrderByDescending(h => h.Priority).ToList();

            // Додаємо в кеш універсальну обгортку
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

            _logger.LogDebug($"Publishing async event {eventType.Name} with ID {eventData.GetEventId()} and priority {eventData.GetPriority()}");
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
                    await UniTask.WhenAll(tasks).WithExceptionHandler(ex =>
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
            if (!EqualityComparer<TEvent>.Default.Equals(eventData, default))
            {
                _eventPool.Return(eventData);
            }
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
                    EventType = typeof(TEvent),
                    IsAsync = false,
                    Priority = priority
                });
            }
            if (!_logExcludedEvents.Contains(typeof(TEvent)))
            {
                _logger.LogDebug($"Enqueued event {typeof(TEvent).Name} with ID {eventData.GetEventId()} and priority {priority}");
            }
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
                    EventType = typeof(TEvent),
                    IsAsync = true,
                    Priority = priority
                });
            }

            _logger.LogDebug($"Enqueued async event {typeof(TEvent).Name} with ID {eventData.GetEventId()} and priority {priority}");
        }

        /// <summary>
        /// Асинхронно обробляє чергу подій
        /// </summary>
        /// <summary>
        /// EPIC 08: Added CancellationToken parameter for external cancellation
        /// </summary>
        private async UniTaskVoid StartProcessingAsync(CancellationToken externalToken = default)
        {
            _isProcessing = true;

            // EPIC 08: Create linked token source to combine internal and external cancellation
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                _cancellationTokenSource.Token, externalToken);
            var combinedToken = linkedCts.Token;

            try
            {
                while (!combinedToken.IsCancellationRequested)
                {
                    bool processedAny = false;

                    // Обробляємо події високого пріоритету
                    processedAny |= await ProcessQueueAsync(_highPriorityQueue, combinedToken);

                    // Якщо є ще час, обробляємо події нормального пріоритету
                    if (!processedAny)
                        processedAny |= await ProcessQueueAsync(_normalPriorityQueue, combinedToken);

                    // Якщо є ще час, обробляємо події низького пріоритету
                    if (!processedAny)
                        processedAny |= await ProcessQueueAsync(_lowPriorityQueue, combinedToken);

                    // Якщо не було оброблено жодної події, чекаємо перед наступною спробою
                    if (!processedAny)
                    {
                        await UniTask.Delay(10, cancellationToken: combinedToken);
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
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        private async UniTask<bool> ProcessQueueAsync(Queue<EventQueueItem> queue, CancellationToken cancellationToken = default)
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
                    // EPIC 08 Optimization: Avoid DynamicInvoke for async handlers
                    if (_asyncHandlers.TryGetValue(item.EventType, out var asyncHandlers))
                    {
                        var tasks = new List<UniTask>();
                        foreach (var handler in asyncHandlers)
                        {
                            try
                            {
                                // Use compiled async handler instead of DynamicInvoke
                                UniTask task = InvokeAsyncHandler(handler.Handler, item.Event, item.EventType);
                                tasks.Add(task);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error in async handler for {item.EventType.Name}: {ex.Message}", "EventBus", ex);
                            }
                        }

                        try
                        {
                            // EPIC 08: Pass cancellation token to WhenAll
                            await UniTask.WhenAll(tasks).AttachExternalCancellation(cancellationToken);
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

                // Повертаємо подію в пул (виправлення помилки CS0453)
                if (item.Event != null)
                {
                    try
                    {
                        ReturnToPool(item.Event, item.EventType);
                    }
                    catch (Exception)
                    {
                        // Ігноруємо помилки при поверненні події в пул
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
        /// Безпечно повертає подію в пул з врахуванням типу
        /// EPIC 08 Optimization: Cache MethodInfo to avoid repeated reflection calls (10-20x performance improvement)
        /// </summary>
        private void ReturnToPool(IEvent eventData, Type eventType)
        {
            // Check cache first
            if (!_returnToPoolMethods.TryGetValue(eventType, out var method))
            {
                // Create and cache MethodInfo for this event type
                method = typeof(IEventPool).GetMethod("Return").MakeGenericMethod(eventType);
                _returnToPoolMethods[eventType] = method;
            }

            // Invoke cached method (still uses reflection, but only GetMethod once per type)
            method.Invoke(_eventPool, new object[] { eventData });
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
            // Виправляємо помилку CS1593, використовуючи правильну сигнатуру для Action
            RegisterEventProcessor<GameStartedEvent>((evt) => ProcessEventImmediately(evt));
            RegisterEventProcessor<GamePausedEvent>((evt) => ProcessEventImmediately(evt));
            RegisterEventProcessor<GameEndedEvent>((evt) => ProcessEventImmediately(evt));
            // Додайте інші типи подій за потреби
        }
        #endregion
        // Додати метод для моніторингу типів подій
        public void MonitorEventType<TEvent>() where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);
            _monitoredEventTypes.Add(eventType);

            if (!_pendingEvents.ContainsKey(eventType))
            {
                _pendingEvents[eventType] = new List<IEvent>();
            }
        }
        // Додати метод перевірки наявності обробників
        private bool HasHandlersForEvent<TEvent>() where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);
            return (_syncHandlers.TryGetValue(eventType, out var syncHandlers) && syncHandlers.Count > 0) ||
                   (_asyncHandlers.TryGetValue(eventType, out var asyncHandlers) && asyncHandlers.Count > 0);
        }
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
