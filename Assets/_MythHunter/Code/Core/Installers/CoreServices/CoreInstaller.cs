// Шлях: Assets/_MythHunter/Code/Core/Installers/CoreInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Core.SceneManagement;
using MythHunter.Resources.SceneManagement;
using MythHunter.Systems.Core;
using MythHunter.Systems.Phase;
using MythHunter.Services.GameSettings;
using MythHunter.Entities;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Resources;
using RPGFramework.ECS;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор базових сервісів ядра системи
    /// </summary>
    public class CoreInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            // Логер з GameBotstrapper.cs
            var logger = container.Resolve<IMythLogger>();

            BindSingleton<IEntityManager, EntityManager>(container);
            BindSingleton<IEventPool, EventPool>(container);
            BindSingleton<IEventBus, SimpleEventBus>(container);
            BindSingleton<IGameStateMachine, GameStateMachine>(container);
            BindSingleton<ISystemRegistry, SystemRegistry>(container);
            BindSingleton<IEcsWorld, EcsWorld>(container);
            BindSingleton<IDILifecycleManager, DILifecycleManager>(container);
            BindSingleton<IPhaseProvider, GamePhaseProvider>(container);
            BindSingleton<SceneLoader, SceneLoader>(container);
            BindSingleton<ISceneDispatcher, SceneDispatcher>(container);
            BindSingleton<IGameFlowManager, GameFlowManager>(container);
            BindSingleton<IGameSettingsService, GameSettingsService>(container);
            BindSingleton<AutoPreloadConfigurator, AutoPreloadConfigurator>(container);

            var diExtensionsInstaller = new DIExtensionsInstaller();
            diExtensionsInstaller.InstallBindings(container);
        }
    }
}
