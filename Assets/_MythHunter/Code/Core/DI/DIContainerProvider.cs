// Шлях: Assets/_MythHunter/Code/Core/DI/DIContainerProvider.cs
using UnityEngine;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Провайдер для доступу до DI контейнера
    /// </summary>
    public static class DIContainerProvider
    {
        private static IDIContainer _container;

        /// <summary>
        /// Встановлює глобальний контейнер DI
        /// </summary>
        public static void SetContainer(IDIContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Отримує глобальний контейнер DI
        /// </summary>
        public static IDIContainer GetContainer()
        {
            if (_container == null)
            {
                // Використання статичного методу з нашої фабрики логерів
                MythLoggerFactory.GetDefaultLogger().LogWarning("DI container is not initialized", "DI");
            }
            return _container;
        }
    }
}
