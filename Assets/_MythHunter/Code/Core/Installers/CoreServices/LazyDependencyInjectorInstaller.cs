// Шлях: Assets/_MythHunter/Code/Core/Installers/LazyDependencyInjectorInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для LazyDependencyInjector
    /// </summary>
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
