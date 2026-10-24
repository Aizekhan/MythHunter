// Шлях: Assets/_MythHunter/Code/Core/Installers/LazyDependencyInjectorInstaller.cs
using RPGFramework.DI;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// [OBSOLETE] LazyDependencyInjector installer - now integrated into InfrastructureModule.
    /// This class will be removed in a future version.
    /// </summary>
    [System.Obsolete("Use InfrastructureModule instead.", false)]
    public class LazyDependencyInjectorInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            // Отримуємо логер
            var logger = container.Resolve<IMythLogger>();

            // Cтворюємо GameObject, який буде містити LazyDependencyInjector
            var lazyDIGameObject = new GameObject("LazyDependencyInjector");
            Object.DontDestroyOnLoad(lazyDIGameObject);

            // Додаємо компонент LazyDependencyInjector до GameObject
            var lazyDI = lazyDIGameObject.AddComponent<LazyDependencyInjector>();

            // Ініціалізуємо LazyDependencyInjector
            lazyDI.Initialize(container, logger);

            // Реєструємо LazyDependencyInjector в контейнері як IDependencyInjector
            container.RegisterInstance<IDependencyInjector>(lazyDI);

            logger.LogInfo("LazyDependencyInjector успішно зареєстровано у контейнері", "DI");
        }
    }
}
