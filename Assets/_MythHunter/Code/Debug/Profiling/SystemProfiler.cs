// SystemProfiler.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;

namespace MythHunter.Debug.Profiling
{
    /// <summary>
    /// Профайлер для вимірювання продуктивності систем
    /// </summary>
    public class SystemProfiler
    {
        private readonly IMythLogger _logger;
        private readonly Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>();
        private readonly Dictionary<string, List<long>> _timings = new Dictionary<string, List<long>>();

        public SystemProfiler(IMythLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Починає вимірювання для системи
        /// </summary>
        public void Begin(string systemName)
        {
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
            if (!_stopwatches.TryGetValue(systemName, out var stopwatch))
            {
                _logger.LogWarning($"No profiling session was started for system: {systemName}", "Profiling");
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
            if (systemTimings.Count > 100)
            {
                systemTimings.RemoveAt(0);
            }

            return elapsedMs;
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
        /// Отримує повну статистику для всіх систем
        /// </summary>
        public Dictionary<string, SystemProfileStats> GetAllStats()
        {
            var result = new Dictionary<string, SystemProfileStats>();

            foreach (var kvp in _timings)
            {
                string systemName = kvp.Key;
                List<long> timings = kvp.Value;

                if (timings.Count == 0)
                    continue;

                long min = long.MaxValue;
                long max = 0;
                long sum = 0;

                foreach (var timing in timings)
                {
                    if (timing < min)
                        min = timing;
                    if (timing > max)
                        max = timing;
                    sum += timing;
                }

                double avg = (double)sum / timings.Count;

                result[systemName] = new SystemProfileStats
                {
                    MinMs = min,
                    MaxMs = max,
                    AvgMs = avg,
                    LastMs = timings[timings.Count - 1],
                    SampleCount = timings.Count
                };
            }

            return result;
        }

        /// <summary>
        /// Очищує всі вимірювання
        /// </summary>
        public void Reset()
        {
            _timings.Clear();
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
