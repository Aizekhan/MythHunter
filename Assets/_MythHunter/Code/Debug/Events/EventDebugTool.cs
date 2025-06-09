// Файл: Assets/_MythHunter/Code/Debug/Events/EventDebugTool.cs
using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, int> _eventCounts = new Dictionary<string, int>();
        private readonly Dictionary<EventPriority, int> _priorityCounts = new Dictionary<EventPriority, int>();
        private Action<IEvent, Type> _debugHandler;
        private bool _isSubscribed = false;

        public EventDebugTool(IEventBus eventBus, IMythLogger logger)
            : base("Event Monitor", "Events", logger)
        {
            _eventBus = eventBus;
            _debugHandler = OnAnyEvent;

            // Ініціалізація лічильників за пріоритетами
            foreach (EventPriority priority in Enum.GetValues(typeof(EventPriority)))
            {
                _priorityCounts[priority] = 0;
            }
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

        public void SubscribeToEvents()
        {
            if (!IsEnabled || _isSubscribed)
                return;

            DebugEventMiddleware.Subscribe(_debugHandler);
            _isSubscribed = true;
            _logger?.LogInfo("Subscribed to all events", "EventDebugger");
        }

        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            DebugEventMiddleware.Unsubscribe(_debugHandler);
            _isSubscribed = false;
            _logger?.LogInfo("Unsubscribed from all events", "EventDebugger");
        }

        private void OnAnyEvent(IEvent evt, Type eventType)
        {
            if (!IsEnabled)
                return;

            string eventTypeName = eventType.Name;

            // Оновлюємо лічильники
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

        // Решта коду залишається без змін...
    }
}
