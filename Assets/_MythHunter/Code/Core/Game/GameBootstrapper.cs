using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Core.ECS;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Systems.Core;
using MythHunter.Debug;
using MythHunter.Utils;
using MythHunter.Resources.Pool;

namespace MythHunter.Core.Game
{
    public class GameBootstrapper : MonoBehaviour
    {
        private static GameBootstrapper _instance;
        public static GameBootstrapper Instance => _instance;

        [SerializeField] private bool _injectOnAwake = true;

        private IDIContainer _container;
        private IMythLogger _logger;
        private IEventBus _eventBus;

        private IEcsWorld _ecsWorld;
        private IGameStateMachine _stateMachine;
        private IDependencyInjector _dependencyInjector;

        private async void Awake()
        {
            if (!TryInitializeSingleton())
                return;

            DontDestroyOnLoad(gameObject);

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
            _instance = null;
        }

        private bool TryInitializeSingleton()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return false;
            }

            _instance = this;
            return true;
        }

        private void InitializeCore()
        {
            var logger = MythLogger.CreateDefaultLogger();
            _container = new DIContainer(logger);

            _container.BindSingleton<IMythLogger>(logger);
            _container.BindSingleton<IDIContainer>(_container);
            InitializeDependencyInjector();
            InstallerRegistry.RegisterInstallers(_container);

            _logger = _container.Resolve<IMythLogger>();
        }
        private void InitializeDependencyInjector()
        {
            // Знаходимо або створюємо DependencyInjector
            var dependencyInjector = UnityEngine.Object.FindFirstObjectByType<DependencyInjector>();
            if (dependencyInjector == null)
            {
                var gameObject = new GameObject("MythHunter_DependencyInjector");
                dependencyInjector = gameObject.AddComponent<DependencyInjector>();
                DontDestroyOnLoad(gameObject);
                _logger?.LogInfo("Created new DependencyInjector", "Bootstrapper");
            }
            // Реєструємо в контейнері
            _container.RegisterInstance<IDependencyInjector>(dependencyInjector);
            _dependencyInjector = dependencyInjector;
        }
        private void InitializeGameSystems()
        {
            _eventBus = _container.Resolve<IEventBus>();

            _ecsWorld = _container.Resolve<IEcsWorld>();
            _stateMachine = _container.Resolve<IGameStateMachine>();
            _dependencyInjector = _container.Resolve<IDependencyInjector>();

            _stateMachine.Initialize();

            if (_injectOnAwake)
                _dependencyInjector.InjectDependenciesInScene();

            var debugService = _container.Resolve<IDebugService>();
            debugService.CreateDebugDashboard();

            _logger.LogInfo("Game systems initialized", "Bootstrapper");
        }

        private async UniTask InitializeServicesAsync()
        {
            _logger.LogInfo("Starting async services initialization", "Bootstrapper");

            await UniTask.Delay(100); // Placeholder async init

            _logger.LogInfo("Async services initialization completed", "Bootstrapper");
            _stateMachine.ChangeState(GameStateType.Boot);
        }

        public void RegisterForInjection(MonoBehaviour component)
        {
            if (component != null && _container != null)
            {
                _container.InjectDependencies(component);
                _logger?.LogDebug($"Injected dependencies into {component.GetType().Name}", "DI");
            }
            else
            {
                _logger?.LogWarning("Injection failed: component or container is null", "DI");
            }
        }

        public void InjectInto(MonoBehaviour component)
        {
            if (component != null && _container != null)
                _container.InjectDependencies(component);
        }
        public IPoolManager GetPoolManager()
        {
            return _container?.Resolve<IPoolManager>();
        }
    }
}
