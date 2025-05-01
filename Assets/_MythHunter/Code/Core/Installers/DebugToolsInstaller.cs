// Шлях: Assets/_MythHunter/Code/Core/Installers/DebugToolsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Debug.Core;
using MythHunter.Debug.Profiling;
using MythHunter.Debug.Events;
using MythHunter.Debug.UI;
using MythHunter.Utils.Logging;
using MythHunter.Events;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для інструментів відлагодження
    /// </summary>
    public class DebugToolsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей DebugTools...", "Installer");

            // Реєстрація фабрики інструментів
            BindSingleton<DebugToolFactory, DebugToolFactory>(container);

            // Реєстрація профайлера системи
            BindSingleton<SystemProfiler, SystemProfiler>(container);

            // Реєстрація монітора продуктивності
            BindSingleton<PerformanceMonitor, PerformanceMonitor>(container);

            // Реєстрація інструменту відстеження подій
            var eventBus = container.Resolve<IEventBus>();
            container.RegisterInstance<EventDebugTool>(new EventDebugTool(eventBus, logger));

            // Реєстрація центрального дашборду
            container.Register<DebugDashboard, DebugDashboard>();
            // Примітка: дашборд буде створений під час завантаження сцени
            // через MonoBehaviour, тому просто реєструємо тип

            logger.LogInfo("Встановлення залежностей DebugTools завершено", "Installer");
        }
    }
}
