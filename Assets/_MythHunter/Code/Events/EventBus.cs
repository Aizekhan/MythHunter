using System;
using System.Collections.Generic;

namespace MythHunter.Events
{
    /// <summary>
    /// Базова реалізація IEventBus
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            var type = typeof(TEvent);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();

            _handlers[type].Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            var type = typeof(TEvent);
            if (_handlers.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        public void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            var type = typeof(TEvent);
            if (_handlers.TryGetValue(type, out var list))
            {
                foreach (var handler in list)
                    ((Action<TEvent>)handler)?.Invoke(eventData);
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}