using MythHunter.Core.DI;
using MythHunter.Debug.Core;
using MythHunter.Debug.Events;
using MythHunter.Debug.Profiling;
using MythHunter.Debug.UI;
using MythHunter.Utils.Logging;

public class DebugToolsInstaller : DIInstaller
{
    public override void InstallBindings(IDIContainer container)
    {
        var logger = container.Resolve<IMythLogger>();
        logger.LogInfo("Встановлення залежностей DebugTools...", "Installer");

        // Реєструємо інструменти
        container.RegisterSingleton<SystemProfiler, SystemProfiler>();
        container.RegisterSingleton<PerformanceMonitor, PerformanceMonitor>();
        container.RegisterSingleton<EventDebugTool, EventDebugTool>();

        // Реєструємо фабрику (вона отримає всі інструменти через конструктор)
        container.RegisterSingleton<DebugToolFactory, DebugToolFactory>();

        // Реєструємо дашборд
        container.Register<DebugDashboard, DebugDashboard>();

        logger.LogInfo("Встановлення залежностей DebugTools завершено", "Installer");
    }
}
