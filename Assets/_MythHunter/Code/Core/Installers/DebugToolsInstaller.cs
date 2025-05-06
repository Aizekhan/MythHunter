using MythHunter.Core.DI;
using MythHunter.Debug.Core;
using MythHunter.Debug.Events;
using MythHunter.Debug.Profiling;
using MythHunter.Debug.UI;
using MythHunter.Utils.Logging;
using MythHunter.Debug;

public class DebugToolsInstaller : DIInstaller
{
    public override void InstallBindings(IDIContainer container)
    {
        var logger = container.Resolve<IMythLogger>();
        logger.LogInfo("Встановлення залежностей DebugTools...", "Installer");

        // Інструменти (з власними інтерфейсами або прямі типи, якщо вони потрібні як конкретні класи)
        BindSingleton<SystemProfiler, SystemProfiler>(container);
        BindSingleton<PerformanceMonitor, PerformanceMonitor>(container);
        BindSingleton<EventDebugTool, EventDebugTool>(container);

        // Фабрика
        BindSingleton<DebugToolFactory, DebugToolFactory>(container);

        // Сервіс DebugService
        BindSingleton<IDebugService, DebugService>(container);

        logger.LogInfo("Встановлення залежностей DebugTools завершено", "Installer");
    }
}
