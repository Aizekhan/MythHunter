// Шлях: Assets/_MythHunter/Code/Core/Installers/PoolSystemInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для розширеної системи пулінгу об'єктів
    /// </summary>
    public class PoolSystemInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Pool System...", "Installer");

            // Основна функціональність пулінгу
            InstallCorePoolSystem(container);

            // Моніторинг та діагностика
            InstallPoolMonitoring(container);

            // Інтеграція підсистем
            IntegratePoolSubsystems(container);

            logger.LogInfo("Pool System installation completed", "Installer");
        }

        private void InstallCorePoolSystem(IDIContainer container)
        {
            // Реєструємо оптимізований менеджер пулів
            BindSingleton<IPoolManager, OptimizedPoolManager>(container);

            // Реєструємо фабрику для GameObjectPool з коректним логером
            container.RegisterFactory<GameObjectPool>((c) => {
                var defaultPrefab = Resources.Load<GameObject>("DefaultPoolPrefab");

                // Якщо префаб не знайдено, створюємо пустий
                if (defaultPrefab == null)
                {
                    defaultPrefab = new GameObject("DefaultPoolPrefab");
                    defaultPrefab.SetActive(false);
                    Object.DontDestroyOnLoad(defaultPrefab);
                }

                return new GameObjectPool(
                    defaultPrefab,
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
            logger.LogInfo("Installing Pool Monitoring System...", "Installer");

            // Створюємо GameObject для моніторингу
            var monitorObject = new GameObject("MythHunter_PoolMonitor");
            var poolMonitor = monitorObject.AddComponent<PoolMonitor>();

            // Ін'єктуємо залежності
            container.InjectDependencies(poolMonitor);

            // Реєструємо PoolMonitor в контейнері
            container.RegisterInstance<PoolMonitor>(poolMonitor);

            // Зберігаємо об'єкт між сценами
            Object.DontDestroyOnLoad(monitorObject);

            logger.LogInfo("Pool Monitoring System installed", "Installer");
        }

        private void IntegratePoolSubsystems(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Integrating Pool Subsystems...", "Installer");

            // Отримуємо основні компоненти
            var poolManager = container.Resolve<IPoolManager>() as OptimizedPoolManager;
            var poolMonitor = container.Resolve<PoolMonitor>();

            if (poolManager != null && poolMonitor != null)
            {
                // Встановлюємо зв'язок між менеджером та монітором
                poolManager.SetPoolMonitor(poolMonitor);
                logger.LogInfo("Pool Manager successfully linked with Pool Monitor", "Installer");
            }
            else
            {
                logger.LogWarning("Failed to link Pool Manager with Pool Monitor", "Installer");
            }
        }
    }
}
