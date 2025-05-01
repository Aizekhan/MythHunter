// Шлях: Assets/_MythHunter/Code/Events/AsyncEventQueue.cs

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Асинхронна черга подій з обробкою в окремому потоці
    /// </summary>
    public class AsyncEventQueue : IEventQueue, IDisposable
    {
        private readonly Queue<EventQueueItem> _highPriorityQueue = new Queue<EventQueueItem>();
        private readonly Queue<EventQueueItem> _normalPriorityQueue = new Queue<EventQueueItem>();
        private readonly Queue<EventQueueItem> _lowPriorityQueue = new Queue<EventQueueItem>();

        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly IEventPool _eventPool;

        private bool _isProcessing;
        private CancellationTokenSource _cancellationTokenSource;

        private struct EventQueueItem
        {
            public IEvent Event;
            public bool IsAsync;
            public EventPriority Priority;
        }

        [MythHunter.Core.DI.Inject]
        public AsyncEventQueue(IEventBus eventBus, IMythLogger logger, IEventPool eventPool)
        {
            _eventBus = eventBus;
            _logger = logger;
            _eventPool = eventPool;
            _isProcessing = false;
            _cancellationTokenSource = new CancellationTokenSource();

            // Запускаємо асинхронну обробку в окремому потоці
            StartProcessingAsync().Forget();
        }

        /// <summary>
        /// Додає подію в чергу для синхронної обробки
        /// </summary>
        public void Enqueue<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            var priority = eventData.GetPriority();

            lock (GetQueueForPriority(priority))
            {
                GetQueueForPriority(priority).Enqueue(new EventQueueItem
                {
                    Event = eventData,
                    IsAsync = false,
                    Priority = priority
                });
            }

            _logger.LogDebug($"Enqueued event {typeof(TEvent).Name} with ID {eventData.GetEventId()} and priority {priority}");
        }

        /// <summary>
        /// Додає подію в чергу для асинхронної обробки
        /// </summary>
        public void EnqueueAsync<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            var priority = eventData.GetPriority();

            lock (GetQueueForPriority(priority))
            {
                GetQueueForPriority(priority).Enqueue(new EventQueueItem
                {
                    Event = eventData,
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
                    processedAny |= await ProcessQueueAsync(_normalPriorityQueue);

                    // Якщо є ще час, обробляємо події низького пріоритету
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
                _logger.LogError($"Error in async event processing: {ex.Message}", "EventQueue", ex);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        /// <summary>
        /// Обробляє одну подію з черги
        /// </summary>
        private async UniTask<bool> ProcessQueueAsync(Queue<EventQueueItem> queue)
        {
            EventQueueItem item;

            lock (queue)
            {
                if (queue.Count == 0)
                {
                    return false;
                }

                item = queue.Dequeue();
            }

            // Обробляємо подію синхронно або асинхронно
            try
            {
                if (item.IsAsync)
                {
                    // Використовуємо рефлексію для виклику PublishAsync з правильним типом
                    var eventType = item.Event.GetType();
                    var method = typeof(IEventBus).GetMethod("PublishAsync").MakeGenericMethod(eventType);
                    await (UniTask)method.Invoke(_eventBus, new object[] { item.Event });
                }
                else
                {
                    // Використовуємо рефлексію для виклику Publish з правильним типом
                    var eventType = item.Event.GetType();
                    var method = typeof(IEventBus).GetMethod("Publish").MakeGenericMethod(eventType);
                    method.Invoke(_eventBus, new object[] { item.Event });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing event: {ex.Message}", "EventQueue", ex);
                return true; // Повертаємо true, щоб не затримуватись на помилці
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

        /// <summary>
        /// Очищає чергу подій
        /// </summary>
        public void Clear()
        {
            lock (_highPriorityQueue)
            {
                _highPriorityQueue.Clear();
            }

            lock (_normalPriorityQueue)
            {
                _normalPriorityQueue.Clear();
            }

            lock (_lowPriorityQueue)
            {
                _lowPriorityQueue.Clear();
            }

            _logger.LogInfo("Event queue cleared");
        }

        /// <summary>
        /// Звільняє ресурси
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
