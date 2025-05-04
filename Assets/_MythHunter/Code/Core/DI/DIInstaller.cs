// Файл: Assets/_MythHunter/Code/Core/DI/DIInstaller.cs
using System;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Базовий клас для інсталяторів залежностей
    /// </summary>
    public abstract class DIInstaller : IDIInstaller
    {
        public abstract void InstallBindings(IDIContainer container);

        protected void BindSingleton<TService, TImplementation>(IDIContainer container)
            where TImplementation : TService
        {
            container.RegisterSingleton<TService, TImplementation>();
        }

        protected void Bind<TService, TImplementation>(IDIContainer container)
            where TImplementation : TService
        {
            container.Register<TService, TImplementation>();
        }

        protected void BindInstance<TService>(IDIContainer container, TService instance)
        {
            container.RegisterInstance<TService>(instance);
        }

        /// <summary>
        /// Виконує інсталяцію з логуванням початку і кінця
        /// </summary>
        protected void InstallWithLogging(IDIContainer container, string subsystemName, Action<IDIContainer> installAction)
        {
            var logger = container.Resolve<Utils.Logging.IMythLogger>();
            logger.LogInfo($"Встановлення залежностей {subsystemName}...", "Installer");

            installAction(container);

            logger.LogInfo($"Встановлення залежностей {subsystemName} завершено", "Installer");
        }
    }
}
