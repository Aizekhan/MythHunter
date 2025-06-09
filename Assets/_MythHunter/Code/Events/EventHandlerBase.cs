// Файл: Assets/_MythHunter/Code/Events/EventHandlerBase.cs
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Базовий клас для обробників подій
    /// </summary>
    public abstract class EventHandlerBase : IEventSubscriber, IDisposable
    {
        protected readonly IEventBus _eventBus;
        protected readonly IMythLogger _logger;
        protected bool _isSubscribed = false;

        [Inject]
        protected EventHandlerBase(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        /// <summary>
        /// Підписатись на подію
        /// </summary>
        protected void Subscribe<TEvent>(Action<TEvent> handler, EventPriority priority = EventPriority.Normal)
            where TEvent : struct, IEvent
        {
            _eventBus.Subscribe<TEvent>(handler, priority);
            _logger.LogDebug($"Підписано на подію {typeof(TEvent).Name}", GetType().Name);
        }

        /// <summary>
        /// Відписатись від події
        /// </summary>
        protected void Unsubscribe<TEvent>(Action<TEvent> handler)
            where TEvent : struct, IEvent
        {
            _eventBus.Unsubscribe<TEvent>(handler);
            _logger.LogDebug($"Відписано від події {typeof(TEvent).Name}", GetType().Name);
        }

        /// <summary>
        /// Підписатись на асинхронну подію
        /// </summary>
        protected void SubscribeAsync<TEvent>(Func<TEvent, UniTask> handler, EventPriority priority = EventPriority.Normal)
            where TEvent : struct, IEvent
        {
            _eventBus.SubscribeAsync<TEvent>(handler, priority);
            _logger.LogDebug($"Підписано на асинхронну подію {typeof(TEvent).Name}", GetType().Name);
        }

        /// <summary>
        /// Відписатись від асинхронної події
        /// </summary>
        protected void UnsubscribeAsync<TEvent>(Func<TEvent, UniTask> handler)
            where TEvent : struct, IEvent
        {
            _eventBus.UnsubscribeAsync<TEvent>(handler);
            _logger.LogDebug($"Відписано від асинхронної події {typeof(TEvent).Name}", GetType().Name);
        }

        /// <summary>
        /// Публікація події
        /// </summary>
        protected void Publish<TEvent>(TEvent eventData)
            where TEvent : struct, IEvent
        {
            _eventBus.Publish(eventData);
            _logger.LogDebug($"Опубліковано подію {typeof(TEvent).Name}", GetType().Name);
        }

        /// <summary>
        /// Асинхронна публікація події
        /// </summary>
        protected async UniTask PublishAsync<TEvent>(TEvent eventData)
            where TEvent : struct, IEvent
        {
            await _eventBus.PublishAsync(eventData);
            _logger.LogDebug($"Опубліковано асинхронну подію {typeof(TEvent).Name}", GetType().Name);
        }

        /// <summary>
        /// Підписатись на події
        /// </summary>
        public virtual void SubscribeToEvents()
        {
            if (!_isSubscribed)
            {
                _isSubscribed = true;
                _logger.LogInfo($"{GetType().Name} підписався на події", "Events");
            }
        }

        /// <summary>
        /// Відписатись від подій
        /// </summary>
        public virtual void UnsubscribeFromEvents()
        {
            if (_isSubscribed)
            {
                _isSubscribed = false;
                _logger.LogInfo($"{GetType().Name} відписався від подій", "Events");
            }
        }

        /// <summary>
        /// Звільнення ресурсів
        /// </summary>
        public virtual void Dispose()
        {
            UnsubscribeFromEvents();
        }
    }
}
