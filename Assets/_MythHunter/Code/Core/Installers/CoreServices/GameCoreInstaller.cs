using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.Game;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Core.SceneManagement;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Events.Debugging;
using MythHunter.Events.Network;
using MythHunter.Networking.Core;
using MythHunter.Resources;
using MythHunter.Resources.SceneManagement;
using MythHunter.Services.GameSettings;
using MythHunter.Systems.Core;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Game Core installer - registers MythHunter-specific game services.
    /// These services implement game-specific policies and depend on Framework Core.
    /// </summary>
    public class GameCoreInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Game Core services...", "GameCoreInstaller");

            // Game state and flow management
            BindSingleton<IGameStateMachine, GameStateMachine>(container);
            BindSingleton<IPhaseProvider, GamePhaseProvider>(container);
            BindSingleton<IGameFlowManager, GameFlowManager>(container);

            // Scene management
            BindSingleton<SceneLoader, SceneLoader>(container);
            BindSingleton<ISceneDispatcher, SceneDispatcher>(container);

            // Game settings and configuration
            BindSingleton<IGameSettingsService, GameSettingsService>(container);
            BindSingleton<AutoPreloadConfigurator, AutoPreloadConfigurator>(container);

            // Event system (network-aware)
            InstallEventSystem(container, logger);

            // DI extensions
            var diExtensionsInstaller = new DIExtensionsInstaller();
            diExtensionsInstaller.InstallBindings(container);

            logger.LogInfo("Game Core services installed successfully", "GameCoreInstaller");
        }

        private void InstallEventSystem(IDIContainer container, IMythLogger logger)
        {
            logger.LogInfo("Installing Event System...", "GameCoreInstaller");

            // Register event infrastructure components
            BindSingleton<IEventThrottler, EventThrottler>(container);
            BindSingleton<IEventThrottlerUpdateSystem, EventThrottlerUpdateSystem>(container);
            BindSingleton<IEventBatcher, EventBatcher>(container);
            BindSingleton<IEventStore, EventStore>(container);

            // Select event bus implementation based on network mode
            var networkMode = container.Resolve<EventNetworkMode>();
            if (networkMode == EventNetworkMode.Offline)
            {
                logger.LogInfo("Event network synchronization disabled; using EventBus", "GameCoreInstaller");
                BindSingleton<IEventBus, EventBus>(container);
            }
            else
            {
                logger.LogInfo($"Event network synchronization enabled ({networkMode}); using NetworkEventBus", "GameCoreInstaller");
                // Register and resolve exactly one network bus, then alias the same
                // singleton through IEventBus so SystemRegistry and network consumers
                // cannot receive separate bus instances.
                BindSingleton<INetworkEventBus, NetworkEventBus>(container);
                BindInstance<IEventBus>(container, container.Resolve<INetworkEventBus>());
            }

            // Register timer system with the system registry
            var systemRegistry = container.Resolve<ISystemRegistry>();
            BindSingleton<ITimerSystem, TimerSystem>(container);
            systemRegistry.RegisterSystem(container.Resolve<ITimerSystem>());

            // Event debugging
            Bind<EventLogger, EventLogger>(container);

            logger.LogInfo("Event System installed", "GameCoreInstaller");
        }
    }
}
