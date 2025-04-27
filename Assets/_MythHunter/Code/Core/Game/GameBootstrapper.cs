using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Core.ECS;
using System.Collections;
using System;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Точка входу в гру
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] private bool _initializeOnAwake = true;

        // Флаг для фіксації стану ініціалізації
        private bool _isInitialized = false;
        private bool _isInitializing = false;

        // Залежності
        private IDIContainer _container;
        private IEventBus _eventBus;
        private IMythLogger _logger;
        private IEcsWorld _ecsWorld;
        private GameStateMachine _stateMachine;

        // Подія, що сигналізує про завершення ініціалізації
        public event Action OnInitializationCompleted;

        // Властивість для доступу до контейнера
        public IDIContainer Container
        {
            get
            {
                if (_container == null)
                {
                    Debug.LogWarning("[GameBootstrapper] Container accessed before initialization!");
                }
                return _container;
            }
        }

        // Властивість, що відображає статус ініціалізації
        public bool IsInitialized => _isInitialized;

        private void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            // Переконуємося, що в сцені лише один GameBootstrapper
            EnsureSingleInstance();

            if (_initializeOnAwake)
            {
                InitializeAsync().Forget();
            }
        }

        private void EnsureSingleInstance()
        {
            var bootstrappers = FindObjectsOfType<GameBootstrapper>();
            if (bootstrappers.Length > 1)
            {
                Debug.LogWarning("[GameBootstrapper] Multiple GameBootstrapper instances found! Destroying duplicates.");
                foreach (var bootstrapper in bootstrappers)
                {
                    if (bootstrapper != this)
                    {
                        Destroy(bootstrapper.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Асинхронна ініціалізація всіх систем
        /// </summary>
        public async UniTaskVoid InitializeAsync()
        {
            if (_isInitialized || _isInitializing)
            {
                Debug.LogWarning("[GameBootstrapper] Already initialized or initializing");
                return;
            }

            _isInitializing = true;

            try
            {
                InitializeDependencyInjection();
                InitializeLogging();
                InitializeEcs();
                InitializeStateMachine();

                // Асинхронна ініціалізація сервісів
                await InitializeServicesAsync();

                _isInitialized = true;
                _isInitializing = false;

                _logger.LogInfo("[GameBootstrapper] Initialization completed successfully");

                // Сповіщаємо про завершення ініціалізації
                OnInitializationCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                _isInitializing = false;
                Debug.LogError($"[GameBootstrapper] Initialization failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Метод для ручного запуску ініціалізації
        /// </summary>
        public void Initialize()
        {
            if (!_isInitialized && !_isInitializing)
            {
                InitializeAsync().Forget();
            }
        }

        private void InitializeDependencyInjection()
        {
            Debug.Log("[GameBootstrapper] Initializing dependency injection");
            _container = new DIContainer();

            // Реєстрація базових сервісів
            _container.RegisterSingleton<IEventBus, EventBus>();
            _container.RegisterSingleton<IMythLogger, UnityLogger>();

            // Реєстрація всіх інсталяторів
            InstallerRegistry.RegisterInstallers(_container);

            Debug.Log("[GameBootstrapper] Dependency injection initialized");
        }

        private void InitializeLogging()
        {
            _logger = _container.Resolve<IMythLogger>();
            _logger.LogInfo("[GameBootstrapper] Logging system initialized");
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
            _container.RegisterInstance<Systems.Core.SystemRegistry>(systemRegistry);

            _logger.LogInfo("[GameBootstrapper] ECS world initialized");
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new GameStateMachine(_container);
            _stateMachine.Initialize();
            _container.RegisterInstance(_stateMachine);

            _logger.LogInfo("[GameBootstrapper] Game state machine initialized");
        }

        private async UniTask InitializeServicesAsync()
        {
            _logger.LogInfo("[GameBootstrapper] Starting async services initialization");

            // Тут можна ініціалізувати сервіси, які потребують асинхронності
            // Наприклад, завантаження конфігурацій, підключення до серверів тощо

            // Імітація асинхронної операції
            await UniTask.Delay(100);

            _logger.LogInfo("[GameBootstrapper] Async services initialization completed");
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            _ecsWorld?.Update(Time.deltaTime);
            _stateMachine?.Update();
        }

        private void OnDestroy()
        {
            if (_ecsWorld != null)
            {
                _ecsWorld.Dispose();
                _ecsWorld = null;
            }

            _logger?.LogInfo("[GameBootstrapper] Destroyed");
        }

        /// <summary>
        /// Метод для очікування завершення ініціалізації (використовуйте в інших компонентах)
        /// </summary>
        public async UniTask WaitForInitializationAsync()
        {
            if (_isInitialized)
                return;

            if (!_isInitializing)
            {
                InitializeAsync().Forget();
            }

            // Чекаємо завершення ініціалізації
            while (_isInitializing)
            {
                await UniTask.Yield();
            }
        }

        /// <summary>
        /// Корутина для очікування завершення ініціалізації (альтернативний спосіб для MonoBehaviour)
        /// </summary>
        public IEnumerator WaitForInitializationCoroutine()
        {
            if (_isInitialized)
                yield break;

            if (!_isInitializing)
            {
                InitializeAsync().Forget();
            }

            // Чекаємо завершення ініціалізації
            while (_isInitializing)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Отримати GameBootstrapper зі сцени або створити новий
        /// </summary>
        public static GameBootstrapper GetOrCreate()
        {
            var bootstrapper = FindFirstObjectByType<GameBootstrapper>();
            if (bootstrapper == null)
            {
                var go = new GameObject("GameBootstrapper");
                bootstrapper = go.AddComponent<GameBootstrapper>();
            }
            return bootstrapper;
        }
    }
}
