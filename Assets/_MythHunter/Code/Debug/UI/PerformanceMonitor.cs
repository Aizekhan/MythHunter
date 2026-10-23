// Шлях: Assets/_MythHunter/Code/Debug/UI/PerformanceMonitor.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using MythHunter.Debug.Core;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using UnityEngine.Profiling;

namespace MythHunter.Debug.UI
{
    /// <summary>
    /// Моніторинг продуктивності Unity
    /// </summary>
    public class PerformanceMonitor : DebugToolBase
    {
        private float _updateInterval = 0.5f;
        private float _timer;
        private int _frameCount;
        private float _fps;
        private float _minFps = float.MaxValue;
        private float _maxFps = 0f;
        private readonly Queue<float> _fpsHistory = new Queue<float>(60);

        // Налаштування порогів для FPS
        private float _goodFpsThreshold = 55f;
        private float _okFpsThreshold = 30f;

        // Дані про пам'ять
        private long _totalAllocatedMemory;
        private long _totalReservedMemory;
        private long _gcMemory;

        [Inject]
        public PerformanceMonitor(IMythLogger logger)
            : base("Performance Monitor", "Performance", logger)
        {
            // Ініціалізація статистики
            UpdateStatistic("FPS", 0f);
            UpdateStatistic("Min FPS", 0f);
            UpdateStatistic("Max FPS", 0f);
            UpdateStatistic("Avg FPS", 0f);
            UpdateStatistic("Memory Used", "0 MB");
            UpdateStatistic("Memory Reserved", "0 MB");
            UpdateStatistic("GC Memory", "0 MB");
        }

        public override void Update()
        {
            if (!IsEnabled)
                return;

            _frameCount++;
            _timer += Time.unscaledDeltaTime;

            if (_timer >= _updateInterval)
            {
                // Обчислення FPS
                _fps = _frameCount / _timer;
                _timer = 0f;
                _frameCount = 0;

                // Оновлення екстремумів
                _minFps = Mathf.Min(_minFps, _fps);
                _maxFps = Mathf.Max(_maxFps, _fps);

                // Оновлення історії
                _fpsHistory.Enqueue(_fps);
                if (_fpsHistory.Count > 60)
                {
                    _fpsHistory.Dequeue();
                }

                // Обчислення середнього FPS
                float sum = 0;
                foreach (float f in _fpsHistory)
                {
                    sum += f;
                }
                float avgFps = _fpsHistory.Count > 0 ? sum / _fpsHistory.Count : 0;

                // Оновлення інформації про пам'ять
                _totalAllocatedMemory = GC.GetTotalMemory(false);
                _totalReservedMemory = (long)Profiler.GetTotalReservedMemoryLong();
                _gcMemory = (long)Profiler.GetMonoHeapSizeLong();

                // Оновлення статистики
                UpdateStatistic("FPS", _fps);
                UpdateStatistic("Min FPS", _minFps);
                UpdateStatistic("Max FPS", _maxFps);
                UpdateStatistic("Avg FPS", avgFps);
                UpdateStatistic("Memory Used", FormatMemorySize(_totalAllocatedMemory));
                UpdateStatistic("Memory Reserved", FormatMemorySize(_totalReservedMemory));
                UpdateStatistic("GC Memory", FormatMemorySize(_gcMemory));

                // Запис логу у випадку проблем з продуктивністю
                if (_fps < _okFpsThreshold)
                {
                    AddLogEntry($"Low FPS: {_fps:F1} | Memory: {FormatMemorySize(_totalAllocatedMemory)}");
                }
            }
        }

        protected override void RenderContent()
        {
            // Кнопка скидання
            if (GUILayout.Button("Reset Statistics", GUILayout.Width(150)))
            {
                ResetStats();
            }

            // Параметри FPS
            float currentFps = (float)_statistics["FPS"];
            string fpsColor = GetFpsColor(currentFps);

            // Відображення FPS
            GUILayout.BeginHorizontal();
            GUILayout.Label("FPS:", GUILayout.Width(100));
            GUILayout.Label($"<color={fpsColor}>{currentFps:F1}</color>");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Min FPS:", GUILayout.Width(100));
            GUILayout.Label($"{(float)_statistics["Min FPS"]:F1}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max FPS:", GUILayout.Width(100));
            GUILayout.Label($"{(float)_statistics["Max FPS"]:F1}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Avg FPS:", GUILayout.Width(100));
            GUILayout.Label($"{(float)_statistics["Avg FPS"]:F1}");
            GUILayout.EndHorizontal();

            // Розділювач
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

            // Відображення інформації про пам'ять
            GUILayout.BeginHorizontal();
            GUILayout.Label("Memory Used:", GUILayout.Width(150));
            GUILayout.Label(_statistics["Memory Used"].ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Memory Reserved:", GUILayout.Width(150));
            GUILayout.Label(_statistics["Memory Reserved"].ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("GC Memory:", GUILayout.Width(150));
            GUILayout.Label(_statistics["GC Memory"].ToString());
            GUILayout.EndHorizontal();

            // Додаткова інформація про середовище
            if (DrawFoldout("System Info"))
            {
                GUILayout.Label($"GPU: {SystemInfo.graphicsDeviceName}", GUI.skin.box);
                GUILayout.Label($"GPU Memory: {SystemInfo.graphicsMemorySize} MB", GUI.skin.box);
                GUILayout.Label($"CPU: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)", GUI.skin.box);
                GUILayout.Label($"RAM: {SystemInfo.systemMemorySize} MB", GUI.skin.box);
                GUILayout.Label($"Unity Version: {Application.unityVersion}", GUI.skin.box);
            }

            // Історія FPS та логи
            base.RenderContent();
        }

        private string GetFpsColor(float fps)
        {
            if (fps >= _goodFpsThreshold)
                return "green";
            if (fps >= _okFpsThreshold)
                return "yellow";
            return "red";
        }

        private static string FormatMemorySize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int index = 0;
            double size = bytes;

            while (size >= 1024 && index < suffixes.Length - 1)
            {
                size /= 1024;
                index++;
            }

            return $"{size:F2} {suffixes[index]}";
        }

        private void ResetStats()
        {
            _minFps = float.MaxValue;
            _maxFps = 0f;
            _fpsHistory.Clear();
            ClearLogs();

            _logger?.LogInfo("Performance statistics reset", "Performance");
        }
    }
}
