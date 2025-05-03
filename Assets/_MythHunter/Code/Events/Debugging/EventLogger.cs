// Файл: Assets/_MythHunter/Code/Events/Debugging/EventLogger.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.Events.Debugging
{
    /// <summary>
    /// Логер подій для відлагодження
    /// </summary>
    public class EventLogger : EventHandlerBase
    {
        private bool _isEnabled = false;
        private readonly List<Type> _eventTypes = new List<Type>();
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        [Inject]
        public EventLogger(IEventBus eventBus, IMythLogger logger) : base(eventBus, logger)
        {
            // Знаходимо всі типи подій при створенні
            FindAllEventTypes();
        }

        private void FindAllEventTypes()
        {
            try
            {
                // Отримуємо всі завантажені збірки
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // Знаходимо всі структури, які реалізують IEvent
                        var allTypes = assembly.GetTypes();
                        var eventTypes = new List<Type>();
                        foreach (var type in allTypes)
                        {
                            if (type.IsValueType && typeof(IEvent).IsAssignableFrom(type))
                            {
                                eventTypes.Add(type);
                            }
                        }

                        _eventTypes.AddRange(eventTypes);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to scan assembly {assembly.FullName}: {ex.Message}", "EventLogger");
                    }
                }

                _logger.LogInfo($"Found {_eventTypes.Count} event types", "EventLogger");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error finding event types: {ex.Message}", "EventLogger", ex);
            }
        }

        public void Enable()
        {
            if (!_isEnabled)
            {
                SubscribeToEvents();
                _isEnabled = true;
                _logger.LogInfo("Event logger enabled", "EventLogger");
            }
        }

        public void Disable()
        {
            if (_isEnabled)
            {
                UnsubscribeFromEvents();
                _isEnabled = false;
                _logger.LogInfo("Event logger disabled", "EventLogger");
            }
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            try
            {
                foreach (var eventType in _eventTypes)
                {
                    // Створюємо типізований метод підписки для кожного типу події
                    SubscribeToEventType(eventType);
                }

                _logger.LogInfo($"Subscribed to {_handlers.Count} event types", "EventLogger");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error subscribing to events: {ex.Message}", "EventLogger", ex);
            }
        }

        private void SubscribeToEventType(Type eventType)
        {
            try
            {
                // Отримуємо метод Subscribe з правильним типом
                var methodInfo = typeof(IEventBus).GetMethod("Subscribe").MakeGenericMethod(eventType);

                // Створюємо типізований делегат для обробки події
                var handlerType = typeof(Action<>).MakeGenericType(eventType);
                var handler = Delegate.CreateDelegate(handlerType, this,
                    GetType().GetMethod("OnAnyEvent", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(eventType));

                // Зберігаємо делегат для подальшої відписки
                _handlers[eventType] = handler;

                // Викликаємо метод Subscribe напряму через _eventBus, оскільки тут динамічність
                methodInfo.Invoke(_eventBus, new object[] { handler });
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to subscribe to event type {eventType.Name}: {ex.Message}", "EventLogger");
            }
        }

        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();

            try
            {
                foreach (var pair in _handlers)
                {
                    var eventType = pair.Key;
                    var handler = pair.Value;

                    // Отримуємо метод Unsubscribe з правильним типом
                    var methodInfo = typeof(IEventBus).GetMethod("Unsubscribe").MakeGenericMethod(eventType);

                    // Викликаємо метод Unsubscribe напряму через _eventBus
                    methodInfo.Invoke(_eventBus, new object[] { handler });
                }

                _handlers.Clear();
                _logger.LogInfo("Unsubscribed from all events", "EventLogger");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error unsubscribing from events: {ex.Message}", "EventLogger", ex);
            }
        }

        // Універсальний метод обробки будь-якої події
        private void OnAnyEvent<T>(T evt) where T : struct, IEvent
        {
            _logger.LogDebug($"Event: {typeof(T).Name}, ID: {evt.GetEventId()}", "EventDebug");
        }
    }
}
