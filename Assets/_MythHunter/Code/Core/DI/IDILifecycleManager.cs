// Шлях: Assets/_MythHunter/Code/Core/DI/IDILifecycleManager.cs
using System;
using System.Collections.Generic;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Інтерфейс для менеджера життєвого циклу DI
    /// </summary>
    public interface IDILifecycleManager
    {
        /// <summary>
        /// Створює новий скоуп і реєструє його для автоматичного відслідковування
        /// </summary>
        DIScope CreateTrackedScope(string scopeId = null);

        /// <summary>
        /// Звільняє всі відстежувані скоупи
        /// </summary>
        void DisposeAllScopes();

        /// <summary>
        /// Додає обробник завершення роботи
        /// </summary>
        void AddShutdownHandler(Action handler);

        /// <summary>
        /// Викликає всі обробники завершення роботи
        /// </summary>
        void ExecuteShutdownHandlers();

        /// <summary>
        /// Завершує роботу менеджера життєвого циклу
        /// </summary>
        void Shutdown();
    }
}
