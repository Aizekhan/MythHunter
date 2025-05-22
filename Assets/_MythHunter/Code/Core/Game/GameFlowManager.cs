// Assets/_MythHunter/Code/Core/Game/GameFlowManager.cs

using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.SceneManagement;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Lobby;
using MythHunter.States;
using MythHunter.Systems.Core;
using MythHunter.UI.Core;
using MythHunter.UI.Navigation;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;
using System;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Централізований сервіс для управління переходами між ігровими станами та сценами
    /// </summary>
    public class GameFlowManager : IGameFlowManager
    {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ISceneDispatcher _sceneDispatcher;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly ISystemRegistry _systemRegistry;
        private readonly INavigationService _navigationService;
        private string _currentSceneName = string.Empty;
        private bool _isSubscribed;

        [Inject]
        public GameFlowManager(
     IGameStateMachine gameStateMachine,
     ISceneDispatcher sceneDispatcher,
     IEventBus eventBus,
     IMythLogger logger,
     ISystemRegistry systemRegistry,
     INavigationService navigationService)
        {
            _gameStateMachine = gameStateMachine;
            _sceneDispatcher = sceneDispatcher;
            _eventBus = eventBus;
            _logger = logger;
            _systemRegistry = systemRegistry;
            _navigationService = navigationService;

            SubscribeToEvents();
        }

        /// <summary>
        /// Підписується на події
        /// </summary>
        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<GameStartRequestEvent>(OnGameStartRequested);
            _isSubscribed = true;
            _logger.LogInfo("GameFlowManager підписався на події", "GameFlow");
        }

        /// <summary>
        /// Відписується від подій
        /// </summary>
        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<GameStartRequestEvent>(OnGameStartRequested);
            _isSubscribed = false;
            _logger.LogInfo("GameFlowManager відписався від подій", "GameFlow");
        }

        /// <summary>
        /// Обробник події запиту на початок гри
        /// </summary>

        private void OnGameStartRequested(GameStartRequestEvent evt)
        {
            _logger.LogInfo("Отримано запит на початок гри", "GameFlow");

            // Отримуємо системи для взаємодії з лоббі
            var lobbySystem = _systemRegistry.GetSystem<ILobbySystem>();

            // Отримуємо список вибраних героїв, якщо система лоббі доступна
            string[] selectedHeroes = null;
            if (lobbySystem != null)
            {
                selectedHeroes = lobbySystem.GetSelectedHeroes().ToArray();
                _logger.LogInfo($"Отримано {selectedHeroes.Length} вибраних героїв з Lobby", "GameFlow");
            }

            // Запускаємо асинхронний перехід до Gameplay без очікування завершення
            EnterGameplayAsync(selectedHeroes).Forget();
        }

        /// <summary>
        /// Запускає перехід від Boot до Lobby
        /// </summary>
        // Оновіть метод для переходу до лоббі:
        // Assets/_MythHunter/Code/Core/Game/GameFlowManager.cs

        public async UniTask EnterLobbyAsync()
        {
            _logger.LogInfo("🚀 GameFlowManager: Ініціація переходу до лобі", "GameFlow");

            try
            {
                // 1. Ініціюємо зміну стану на Loading
                var context = new LoadingStateContext
                {
                    SelectedHeroArchetypes = Array.Empty<string>(),
                    MapId = "lobby_preload",
                    NextState = GameStateType.Lobby
                };

                // 2. Завантажуємо LoadingScene
                await _sceneDispatcher.LoadSceneAsync("LoadingScene");
                _currentSceneName = "LoadingScene";

                // 3. Можемо зробити preload базових ресурсів
                await PreloadBasicResourcesAsync();

                // 4. Переходимо в LoadingState (він покаже UI і запустить LoadingSystem)
                _gameStateMachine.ChangeState(GameStateType.Loading, context);

                _logger.LogInfo("✅ GameFlowManager: Ініціація завершена, контроль передано LoadingState", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ GameFlowManager помилка: {ex.Message}", "GameFlow", ex);
                throw;
            }
        }

        public async UniTask EnterGameplayAsync(string[] selectedHeroArchetypes, string mapId = "default")
        {
            _logger.LogInfo("🚀 GameFlowManager: Ініціація переходу до геймплею", "GameFlow");

            try
            {
                // 1. Контекст з повними даними для важкого завантаження
                var context = new LoadingStateContext
                {
                    SelectedHeroArchetypes = selectedHeroArchetypes,
                    MapId = mapId,
                    NextState = GameStateType.Gameplay
                };

                // 2. Завантажуємо LoadingScene
                await _sceneDispatcher.LoadSceneAsync("LoadingScene");
                _currentSceneName = "LoadingScene";

                // 3. Можемо preload-ити критичні ресурси
                await PreloadCriticalResourcesAsync();

                // 4. Переходимо в LoadingState для важкого завантаження
                _gameStateMachine.ChangeState(GameStateType.Loading, context);

                _logger.LogInfo("✅ GameFlowManager: Ініціація завершена, LoadingState займеться рештою", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ GameFlowManager помилка: {ex.Message}", "GameFlow", ex);
                await ReturnToLobbyAsync();
            }
        }
        public async UniTask ReturnToLobbyAsync()
        {
            _logger.LogInfo("Повернення до лобі після помилки", "GameFlow");

            try
            {
                await EnterLobbyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при поверненні в лобі: {ex.Message}", "GameFlow", ex);
                throw;
            }
        }
        private async UniTask PreloadBasicResourcesAsync()
        {
            _logger.LogInfo("📦 GameFlowManager: Preload базових ресурсів", "GameFlow");
            // Можемо завантажити UI prefab-и, іконки тощо
            await UniTask.Delay(200); // Симуляція
        }

        private async UniTask PreloadCriticalResourcesAsync()
        {
            _logger.LogInfo("📦 GameFlowManager: Preload критичних ресурсів", "GameFlow");
            // Можемо завантажити основні системи, базові префаби
            await UniTask.Delay(500); // Симуляція
        }

        /// <summary>
        /// Повертається з будь-якого стану до головного меню
        /// </summary>
        public async UniTask ReturnToMainMenuAsync()
        {
            _logger.LogInfo("Початок повернення до головного меню", "GameFlow");

            try
            {
                GameStateType previousState = _gameStateMachine.CurrentState;

                await _sceneDispatcher.LoadSceneAsync("MainMenuScene");
                _currentSceneName = "MainMenuScene";

                _gameStateMachine.ChangeState(GameStateType.MainMenu);

                // Публікуємо подію зміни стану гри
                PublishStateChange(previousState, GameStateType.MainMenu);

                _logger.LogInfo("Повернення до головного меню завершено", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при поверненні до головного меню: {ex.Message}", "GameFlow", ex);
                throw;
            }
        }

        /// <summary>
        /// Перезавантажує поточну сцену
        /// </summary>
        public async UniTask ReloadCurrentSceneAsync()
        {
            _logger.LogInfo($"Початок перезавантаження поточної сцени: {_currentSceneName}", "GameFlow");

            try
            {
                if (string.IsNullOrEmpty(_currentSceneName))
                {
                    _currentSceneName = _sceneDispatcher.GetActiveScene();
                }

                GameStateType currentState = _gameStateMachine.CurrentState;

                await _sceneDispatcher.LoadSceneAsync(_currentSceneName);

                // Публікуємо подію перезавантаження сцени
                _eventBus.Publish(new SceneReloadedEvent
                {
                    SceneName = _currentSceneName,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInfo($"Перезавантаження сцени {_currentSceneName} завершено", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при перезавантаженні сцени: {ex.Message}", "GameFlow", ex);
                throw;
            }
        }

        /// <summary>
        /// Запускає гру з самого початку (Boot)
        /// </summary>
        public async UniTask RestartGameAsync()
        {
            _logger.LogInfo("Початок перезапуску гри", "GameFlow");

            try
            {
                GameStateType previousState = _gameStateMachine.CurrentState;

                await _sceneDispatcher.LoadSceneAsync("LoadingScene");
                _currentSceneName = "LoadingScene";

                _gameStateMachine.ChangeState(GameStateType.Boot);

                // Публікуємо подію зміни стану гри
                PublishStateChange(previousState, GameStateType.Boot);

                _logger.LogInfo("Перезапуск гри завершено", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при перезапуску гри: {ex.Message}", "GameFlow", ex);
                throw;
            }
        }

        private void PublishStateChange(GameStateType previousState, GameStateType newState)
        {
            _eventBus.Publish(new GameStateChangedEvent
            {
                PreviousState = previousState,
                NewState = newState,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Звільнення ресурсів при знищенні об'єкту
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("GameFlowManager видалено", "GameFlow");
        }
        private async void EnterLobby()
        {
            // Підготовка параметрів
            var parameters = new NavigationParameters();
            parameters.Add("Mode", "Standard");

            // Налаштування представлень для сцени
            await _navigationService.SetupForSceneAsync("LobbyScene", parameters);

            // Публікуємо подію входу в лобі
            _eventBus.Publish(new LobbyStateEnteredEvent { Timestamp = DateTime.UtcNow });
        }
    }

   
}
