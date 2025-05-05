using System;
using System.Collections.Generic;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.Debug.Events;
using MythHunter.Debug.Profiling;
using MythHunter.Debug.UI;

namespace MythHunter.Debug.Core
{
    /// <summary>
    /// Фабрика для створення інструментів відлагодження
    /// </summary>
    public class DebugToolFactory
    {
        private readonly IMythLogger _logger;
        private readonly Dictionary<Type, object> _registeredTools = new Dictionary<Type, object>();

        [Inject]
        public DebugToolFactory(
            IMythLogger logger,
            SystemProfiler systemProfiler,
            PerformanceMonitor performanceMonitor,
            EventDebugTool eventDebugTool)
        {
            _logger = logger;

            // Зберігаємо зареєстровані інструменти
            _registeredTools[typeof(SystemProfiler)] = systemProfiler;
            _registeredTools[typeof(PerformanceMonitor)] = performanceMonitor;
            _registeredTools[typeof(EventDebugTool)] = eventDebugTool;
        }

        /// <summary>
        /// Створює всі зареєстровані інструменти
        /// </summary>
        public List<IDebugTool> CreateAllTools()
        {
            List<IDebugTool> tools = new List<IDebugTool>();

            foreach (var tool in _registeredTools.Values)
            {
                try
                {
                    if (tool is IDebugTool debugTool)
                    {
                        tools.Add(debugTool);
                        _logger?.LogDebug($"Added debug tool: {debugTool.ToolName}", "Debug");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error creating debug tool: {ex.Message}", "Debug", ex);
                }
            }

            return tools;
        }
    }
}
