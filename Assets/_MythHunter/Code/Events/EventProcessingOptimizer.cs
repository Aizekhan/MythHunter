// Шлях: Assets/_MythHunter/Code/Events/EventProcessingOptimizer.cs

using System;
using System.Collections.Generic;
using System.Reflection;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Клас для оптимізації обробки подій
    /// </summary>
    public class EventProcessingOptimizer
    {
        private readonly Dictionary<Type, List<EventHandlerInfo>> _eventHandlerCache = new Dictionary<Type, List<EventHandlerInfo>>();
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        private class EventHandlerInfo
        {
            public Delegate Handler;
            public bool IsAsync;
            public EventPriority Priority;
        }

        [MythHunter.Core.DI.Inject]
        public EventProcessingOptimizer(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;

            // При ініціалізації аналізуємо всі обробники подій для оптимізації
            AnalyzeEventHandlers();
        }

        /// <summary>
        /// Аналізує всі зареєстровані обробники подій для оптимізації
        /// </summary>
        private void AnalyzeEventHandlers()
        {
            try
            {
                // За допомогою рефлексії отримуємо доступ до приватних полів EventBus
                // Це потребуватиме модифікації EventBus для підтримки цієї функціональності

                var syncHandlerField = _eventBus.GetType().GetField("_syncHandlers", BindingFlags.NonPublic | BindingFlags.Instance);
                var asyncHandlerField = _eventBus.GetType().GetField("_asyncHandlers", BindingFlags.NonPublic | BindingFlags.Instance);

                if (syncHandlerField == null || asyncHandlerField == null)
                {
                    _logger.LogWarning("Cannot access event handlers for optimization", "Events");
                    return;
                }

                var syncHandlers = (Dictionary<Type, List<object>>)syncHandlerField.GetValue(_eventBus);
                var asyncHandlers = (Dictionary<Type, List<object>>)asyncHandlerField.GetValue(_eventBus);

                // Аналізуємо синхронні обробники
                foreach (var pair in syncHandlers)
                {
                    var eventType = pair.Key;
                    var handlers = pair.Value;

                    foreach (var handler in handlers)
                    {
                        if (!_eventHandlerCache.TryGetValue(eventType, out var handlerList))
                        {
                            handlerList = new List<EventHandlerInfo>();
                            _eventHandlerCache[eventType] = handlerList;
                        }

                        handlerList.Add(new EventHandlerInfo
                        {
                            Handler = handler as Delegate,
                            IsAsync = false,
                            Priority = GetHandlerPriority(handler)
                        });
                    }
                }

                // Аналізуємо асинхронні обробники
                foreach (var pair in asyncHandlers)
                {
                    var eventType = pair.Key;
                    var handlers = pair.Value;

                    foreach (var handler in handlers)
                    {
                        if (!_eventHandlerCache.TryGetValue(eventType, out var handlerList))
                        {
                            handlerList = new List<EventHandlerInfo>();
                            _eventHandlerCache[eventType] = handlerList;
                        }

                        handlerList.Add(new EventHandlerInfo
                        {
                            Handler = handler as Delegate,
                            IsAsync = true,
                            Priority = GetHandlerPriority(handler)
                        });
                    }
                }

                // Сортування обробників за пріоритетом
                foreach (var handlerList in _eventHandlerCache.Values)
                {
                    handlerList.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                }

                _logger.LogInfo($"Analyzed and optimized {_eventHandlerCache.Count} event types", "Events");
            }
            catch (Exception ex)
            {
                // Шлях: Assets/_MythHunter/Code/Events/EventProcessingOptimizer.cs (продовження)

                _logger.LogError($"Error analyzing event handlers: {ex.Message}", "Events", ex);
            }
        }

        /// <summary>
        /// Визначає пріоритет обробника подій
        /// </summary>
        private EventPriority GetHandlerPriority(object handler)
        {
            // За замовчуванням всі обробники мають нормальний пріоритет
            // Для визначення пріоритету можна додати спеціальний атрибут або інтерфейс
            return EventPriority.Normal;
        }

        /// <summary>
        /// Оптимізує обробку подій певного типу
        /// </summary>
        public void OptimizeEventProcessing<T>() where T : struct, IEvent
        {
            Type eventType = typeof(T);

            if (!_eventHandlerCache.TryGetValue(eventType, out var handlers))
            {
                _logger.LogInfo($"No handlers found for event type {eventType.Name}", "Events");
                return;
            }

            _logger.LogInfo($"Optimizing processing for event type {eventType.Name} with {handlers.Count} handlers", "Events");

            // Тут можна реалізувати додаткову оптимізацію, наприклад:
            // - Прекомпіляція делегатів для швидкого виклику
            // - Створення спеціалізованих обробників для конкретних типів подій
            // - Кешування результатів для подій, що часто повторюються
        }

        /// <summary>
        /// Повертає статистику про обробку подій
        /// </summary>
        public Dictionary<string, int> GetEventProcessingStatistics()
        {
            var statistics = new Dictionary<string, int>();

            foreach (var pair in _eventHandlerCache)
            {
                statistics[pair.Key.Name] = pair.Value.Count;
            }

            return statistics;
        }
    }
}
