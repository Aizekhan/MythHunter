using MythHunter.Core.ECS;
using System;
using System.Collections.Generic;

namespace MythHunter.Systems.Core
{
    public interface ITimerSystem : ISystem
    {
        // Створення таймерів
        string CreateTimer(string name, float duration, Action onComplete = null, bool autoStart = true);

        // Управління таймерами
        bool StartTimer(string timerId);
        bool StopTimer(string timerId);
        bool PauseTimer(string timerId);
        bool ResumeTimer(string timerId);
        void RemoveTimer(string timerId);

        // Отримання інформації
        float GetRemainingTime(string timerId);
        float GetTotalTime(string timerId);
        bool IsTimerActive(string timerId);
        bool IsTimerPaused(string timerId);

        // Batch операції
        void PauseAllTimers();
        void ResumeAllTimers();
        void ClearAllTimers();

        // Статистика
        Dictionary<string, TimerInfo> GetAllTimers();
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
