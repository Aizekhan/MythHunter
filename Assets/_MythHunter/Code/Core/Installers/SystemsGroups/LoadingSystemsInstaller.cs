// Assets/_MythHunter/Code/Core/Installers/SystemsGroups/LoadingSystemsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.StateMachine;
using MythHunter.States;
using MythHunter.Systems.Loading;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;
using MythHunter.Core.Game;

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

            var stateMachine = container.Resolve<IStateMachine<GameStateType>>();
            var systemRegistry = container.Resolve<ISystemRegistry>();

            // Реєстрація сервісів та класів
            BindSingleton<ILoadingSystem, LoadingSystem>(container);
            BindSingleton<LoadingPhaseProvider, LoadingPhaseProvider>(container);

            // Реєстрація стану LoadingState
            var loadingState = new LoadingState(container);
            stateMachine.RegisterState(loadingState);

            // Отримуємо інстанси для реєстрації
            var loadingSystem = container.Resolve<ILoadingSystem>();

            // Реєструємо систему завантаження
            systemRegistry.RegisterSystemWithPriority(loadingSystem, SystemPriorities.Active + 5);

            logger.LogInfo("Системи завантаження встановлено успішно", "Installer");
        }
    }
}
