using System;
using System.Collections.Generic;
using UnityEngine;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Events
{
    /// <summary>
    /// Клас для контролю частоти відправки подій
    /// </summary>
    public class EventThrottler : IEventThrottler
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly Dictionary<Type, ThrottleSettings> _throttleSettings = new Dictionary<Type, ThrottleSettings>();
        private readonly Dictionary<Type, float> _lastEventTime = new Dictionary<Type, float>();
        private readonly Dictionary<Type, object> _pendingEvents = new Dictionary<Type, object>();
        private readonly Dictionary<string, float> _namedThrottles = new Dictionary<string, float>();

        /// <summary>
        /// Налаштування обмеження частоти для типу подій
        /// </summary>
        private class ThrottleSettings
        {
            public float MinInterval { get; set; } = 0.25f; // За замовчуванням 4 рази на секунду
            public ThrottleMode Mode { get; set; } = ThrottleMode.DropIntermediate;
            public bool UseUnscaledTime { get; set; } = false;
        }

        /// <summary>
        /// Режим обмеження частоти подій
        /// </summary>
        public enum ThrottleMode
        {
            /// <summary>
            /// Проміжні події відкидаються, відправляється лише остання
            /// </summary>
            DropIntermediate,

            /// <summary>
            /// Проміжні події накопичуються і відправляються однією групою
            /// </summary>
            BatchEvents,

            /// <summary>
            /// Відправляються лише перша і остання події за інтервал
            /// </summary>
            FirstAndLast
        }

        [Inject]
        public EventThrottler(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        /// <summary>
        /// Реєструє налаштування обмеження частоти для типу подій
        /// </summary>
        public void RegisterThrottle<TEvent>(float minInterval, ThrottleMode mode = ThrottleMode.DropIntermediate, bool useUnscaledTime = false)
            where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);
            _throttleSettings[eventType] = new ThrottleSettings
            {
                MinInterval = Mathf.Max(0.01f, minInterval),
                Mode = mode,
                UseUnscaledTime = useUnscaledTime
            };

            _logger.LogDebug($"Registered throttle for {eventType.Name}: interval={minInterval}s, mode={mode}", "Events");
        }

        /// <summary>
        /// Публікує подію з урахуванням обмеження частоти
        /// </summary>
        public void PublishThrottled<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);

            // Отримуємо поточний час
            float currentTime = GetCurrentTime(eventType);

            // Перевіряємо, чи є налаштування обмеження частоти
            if (!_throttleSettings.TryGetValue(eventType, out var settings))
            {
                // Якщо немає налаштувань, просто публікуємо подію
                _eventBus.Publish(eventData);
                return;
            }

            // Отримуємо час останньої події цього типу
            if (!_lastEventTime.TryGetValue(eventType, out var lastTime))
            {
                lastTime = currentTime - settings.MinInterval * 2; // Гарантуємо, що перша подія завжди відправляється
            }

            // Перевіряємо, чи минув мінімальний інтервал
            if (currentTime - lastTime >= settings.MinInterval)
            {
                // Інтервал минув, публікуємо подію
                _eventBus.Publish(eventData);
                _lastEventTime[eventType] = currentTime;

                // Очищаємо накопичену подію, якщо була
                _pendingEvents.Remove(eventType);
            }
            else
            {
                // Обробка залежно від режиму
                switch (settings.Mode)
                {
                    case ThrottleMode.DropIntermediate:
                        // Запам'ятовуємо останню подію для відправки пізніше
                        _pendingEvents[eventType] = eventData;
                        break;

                    case ThrottleMode.BatchEvents:
                        // В реальній імплементації тут має бути логіка групування подій
                        // Для спрощення просто запам'ятовуємо останню
                        _pendingEvents[eventType] = eventData;
                        break;

                    case ThrottleMode.FirstAndLast:
                        // Запам'ятовуємо тільки останню подію
                        _pendingEvents[eventType] = eventData;
                        break;
                }
            }
        }

        /// <summary>
        /// Створює іменований обмежувач частоти для кастомної логіки
        /// </summary>
        public bool CheckThrottle(string name, float interval)
        {
            float currentTime = Time.realtimeSinceStartup;

            if (!_namedThrottles.TryGetValue(name, out var lastTime))
            {
                lastTime = currentTime - interval * 2; // Перший виклик завжди дозволений
            }

            if (currentTime - lastTime >= interval)
            {
                _namedThrottles[name] = currentTime;
                return true; // Дозволяємо виконання
            }

            return false; // Відхиляємо - ще не минув інтервал
        }

        /// <summary>
        /// Оновлює обмежувач - публікує відкладені події, якщо потрібно
        /// </summary>
        public void Update()
        {
            var currentTime = Time.realtimeSinceStartup;
            var keysToProcess = new List<Type>();

            // Знаходимо події, для яких минув інтервал
            foreach (var pair in _pendingEvents)
            {
                var eventType = pair.Key;

                if (!_throttleSettings.TryGetValue(eventType, out var settings) ||
                    !_lastEventTime.TryGetValue(eventType, out var lastTime))
                {
                    continue;
                }

                var timeToCheck = settings.UseUnscaledTime ? Time.unscaledTime : currentTime;

                if (timeToCheck - lastTime >= settings.MinInterval)
                {
                    keysToProcess.Add(eventType);
                }
            }

            // Публікуємо відкладені події
            foreach (var eventType in keysToProcess)
            {
                if (_pendingEvents.TryGetValue(eventType, out var pendingEvent))
                {
                    // Публікуємо подію через рефлексію
                    var publishMethod = typeof(IEventBus).GetMethod("Publish").MakeGenericMethod(eventType);
                    publishMethod.Invoke(_eventBus, new[] { pendingEvent });

                    // Оновлюємо час останньої події
                    _lastEventTime[eventType] = GetCurrentTime(eventType);

                    // Видаляємо з відкладених
                    _pendingEvents.Remove(eventType);
                }
            }
        }

        // Допоміжний метод для отримання поточного часу з урахуванням налаштувань
        private float GetCurrentTime(Type eventType)
        {
            if (_throttleSettings.TryGetValue(eventType, out var settings) && settings.UseUnscaledTime)
            {
                return Time.unscaledTime;
            }

            return Time.realtimeSinceStartup;
        }
    }

    /// <summary>
    /// Інтерфейс для обмежувача частоти подій
    /// </summary>
    public interface IEventThrottler
    {
        void RegisterThrottle<TEvent>(float minInterval, EventThrottler.ThrottleMode mode = EventThrottler.ThrottleMode.DropIntermediate, bool useUnscaledTime = false)
            where TEvent : struct, IEvent;
        void PublishThrottled<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
        bool CheckThrottle(string name, float interval);
        void Update();
    }
}
