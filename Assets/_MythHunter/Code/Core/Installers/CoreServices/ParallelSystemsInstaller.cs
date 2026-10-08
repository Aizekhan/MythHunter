// Шлях: Assets/_MythHunter/Code/Core/Installers/ParallelSystemsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Core.ECS.Jobs;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для компонентів паралельного виконання систем
    /// </summary>
    public class ParallelSystemsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей ParallelSystems...", "Installer");

            // Реєстрація планувальника задач
            BindSingleton<ISystemJobScheduler, SystemJobScheduler>(container);

            // Реєстрація паралельного реєстру систем
            BindSingleton<ISystemRegistry, ParallelSystemRegistry>(container);

            logger.LogInfo("Встановлення залежностей ParallelSystems завершено", "Installer");
        }
    }
}
