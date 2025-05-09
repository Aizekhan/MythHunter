// Шлях: Assets/_MythHunter/Code/Core/DI/DILifecycleManager.cs
using System;
using System.Collections.Generic;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Менеджер життєвого циклу системи DI
    /// </summary>
    public class DILifecycleManager
    {
        private readonly IDIContainer _container;
        private readonly IMythLogger _logger;
        private readonly List<DIScope> _scopes = new List<DIScope>();
        private readonly List<Action> _shutdownHandlers = new List<Action>();

        public DILifecycleManager(IDIContainer container, IMythLogger logger)
        {
            _container = container;
            _logger = logger;
        }

        /// <summary>
        /// Створює новий скоуп і реєструє його для автоматичного відслідковування
        /// </summary>
        public DIScope CreateTrackedScope(string scopeId = null)
        {
            var scope = _container.CreateScope(scopeId);
            _scopes.Add(scope);

            _logger.LogDebug($"Created tracked scope {scope.ScopeId}", "DI");

            return scope;
        }

        /// <summary>
        /// Звільняє всі відстежувані скоупи
        /// </summary>
        public void DisposeAllScopes()
        {
            foreach (var scope in _scopes)
            {
                try
                {
                    scope.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error disposing scope {scope.ScopeId}: {ex.Message}", "DI", ex);
                }
            }

            _scopes.Clear();
            _logger.LogInfo("All tracked scopes disposed", "DI");
        }

        /// <summary>
        /// Додає обробник завершення роботи
        /// </summary>
        public void AddShutdownHandler(Action handler)
        {
            if (handler != null)
            {
                _shutdownHandlers.Add(handler);
            }
        }

        /// <summary>
        /// Викликає всі обробники завершення роботи
        /// </summary>
        public void ExecuteShutdownHandlers()
        {
            foreach (var handler in _shutdownHandlers)
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error executing shutdown handler: {ex.Message}", "DI", ex);
                }
            }

            _shutdownHandlers.Clear();
            _logger.LogInfo("All shutdown handlers executed", "DI");
        }

        /// <summary>
        /// Завершує роботу менеджера життєвого циклу
        /// </summary>
        public void Shutdown()
        {
            // Виконуємо всі обробники завершення роботи
            ExecuteShutdownHandlers();

            // Звільняємо всі скоупи
            DisposeAllScopes();

            _logger.LogInfo("DI lifecycle manager shutdown completed", "DI");
        }
    }
}
