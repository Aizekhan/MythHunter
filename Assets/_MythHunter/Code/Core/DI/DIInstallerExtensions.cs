// Шлях: Assets/_MythHunter/Code/Core/DI/DIInstallerExtensions.cs
using MythHunter.Core.DI;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Розширення для DIInstaller
    /// </summary>
    public static class DIInstallerExtensions
    {
        /// <summary>
        /// Реєструє скоупний сервіс у контейнері
        /// </summary>
        public static void BindScoped<TService, TImplementation>(this DIInstaller installer, IDIContainer container)
            where TImplementation : TService
        {
            container.RegisterScoped<TService, TImplementation>();
        }

        /// <summary>
        /// Реєструє транзієнтний сервіс у контейнері
        /// </summary>
        public static void BindTransient<TService, TImplementation>(this DIInstaller installer, IDIContainer container)
            where TImplementation : TService
        {
            container.RegisterTransient<TService, TImplementation>();
        }

        /// <summary>
        /// Реєструє лінивий сінглтон у контейнері
        /// </summary>
        public static void BindLazySingleton<TService, TImplementation>(this DIInstaller installer, IDIContainer container)
            where TImplementation : TService
            where TService : class
        {
            container.RegisterLazySingleton<TService, TImplementation>();
        }

      

        /// <summary>
        /// Реєструє лінивий сінглтон з екземпляром у контейнері
        /// </summary>
        public static void BindLazySingleton<TService>(this DIInstaller installer, IDIContainer container, TService instance)
            where TService : class  // Додати обмеження
        {
            var lazy = new LazyDependency<TService>(() => instance);
            container.RegisterInstance<LazyDependency<TService>>(lazy);
        }
    }
}
