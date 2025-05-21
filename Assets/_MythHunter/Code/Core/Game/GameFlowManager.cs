// Assets/_MythHunter/Code/Core/Game/GameFlowManager.cs

using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.SceneManagement;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Lobby;
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
    public class GameFlowManager : IGameFlowManager, IEventSubscriber
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
        public async UniTask EnterLobbyAsync()
        {
            _logger.LogInfo("Початок переходу до Lobby", "GameFlow");

            try
            {
                // Підготовка до зміни сцени
                await _navigationService.PrepareForSceneChangeAsync();

                // Завантажуємо сцену
                await _sceneDispatcher.LoadSceneAsync("LobbyScene");
                _currentSceneName = "LobbyScene";

                // Ініціалізуємо системи
                _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.Lobby);
                _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.OnDemand);

                // Змінюємо стан гри
                _gameStateMachine.ChangeState(GameStateType.Lobby);

                // Налаштування навігації для сцени - використовуємо enum
                var parameters = new NavigationParameters();
                await _navigationService.NavigateToAsync(ViewId.Lobby, parameters);

                // Публікуємо подію зміни стану гри
                PublishStateChange(GameStateType.Boot, GameStateType.Lobby);

                _logger.LogInfo("Перехід до Lobby завершено", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при переході до Lobby: {ex.Message}", "GameFlow", ex);
                throw;
            }
        }
        /// <summary>
        /// Запускає перехід від Lobby до Gameplay
        /// </summary>
        public async UniTask EnterGameplayAsync(string[] selectedHeroArchetypes = null)
        {
            _logger.LogInfo($"Початок переходу до Gameplay з {selectedHeroArchetypes?.Length ?? 0} вибраними героями", "GameFlow");

            try
            {
                // Якщо передано дані про вибраних героїв, зберігаємо їх для наступної сцени
                if (selectedHeroArchetypes != null && selectedHeroArchetypes.Length > 0)
                {
                    _sceneDispatcher.SetSceneData("SelectedHeroArchetypes", selectedHeroArchetypes);
                    _logger.LogInfo($"Дані про вибраних героїв ({selectedHeroArchetypes.Length}) передано в наступну сцену", "GameFlow");
                }

                // Використовуємо спеціальний метод для завантаження ігрової сцени з вибраними героями
                if (selectedHeroArchetypes != null && selectedHeroArchetypes.Length > 0)
                {
                    await _sceneDispatcher.LoadGameSceneAsync(selectedHeroArchetypes);
                }
                else
                {
                    await _sceneDispatcher.LoadSceneAsync("GameScene");
                }

                _currentSceneName = "GameScene";

                _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.Gameplay);

                _gameStateMachine.ChangeState(GameStateType.Game);

                // Публікуємо подію зміни стану гри
                PublishStateChange(GameStateType.Lobby, GameStateType.Game);

                // Публікуємо подію початку гри
                _eventBus.Publish(new GameStartedEvent
                {
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInfo("Перехід до Gameplay завершено", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при переході до Gameplay: {ex.Message}", "GameFlow", ex);
                throw;
            }
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
