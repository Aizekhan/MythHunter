// Шлях: Assets/_MythHunter/Code/Debug/Core/IDebugTool.cs
using System;
using System.Collections.Generic;

namespace MythHunter.Debug.Core
{
    /// <summary>
    /// Базовий інтерфейс для всіх інструментів відлагодження
    /// </summary>
    public interface IDebugTool
    {
        string ToolName
        {
            get;
        }
        string ToolCategory
        {
            get;
        }
        bool IsEnabled
        {
            get; set;
        }

        void Initialize();
        void Update();
        void Dispose();

        // Методи для збору статистики
        Dictionary<string, object> GetStatistics();
        string[] GetLogEntries(int maxCount = 100);

        // Метод для рендерингу даних
        void RenderGUI(UnityEngine.Rect area);
    }
}
