// Assets/_MythHunter/Code/Events/Core/EventThrottler.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Events
{
    /// <summary>
    /// Обмежувач частоти подій
    /// </summary>
    public class EventThrottler : IEventThrottler
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        private class ThrottleConfig
        {
            public float MinInterval;
            public ThrottleMode Mode;
            public bool UseUnscaledTime;
            public float LastPublishTime;
            public IEvent QueuedEvent;
            public bool HasQueuedEvent;
        }

        private readonly Dictionary<string, ThrottleConfig> _throttleConfigs = new Dictionary<string, ThrottleConfig>();
        private readonly Dictionary<string, float> _throttleTimers = new Dictionary<string, float>();

        [Inject]
        public EventThrottler(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public void RegisterThrottle<TEvent>(float minInterval, ThrottleMode mode = ThrottleMode.DropIntermediate, bool useUnscaledTime = false) where TEvent : struct, IEvent
        {
            string eventTypeName = typeof(TEvent).FullName;

            if (_throttleConfigs.ContainsKey(eventTypeName))
            {
                _throttleConfigs[eventTypeName] = new ThrottleConfig
                {
                    MinInterval = minInterval,
                    Mode = mode,
                    UseUnscaledTime = useUnscaledTime,
                    LastPublishTime = 0,
                    QueuedEvent = default,
                    HasQueuedEvent = false
                };

                _logger.LogDebug($"Updated throttle for event {eventTypeName}: interval={minInterval}, mode={mode}", "EventThrottler");
            }
            else
            {
                _throttleConfigs.Add(eventTypeName, new ThrottleConfig
                {
                    MinInterval = minInterval,
                    Mode = mode,
                    UseUnscaledTime = useUnscaledTime,
                    LastPublishTime = 0,
                    QueuedEvent = default,
                    HasQueuedEvent = false
                });

                _logger.LogDebug($"Registered throttle for event {eventTypeName}: interval={minInterval}, mode={mode}", "EventThrottler");
            }
        }

        public void PublishThrottled<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            string eventTypeName = typeof(TEvent).FullName;

            if (!_throttleConfigs.TryGetValue(eventTypeName, out var config))
            {
                // Якщо конфіг не знайдено, публікуємо без обмежень
                _eventBus.Publish(eventData);
                return;
            }

            float currentTime = config.UseUnscaledTime ? Time.unscaledTime : Time.time;
            float elapsed = currentTime - config.LastPublishTime;

            if (elapsed >= config.MinInterval)
            {
                // Можна публікувати відразу
                config.LastPublishTime = currentTime;
                _eventBus.Publish(eventData);

                // Очищаємо чергу
                config.HasQueuedEvent = false;
            }
            else
            {
                // Обробка згідно з режимом
                switch (config.Mode)
                {
                    case ThrottleMode.First:
                        // Якщо режим "перший", і вже є подія в черзі, ігноруємо
                        if (!config.HasQueuedEvent)
                        {
                            config.HasQueuedEvent = true;
                            config.QueuedEvent = eventData;
                        }
                        break;

                    case ThrottleMode.Last:
                        // Якщо режим "останній", завжди замінюємо подію в черзі
                        config.HasQueuedEvent = true;
                        config.QueuedEvent = eventData;
                        break;

                    case ThrottleMode.DropIntermediate:
                        // Якщо режим "проміжний", замінюємо тільки якщо ще немає події
                        if (!config.HasQueuedEvent)
                        {
                            config.HasQueuedEvent = true;
                            config.QueuedEvent = eventData;
                        }
                        break;
                }
            }
        }

        public bool CheckThrottle(string name, float interval)
        {
            if (!_throttleTimers.ContainsKey(name))
            {
                _throttleTimers[name] = 0;
                return true;
            }

            float elapsed = Time.time - _throttleTimers[name];
            if (elapsed >= interval)
            {
                _throttleTimers[name] = Time.time;
                return true;
            }

            return false;
        }

        public void Update()
        {
            foreach (var kvp in _throttleConfigs)
            {
                var config = kvp.Value;

                if (!config.HasQueuedEvent)
                    continue;

                float currentTime = config.UseUnscaledTime ? Time.unscaledTime : Time.time;
                float elapsed = currentTime - config.LastPublishTime;

                if (elapsed >= config.MinInterval)
                {
                    // Публікуємо чергову подію
                    config.LastPublishTime = currentTime;

                    // Використовуємо dynamic, оскільки точний тип події заздалегідь невідомий
                    _eventBus.Publish((dynamic)config.QueuedEvent);

                    // Очищаємо чергу
                    config.HasQueuedEvent = false;
                }
            }
        }
    }
}
