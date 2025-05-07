// Шлях: Assets/_MythHunter/Code/Core/Installers/PoolSystemInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;
using MythHunter.Resources.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
            InstallCorePoolSystem(container).Forget();

            // Моніторинг та діагностика
            InstallPoolMonitoring(container);

            // Інтеграція підсистем
            IntegratePoolSubsystems(container);

            logger.LogInfo("Pool System installation completed", "Installer");
        }

        private async UniTaskVoid InstallCorePoolSystem(IDIContainer container)
        {
            BindSingleton<IPoolManager, OptimizedPoolManager>(container);

            var resourceProvider = container.Resolve<IResourceProvider>();
            var logger = container.Resolve<IMythLogger>();

            GameObject defaultPrefab = await resourceProvider.LoadAsync<GameObject>("DefaultPoolPrefab");

            if (defaultPrefab == null)
            {
                logger.LogWarning("DefaultPoolPrefab not found. Using fallback empty prefab.", "Installer");

                defaultPrefab = new GameObject("DefaultPoolPrefab");
                defaultPrefab.SetActive(false);
                Object.DontDestroyOnLoad(defaultPrefab);
            }

            container.RegisterFactory<GameObjectPool>((c) => new GameObjectPool(
                defaultPrefab,
                10,
                null,
                null,
                null,
                c.Resolve<IMythLogger>()
            ));
        }

        private void InstallPoolMonitoring(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Pool Monitoring System...", "Installer");

            var monitorObject = new GameObject("MythHunter_PoolMonitor");
            var poolMonitor = monitorObject.AddComponent<PoolMonitor>();
            container.InjectDependencies(poolMonitor);
            container.RegisterInstance<PoolMonitor>(poolMonitor);
            Object.DontDestroyOnLoad(monitorObject);

            logger.LogInfo("Pool Monitoring System installed", "Installer");
        }

        private void IntegratePoolSubsystems(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Integrating Pool Subsystems...", "Installer");

            var poolManager = container.Resolve<IPoolManager>() as OptimizedPoolManager;
            var poolMonitor = container.Resolve<PoolMonitor>();

            if (poolManager != null && poolMonitor != null)
            {
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
