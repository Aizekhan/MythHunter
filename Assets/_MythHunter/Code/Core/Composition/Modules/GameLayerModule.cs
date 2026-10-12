// Assets/_MythHunter/Code/Core/Composition/Modules/GameLayerModule.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.Game;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Core.SceneManagement;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Resources;
using MythHunter.Resources.SceneManagement;
using MythHunter.Services.GameSettings;
using MythHunter.Systems.Core;
using MythHunter.Systems.Phase;
using RPGFramework.Core.Composition;
using RPGFramework.Core.Composition.Modules;

namespace MythHunter.Core.Composition.Modules
{
    /// <summary>
    /// MythHunter Game Layer module: game-specific flow, state machine, scene management.
    /// Depends on Framework Core for ECS and DI infrastructure.
    /// </summary>
    public class GameLayerModule : BaseFeatureModule
    {
        public override string Name => "MythHunter.GameLayer";

        public override IEnumerable<Type> Dependencies => new[]
        {
            typeof(FrameworkCoreModule)
        };

        protected override void OnConfigure()
        {
            var container = Container as IDIContainer;
            if (container == null)
            {
                throw new InvalidOperationException(
                    "GameLayerModule requires IDIContainer");
            }

            // Game Flow & State Management
            container.BindSingleton<IEventPool, EventPool>();
            container.BindSingleton<IEventBus, SimpleEventBus>();
            container.BindSingleton<IGameStateMachine, GameStateMachine>();
            container.BindSingleton<IEcsWorld, EcsWorld>();
            container.BindSingleton<IDILifecycleManager, DILifecycleManager>();
            container.BindSingleton<IPhaseProvider, GamePhaseProvider>();

            // Scene Management
            container.BindSingleton<SceneLoader, SceneLoader>();
            container.BindSingleton<ISceneDispatcher, SceneDispatcher>();
            container.BindSingleton<IGameFlowManager, GameFlowManager>();

            // Game Settings & Configuration
            container.BindSingleton<IGameSettingsService, GameSettingsService>();
            container.BindSingleton<AutoPreloadConfigurator, AutoPreloadConfigurator>();

            // Install DI Extensions
            var diExtensionsInstaller = new Installers.CoreServices.DIExtensionsInstaller();
            diExtensionsInstaller.InstallBindings(container);
        }
    }
}
