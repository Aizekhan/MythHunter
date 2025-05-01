using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Оптимізований обробник подій з підтримкою пріоритетів
    /// </summary>
    public class PrioritizedEventProcessing : IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        // Черги подій за пріоритетами
        private readonly Queue<IEvent> _criticalEvents = new Queue<IEvent>();
        private readonly Queue<IEvent> _highEvents = new Queue<IEvent>();
        private readonly Queue<IEvent> _normalEvents = new Queue<IEvent>();
        private readonly Queue<IEvent> _lowEvents = new Queue<IEvent>();

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isProcessing;

        private readonly object _syncLock = new object();

        public PrioritizedEventProcessing(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();

            // Запускаємо обробку
            StartProcessingAsync().Forget();
        }

        /// <summary>
        /// Додає подію у чергу за пріоритетом
        /// </summary>
        public void EnqueueEvent(IEvent eventData)
        {
            if (eventData == null)
                return;

            var priority = eventData.GetPriority();

            lock (_syncLock)
            {
                switch (priority)
                {
                    case EventPriority.Critical:
                        _criticalEvents.Enqueue(eventData);
                        break;
                    case EventPriority.High:
                        _highEvents.Enqueue(eventData);
                        break;
                    case EventPriority.Normal:
                        _normalEvents.Enqueue(eventData);
                        break;
                    case EventPriority.Low:
                        _lowEvents.Enqueue(eventData);
                        break;
                }
            }
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

                    // Обробка від високих пріоритетів до низьких
                    processedAny |= await ProcessNextEventAsync(_criticalEvents);

                    if (!processedAny)
                        processedAny |= await ProcessNextEventAsync(_highEvents);

                    if (!processedAny)
                        processedAny |= await ProcessNextEventAsync(_normalEvents);

                    if (!processedAny)
                        processedAny |= await ProcessNextEventAsync(_lowEvents);

                    // Якщо немає подій для обробки, чекаємо трохи
                    if (!processedAny)
                    {
                        await UniTask.Delay(10, cancellationToken: _cancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInfo("Event processing cancelled", "Events");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in event processing: {ex.Message}", "Events", ex);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        /// <summary>
        /// Обробляє наступну подію з черги
        /// </summary>
        private async UniTask<bool> ProcessNextEventAsync(Queue<IEvent> queue)
        {
            IEvent eventToProcess = null;

            lock (_syncLock)
            {
                if (queue.Count > 0)
                {
                    eventToProcess = queue.Dequeue();
                }
            }

            if (eventToProcess == null)
                return false;

            try
            {
                // Використовуємо рефлексію для виклику типізованого методу
                var eventType = eventToProcess.GetType();
                var method = typeof(IEventBus).GetMethod("Publish").MakeGenericMethod(eventType);
                method.Invoke(_eventBus, new object[] { eventToProcess });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing event {eventToProcess.GetType().Name}: {ex.Message}", "Events", ex);
                return true; // щоб не затримувалась обробка на помилці
            }
        }

        /// <summary>
        /// Очищає всі черги
        /// </summary>
        public void Clear()
        {
            lock (_syncLock)
            {
                _criticalEvents.Clear();
                _highEvents.Clear();
                _normalEvents.Clear();
                _lowEvents.Clear();
            }
        }

        /// <summary>
        /// Зупиняє обробку і звільняє ресурси
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
