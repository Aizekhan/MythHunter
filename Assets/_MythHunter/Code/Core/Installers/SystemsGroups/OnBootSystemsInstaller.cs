// Шлях: Assets/_MythHunter/Code/Core/Installers/OnBootSystemsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Systems;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Resources.Core;
using MythHunter.Resources.Pool;
using MythHunter.Resources;
using MythHunter.Systems.Groups;
using MythHunter.UI.Core;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для базових систем, що запускаються при завантаженні гри
    /// </summary>
    public class OnBootSystemsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення базових систем (OnBoot)...", "Installer");

            var systemRegistry = container.Resolve<ISystemRegistry>();

            // Реєстрація Core-систем
            var coreGroup = systemRegistry.RegisterGroupWithCategory<SystemGroup>(
                "CoreServices",
                SystemPriorities.Core,
                SystemInitializationCategory.OnBoot,
                logger
            );
            coreGroup.AddSystem(container.Resolve<IEventThrottlerUpdateSystem>());
           

            // Реєстрація UI-систем
            var uiGroup = systemRegistry.RegisterGroupWithCategory<SystemGroup>(
                "UIServices",
                SystemPriorities.UI,
                SystemInitializationCategory.OnBoot,
                logger
            );
           

            // Реєстрація ресурсних систем
            var resourceGroup = systemRegistry.RegisterGroupWithCategory<SystemGroup>(
                "ResourceSystems",
                SystemPriorities.Core - 10,
                SystemInitializationCategory.OnBoot,
                logger
            );
          

            logger.LogInfo("Базові системи (OnBoot) встановлено успішно", "Installer");
        }
    }
}
