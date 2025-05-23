// Assets/_MythHunter/Code/Core/Installers/SystemsGroups/LoadingSystemsInstaller.cs

using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Systems.Loading;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;
using MythHunter.UI.Presenters;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для систем завантаження
    /// </summary>
    public class LoadingSystemsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення систем завантаження...", "Installer");

            var systemRegistry = container.Resolve<ISystemRegistry>();

            // ✅ Реєстрація інтерфейсів та реалізацій
            BindSingleton<ILoadingSystem, LoadingSystem>(container);
            BindSingleton<ILoadingPhaseProvider, LoadingPhaseProvider>(container);
            BindSingleton<ILoadingScreenPresenter, LoadingScreenPresenter>(container);

            // ✅ Реєстрація системи завантаження в registry
            var loadingSystem = container.Resolve<ILoadingSystem>();
            systemRegistry.RegisterSystemWithPriority(loadingSystem, SystemPriorities.Active + 5);

            logger.LogInfo("Системи завантаження встановлено успішно", "Installer");
        }
    }
}
