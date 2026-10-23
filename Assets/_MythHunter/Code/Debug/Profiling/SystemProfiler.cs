// Шлях: Assets/_MythHunter/Code/Debug/Profiling/SystemProfiler.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;
using MythHunter.Debug.Core;
using UnityEngine;

namespace MythHunter.Debug.Profiling
{
    /// <summary>
    /// Профайлер для вимірювання продуктивності систем
    /// </summary>
    public class SystemProfiler : DebugToolBase
    {
        private readonly Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>();
        private readonly Dictionary<string, List<long>> _timings = new Dictionary<string, List<long>>();

        // Кількість останніх зразків для обчислення середнього значення
        private readonly int _maxSamples = 100;

        // Поріг для визначення "повільних" систем (мс)
        private readonly long _slowThreshold = 16; // ~60 FPS

        public SystemProfiler(IMythLogger logger)
            : base("System Profiler", "Profiling", logger)
        {
            // Ініціалізація статистики
            UpdateStatistic("TotalSystems", 0);
            UpdateStatistic("ActiveSystems", 0);
            UpdateStatistic("SlowSystems", 0);
        }

        /// <summary>
        /// Починає вимірювання для системи
        /// </summary>
        public void Begin(string systemName)
        {
            if (!IsEnabled)
                return;

            if (!_stopwatches.TryGetValue(systemName, out var stopwatch))
            {
                stopwatch = new Stopwatch();
                _stopwatches[systemName] = stopwatch;
            }

            stopwatch.Restart();
        }

        /// <summary>
        /// Завершує вимірювання для системи
        /// </summary>
        public long End(string systemName)
        {
            if (!IsEnabled)
                return 0;

            if (!_stopwatches.TryGetValue(systemName, out var stopwatch))
            {
                _logger?.LogWarning($"No profiling session was started for system: {systemName}", "Profiling");
                return 0;
            }

            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;

            // Зберігаємо результат
            if (!_timings.TryGetValue(systemName, out var systemTimings))
            {
                systemTimings = new List<long>();
                _timings[systemName] = systemTimings;
            }

            systemTimings.Add(elapsedMs);

            // Обмежуємо кількість збережених вимірювань
            if (systemTimings.Count > _maxSamples)
            {
                systemTimings.RemoveAt(0);
            }

            // Оновлюємо статистику
            UpdateProfileStats();

            return elapsedMs;
        }

        private void UpdateProfileStats()
        {
            int totalSystems = _timings.Count;
            int activeSystems = 0;
            int slowSystems = 0;

            foreach (var timing in _timings)
            {
                if (timing.Value.Count > 0)
                {
                    activeSystems++;

                    // Перевіряємо останнє вимірювання
                    if (timing.Value[timing.Value.Count - 1] > _slowThreshold)
                    {
                        slowSystems++;
                    }
                }
            }

            UpdateStatistic("TotalSystems", totalSystems);
            UpdateStatistic("ActiveSystems", activeSystems);
            UpdateStatistic("SlowSystems", slowSystems);
        }

        /// <summary>
        /// Отримує середній час виконання системи
        /// </summary>
        public double GetAverageTime(string systemName)
        {
            if (!_timings.TryGetValue(systemName, out var systemTimings) || systemTimings.Count == 0)
            {
                return 0;
            }

            long sum = 0;
            foreach (var timing in systemTimings)
            {
                sum += timing;
            }

            return (double)sum / systemTimings.Count;
        }

        /// <summary>
        /// Отримує максимальний час виконання системи
        /// </summary>
        public long GetMaxTime(string systemName)
        {
            if (!_timings.TryGetValue(systemName, out var systemTimings) || systemTimings.Count == 0)
            {
                return 0;
            }

            long max = 0;
            foreach (var timing in systemTimings)
            {
                if (timing > max)
                {
                    max = timing;
                }
            }

            return max;
        }

        /// <summary>
        /// Очищує всі вимірювання
        /// </summary>
        public void Reset()
        {
            _timings.Clear();
            UpdateProfileStats();
            _logger?.LogInfo("System profiler reset", "Profiling");
        }

        // Перевизначення методів базового класу

        protected override void RenderContent()
        {
            // Кнопка для скидання
            if (GUILayout.Button("Reset All Timings", GUILayout.Width(150)))
            {
                Reset();
            }

            // Загальна статистика
            GUILayout.Label("Total Systems: " + _statistics["TotalSystems"], GUI.skin.box);
            GUILayout.Label("Active Systems: " + _statistics["ActiveSystems"], GUI.skin.box);
            GUILayout.Label("Slow Systems: " + _statistics["SlowSystems"], GUI.skin.box);

            // Відображення статистики по системах
            if (DrawFoldout("System Performance"))
            {
                foreach (var pair in _timings)
                {
                    string systemName = pair.Key;
                    var timingData = pair.Value;

                    if (timingData.Count == 0)
                        continue;

                    long lastTiming = timingData[timingData.Count - 1];
                    double avgTiming = GetAverageTime(systemName);
                    long maxTiming = GetMaxTime(systemName);

                    string color = lastTiming > _slowThreshold ? "red" : "white";

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"<color={color}>{systemName}</color>", GUI.skin.box, GUILayout.Width(200));
                    GUILayout.Label($"Last: {lastTiming}ms, Avg: {avgTiming:F2}ms, Max: {maxTiming}ms");
                    GUILayout.EndHorizontal();
                }
            }

            // Відображення логів
            base.RenderContent();
        }

        protected override void RefreshData()
        {
            UpdateProfileStats();
            base.RefreshData();
        }
    }

    /// <summary>
    /// Статистика профілювання для системи
    /// </summary>
    public struct SystemProfileStats
    {
        public long MinMs;
        public long MaxMs;
        public double AvgMs;
        public long LastMs;
        public int SampleCount;

        public override string ToString()
        {
            return $"Min: {MinMs}ms, Max: {MaxMs}ms, Avg: {AvgMs:F2}ms, Last: {LastMs}ms, Samples: {SampleCount}";
        }
    }
}
