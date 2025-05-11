using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Events.Debugging
{
    /// <summary>
    /// Сховище історії подій для дебагінгу та аналізу
    /// </summary>
    public class EventStore : IEventStore
    {
        private readonly IMythLogger _logger;
        private  int _maxEventsInStore;
        private readonly List<EventRecord> _eventHistory = new List<EventRecord>();
        private readonly HashSet<Type> _includedEventTypes = new HashSet<Type>();
        private readonly HashSet<Type> _excludedEventTypes = new HashSet<Type>();
        private readonly Dictionary<Type, int> _eventTypeCounts = new Dictionary<Type, int>();
        private readonly Dictionary<string, List<EventRecord>> _eventsByCategory = new Dictionary<string, List<EventRecord>>();

        private bool _isEnabled = false;
        private bool _isRegistered = false;

        /// <summary>
        /// Запис про подію в історії
        /// </summary>
        public class EventRecord
        {
            public DateTime Timestamp
            {
                get; set;
            }
            public string EventId
            {
                get; set;
            }
            public string EventType
            {
                get; set;
            }
            public EventPriority Priority
            {
                get; set;
            }
            public object EventData
            {
                get; set;
            }
            public string Category
            {
                get; set;
            }
            public int FrameNumber
            {
                get; set;
            }
            public float GameTime
            {
                get; set;
            }

            public override string ToString()
            {
                return $"[{Timestamp:HH:mm:ss.fff}] {EventType} (ID: {EventId}, Priority: {Priority})";
            }
        }

        [Inject]
        public EventStore(IMythLogger logger, IEventBus eventBus)
        {
            _logger = logger;
            _maxEventsInStore = 1000; // За замовчуванням зберігаємо 1000 останніх подій
        }

        /// <summary>
        /// Активує сховище подій
        /// </summary>
        public void Enable()
        {
            if (_isEnabled)
                return;

            _isEnabled = true;

            if (!_isRegistered)
            {
                RegisterToEventBus();
                _isRegistered = true;
            }

            _logger.LogInfo("Event Store enabled", "EventStore");
        }

        /// <summary>
        /// Деактивує сховище подій
        /// </summary>
        public void Disable()
        {
            if (!_isEnabled)
                return;

            _isEnabled = false;
            _logger.LogInfo("Event Store disabled", "EventStore");
        }

        /// <summary>
        /// Реєструється на отримання всіх подій через DebugEventMiddleware
        /// </summary>
        private void RegisterToEventBus()
        {
            DebugEventMiddleware.Subscribe(OnAnyEvent);
            _logger.LogInfo("Event Store registered to event bus", "EventStore");
        }

        /// <summary>
        /// Обробляє будь-яку подію в системі
        /// </summary>
        private void OnAnyEvent(IEvent evt, Type eventType)
        {
            if (!_isEnabled)
                return;

            // Перевіряємо фільтри
            if (_excludedEventTypes.Contains(eventType) ||
                (_includedEventTypes.Count > 0 && !_includedEventTypes.Contains(eventType)))
            {
                return;
            }

            // Створюємо запис про подію
            var record = new EventRecord
            {
                Timestamp = DateTime.Now,
                EventId = evt.GetEventId(),
                EventType = eventType.Name,
                Priority = evt.GetPriority(),
                EventData = evt,
                Category = GetCategoryFromEventType(eventType),
                FrameNumber = UnityEngine.Time.frameCount,
                GameTime = UnityEngine.Time.time
            };

            // Додаємо до загальної історії
            _eventHistory.Add(record);

            // Додаємо до історії за категорією
            if (!_eventsByCategory.TryGetValue(record.Category, out var categoryHistory))
            {
                categoryHistory = new List<EventRecord>();
                _eventsByCategory[record.Category] = categoryHistory;
            }
            categoryHistory.Add(record);

            // Оновлюємо статистику
            if (!_eventTypeCounts.TryGetValue(eventType, out var count))
            {
                count = 0;
            }
            _eventTypeCounts[eventType] = count + 1;

            // Обмежуємо розмір історії
            if (_eventHistory.Count > _maxEventsInStore)
            {
                _eventHistory.RemoveAt(0);
            }

            // Обмежуємо розмір історії за категоріями
            foreach (var category in _eventsByCategory.Values)
            {
                if (category.Count > _maxEventsInStore / 10)
                {
                    category.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Отримує категорію з типу події
        /// </summary>
        private string GetCategoryFromEventType(Type eventType)
        {
            string fullName = eventType.FullName;

            if (fullName.Contains("Combat"))
                return "Combat";
            if (fullName.Contains("Movement"))
                return "Movement";
            if (fullName.Contains("Phase"))
                return "Phase";
            if (fullName.Contains("Rune"))
                return "Rune";
            if (fullName.Contains("Gameplay"))
                return "Gameplay";
            if (fullName.Contains("Entity"))
                return "Entity";
            if (fullName.Contains("Network"))
                return "Network";

            return "Other";
        }

        /// <summary>
        /// Отримує останні N подій
        /// </summary>
        public List<EventRecord> GetLatestEvents(int count = 10)
        {
            return _eventHistory.Skip(Math.Max(0, _eventHistory.Count - count)).ToList();
        }

        /// <summary>
        /// Отримує події за типом
        /// </summary>
        public List<EventRecord> GetEventsByType(Type eventType, int count = 10)
        {
            return _eventHistory
                .Where(e => e.EventType == eventType.Name)
                .Skip(Math.Max(0, _eventHistory.Count(e => e.EventType == eventType.Name) - count))
                .ToList();
        }

        /// <summary>
        /// Отримує події за категорією
        /// </summary>
        public List<EventRecord> GetEventsByCategory(string category, int count = 10)
        {
            if (!_eventsByCategory.TryGetValue(category, out var categoryHistory))
            {
                return new List<EventRecord>();
            }

            return categoryHistory
                .Skip(Math.Max(0, categoryHistory.Count - count))
                .ToList();
        }

        /// <summary>
        /// Додає тип події до фільтра включення
        /// </summary>
        public void IncludeEventType<T>() where T : struct, IEvent
        {
            _includedEventTypes.Add(typeof(T));
        }

        /// <summary>
        /// Додає тип події до фільтра виключення
        /// </summary>
        public void ExcludeEventType<T>() where T : struct, IEvent
        {
            _excludedEventTypes.Add(typeof(T));
        }

        /// <summary>
        /// Очищає історію подій
        /// </summary>
        public void Clear()
        {
            _eventHistory.Clear();
            _eventsByCategory.Clear();
            _eventTypeCounts.Clear();
            _logger.LogInfo("Event Store cleared", "EventStore");
        }

        /// <summary>
        /// Встановлює максимальну кількість подій для зберігання
        /// </summary>
        public void SetMaxEvents(int maxEvents)
        {
            _maxEventsInStore = Math.Max(100, maxEvents);

            // Обмежуємо поточну історію, якщо потрібно
            while (_eventHistory.Count > _maxEventsInStore)
            {
                _eventHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Отримує статистику за типами подій
        /// </summary>
        public Dictionary<string, int> GetEventTypeStatistics()
        {
            return _eventTypeCounts.ToDictionary(
                pair => pair.Key.Name,
                pair => pair.Value
            );
        }

        /// <summary>
        /// Отримує список категорій та кількість подій у них
        /// </summary>
        public Dictionary<string, int> GetCategoryStatistics()
        {
            return _eventsByCategory.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Count
            );
        }

        /// <summary>
        /// Експортує історію подій у формат, зручний для аналізу
        /// </summary>
        public string ExportHistory()
        {
            // Створюємо простий текстовий формат
            var result = "Event History Export\n";
            result += $"Generated: {DateTime.Now}\n";
            result += $"Total Events: {_eventHistory.Count}\n\n";

            // Додаємо статистику
            result += "== Event Type Statistics ==\n";
            foreach (var stat in GetEventTypeStatistics().OrderByDescending(s => s.Value))
            {
                result += $"{stat.Key}: {stat.Value}\n";
            }

            result += "\n== Category Statistics ==\n";
            foreach (var stat in GetCategoryStatistics().OrderByDescending(s => s.Value))
            {
                result += $"{stat.Key}: {stat.Value}\n";
            }

            // Додаємо останні події
            result += "\n== Latest 50 Events ==\n";
            foreach (var evt in GetLatestEvents(50))
            {
                result += $"{evt}\n";
            }

            return result;
        }
    }

    /// <summary>
    /// Інтерфейс для сховища історії подій
    /// </summary>
    public interface IEventStore
    {
        void Enable();
        void Disable();
        List<EventStore.EventRecord> GetLatestEvents(int count = 10);
        List<EventStore.EventRecord> GetEventsByType(Type eventType, int count = 10);
        List<EventStore.EventRecord> GetEventsByCategory(string category, int count = 10);
        void IncludeEventType<T>() where T : struct, IEvent;
        void ExcludeEventType<T>() where T : struct, IEvent;
        void Clear();
        void SetMaxEvents(int maxEvents);
        Dictionary<string, int> GetEventTypeStatistics();
        Dictionary<string, int> GetCategoryStatistics();
        string ExportHistory();
    }
}
