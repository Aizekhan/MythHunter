// Assets/_MythHunter/Code/Services/Timer/TimerService.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Utils.Logging;

namespace MythHunter.Services.Timer
{
    /// <summary>
    /// EPIC 08 Phase 2: Timer service - decoupled from system architecture
    /// Provides timer functionality as a standalone service
    /// </summary>
    public class TimerService : ITimerService
    {
        private readonly Dictionary<string, Timer> _timers = new();
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly IEventThrottler _eventThrottler;

        [Inject]
        public TimerService(
            IEventBus eventBus,
            IMythLogger logger,
            IEventThrottler eventThrottler)
        {
            _eventBus = eventBus;
            _logger = logger;
            _eventThrottler = eventThrottler;

            // Register throttling for different timer types
            _eventThrottler.RegisterThrottle<TimerUpdatedEvent>(0.25f); // 4 times per second
            _eventThrottler.RegisterThrottle<PhaseUpdateEvent>(0.1f);   // 10 times per second for phases
        }

        public void UpdateTimers(float deltaTime)
        {
            var completedTimers = new List<string>();

            foreach (var kvp in _timers)
            {
                var timer = kvp.Value;

                if (!timer.IsActive || timer.IsPaused)
                    continue;

                timer.RemainingTime -= deltaTime;

                // Publish update with throttling
                PublishTimerUpdate(timer);

                // Check completion
                if (timer.RemainingTime <= 0)
                {
                    timer.RemainingTime = 0;
                    timer.IsActive = false;

                    // Call completion callback
                    try
                    {
                        timer.OnComplete?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in timer callback for '{timer.Name}': {ex.Message}", "TimerService", ex);
                    }

                    // Publish completion event
                    _eventBus.Publish(new TimerCompletedEvent
                    {
                        TimerId = timer.Id,
                        TimerName = timer.Name,
                        Category = timer.Category,
                        Timestamp = DateTime.UtcNow
                    });

                    completedTimers.Add(kvp.Key);
                }
            }

            // Remove completed one-time timers
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
                IsPersistent = false,
                CreatedAt = DateTime.UtcNow,
                Category = DetermineCategory(name)
            };

            _timers[timerId] = timer;

            _eventBus.Publish(new TimerCreatedEvent
            {
                TimerId = timerId,
                TimerName = name,
                Duration = duration,
                Category = timer.Category,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Timer created: {name} ({duration}s) - ID: {timerId}", "TimerService");
            return timerId;
        }

        public bool StartTimer(string timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsActive = true;
                timer.IsPaused = false;
                _logger.LogInfo($"Timer started: {timer.Name}", "TimerService");
                return true;
            }
            _logger.LogWarning($"Timer not found: {timerId}", "TimerService");
            return false;
        }

        public bool StopTimer(string timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsActive = false;
                _logger.LogInfo($"Timer stopped: {timer.Name}", "TimerService");
                return true;
            }
            _logger.LogWarning($"Timer not found: {timerId}", "TimerService");
            return false;
        }

        public bool PauseTimer(string timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsPaused = true;
                _logger.LogInfo($"Timer paused: {timer.Name}", "TimerService");
                return true;
            }
            _logger.LogWarning($"Timer not found: {timerId}", "TimerService");
            return false;
        }

        public bool ResumeTimer(string timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsPaused = false;
                _logger.LogInfo($"Timer resumed: {timer.Name}", "TimerService");
                return true;
            }
            _logger.LogWarning($"Timer not found: {timerId}", "TimerService");
            return false;
        }

        public void RemoveTimer(string timerId)
        {
            if (_timers.Remove(timerId))
            {
                _logger.LogInfo($"Timer removed: {timerId}", "TimerService");
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
            _logger.LogInfo("All timers paused", "TimerService");
        }

        public void ResumeAllTimers()
        {
            foreach (var timer in _timers.Values)
            {
                timer.IsPaused = false;
            }
            _logger.LogInfo("All timers resumed", "TimerService");
        }

        public void ClearAllTimers()
        {
            _timers.Clear();
            _logger.LogInfo("All timers cleared", "TimerService");
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
            // Different events for different categories
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

        /// <summary>
        /// Internal timer class
        /// </summary>
        private class Timer
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public float TotalTime { get; set; }
            public float RemainingTime { get; set; }
            public bool IsActive { get; set; }
            public bool IsPaused { get; set; }
            public bool IsPersistent { get; set; }
            public Action OnComplete { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Category { get; set; }
        }
    }
}
