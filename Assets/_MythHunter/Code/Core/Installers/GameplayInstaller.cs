using MythHunter.Core.DI;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Systems.Core;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для основних ігрових систем
    /// </summary>
    public class GameplayInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей GameplaySystem...", "Installer");

            // Реєстрація фазової системи
            BindSingleton<IPhaseSystem, PhaseSystem>(container);

            // Реєстрація в SystemRegistry
            var systemRegistry = container.Resolve<ISystemRegistry>();
            systemRegistry.RegisterSystem(container.Resolve<IPhaseSystem>());

            logger.LogInfo("Встановлення залежностей GameplaySystem завершено", "Installer");
        }
    }
}
