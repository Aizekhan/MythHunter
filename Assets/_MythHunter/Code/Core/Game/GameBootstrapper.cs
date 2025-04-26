using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Core.ECS;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Точка входу в гру
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        private IDIContainer _container;
        private IEventBus _eventBus;
        private MythHunter.Utils.Logging.ILogger _logger;
        private IEcsWorld _ecsWorld;
        private GameStateMachine _stateMachine;
        
        private async void Awake()
        {
            InitializeDependencyInjection();
            InitializeLogging();
            InitializeEcs();
            InitializeStateMachine();
            
            DontDestroyOnLoad(gameObject);
            
            _logger.LogInfo("GameBootstrapper initialized successfully");
            
            // Асинхронна ініціалізація сервісів
            await InitializeServicesAsync();
        }
        
        private void InitializeDependencyInjection()
        {
            _container = new DIContainer();
            
            // Реєстрація базових сервісів
            _container.RegisterSingleton<IEventBus, EventBus>();
            _container.RegisterSingleton<MythHunter.Utils.Logging.ILogger, UnityLogger>();
            
            // Реєстрація всіх інсталяторів
            InstallerRegistry.RegisterInstallers(_container);
        }
        
        private void InitializeLogging()
        {
            _logger = _container.Resolve<MythHunter.Utils.Logging.ILogger>();
            _logger.LogInfo("Logging system initialized");
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
            
            _logger.LogInfo("ECS world initialized");
        }
        
        private void InitializeStateMachine()
        {
            _stateMachine = new GameStateMachine(_container);
            _stateMachine.Initialize();
            
            _logger.LogInfo("Game state machine initialized");
        }
        
        private async UniTask InitializeServicesAsync()
        {
            _logger.LogInfo("Starting async services initialization");
            
            // Тут можна ініціалізувати сервіси, які потребують асинхронності
            // Наприклад, завантаження конфігурацій, підключення до серверів тощо
            
            // Імітація асинхронної операції
            await UniTask.Delay(100);
            
            _logger.LogInfo("Async services initialization completed");
        }
        
        private void Update()
        {
            _ecsWorld?.Update(Time.deltaTime);
            _stateMachine?.Update();
        }
        
        private void OnDestroy()
        {
            _ecsWorld?.Dispose();
            _logger?.LogInfo("GameBootstrapper destroyed");
        }
    }
}
