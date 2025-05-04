// Шлях: Assets/_MythHunter/Code/Debug/UI/DebugDashboard.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using MythHunter.Debug.Core;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.Core.MonoBehaviours;

namespace MythHunter.Debug.UI
{
    /// <summary>
    /// Головний дашборд для всіх інструментів відлагодження
    /// </summary>
    public class DebugDashboard : InjectableMonoBehaviour
    {
        [Inject] private IMythLogger _logger;
        [Inject] private DebugToolFactory _toolFactory;

        [SerializeField] private bool _showOnStart = false;
        [SerializeField] private KeyCode _toggleKey = KeyCode.F9;

        private readonly List<IDebugTool> _registeredTools = new List<IDebugTool>();
        private bool _isVisible = false;
        private int _selectedTabIndex = 0;
        private string[] _tabNames = new string[0];

        protected override void OnInjectionsCompleted()
        {
            base.OnInjectionsCompleted();
            _isVisible = _showOnStart;

            // Збираємо всі зареєстровані інструменти
            CollectDebugTools();

            _logger?.LogInfo("Debug Dashboard initialized", "Debug");
        }

        private void CollectDebugTools()
        {
            try
            {
                // Створюємо всі інструменти через фабрику
                var tools = _toolFactory.CreateAllTools();

                foreach (var tool in tools)
                {
                    RegisterTool(tool);
                }

                // Оновлюємо вкладки
                UpdateTabNames();

                _logger?.LogInfo($"Collected {_registeredTools.Count} debug tools", "Debug");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error collecting debug tools: {ex.Message}", "Debug", ex);
            }
        }

        /// <summary>
        /// Реєструє інструмент в дашборді
        /// </summary>
        public void RegisterTool(IDebugTool tool)
        {
            if (tool == null)
                return;

            // Перевіряємо, чи інструмент вже зареєстрований
            if (_registeredTools.Exists(t => t.GetType() == tool.GetType()))
            {
                _logger?.LogWarning($"Debug tool {tool.ToolName} already registered", "Debug");
                return;
            }

            // Ініціалізуємо інструмент
            try
            {
                tool.Initialize();
                _registeredTools.Add(tool);

                // Оновлюємо вкладки
                UpdateTabNames();

                _logger?.LogInfo($"Registered debug tool: {tool.ToolName}", "Debug");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error initializing debug tool {tool.ToolName}: {ex.Message}", "Debug", ex);
            }
        }

        /// <summary>
        /// Видаляє інструмент з дашборду
        /// </summary>
        public void UnregisterTool(IDebugTool tool)
        {
            if (tool == null)
                return;

            // Знаходимо інструмент
            int index = _registeredTools.IndexOf(tool);
            if (index >= 0)
            {
                try
                {
                    // Звільняємо ресурси
                    _registeredTools[index].Dispose();
                    _registeredTools.RemoveAt(index);

                    // Оновлюємо вкладки
                    UpdateTabNames();

                    // Коригуємо індекс вкладки
                    if (_selectedTabIndex >= _registeredTools.Count)
                    {
                        _selectedTabIndex = Math.Max(0, _registeredTools.Count - 1);
                    }

                    _logger?.LogInfo($"Unregistered debug tool: {tool.ToolName}", "Debug");
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error unregistering debug tool {tool.ToolName}: {ex.Message}", "Debug", ex);
                }
            }
        }

        private void UpdateTabNames()
        {
            _tabNames = new string[_registeredTools.Count];

            for (int i = 0; i < _registeredTools.Count; i++)
            {
                _tabNames[i] = _registeredTools[i].ToolName;
            }
        }

        private void Update()
        {
            // Переключення видимості панелі
            if (Input.GetKeyDown(_toggleKey))
            {
                _isVisible = !_isVisible;
                _logger?.LogInfo($"Debug dashboard {(_isVisible ? "shown" : "hidden")}", "Debug");
            }

            // Оновлення всіх інструментів
            foreach (var tool in _registeredTools)
            {
                if (tool.IsEnabled)
                {
                    tool.Update();
                }
            }
        }

        private void OnGUI()
        {
            if (!_isVisible)
                return;

            // Обчислення розмірів
            float width = Math.Min(1000, Screen.width - 40);
            float height = Math.Min(800, Screen.height - 40);
            float x = (Screen.width - width) / 2;
            float y = (Screen.height - height) / 2;

            // Основна панель
            GUI.Box(new Rect(x, y, width, height), "");

            // Заголовок
            GUI.Box(new Rect(x, y, width, 30), "MythHunter Debug Dashboard");

            // Вкладки
            if (_registeredTools.Count > 0)
            {
                int newTab = GUI.Toolbar(new Rect(x + 10, y + 40, width - 20, 30), _selectedTabIndex, _tabNames);

                if (newTab != _selectedTabIndex)
                {
                    _selectedTabIndex = newTab;
                    _logger?.LogDebug($"Switched to debug tool: {_tabNames[_selectedTabIndex]}", "Debug");
                }

                // Кнопка закриття
                if (GUI.Button(new Rect(x + width - 90, y + 40, 80, 20), "Close"))
                {
                    _isVisible = false;
                }

                // Відображення вибраного інструменту
                if (_selectedTabIndex >= 0 && _selectedTabIndex < _registeredTools.Count)
                {
                    var selectedTool = _registeredTools[_selectedTabIndex];

                    // Область для інструменту
                    var toolArea = new Rect(x + 10, y + 80, width - 20, height - 90);
                    selectedTool.RenderGUI(toolArea);
                }
            }
            else
            {
                // Повідомлення, якщо інструментів немає
                GUI.Label(new Rect(x + 10, y + 80, width - 20, 30), "No debug tools registered");

                // Кнопка закриття
                if (GUI.Button(new Rect(x + width - 90, y + 40, 80, 20), "Close"))
                {
                    _isVisible = false;
                }
            }
        }

        private void OnDestroy()
        {
            // Звільнення ресурсів усіх інструментів
            foreach (var tool in _registeredTools)
            {
                try
                {
                    tool.Dispose();
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"Error disposing debug tool {tool.ToolName}: {ex.Message}", "Debug");
                }
            }

            _registeredTools.Clear();
            _logger?.LogInfo("Debug Dashboard disposed", "Debug");
        }
    }
}
