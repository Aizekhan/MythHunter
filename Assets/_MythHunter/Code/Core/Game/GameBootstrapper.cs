using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Core.ECS;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Systems.Core;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Точка входу в гру
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        #region Singleton Pattern

        private static GameBootstrapper _instance;
        public static GameBootstrapper Instance => _instance;

        #endregion

        #region Inspector Fields

        [SerializeField] private bool _injectOnAwake = true;

        #endregion

        #region Core Services

        private IDIContainer _container;
        private IMythLogger _logger;
        private IEventBus _eventBus;

        #endregion

        #region Game Systems

        private IEcsWorld _ecsWorld;
        private GameStateMachine _stateMachine;
        private DependencyInjector _dependencyInjector;

        #endregion

        #region Unity Lifecycle

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
            Cleanup();
        }

        #endregion

        #region Initialization Methods

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
            InitializeDependencyInjection();
            _logger = _container.Resolve<IMythLogger>();
        }

        private void InitializeGameSystems()
        {
            InitializeEcs();
            InitializeStateMachine();
            InitializeDependencyInjector();
        }

        private void InitializeDependencyInjection()
        {
            var logger = MythLogger.CreateDefaultLogger();
            _container = new DIContainer(logger);

            // ТІЛЬКИ логер як початкова залежність
            _container.RegisterInstance<IMythLogger>(logger);

            // Всі інші сервіси реєструються через інсталери
            InstallerRegistry.RegisterInstallers(_container);
        }


        

        private void InitializeEcs()
        {
            // Просто отримуємо вже зареєстровані сервіси
            _eventBus = _container.Resolve<IEventBus>();
            var entityManager = _container.Resolve<IEntityManager>();
            var systemRegistry = _container.Resolve<SystemRegistry>();

            _ecsWorld = new EcsWorld(entityManager, systemRegistry);

            _logger.LogInfo("ECS world initialized", "Bootstrapper");
        }

       

        private void InitializeStateMachine()
        {
            _stateMachine = new GameStateMachine(_container);
            _stateMachine.Initialize();

            _logger.LogInfo("Game state machine initialized", "Bootstrapper");
        }

        private void InitializeDependencyInjector()
        {
            var injectorObject = CreateDependencyInjectorObject();
            _dependencyInjector = injectorObject.AddComponent<DependencyInjector>();

            if (_injectOnAwake)
            {
                _dependencyInjector.InjectDependenciesInScene();
            }

            _logger.LogInfo("Dependency injector initialized", "Bootstrapper");
        }

        private GameObject CreateDependencyInjectorObject()
        {
            var injectorObject = new GameObject("DependencyInjector");
            injectorObject.transform.SetParent(transform);
            return injectorObject;
        }

        private async UniTask InitializeServicesAsync()
        {
            _logger.LogInfo("Starting async services initialization", "Bootstrapper");

            // Асинхронна ініціалізація сервісів
            await PerformAsyncInitialization();

            _logger.LogInfo("Async services initialization completed", "Bootstrapper");
            _stateMachine.ChangeState(GameStateType.Boot);
        }

        private async UniTask PerformAsyncInitialization()
        {
            // Тут можна додати асинхронну логіку ініціалізації
            await UniTask.Delay(100);
        }

        #endregion

        #region Public Methods

        public void RegisterForInjection(MonoBehaviour component)
        {
            if (!ValidateInjectionParameters(component))
                return;

            _container.InjectDependencies(component);
            _logger?.LogDebug($"Injected dependencies into {component.GetType().Name}", "DI");
        }

        #endregion

        #region Internal Methods

        internal IDIContainer GetContainerInternal()
        {
            return _container;
        }

        #endregion

        #region Private Helper Methods

        private bool ValidateInjectionParameters(MonoBehaviour component)
        {
            if (component != null && _container != null)
                return true;

            _logger?.LogWarning(
                $"Cannot inject dependencies - component is {(component == null ? "null" : "valid")} " +
                $"and container is {(_container == null ? "null" : "valid")}",
                "DI"
            );
            return false;
        }

        private void Cleanup()
        {
            _ecsWorld?.Dispose();
            _logger?.LogInfo("GameBootstrapper destroyed", "Bootstrapper");
            _instance = null;
        }

        #endregion
    }
}
