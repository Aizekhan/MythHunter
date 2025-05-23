// Шлях: Assets/_MythHunter/Code/Core/Installers/PoolInstaller.cs

using MythHunter.Core.DI;
using MythHunter.Resources.Pool;
using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для пулінгової системи
    /// </summary>
    public class PoolInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення пулінгової системи...", "Installer");

            // ✅ Основні сервіси пулінгу
            BindSingleton<IPoolManager, PoolManager>(container);

            // ✅ Інтеграція з підсистемами
            IntegratePoolSubsystems(container);

            logger.LogInfo("Pool System installation completed", "Installer");
        }

        /// <summary>
        /// Інтеграція підсистем пулінгу
        /// </summary>
        private void IntegratePoolSubsystems(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Інтеграція підсистем пулінгу...", "Installer");

            try
            {
                var poolManager = container.Resolve<IPoolManager>();

                // ✅ ВИПРАВЛЕНО: Створюємо PoolMonitor правильно
                var monitorObject = CreatePoolMonitorObject();
                var poolMonitor = monitorObject.GetComponent<PoolMonitor>();

                if (poolMonitor == null)
                {
                    logger.LogError("Не вдалося отримати компонент PoolMonitor", "Installer");
                    return;
                }

                // ✅ Встановлюємо зв'язок між PoolManager та PoolMonitor
                poolManager.SetPoolMonitor(poolMonitor);

                // ✅ Реєструємо як інстанс в контейнері
                container.RegisterInstance<PoolMonitor>(poolMonitor);

                logger.LogInfo("Pool Monitoring System ініціалізовано", "Installer");
            }
            catch (System.Exception ex)
            {
                logger.LogError($"Помилка при інтеграції підсистем пулінгу: {ex.Message}", "Installer", ex);
            }
        }

        /// <summary>
        /// Створює GameObject з компонентом PoolMonitor
        /// </summary>
        private GameObject CreatePoolMonitorObject()
        {
            var monitorObject = new GameObject("MythHunter_PoolMonitor");

            // ✅ ВИПРАВЛЕНО: Використовуємо non-generic версію AddComponent
            var poolMonitor = monitorObject.AddComponent(typeof(PoolMonitor)) as PoolMonitor;

            if (poolMonitor == null)
            {
                throw new System.Exception("Не вдалося створити компонент PoolMonitor");
            }

            // ✅ Налаштовуємо як persistent об'єкт
            Object.DontDestroyOnLoad(monitorObject);

            return monitorObject;
        }
    }
}
