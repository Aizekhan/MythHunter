using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Система для групової обробки однотипних подій
    /// </summary>
    public class EventBatcher : IEventBatcher
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly Dictionary<Type, List<object>> _eventBatches = new Dictionary<Type, List<object>>();
        private readonly Dictionary<Type, float> _batchDurations = new Dictionary<Type, float>();
        private readonly Dictionary<Type, float> _lastBatchTime = new Dictionary<Type, float>();
        private readonly Dictionary<Type, BatchSettings> _batchSettings = new Dictionary<Type, BatchSettings>();

        /// <summary>
        /// Налаштування для групової обробки подій
        /// </summary>
        private class BatchSettings
        {
            public int MaxBatchSize { get; set; } = 10;
            public float MaxBatchDuration { get; set; } = 0.1f; // Секунд
            public bool ProcessImmediatelyIfEmpty { get; set; } = true;
        }

        [Inject]
        public EventBatcher(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        /// <summary>
        /// Реєструє тип події для групової обробки
        /// </summary>
        public void RegisterBatch<TEvent, TBatchEvent>(
            int maxBatchSize = 10,
            float maxBatchDuration = 0.1f,
            bool processImmediatelyIfEmpty = true,
            Func<List<TEvent>, TBatchEvent> batchProcessor = null)
            where TEvent : struct, IEvent
            where TBatchEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);

            // Зберігаємо налаштування
            _batchSettings[eventType] = new BatchSettings
            {
                MaxBatchSize = maxBatchSize,
                MaxBatchDuration = maxBatchDuration,
                ProcessImmediatelyIfEmpty = processImmediatelyIfEmpty
            };

            // Ініціалізуємо пусту пачку
            if (!_eventBatches.ContainsKey(eventType))
            {
                _eventBatches[eventType] = new List<object>();
                _lastBatchTime[eventType] = UnityEngine.Time.realtimeSinceStartup;
            }

            _logger.LogDebug($"Registered batch for {eventType.Name}: size={maxBatchSize}, duration={maxBatchDuration}s", "Events");

            // Якщо передано процесор - зберігаємо його
            if (batchProcessor != null)
            {
                // TODO: Зберігання процесора для типу
            }
        }

        /// <summary>
        /// Додає подію до відповідної пачки
        /// </summary>
        public void AddToBatch<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);

            // Перевіряємо, чи тип зареєстрований для групової обробки
            if (!_batchSettings.TryGetValue(eventType, out var settings))
            {
                // Якщо не зареєстрований, просто публікуємо подію
                _eventBus.Publish(eventData);
                return;
            }

            // Отримуємо або створюємо пачку
            if (!_eventBatches.TryGetValue(eventType, out var batch))
            {
                batch = new List<object>();
                _eventBatches[eventType] = batch;
                _lastBatchTime[eventType] = UnityEngine.Time.realtimeSinceStartup;
            }

            // Якщо пачка порожня і налаштовано негайну обробку для першої події, публікуємо відразу
            if (batch.Count == 0 && settings.ProcessImmediatelyIfEmpty)
            {
                _eventBus.Publish(eventData);
                return;
            }

            // Додаємо подію до пачки
            batch.Add(eventData);

            // Якщо пачка досягла максимального розміру, обробляємо її
            if (batch.Count >= settings.MaxBatchSize)
            {
                ProcessBatch(eventType);
            }
        }

        /// <summary>
        /// Оновлення бетчера - перевірка часу пачок
        /// </summary>
        public void Update()
        {
            float currentTime = UnityEngine.Time.realtimeSinceStartup;
            var typesToProcess = new List<Type>();

            // Перевіряємо всі пачки на перевищення максимального часу
            foreach (var pair in _eventBatches)
            {
                var eventType = pair.Key;
                var batch = pair.Value;

                if (batch.Count == 0)
                    continue;

                if (!_batchSettings.TryGetValue(eventType, out var settings) ||
                    !_lastBatchTime.TryGetValue(eventType, out var lastTime))
                {
                    continue;
                }

                // Якщо минув максимальний час, обробляємо пачку
                if (currentTime - lastTime >= settings.MaxBatchDuration)
                {
                    typesToProcess.Add(eventType);
                }
            }

            // Обробляємо всі пачки, для яких минув максимальний час
            foreach (var eventType in typesToProcess)
            {
                ProcessBatch(eventType);
            }
        }

        /// <summary>
        /// Обробляє пачку подій
        /// </summary>
        private void ProcessBatch(Type eventType)
        {
            if (!_eventBatches.TryGetValue(eventType, out var batch) || batch.Count == 0)
                return;

            // Спрощений варіант - просто публікуємо кожну подію окремо
            foreach (var evt in batch)
            {
                // Публікуємо подію через рефлексію
                var publishMethod = typeof(IEventBus).GetMethod("Publish").MakeGenericMethod(eventType);
                publishMethod.Invoke(_eventBus, new[] { evt });
            }

            // Очищаємо пачку і оновлюємо час
            batch.Clear();
            _lastBatchTime[eventType] = UnityEngine.Time.realtimeSinceStartup;

            _logger.LogDebug($"Processed batch of {batch.Count} events of type {eventType.Name}", "Events");

            // TODO: В розширеній реалізації тут має бути перетворення групи подій в одну BatchEvent
        }

        /// <summary>
        /// Примусово обробляє всі незавершені пачки
        /// </summary>
        public void ProcessAllBatches()
        {
            foreach (var eventType in _eventBatches.Keys)
            {
                ProcessBatch(eventType);
            }
        }
    }

    /// <summary>
    /// Інтерфейс для системи групової обробки подій
    /// </summary>
    public interface IEventBatcher
    {
        void RegisterBatch<TEvent, TBatchEvent>(
             int maxBatchSize = 10,
             float maxBatchDuration = 0.1f,
             bool processImmediatelyIfEmpty = true,
             Func<List<TEvent>, TBatchEvent> batchProcessor = null)
             where TEvent : struct, IEvent
             where TBatchEvent : struct, IEvent;
        void AddToBatch<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
        void Update();
        void ProcessAllBatches();
    }
}
