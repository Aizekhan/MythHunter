// Шлях: Assets/_MythHunter/Code/Debug/Events/EventDebugTool.cs
using System;
using System.Collections.Generic;
using System.Reflection;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Debug.Core;
using UnityEngine;

namespace MythHunter.Debug.Events
{
    /// <summary>
    /// Інструмент для відстеження та аналізу подій системи
    /// </summary>
    public class EventDebugTool : DebugToolBase
    {
        private readonly IEventBus _eventBus;
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();
        private readonly List<Type> _eventTypes = new List<Type>();
        private readonly Dictionary<string, int> _eventCounts = new Dictionary<string, int>();
        private readonly Dictionary<EventPriority, int> _priorityCounts = new Dictionary<EventPriority, int>();

        public EventDebugTool(IEventBus eventBus, IMythLogger logger)
            : base("Event Monitor", "Events", logger)
        {
            _eventBus = eventBus;

            // Ініціалізація лічильників за пріоритетами
            foreach (EventPriority priority in Enum.GetValues(typeof(EventPriority)))
            {
                _priorityCounts[priority] = 0;
            }

            // Знаходимо всі типи подій
            FindAllEventTypes();
        }

        public override void Initialize()
        {
            base.Initialize();
            SubscribeToEvents();
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            base.Dispose();
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
                        _logger?.LogWarning($"Failed to scan assembly {assembly.FullName}: {ex.Message}", "EventDebugger");
                    }
                }

                UpdateStatistic("FoundEventTypes", _eventTypes.Count);
                _logger?.LogInfo($"Found {_eventTypes.Count} event types", "EventDebugger");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error finding event types: {ex.Message}", "EventDebugger", ex);
            }
        }

        public void SubscribeToEvents()
        {
            if (!IsEnabled)
                return;

            try
            {
                foreach (var eventType in _eventTypes)
                {
                    // Створюємо типізований метод підписки для кожного типу події
                    SubscribeToEventType(eventType);
                }

                UpdateStatistic("SubscribedEventTypes", _handlers.Count);
                _logger?.LogInfo($"Subscribed to {_handlers.Count} event types", "EventDebugger");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error subscribing to events: {ex.Message}", "EventDebugger", ex);
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

                // Ініціалізуємо лічильник для цього типу події
                _eventCounts[eventType.Name] = 0;

                // Викликаємо метод Subscribe
                methodInfo.Invoke(_eventBus, new object[] { handler, EventPriority.Low });
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to subscribe to event type {eventType.Name}: {ex.Message}", "EventDebugger");
            }
        }

        public void UnsubscribeFromEvents()
        {
            try
            {
                foreach (var pair in _handlers)
                {
                    var eventType = pair.Key;
                    var handler = pair.Value;

                    // Отримуємо метод Unsubscribe з правильним типом
                    var methodInfo = typeof(IEventBus).GetMethod("Unsubscribe").MakeGenericMethod(eventType);

                    // Викликаємо метод Unsubscribe
                    methodInfo.Invoke(_eventBus, new object[] { handler });
                }

                _handlers.Clear();
                _logger?.LogInfo("Unsubscribed from all events", "EventDebugger");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error unsubscribing from events: {ex.Message}", "EventDebugger", ex);
            }
        }

        // Універсальний метод обробки будь-якої події
        private void OnAnyEvent<T>(T evt) where T : struct, IEvent
        {
            if (!IsEnabled)
                return;

            // Оновлюємо лічильники
            string eventTypeName = typeof(T).Name;
            if (_eventCounts.TryGetValue(eventTypeName, out int count))
            {
                _eventCounts[eventTypeName] = count + 1;
            }
            else
            {
                _eventCounts[eventTypeName] = 1;
            }

            // Оновлюємо лічильник за пріоритетом
            var priority = evt.GetPriority();
            _priorityCounts[priority]++;

            // Додаємо запис у лог
            AddLogEntry($"{eventTypeName} (ID: {evt.GetEventId()}, Priority: {priority})");

            // Оновлюємо загальну статистику
            UpdateEventStats();
        }

        private void UpdateEventStats()
        {
            int totalEvents = 0;
            foreach (var count in _eventCounts.Values)
            {
                totalEvents += count;
            }

            UpdateStatistic("TotalEventsRecorded", totalEvents);

            // Оновлюємо статистику за пріоритетами
            foreach (var priority in _priorityCounts.Keys)
            {
                UpdateStatistic($"Priority_{priority}", _priorityCounts[priority]);
            }
        }

        // Перевизначення методів базового класу

        protected override void RenderContent()
        {
            // Кнопки для управління
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Counters", GUILayout.Width(120)))
            {
                ClearEventCounters();
            }

            if (GUILayout.Button("Resubscribe", GUILayout.Width(120)))
            {
                UnsubscribeFromEvents();
                SubscribeToEvents();
            }
            GUILayout.EndHorizontal();

            // Загальна статистика
            GUILayout.Label("Total Events: " + _statistics["TotalEventsRecorded"], GUI.skin.box);

            // Статистика за пріоритетами
            if (DrawFoldout("Events by Priority"))
            {
                foreach (EventPriority priority in Enum.GetValues(typeof(EventPriority)))
                {
                    int count = _priorityCounts[priority];
                    GUILayout.Label($"{priority}: {count}");
                }
            }

            // Статистика за типами подій
            if (DrawFoldout("Events by Type"))
            {
                foreach (var pair in _eventCounts)
                {
                    if (pair.Value > 0)
                    {
                        GUILayout.Label($"{pair.Key}: {pair.Value}");
                    }
                }
            }

            // Останні події
            base.RenderContent();
        }

        private void ClearEventCounters()
        {
            // Очищення лічильників подій
            foreach (var key in _eventCounts.Keys)
            {
                _eventCounts[key] = 0;
            }

            // Очищення лічильників за пріоритетами
            foreach (var priority in _priorityCounts.Keys)
            {
                _priorityCounts[priority] = 0;
            }

            // Очищення логів
            ClearLogs();

            // Оновлення статистики
            UpdateEventStats();

            _logger?.LogInfo("Event counters cleared", "EventDebugger");
        }
    }
}
