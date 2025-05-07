// Шлях: Assets/_MythHunter/Code/Resources/Pool/PoolMonitorInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Інсталятор для автоматичної реєстрації PoolMonitor
    /// </summary>
    public class PoolMonitorInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing PoolMonitor...", "Installer");

            // Створюємо GameObject для моніторингу
            var monitorObject = new GameObject("MythHunter_PoolMonitor");
            var poolMonitor = monitorObject.AddComponent<PoolMonitor>();

            // Ін'єктуємо залежності
            container.InjectDependencies(poolMonitor);

            // Зберігаємо об'єкт між сценами
            Object.DontDestroyOnLoad(monitorObject);

            logger.LogInfo("PoolMonitor installed", "Installer");
        }
    }
}
