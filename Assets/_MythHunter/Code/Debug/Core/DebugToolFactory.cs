// Шлях: Assets/_MythHunter/Code/Debug/Core/DebugToolFactory.cs
using System;
using System.Collections.Generic;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.Debug.Core
{
    /// <summary>
    /// Фабрика для створення інструментів відлагодження
    /// </summary>
    public class DebugToolFactory
    {
        private readonly IDIContainer _container;
        private readonly IMythLogger _logger;
        private readonly Dictionary<Type, Type> _implementationTypes = new Dictionary<Type, Type>();

        [Inject]
        public DebugToolFactory(IDIContainer container, IMythLogger logger)
        {
            _container = container;
            _logger = logger;
        }

        /// <summary>
        /// Реєструє тип реалізації для інтерфейсу інструменту
        /// </summary>
        public void RegisterImplementation<TInterface, TImplementation>()
            where TInterface : IDebugTool
            where TImplementation : IDebugTool
        {
            _implementationTypes[typeof(TInterface)] = typeof(TImplementation);
            _logger?.LogDebug($"Registered debug tool implementation: {typeof(TImplementation).Name}", "Debug");
        }

        /// <summary>
        /// Створює інструмент вказаного типу
        /// </summary>
        public TInterface Create<TInterface>() where TInterface : IDebugTool
        {
            Type interfaceType = typeof(TInterface);

            if (_implementationTypes.TryGetValue(interfaceType, out Type implementationType))
            {
                try
                {
                    return (TInterface)_container.Resolve(implementationType);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error creating debug tool {implementationType.Name}: {ex.Message}", "Debug", ex);
                }
            }
            else
            {
                _logger?.LogWarning($"No implementation registered for debug tool interface {interfaceType.Name}", "Debug");
            }

            return default;
        }

        /// <summary>
        /// Перевіряє, чи зареєстрована реалізація для інтерфейсу
        /// </summary>
        public bool IsImplementationRegistered<TInterface>() where TInterface : IDebugTool
        {
            return _implementationTypes.ContainsKey(typeof(TInterface));
        }

        /// <summary>
        /// Створює всі зареєстровані інструменти
        /// </summary>
        public List<IDebugTool> CreateAllTools()
        {
            List<IDebugTool> tools = new List<IDebugTool>();

            foreach (var pair in _implementationTypes)
            {
                try
                {
                    var tool = (IDebugTool)_container.Resolve(pair.Value);
                    tools.Add(tool);
                    _logger?.LogDebug($"Created debug tool: {tool.ToolName}", "Debug");
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error creating debug tool {pair.Value.Name}: {ex.Message}", "Debug", ex);
                }
            }

            return tools;
        }
    }
}
