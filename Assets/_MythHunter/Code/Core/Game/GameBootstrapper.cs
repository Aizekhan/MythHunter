using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Core.ECS;
using MythHunter.Core.MonoBehaviours;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Точка входу в гру
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        private static GameBootstrapper _instance;

        [SerializeField] private bool _injectOnAwake = true;

        private IDIContainer _container;
        private IEventBus _eventBus;
        private IMythLogger _logger;
        private IEcsWorld _ecsWorld;
        private GameStateMachine _stateMachine;
        private DependencyInjector _dependencyInjector;

        // Singleton для доступу
        public static GameBootstrapper Instance => _instance;
        public IDIContainer Container => _container;

        private async void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDependencyInjection();
            InitializeLogging();
            InitializeEcs();
            InitializeStateMachine();
            InitializeDependencyInjector();

            _logger.LogInfo("GameBootstrapper initialized successfully");

            // Асинхронна ініціалізація сервісів
            await InitializeServicesAsync();
        }

        private void InitializeDependencyInjection()
        {
            // Створення логера перед усім іншим
            var logger = MythLogger.CreateDefaultLogger();

            // Створення контейнера з логером
            _container = new DIContainer(logger);

            // Реєстрація базових сервісів
            _container.RegisterInstance<IMythLogger>(logger);
            _container.RegisterSingleton<IEventBus, EventBus>();

            // Реєстрація всіх інсталяторів
            InstallerRegistry.RegisterInstallers(_container);

            // Встановлюємо глобальний контейнер
            DIContainerProvider.SetContainer(_container);
        }

        private void InitializeLogging()
        {
            _logger = _container.Resolve<IMythLogger>();
            _logger.LogInfo("Logging system initialized", "Bootstrapper");
        }

        private void InitializeEcs()
        {
            _eventBus = _container.Resolve<IEventBus>();

            // Створення ECS світу
            var entityManager = new EntityManager();
            var systemRegistry = new Systems.Core.SystemRegistry();

            _ecsWorld = new EcsWorld(entityManager, systemRegistry);
            _container.RegisterInstance<IEntityManager>(entityManager);
            _container.RegisterInstance<IEcsWorld>(_ecsWorld);

            // Створення оптимізатора ECS
            var ecsOptimizer = new EcsOptimizer(entityManager, _logger);
            _container.RegisterInstance<EcsOptimizer>(ecsOptimizer);

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
            // Створення інжектора на сцені
            var injectorObject = new GameObject("DependencyInjector");
            injectorObject.transform.SetParent(transform);
            _dependencyInjector = injectorObject.AddComponent<DependencyInjector>();

            // Ін'єкція компонентів на сцені, якщо потрібно
            if (_injectOnAwake)
            {
                _dependencyInjector.InjectDependenciesInScene();
            }

            _logger.LogInfo("Dependency injector initialized", "Bootstrapper");
        }

        private async UniTask InitializeServicesAsync()
        {
            _logger.LogInfo("Starting async services initialization", "Bootstrapper");

            // Тут можна ініціалізувати сервіси, які потребують асинхронності
            // Наприклад, завантаження конфігурацій, підключення до серверів тощо

            // Імітація асинхронної операції
            await UniTask.Delay(100);

            _logger.LogInfo("Async services initialization completed", "Bootstrapper");

            // Перехід до початкового стану
            _stateMachine.ChangeState(GameStateType.Boot);
        }

        /// <summary>
        /// Зареєструвати MonoBehaviour для подальшої ін'єкції
        /// </summary>
        public void RegisterForInjection(MonoBehaviour component)
        {
            if (component != null && _container != null)
            {
                _container.InjectDependencies(component);
                _logger?.LogDebug($"Injected dependencies into {component.GetType().Name}", "DI");
            }
            else
            {
                _logger?.LogWarning($"Cannot inject dependencies - component is {(component == null ? "null" : "valid")} and container is {(_container == null ? "null" : "valid")}", "DI");
            }
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
    }
}
