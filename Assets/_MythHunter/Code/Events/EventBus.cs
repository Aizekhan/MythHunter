using System;
using System.Collections.Generic;

namespace MythHunter.Events
{
    /// <summary>
    /// Реалізація шини подій
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private readonly List<Action<IEvent>> _globalSubscribers = new();
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);
            
            if (!_handlers.ContainsKey(eventType))
                _handlers[eventType] = new List<Delegate>();
                
            _handlers[eventType].Add(handler);
        }
        
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);
            
            if (!_handlers.ContainsKey(eventType))
                return;
                
            _handlers[eventType].Remove(handler);
            
            if (_handlers[eventType].Count == 0)
                _handlers.Remove(eventType);
        }
        public void SubscribeAny(Action<IEvent> callback)
        {
            _globalSubscribers.Add(callback);
        }

        public void UnsubscribeAny(Action<IEvent> callback)
        {
            _globalSubscribers.Remove(callback);
        }
        public void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);
            
            if (!_handlers.ContainsKey(eventType))
                return;
                
            foreach (var handler in _handlers[eventType])
            {
                try
                {
                    ((Action<TEvent>)handler).Invoke(eventData);
                }
                catch (Exception ex)
                {
                    // Логування виключень при обробці подій
                    Console.Error.WriteLine($"Error handling event {eventType.Name}: {ex.Message}");
                }
            }
            foreach (var subscriber in _globalSubscribers)
            {
                subscriber.Invoke(eventData);
            }
        }
        
        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
