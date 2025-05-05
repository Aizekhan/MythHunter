using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Базовий клас для всіх систем з підтримкою подій
    /// </summary>
    public abstract class SystemBase : ISystem, IEventSystem
    {
        protected readonly IMythLogger _logger;
        protected readonly IEventBus _eventBus;
        protected bool _isSubscribed = false;

        [Inject]
        protected SystemBase(IMythLogger logger, IEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        public virtual void Initialize()
        {
            SubscribeToEvents();
        }

        public virtual void Update(float deltaTime)
        {
            // Базова реалізація - нічого
        }

        public virtual void Dispose()
        {
            UnsubscribeFromEvents();
        }

        public virtual void SubscribeToEvents()
        {
            if (!_isSubscribed)
            {
                _isSubscribed = true;
                OnSubscribeToEvents();
            }
        }

        public virtual void UnsubscribeFromEvents()
        {
            if (_isSubscribed)
            {
                _isSubscribed = false;
                OnUnsubscribeFromEvents();
            }
        }

        /// <summary>
        /// Шаблонний метод для підписки на події
        /// </summary>
        protected virtual void OnSubscribeToEvents()
        {
            // Реалізують нащадки
        }

        /// <summary>
        /// Шаблонний метод для відписки від подій
        /// </summary>
        protected virtual void OnUnsubscribeFromEvents()
        {
            // Реалізують нащадки
        }

        /// <summary>
        /// Допоміжний метод для підписки на події
        /// </summary>
        protected void Subscribe<TEvent>(System.Action<TEvent> handler, EventPriority priority = EventPriority.Normal)
            where TEvent : struct, IEvent
        {
            _eventBus.Subscribe(handler, priority);
        }

        /// <summary>
        /// Допоміжний метод для відписки від подій
        /// </summary>
        protected void Unsubscribe<TEvent>(System.Action<TEvent> handler)
            where TEvent : struct, IEvent
        {
            _eventBus.Unsubscribe(handler);
        }

        /// <summary>
        /// Допоміжний метод для публікації подій
        /// </summary>
        protected void Publish<TEvent>(TEvent eventData)
            where TEvent : struct, IEvent
        {
            _eventBus.Publish(eventData);
        }
    }
}
