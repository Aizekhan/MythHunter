using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using MythHunter.Systems.Core;
using MythHunter.Events.Domain.Lobby;

namespace MythHunter.Systems.Core
{
    public class TimerSystem : SystemBase, ITimerSystem
    {
        private readonly Dictionary<string, Timer> _timers = new();
        private readonly IEventThrottler _eventThrottler;

        [Inject]
        public TimerSystem(
            IEventBus eventBus,
            IMythLogger logger,
            IEventThrottler eventThrottler)
            : base(logger, eventBus)
        {
            _eventThrottler = eventThrottler;

            // Реєструємо throttling для різних типів таймерів
            _eventThrottler.RegisterThrottle<TimerUpdatedEvent>(0.25f); // 4 рази на сек
            _eventThrottler.RegisterThrottle<PhaseUpdateEvent>(0.1f);   // 10 раз на сек для фаз
        }

        public override void Update(float deltaTime)
        {
            var completedTimers = new List<string>();

            foreach (var kvp in _timers)
            {
                var timer = kvp.Value;

                if (!timer.IsActive || timer.IsPaused)
                    continue;

                timer.RemainingTime -= deltaTime;

                // Публікуємо оновлення з throttling
                PublishTimerUpdate(timer);

                // Перевіряємо завершення
                if (timer.RemainingTime <= 0)
                {
                    timer.RemainingTime = 0;
                    timer.IsActive = false;

                    // Викликаємо callback
                    timer.OnComplete?.Invoke();

                    // Публікуємо подію завершення
                    Publish(new TimerCompletedEvent
                    {
                        TimerId = timer.Id,
                        TimerName = timer.Name,
                        Category = timer.Category,
                        Timestamp = DateTime.UtcNow
                    });

                    completedTimers.Add(kvp.Key);
                }
            }

            // Видаляємо завершені одноразові таймери
            foreach (var timerId in completedTimers)
            {
                if (_timers.TryGetValue(timerId, out var timer) && !timer.IsPersistent)
                {
                    RemoveTimer(timerId);
                }
            }
        }

        public string CreateTimer(string name, float duration, Action onComplete = null, bool autoStart = true)
        {
            string timerId = Guid.NewGuid().ToString();

            var timer = new Timer
            {
                Id = timerId,
                Name = name,
                TotalTime = duration,
                RemainingTime = duration,
                OnComplete = onComplete,
                IsActive = autoStart,
                IsPaused = false,
                CreatedAt = DateTime.UtcNow,
                Category = DetermineCategory(name)
            };

            _timers[timerId] = timer;

            Publish(new TimerCreatedEvent
            {
                TimerId = timerId,
                TimerName = name,
                Duration = duration,
                Category = timer.Category,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Timer created: {name} ({duration}s) - ID: {timerId}", "Timer");
            return timerId;
        }

        // Спеціальні методи для різних категорій
        public string CreateLobbyTimer(float duration, Action onComplete = null)
        {
            return CreateTimer("LobbySelection", duration, onComplete);
        }

        public string CreatePhaseTimer(GamePhase phase, float duration, Action onComplete = null)
        {
            return CreateTimer($"Phase_{phase}", duration, onComplete);
        }

        public string CreateCombatTimer(string combatId, float duration, Action onComplete = null)
        {
            return CreateTimer($"Combat_{combatId}", duration, onComplete);
        }

        // Решта методів інтерфейсу...
        public bool StartTimer(string timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsActive = true;
                timer.IsPaused = false;
                _logger.LogInfo($"Timer started: {timer.Name}", "Timer");
                return true;
            }
            return false;
        }

        public bool StopTimer(string timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsActive = false;
                _logger.LogInfo($"Timer stopped: {timer.Name}", "Timer");
                return true;
            }
            return false;
        }

        public bool PauseTimer(string timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsPaused = true;
                _logger.LogInfo($"Timer paused: {timer.Name}", "Timer");
                return true;
            }
            return false;
        }

        public bool ResumeTimer(string timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsPaused = false;
                _logger.LogInfo($"Timer resumed: {timer.Name}", "Timer");
                return true;
            }
            return false;
        }

        public void RemoveTimer(string timerId)
        {
            if (_timers.Remove(timerId))
            {
                _logger.LogInfo($"Timer removed: {timerId}", "Timer");
            }
        }

        public float GetRemainingTime(string timerId)
        {
            return _timers.TryGetValue(timerId, out var timer) ? timer.RemainingTime : 0f;
        }

        public float GetTotalTime(string timerId)
        {
            return _timers.TryGetValue(timerId, out var timer) ? timer.TotalTime : 0f;
        }

        public bool IsTimerActive(string timerId)
        {
            return _timers.TryGetValue(timerId, out var timer) && timer.IsActive;
        }

        public bool IsTimerPaused(string timerId)
        {
            return _timers.TryGetValue(timerId, out var timer) && timer.IsPaused;
        }

        public void PauseAllTimers()
        {
            foreach (var timer in _timers.Values)
            {
                timer.IsPaused = true;
            }
            _logger.LogInfo("All timers paused", "Timer");
        }

        public void ResumeAllTimers()
        {
            foreach (var timer in _timers.Values)
            {
                timer.IsPaused = false;
            }
            _logger.LogInfo("All timers resumed", "Timer");
        }

        public void ClearAllTimers()
        {
            _timers.Clear();
            _logger.LogInfo("All timers cleared", "Timer");
        }

        public Dictionary<string, TimerInfo> GetAllTimers()
        {
            var result = new Dictionary<string, TimerInfo>();

            foreach (var kvp in _timers)
            {
                var timer = kvp.Value;
                result[kvp.Key] = new TimerInfo
                {
                    Name = timer.Name,
                    RemainingTime = timer.RemainingTime,
                    TotalTime = timer.TotalTime,
                    IsActive = timer.IsActive,
                    IsPaused = timer.IsPaused,
                    CreatedAt = timer.CreatedAt,
                    Category = timer.Category
                };
            }

            return result;
        }

        private void PublishTimerUpdate(Timer timer)
        {
            // Різні події для різних категорій
            switch (timer.Category)
            {
                case "Lobby":
                    _eventThrottler.PublishThrottled(new SelectionTimerUpdatedEvent
                    {
                        RemainingTime = timer.RemainingTime,
                        Timestamp = DateTime.UtcNow
                    });
                    break;

                case "Phase":
                    _eventThrottler.PublishThrottled(new PhaseUpdateEvent
                    {
                        Phase = ExtractPhaseFromName(timer.Name),
                        ElapsedTime = timer.TotalTime - timer.RemainingTime,
                        RemainingTime = timer.RemainingTime,
                        TotalDuration = timer.TotalTime,
                        Timestamp = DateTime.UtcNow
                    });
                    break;

                default:
                    _eventThrottler.PublishThrottled(new TimerUpdatedEvent
                    {
                        TimerId = timer.Id,
                        TimerName = timer.Name,
                        RemainingTime = timer.RemainingTime,
                        TotalTime = timer.TotalTime,
                        Category = timer.Category,
                        Timestamp = DateTime.UtcNow
                    });
                    break;
            }
        }

        private string DetermineCategory(string timerName)
        {
            if (timerName.Contains("Lobby"))
                return "Lobby";
            if (timerName.Contains("Phase"))
                return "Phase";
            if (timerName.Contains("Combat"))
                return "Combat";
            return "General";
        }

        private GamePhase ExtractPhaseFromName(string timerName)
        {
            if (timerName.Contains("Rune"))
                return GamePhase.Rune;
            if (timerName.Contains("Planning"))
                return GamePhase.Planning;
            if (timerName.Contains("Active"))
                return GamePhase.Active;
            if (timerName.Contains("Freeze"))
                return GamePhase.Freeze;
            return GamePhase.None;
        }

        // Внутрішній клас таймера
        private class Timer
        {
            public string Id
            {
                get; set;
            }
            public string Name
            {
                get; set;
            }
            public float TotalTime
            {
                get; set;
            }
            public float RemainingTime
            {
                get; set;
            }
            public bool IsActive
            {
                get; set;
            }
            public bool IsPaused
            {
                get; set;
            }
            public bool IsPersistent { get; set; } = false;
            public Action OnComplete
            {
                get; set;
            }
            public DateTime CreatedAt
            {
                get; set;
            }
            public string Category
            {
                get; set;
            }
        }
    }
}
