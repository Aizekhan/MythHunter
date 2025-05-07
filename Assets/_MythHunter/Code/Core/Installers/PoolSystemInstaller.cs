// Шлях: Assets/_MythHunter/Code/Installers/PoolSystemInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources
{
    /// <summary>
    /// Інсталятор для системи пулінгу об'єктів
    /// </summary>
    public class PoolSystemInstaller : DIInstaller
    {
        // У файлі PoolSystemInstaller.cs
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Pool System", "Installer");

            // Основна функціональність пулінгу
            InstallCorePoolSystem(container);

            // Моніторинг та діагностика
            InstallPoolMonitoring(container);

            logger.LogInfo("Pool System installed", "Installer");
        }

        private void InstallCorePoolSystem(IDIContainer container)
        {
            // Реєструємо оптимізований менеджер пулів
            BindSingleton<IPoolManager, OptimizedPoolManager>(container);

            // Реєструємо фабрику для GameObjectPool
            container.RegisterFactory<GameObjectPool>((c) => {
                return new GameObjectPool(
                    UnityEngine.Resources.Load<UnityEngine.GameObject>("DefaultPoolPrefab"),
                    10,
                    null,
                    null,
                    null,
                    c.Resolve<IMythLogger>()
                );
            });
        }

        private void InstallPoolMonitoring(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing PoolMonitor...", "Installer");

            // Створюємо GameObject для моніторингу
            var monitorObject = new GameObject("MythHunter_PoolMonitor");
            var poolMonitor = monitorObject.AddComponent<PoolMonitor>();

            // Ін'єктуємо залежності
            container.InjectDependencies(poolMonitor);

            // Зберігаємо об'єкт між сценами
            UnityEngine.Object.DontDestroyOnLoad(monitorObject);

            logger.LogInfo("PoolMonitor installed", "Installer");
        }
    }
}
