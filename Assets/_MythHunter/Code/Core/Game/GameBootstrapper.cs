// Assets/_MythHunter/Code/Core/Game/GameBootstrapper.cs
using UnityEngine;
using System;
using System.Threading;
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
using MythHunter.Services.Timer; // EPIC 08 Phase 2
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
        private ITimerService _timerService; // EPIC 08 Phase 2

        // EPIC 08: Cancellation token for async initialization
        private CancellationTokenSource _initCancellationTokenSource;
        private const int INIT_TIMEOUT_MS = 30000; // 30 seconds

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // EPIC 08: Create cancellation token source for initialization
            _initCancellationTokenSource = new CancellationTokenSource();
            InitializeAsync(_initCancellationTokenSource.Token).Forget();
        }

      
        private void Update()
        {
            // EPIC 08 Phase 2: Update TimerService
            _timerService?.UpdateTimers(Time.deltaTime);

            _ecsWorld?.Update(Time.deltaTime);
            _stateMachine?.Update();
        }

        private void OnDestroy()
        {
            // EPIC 08: Cancel initialization if still running
            _initCancellationTokenSource?.Cancel();
            _initCancellationTokenSource?.Dispose();

            _composition?.Shutdown();
            _ecsWorld?.Dispose();
            _logger?.LogInfo("GameBootstrapper destroyed", "Bootstrapper");
        }
        /// <summary>
        /// EPIC 08: Added CancellationToken parameter and timeout
        /// </summary>
        private async UniTaskVoid InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // EPIC 08: Add timeout to prevent indefinite hang
                using var timeoutCts = new CancellationTokenSource(INIT_TIMEOUT_MS);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                InitializeCore();
                InitializeGameSystems();

                _logger.LogInfo("GameBootstrapper ініціалізовано успішно");

                await InitializeServicesAsync(linkedCts.Token);

                // Додаємо затримку для завершення всіх ініціалізацій
                await UniTask.DelayFrame(5, cancellationToken: linkedCts.Token);

                // Змінюємо стан на Boot
                _stateMachine?.ChangeState(GameStateType.Boot);

                _logger.LogInfo("GameBootstrapper initialization completed", "Bootstrapper");
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("GameBootstrapper initialization was cancelled", "Bootstrapper");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"GameBootstrapper initialization failed: {ex.Message}", "Bootstrapper", ex);
                throw; // Re-throw to ensure failure is visible
            }
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

        /// <summary>
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        private async UniTask InitializeServicesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInfo("Початок асинхронної ініціалізації сервісів", "Bootstrapper");

            await UniTask.Delay(100, cancellationToken: cancellationToken);
            _sceneDispatcher = _container.Resolve<ISceneDispatcher>();

            // Явно ініціалізуємо GameFlowManager перед зміною стану
            var gameFlowManager = _container.Resolve<IGameFlowManager>();

            // EPIC 08 Phase 2: Resolve TimerService for Update loop
            _timerService = _container.Resolve<ITimerService>();

            _logger.LogInfo("Асинхронна ініціалізація сервісів завершена", "Bootstrapper");
        }
    }
}
