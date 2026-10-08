using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Спрощена реалізація шини подій без залежностей від мережі
    /// Використовується як тимчасова заміна до повної реєстрації всіх систем
    /// </summary>
    public class SimpleEventBus : IEventBus
    {
        private readonly IEventPool _eventPool;
        private readonly IMythLogger _logger;
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private readonly Dictionary<Type, List<Delegate>> _asyncHandlers = new Dictionary<Type, List<Delegate>>();

        [Inject]
        public SimpleEventBus(IEventPool eventPool, IMythLogger logger)
        {
            _eventPool = eventPool;
            _logger = logger;
        }

        // Базова реалізація методів IEventBus
        public void Subscribe<TEvent>(Action<TEvent> handler, EventPriority priority = EventPriority.Normal) where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);

            if (!_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>();
                _handlers[eventType] = handlers;
            }

            handlers.Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);

            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        public void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);

            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers.ToArray()) // Копіюємо список для безпечної ітерації
                {
                    try
                    {
                        ((Action<TEvent>)handler)(eventData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error handling event {eventType.Name}: {ex.Message}", "EventBus", ex);
                    }
                }
            }
        }

        public void SubscribeAsync<TEvent>(Func<TEvent, UniTask> handler, EventPriority priority = EventPriority.Normal) where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);

            if (!_asyncHandlers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>();
                _asyncHandlers[eventType] = handlers;
            }

            handlers.Add(handler);
        }

        public void UnsubscribeAsync<TEvent>(Func<TEvent, UniTask> handler) where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);

            if (_asyncHandlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        public async UniTask PublishAsync<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);

            if (_asyncHandlers.TryGetValue(eventType, out var handlers))
            {
                var tasks = new List<UniTask>();

                foreach (var handler in handlers.ToArray()) // Копіюємо список для безпечної ітерації
                {
                    try
                    {
                        tasks.Add(((Func<TEvent, UniTask>)handler)(eventData));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error handling async event {eventType.Name}: {ex.Message}", "EventBus", ex);
                    }
                }

                await UniTask.WhenAll(tasks);
            }
        }

        public void Clear()
        {
            _handlers.Clear();
            _asyncHandlers.Clear();
        }
    }
}
