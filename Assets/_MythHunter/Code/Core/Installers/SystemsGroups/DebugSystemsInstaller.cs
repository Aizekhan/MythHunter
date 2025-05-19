// Шлях: Assets/_MythHunter/Code/Core/Installers/DebugSystemsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;
using MythHunter.Debug;
using MythHunter.Debug.Profiling;
using MythHunter.Debug.Events;
using MythHunter.Debug.Pool;
using MythHunter.Debug.UI;
using MythHunter.Systems.Groups;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для систем відлагодження
    /// </summary>
    public class DebugSystemsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення систем відлагодження...", "Installer");

            var systemRegistry = container.Resolve<ISystemRegistry>();

            // Реєстрація групи систем відлагодження з конкретними типами
            systemRegistry.RegisterGroupWithCategory<SystemGroup>(
                "DebugSystems",
                SystemPriorities.Analytics,
                SystemInitializationCategory.Manual,
                logger,
                new[] {
                    typeof(SystemProfiler),
                    typeof(PerformanceMonitor),
                    typeof(EventDebugTool),
                    typeof(PoolDebugTool)
                }
            );

            logger.LogInfo("Системи відлагодження встановлено успішно", "Installer");
        }
    }
}
