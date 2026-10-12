// Assets/_MythHunter/Code/Core/Game/GameBootstrapper.cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Network;
using MythHunter.Utils.Logging;
using MythHunter.Core.ECS;
using MythHunter.Systems.Core;
using MythHunter.Core.SceneManagement;
using MythHunter.Debug;
using MythHunter.Core.MonoBehaviours;
using RPGFramework.Core.Composition;
using RPGFramework.Core.Composition.Modules;
using MythHunter.Core.Composition.Modules;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Центральна точка входу в гру. Відповідає за ініціалізацію DI, систем і станів.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool _injectOnAwake = true;

        [Header("Networking")]
        [SerializeField] private EventNetworkMode _eventNetworkMode = EventNetworkMode.Offline;

        private IDIContainer _container;
        private IMythLogger _logger;
        private IEventBus _eventBus;
        private IEcsWorld _ecsWorld;
        private IGameStateMachine _stateMachine;
        private IDependencyInjector _dependencyInjector;
        private ISceneDispatcher _sceneDispatcher;
        private IGameComposition _composition;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeAsync().Forget();
        }

      
        private void Update()
        {
            _ecsWorld?.Update(Time.deltaTime);
            _stateMachine?.Update();
        }

        private void OnDestroy()
        {
            _composition?.Shutdown();
            _ecsWorld?.Dispose();
            _logger?.LogInfo("GameBootstrapper destroyed", "Bootstrapper");
        }
        private async UniTaskVoid InitializeAsync()
        {
            InitializeCore();
            InitializeGameSystems();

            _logger.LogInfo("GameBootstrapper ініціалізовано успішно");

            await InitializeServicesAsync();

            // Додаємо затримку для завершення всіх ініціалізацій
            await UniTask.DelayFrame(5);

            // Змінюємо стан на Boot
            _stateMachine?.ChangeState(GameStateType.Boot);
        }
        private void InitializeCore()
        {
            var logger = MythLogger.CreateDefaultLogger();
            _container = new DIContainer(logger);

            _container.BindSingleton<IMythLogger>(logger);
            _container.BindSingleton<RPGFramework.Logging.ILogger>(logger);
            // Authoritative composition-root setting; Offline preserves current behavior.
            _container.RegisterInstance(_eventNetworkMode);

            // NEW: Modular composition system
            var builder = new GameCompositionBuilder(_container);

            // Framework Core (always)
            builder.UseModule<FrameworkCoreModule>();

            // Game Layer (always)
            builder.UseModule<GameLayerModule>();

            // Infrastructure (always - resources, pooling, serialization, UI)
            builder.UseModule<InfrastructureModule>();

            // Gameplay Systems (always - movement, visibility, hero)
            builder.UseModule<GameplayModule>();

            // Optional: Networking (only if not offline)
            if (_eventNetworkMode != EventNetworkMode.Offline)
            {
                builder.UseModule<NetworkingModule>();
                builder.UseModule<LobbyModule>();
            }

            // Optional: Debug Tools (only in editor/development)
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            builder.UseModule<DebugToolsModule>();
            #endif

            // Additional installers not yet modularized
            var lazyInjectorInstaller = new MythHunter.Core.Installers.LazyDependencyInjectorInstaller();
            lazyInjectorInstaller.InstallBindings(_container);

            var eventsInstaller = new MythHunter.Core.Installers.EventsInstaller();
            eventsInstaller.InstallBindings(_container);

            var profileInstaller = new MythHunter.Core.Installers.ProfileInstaller();
            profileInstaller.InstallBindings(_container);

            var parallelSystemsInstaller = new MythHunter.Core.Installers.ParallelSystemsInstaller();
            parallelSystemsInstaller.InstallBindings(_container);

            // Build composition (configure and initialize all modules)
            _composition = builder.Build();

            _logger = _container.Resolve<IMythLogger>();
            _eventBus = _container.Resolve<IEventBus>();
            _dependencyInjector = _container.Resolve<IDependencyInjector>();

            _logger.LogInfo($"Game composition built with {_composition.Modules.Count} modules", "Bootstrapper");
        }

        private void InitializeGameSystems()
        {
            _ecsWorld = _container.Resolve<IEcsWorld>();
            _ecsWorld.Initialize();

            _stateMachine = _container.Resolve<IGameStateMachine>();
            _stateMachine.Initialize();

            var systemRegistry = _container.Resolve<ISystemRegistry>();
            systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.OnBoot);

            if (_injectOnAwake)
                _dependencyInjector.InjectDependenciesInScene();

            var debugService = _container.Resolve<IDebugService>();
            debugService.CreateDebugDashboard();

            _logger.LogInfo("Ігрові системи ініціалізовано", "Bootstrapper");
        }

        private async UniTask InitializeServicesAsync()
        {
            _logger.LogInfo("Початок асинхронної ініціалізації сервісів", "Bootstrapper");

            await UniTask.Delay(100);
            _sceneDispatcher = _container.Resolve<ISceneDispatcher>();

            // Явно ініціалізуємо GameFlowManager перед зміною стану
            var gameFlowManager = _container.Resolve<IGameFlowManager>();

            _logger.LogInfo("Асинхронна ініціалізація сервісів завершена", "Bootstrapper");
        }
    }
}
