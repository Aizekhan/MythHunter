using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Черга подій для контролю потоку подій та запобігання рекурсивним викликам
    /// </summary>
    public class EventQueue : IEventQueue
    {
        private readonly Queue<EventQueueItem> _eventQueue = new Queue<EventQueueItem>();
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        private bool _isProcessing;

        // Структура для зберігання інформації про подію в черзі
        private struct EventQueueItem
        {
            public IEvent Event;
            public bool IsAsync;
        }

        public EventQueue(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
            _isProcessing = false;
        }

        /// <summary>
        /// Додає подію в чергу для синхронної обробки
        /// </summary>
        public void Enqueue<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            _eventQueue.Enqueue(new EventQueueItem
            {
                Event = eventData,
                IsAsync = false
            });

            _logger.LogDebug($"Enqueued event {typeof(TEvent).Name} with ID {eventData.GetEventId()}");

            // Запускаємо обробку черги, якщо вона ще не запущена
            if (!_isProcessing)
            {
                ProcessQueueAsync().Forget();
            }
        }

        /// <summary>
        /// Додає подію в чергу для асинхронної обробки
        /// </summary>
        public void EnqueueAsync<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            _eventQueue.Enqueue(new EventQueueItem
            {
                Event = eventData,
                IsAsync = true
            });

            _logger.LogDebug($"Enqueued async event {typeof(TEvent).Name} with ID {eventData.GetEventId()}");

            // Запускаємо обробку черги, якщо вона ще не запущена
            if (!_isProcessing)
            {
                ProcessQueueAsync().Forget();
            }
        }

        /// <summary>
        /// Асинхронно обробляє чергу подій
        /// </summary>
        private async UniTaskVoid ProcessQueueAsync()
        {
            if (_isProcessing)
                return;

            _isProcessing = true;
            _logger.LogDebug("Started processing event queue");

            try
            {
                while (_eventQueue.Count > 0)
                {
                    // Отримуємо подію з черги
                    var item = _eventQueue.Dequeue();

                    // Обробляємо подію синхронно або асинхронно
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
                }
            }
            finally
            {
                _isProcessing = false;
                _logger.LogDebug("Finished processing event queue");
            }
        }

        /// <summary>
        /// Очищає чергу подій
        /// </summary>
        public void Clear()
        {
            _eventQueue.Clear();
            _logger.LogInfo("Event queue cleared");
        }
    }

    /// <summary>
    /// Інтерфейс для черги подій
    /// </summary>
    public interface IEventQueue
    {
        void Enqueue<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
        void EnqueueAsync<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
        void Clear();
    }
}
