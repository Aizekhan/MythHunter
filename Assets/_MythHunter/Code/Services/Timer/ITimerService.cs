// Assets/_MythHunter/Code/Services/Timer/ITimerService.cs
using System;
using System.Collections.Generic;

namespace MythHunter.Services.Timer
{
    /// <summary>
    /// EPIC 08 Phase 2: Timer service extracted from TimerSystem
    /// Provides timer functionality without system coupling
    /// </summary>
    public interface ITimerService
    {
        // Timer creation
        string CreateTimer(string name, float duration, Action onComplete = null, bool autoStart = true);

        // Timer management
        bool StartTimer(string timerId);
        bool StopTimer(string timerId);
        bool PauseTimer(string timerId);
        bool ResumeTimer(string timerId);
        void RemoveTimer(string timerId);

        // Timer information
        float GetRemainingTime(string timerId);
        float GetTotalTime(string timerId);
        bool IsTimerActive(string timerId);
        bool IsTimerPaused(string timerId);

        // Batch operations
        void PauseAllTimers();
        void ResumeAllTimers();
        void ClearAllTimers();

        // Statistics
        Dictionary<string, TimerInfo> GetAllTimers();

        /// <summary>
        /// Update all active timers (call from Update loop)
        /// </summary>
        void UpdateTimers(float deltaTime);
    }

    public struct TimerInfo
    {
        public string Name;
        public float RemainingTime;
        public float TotalTime;
        public bool IsActive;
        public bool IsPaused;
        public DateTime CreatedAt;
        public string Category; // Lobby, Phase, Combat, etc.
    }
}
