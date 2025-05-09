// Шлях: Assets/_MythHunter/Code/Debug/Core/DebugToolBase.cs
using System;
using System.Collections.Generic;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Debug.Core
{
    /// <summary>
    /// Базовий клас для всіх інструментів відлагодження з реалізацією спільної функціональності
    /// </summary>
    public abstract class DebugToolBase : IDebugTool
    {
        // Базові властивості
        public string ToolName
        {
            get; protected set;
        }
        public string ToolCategory
        {
            get; protected set;
        }
        public bool IsEnabled { get; set; } = true;

        // Логування
        protected readonly IMythLogger _logger;

        // Спільні дані
        protected readonly Queue<string> _logEntries = new Queue<string>(100);
        protected readonly Dictionary<string, object> _statistics = new Dictionary<string, object>();
        protected readonly Dictionary<string, bool> _sectionFoldouts = new Dictionary<string, bool>();

        // Дані для рендерингу
        protected Vector2 _scrollPosition;
        protected bool _isVisible = false;

        // Спільний формат часу
        protected string TimeFormat => "HH:mm:ss.fff";

        protected DebugToolBase(string toolName, string toolCategory, IMythLogger logger)
        {
            ToolName = toolName;
            ToolCategory = toolCategory;
            _logger = logger;
        }

        // Базова реалізація методів IDebugTool
        public virtual void Initialize()
        {
            _logger?.LogInfo($"{ToolName} initialized", ToolCategory);
        }

        public virtual void Update()
        {
        }

        public virtual void Dispose()
        {
            _logger?.LogInfo($"{ToolName} disposed", ToolCategory);
        }

        public virtual Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>(_statistics);
        }

        public virtual string[] GetLogEntries(int maxCount = 100)
        {
            string[] result = new string[Math.Min(_logEntries.Count, maxCount)];
            _logEntries.CopyTo(result, 0);
            return result;
        }

        public virtual void RenderGUI(Rect area)
        {
            if (!IsEnabled)
                return;

            GUILayout.BeginArea(area);

            // Заголовок інструменту
            GUILayout.BeginVertical("box");
            GUILayout.Label(ToolName, GUI.skin.box);

            // Кнопки для управління
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                RefreshData();
            }

            IsEnabled = GUILayout.Toggle(IsEnabled, "Enabled", GUILayout.Width(80));

            GUILayout.EndHorizontal();

            // Контент
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            RenderContent();

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        // Методи для нащадків
        protected virtual void RenderContent()
        {
            // Базова реалізація - відображення статистики
            foreach (var stat in _statistics)
            {
                GUILayout.Label($"{stat.Key}: {stat.Value}");
            }

            // Відображення логів
            if (_logEntries.Count > 0)
            {
                if (DrawFoldout("Logs"))
                {
                    foreach (var log in _logEntries)
                    {
                        GUILayout.Label(log);
                    }
                }
            }
        }

        protected virtual void RefreshData()
        {
            _logger?.LogDebug($"Refreshing {ToolName} data", ToolCategory);
        }

        // Спільні методи для всіх інструментів
        protected void AddLogEntry(string entry)
        {
            _logEntries.Enqueue($"[{DateTime.Now.ToString(TimeFormat)}] {entry}");

            // Обмеження кількості записів
            while (_logEntries.Count > 100)
            {
                _logEntries.Dequeue();
            }
        }

        protected void ClearLogs()
        {
            _logEntries.Clear();
            _logger?.LogInfo($"Cleared logs for {ToolName}", ToolCategory);
        }

        protected void UpdateStatistic(string key, object value)
        {
            _statistics[key] = value;
        }

        protected void ClearStatistics()
        {
            _statistics.Clear();
            _logger?.LogInfo($"Cleared statistics for {ToolName}", ToolCategory);
        }

        // Спільні елементи інтерфейсу
        protected bool DrawFoldout(string title)
        {
            if (!_sectionFoldouts.TryGetValue(title, out bool isOpen))
            {
                isOpen = false;
                _sectionFoldouts[title] = isOpen;
            }

            bool newValue = GUILayout.Toggle(isOpen, title, GUI.skin.box);

            if (newValue != isOpen)
            {
                _sectionFoldouts[title] = newValue;
            }

            return _sectionFoldouts[title];
        }
    }
}
