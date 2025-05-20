// Assets/_MythHunter/Code/Core/Game/GameBootstrapper.cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Core.ECS;
using MythHunter.Systems.Core;
using MythHunter.Core.SceneManagement;
using MythHunter.Debug;
using MythHunter.Core.MonoBehaviours;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Центральна точка входу в гру. Відповідає за ініціалізацію DI, систем і станів.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool _injectOnAwake = true;

        private IDIContainer _container;
        private IMythLogger _logger;
        private IEventBus _eventBus;
        private IEcsWorld _ecsWorld;
        private IGameStateMachine _stateMachine;
        private IDependencyInjector _dependencyInjector;
        private ISceneDispatcher _sceneDispatcher;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            InitializeCore();
            InitializeGameSystems();

            _logger.LogInfo("GameBootstrapper initialized successfully");

            await InitializeServicesAsync();
        }

        private void Update()
        {
            _ecsWorld?.Update(Time.deltaTime);
            _stateMachine?.Update();
        }

        private void OnDestroy()
        {
            _ecsWorld?.Dispose();
            _logger?.LogInfo("GameBootstrapper destroyed", "Bootstrapper");
        }

        private void InitializeCore()
        {
            var logger = MythLogger.CreateDefaultLogger();
            _container = new DIContainer(logger);

            _container.BindSingleton<IMythLogger>(logger);
            _container.BindSingleton<IDIContainer>(_container);

            InstallerRegistry.RegisterInstallers(_container);

            _logger = _container.Resolve<IMythLogger>();
            _eventBus = _container.Resolve<IEventBus>();
            _dependencyInjector = _container.Resolve<IDependencyInjector>();
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

            _logger.LogInfo("Game systems initialized", "Bootstrapper");
        }

        private async UniTask InitializeServicesAsync()
        {
            _logger.LogInfo("Starting async services initialization", "Bootstrapper");

            await UniTask.Delay(100);
            _sceneDispatcher = _container.Resolve<ISceneDispatcher>();

            var gameFlowManager = _container.Resolve<IGameFlowManager>();
            _stateMachine.ChangeState(GameStateType.Boot);

            _logger.LogInfo("Async services initialization completed", "Bootstrapper");
        }
    }
}
