// Assets/_MythHunter/Code/Core/Game/States/LoadingState.cs
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Systems.Loading;
using MythHunter.UI.Core;
using MythHunter.UI.Navigation;
using MythHunter.Utils.Logging;
using System.Threading;
using MythHunter.Systems.Core;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Core.SceneManagement;
using MythHunter.UI.Presenters;
using MythHunter.UI.Views;

namespace MythHunter.States
{
    /// <summary>
    /// Стан для управління процесом завантаження гри
    /// </summary>
    public class LoadingState : BaseState<GameStateType>
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly ILoadingSystem _loadingSystem;
        private readonly INavigationService _navigationService;
        private readonly ISystemRegistry _systemRegistry;
        private readonly IGameStateMachine _stateMachine;
        private readonly ISceneDispatcher _sceneDispatcher;
        private GameStateType _nextState;
        private string[] _selectedHeroArchetypes;
        private string _mapId;
        private bool _isEntered = false;

        // Семафор для синхронізації
        private readonly SemaphoreSlim _enterSemaphore = new SemaphoreSlim(1, 1);

        public LoadingState(IDIContainer container) : base(container)
        {
            _eventBus = container.Resolve<IEventBus>();
            _logger = container.Resolve<IMythLogger>();
            _loadingSystem = container.Resolve<ILoadingSystem>();
            _navigationService = container.Resolve<INavigationService>();
            _systemRegistry = container.Resolve<ISystemRegistry>();
            _stateMachine = container.Resolve<IGameStateMachine>();
            _sceneDispatcher = container.Resolve<ISceneDispatcher>();

        }

        public override GameStateType StateId => GameStateType.Loading;


        public override void Enter(GameStateType previousState)
        {
            _logger.LogInfo("🔄 LoadingState: Початок завантаження", "LoadingState");

            ManageLoadingProcessAsync(previousState).Forget(); // ✅ ПРАВИЛЬНИЙ МЕТОД
        }
        private async UniTaskVoid EnterLobbyAsync(GameStateType previousState)
        {
            try
            {
                await _enterSemaphore.WaitAsync();
                if (_isEntered)
                    return;
                _isEntered = true;

                // 1. Завантажуємо LobbyScene (LoadingScene → LobbyScene)
                await _sceneDispatcher.LoadSceneAsync("LobbyScene");

                // 2. Налаштовуємо навігацію для лобі
                var parameters = new NavigationParameters();
                parameters.Add("PreviousState", previousState.ToString());
                await _navigationService.SetupForSceneAsync("LobbyScene", parameters);

                // 3. Ініціалізуємо системи лобі
                await InitializeStateSystemsAsync();
                await InitializeLobbyAsync();

                // 4. Публікуємо події
                _eventBus.Publish(new GameStateChangedEvent
                {
                    PreviousState = previousState,
                    NewState = GameStateType.Lobby,
                    Timestamp = DateTime.UtcNow
                });

                _eventBus.Publish(new LobbyStateEnteredEvent
                {
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInfo("✅ LobbyState: Повністю ініціалізовано", "LobbyState");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ LobbyState помилка: {ex.Message}", "LobbyState", ex);
            }
            finally
            {
                _enterSemaphore.Release();
            }
        }
        private async UniTask InitializeAndStartLoadingAsync()
        {
            _logger.LogInfo("🔧 LoadingState: Ініціалізація LoadingSystem", "LoadingState");

            // 1. Ініціалізуємо системи завантаження
            _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.OnDemand);

            // ✅ ЗАТРИМКА для ініціалізації UI
            await UniTask.Delay(500);

            // 2. Запускаємо LoadingSystem
            bool success = false;

            if (_nextState == GameStateType.Lobby)
            {
                _logger.LogInfo("🎯 LoadingState: Легке завантаження для лобі", "LoadingState");
                success = await _loadingSystem.StartLoadingGameAsync(_selectedHeroArchetypes, _mapId);
            }
            else if (_nextState == GameStateType.Gameplay)
            {
                _logger.LogInfo("🎯 LoadingState: Повне завантаження для геймплею", "LoadingState");
                success = await _loadingSystem.StartLoadingGameAsync(_selectedHeroArchetypes, _mapId);
            }

            // ✅ ЗАТРИМКА перед переходом
            await UniTask.Delay(300);

            // 3. Завершуємо і переходимо до цільового стану
            if (success)
            {
                await _loadingSystem.FinishLoadingAsync();

                // ✅ ЩЕ ОДНА ЗАТРИМКА
                await UniTask.Delay(200);

                _stateMachine.ChangeState(_nextState);
            }
            else
            {
                _logger.LogWarning("⚠️ LoadingState: Завантаження не вдалося", "LoadingState");
                _stateMachine.ChangeState(GameStateType.MainMenu);
            }
        }
        private async UniTask InitializeStateSystemsAsync()
        {
            _logger.LogInfo("Ініціалізація систем Lobby...", "LoadingState");
            _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.Lobby);
            await UniTask.Yield(); // щоб прибрати CS1998
        }

        private async UniTask InitializeLobbyAsync()
        {
            _logger.LogInfo("Ініціалізація Lobby логіки...", "LoadingState");

            // Приклад: резолв та ініціалізація LobbySystem, якщо потрібно
            var lobbySystem = _container.Resolve<ILobbySystem>();
            lobbySystem.Initialize(); // якщо такий метод є

            await UniTask.Yield();
        }
        private async UniTaskVoid ManageLoadingProcessAsync(GameStateType previousState)
        {
            try
            {
                await _enterSemaphore.WaitAsync();

                if (_isEntered)
                    return;
                _isEntered = true;

                // 1. Отримуємо контекст від GameFlowManager
                var context = GetStateContext<LoadingStateContext>();
                ExtractContextParameters(context);

                // 2. Публікуємо подію зміни стану
                _eventBus.Publish(new GameStateChangedEvent
                {
                    PreviousState = previousState,
                    NewState = GameStateType.Loading,
                    Timestamp = DateTime.UtcNow
                });

                // 3. ✅ ПОКАЗУЄМО UI ЗАВАНТАЖЕННЯ
                await _navigationService.SetupForSceneAsync("LoadingScene", new NavigationParameters());

                // ✅ ЧЕКАЄМО НА СТВОРЕННЯ UI
                await UniTask.DelayFrame(5);

                // 4. ✅ СТВОРЮЄМО LOADINGSCREEN VIEW З ПЕРЕВІРКОЮ
                var uiService = _container.Resolve<IUIService>();
                var loadingView = await uiService.ShowScreenAsync(ViewId.LoadingScreen);

                if (loadingView == null)
                {
                    _logger.LogError("❌ Не вдалося створити LoadingScreen view!", "LoadingState");
                    return;
                }

                // ✅ ПРИВ'ЯЗУЄМО PRESENTER ДО VIEW ЧЕРЕЗ ІНТЕРФЕЙС
                var loadingPresenter = _container.Resolve<ILoadingScreenPresenter>();
                if (loadingPresenter != null && loadingView is ILoadingScreenView typedView)
                {
                    _logger.LogInfo("🔗 Прив'язування LoadingScreenPresenter до View", "LoadingState");
                    loadingPresenter.SetView(typedView);
                }
                else
                {
                    _logger.LogError("❌ Не вдалося прив'язати presenter до view", "LoadingState");
                }

                // ✅ ЗАТРИМКА для ініціалізації UI
                await UniTask.Delay(500);

                // 5. Ініціалізуємо LoadingSystem і запускаємо завантаження
                await InitializeAndStartLoadingAsync();

                _logger.LogInfo("✅ LoadingState: Завантаження завершено", "LoadingState");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ LoadingState помилка: {ex.Message}", "LoadingState", ex);
                _stateMachine.ChangeState(GameStateType.MainMenu);
            }
            finally
            {
                _enterSemaphore.Release();
            }
        }
        private async UniTaskVoid EnterAsyncProcess(GameStateType previousState)
        {
            try
            {
                // Захоплюємо семафор
                await _enterSemaphore.WaitAsync();

                if (_isEntered)
                {
                    _logger.LogWarning("Повторний вхід у LoadingState проігноровано", "GameState");
                    return;
                }

                _isEntered = true;

                // Публікуємо подію зміни стану гри
                _eventBus.Publish(new GameStateChangedEvent
                {
                    PreviousState = previousState,
                    NewState = GameStateType.Loading,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInfo("GameStateChangedEvent опубліковано", "LoadingState");

                // Отримуємо параметри з контексту
                var context = GetStateContext<LoadingStateContext>();
                ExtractContextParameters(context);

                // Налаштовуємо навігацію для екрану завантаження
                var parameters = new NavigationParameters();
                parameters.Add("PreviousState", previousState.ToString());
                parameters.Add("SelectedHeroes", _selectedHeroArchetypes);
                parameters.Add("MapId", _mapId);

                await _navigationService.SetupForSceneAsync("LoadingScene", parameters);

                // Ініціалізуємо системи завантаження
                await InitializeLoadingSystemsAsync();
                await _container.Resolve<IUIService>().ShowScreenAsync(ViewId.LoadingScreen);
                // Починаємо процес завантаження
                await StartLoadingProcessAsync();

                _logger.LogInfo("[LIFECYCLE] Завершено вхід у LoadingState", "GameState");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при вході в LoadingState: {ex.Message}", "GameState", ex);
            }
            finally
            {
                _enterSemaphore.Release();
            }
        }
       
        private void ExtractContextParameters(object context)
        {
            if (context is LoadingStateContext loadingContext)
            {
                _selectedHeroArchetypes = loadingContext.SelectedHeroArchetypes;
                _mapId = loadingContext.MapId;
                _nextState = loadingContext.NextState; // 👈 зберігаємо наступний стан
            }
            else
            {
                _selectedHeroArchetypes = Array.Empty<string>();
                _mapId = "default";
                _nextState = GameStateType.Lobby; // 👈 дефолт
            }
        }

        private async UniTask InitializeLoadingSystemsAsync()
        {
            _logger.LogInfo("Ініціалізація систем завантаження", "LoadingState");

            try
            {
                // Ініціалізуємо системи категорії Loading
                _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.OnDemand);

                // Даємо можливість Unity завершити оновлення UI
                await UniTask.DelayFrame(2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при ініціалізації систем завантаження: {ex.Message}", "LoadingState", ex);
                throw;
            }
        }

        private async UniTask StartLoadingProcessAsync()
        {
            _logger.LogInfo("Початок процесу завантаження", "LoadingState");

            try
            {
                // Починаємо процес завантаження
                bool result = await _loadingSystem.StartLoadingGameAsync(_selectedHeroArchetypes, _mapId);

                if (result)
                {
                    // Завантаження успішне, переходимо до ігрової сцени
                    await _loadingSystem.FinishLoadingAsync();

                    // Переходимо в ігровий стан
                    _stateMachine.ChangeState(_nextState);

                }
                else
                {
                    // Завантаження не вдалося, повертаємося в лобі
                    _logger.LogWarning("Завантаження не вдалося, повернення в лобі", "LoadingState");
                    _stateMachine.ChangeState(GameStateType.Lobby);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка в процесі завантаження: {ex.Message}", "LoadingState", ex);

                // При критичній помилці повертаємося в лобі
                _stateMachine.ChangeState(GameStateType.Lobby);

            }
        }

        public override void Exit()
        {
            _logger.LogInfo("[LIFECYCLE] Вихід зі стану LoadingState", "GameState");

            // 🔥 КРИТИЧНО: Приховуємо LoadingScreen при виході
            var uiService = _container.Resolve<IUIService>();
            uiService.HideScreen(ViewId.LoadingScreen);

            // Скидаємо стан
            _isEntered = false;
            _selectedHeroArchetypes = null;
            _mapId = null;
        }

    }

    /// <summary>
    /// Контекст для передачі параметрів у стан завантаження
    /// </summary>
    public class LoadingStateContext
    {
        public string[] SelectedHeroArchetypes { get; set; } = Array.Empty<string>();
        public string MapId { get; set; } = "default";
        public GameStateType NextState { get; set; } = GameStateType.Gameplay; // 👈 оце головне
    }
}
