// Assets/_MythHunter/Code/Core/Game/GameFlowManager.cs

using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.SceneManagement;
using MythHunter.Entities.Archetypes;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Resources;
using MythHunter.Services.GameSettings;
using MythHunter.States;
using MythHunter.Systems.Core;
using MythHunter.UI.Core;
using MythHunter.UI.Navigation;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;
using System;
using UnityEngine;

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
        private readonly IPreloadManager _preloadManager;
        private readonly AutoPreloadConfigurator _autoPreloadConfigurator;
        private string _currentSceneName = string.Empty;
        private bool _isSubscribed;
        private readonly IGameSettingsService _gameSettings;
        [Inject]
        public GameFlowManager(
     IGameStateMachine gameStateMachine,
     ISceneDispatcher sceneDispatcher,
     IEventBus eventBus,
     IMythLogger logger,
     ISystemRegistry systemRegistry,
     INavigationService navigationService,
      IPreloadManager preloadManager,
      AutoPreloadConfigurator autoPreloadConfigurator,
        IGameSettingsService gameSettings
     )
        {

            _gameStateMachine = gameStateMachine;
            _sceneDispatcher = sceneDispatcher;
            _eventBus = eventBus;
            _logger = logger;
            _systemRegistry = systemRegistry;
            _navigationService = navigationService;
            _preloadManager = preloadManager;
            _autoPreloadConfigurator = autoPreloadConfigurator;
            _gameSettings = gameSettings;

            SubscribeToEvents();
            InitializePreloadConfigsAsync().Forget();
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

        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<GameStartRequestEvent>(OnGameStartRequested);
            _isSubscribed = false;
            _logger.LogInfo("GameFlowManager відписався від подій", "GameFlow");
        }

        /// <summary>
        /// ✅ Ініціалізуємо preload конфігурації зі ScriptableObject-ів
        /// </summary>
        private async UniTaskVoid InitializePreloadConfigsAsync()
        {
            try
            {
                await _autoPreloadConfigurator.LoadAndRegisterAllConfigsAsync();

                var stats = _autoPreloadConfigurator.GetStatistics();
                _logger.LogInfo($"📊 Preload статистика: {stats.TotalConfigs} конфігурацій, {stats.TotalResources} ресурсів", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка ініціалізації preload конфігурацій: {ex.Message}", "GameFlow", ex);
            }
        }


        /// <summary>
        /// Обробник події запиту на початок гри
        /// </summary>

        // Assets/_MythHunter/Code/Core/Game/GameFlowManager.cs
        private void OnGameStartRequested(GameStartRequestEvent evt)
        {
            _logger.LogInfo("Отримано запит на початок гри", "GameFlow");

            // ✅ ОТРИМУЄМО ГЕРОЇВ ВІД LOBBY SYSTEM
            var lobbySystem = _systemRegistry.GetSystem<ILobbySystem>();
            if (lobbySystem == null)
            {
                _logger.LogError("LobbySystem не знайдено!", "GameFlow");
                return;
            }

            var selectedHeroes = lobbySystem.GetSelectedHeroes()?.ToArray();
            if (selectedHeroes == null || selectedHeroes.Length == 0)
            {
                _logger.LogWarning("Жодного героя не вибрано!", "GameFlow");
                selectedHeroes = new string[0];
            }

            _logger.LogInfo($"Отримано {selectedHeroes.Length} вибраних героїв з Lobby", "GameFlow");

            // ✅ ПЕРЕДАЄМО ЧЕРЕЗ SCENE DISPATCHER (як і було)
            EnterGameplayAsync(selectedHeroes).Forget();
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

        /// <summary>
        /// ✅ ОНОВЛЕНИЙ метод з урахуванням режиму гри
        /// </summary>
        public async UniTask EnterLobbyAsync()
        {
           
            string modeText = _gameSettings.CurrentGameMode switch
            {
                GameMode.OnlinePvP => "онлайн PvP",
                GameMode.LocalPvP => "локальний PvP",
                GameMode.PvAI => "гру з AI",
                _ => "невідомий режим"
            };

            _logger.LogInfo($"🚀 GameFlowManager: Перехід до лобі ({modeText})", "GameFlow");

            try
            {
                // 1. Показуємо LoadingScene з повідомленням про режим
                await _sceneDispatcher.LoadSceneAsync("LoadingScene");
                _currentSceneName = "LoadingScene";
                await ShowLoadingUIAsync($"Підготовка лобі для {modeText}...");

                // 2. Невелика затримка для показу Loading UI
                await UniTask.Delay(800);

                // 3. Переходимо до LobbyScene
                await _sceneDispatcher.LoadSceneAsync("LobbyScene");
                _currentSceneName = "LobbyScene";

                // 4. Передаємо інформацію про режим
                _sceneDispatcher.SetSceneData("GameMode", _gameSettings.CurrentGameMode);

                // 5. Зміна стану
                _gameStateMachine.ChangeState(GameStateType.Lobby);

                _logger.LogInfo($"✅ GameFlowManager: Лобі готове для режиму {modeText}", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка переходу до лобі: {ex.Message}", "GameFlow", ex);
                await ReturnToMainMenuAsync();
            }
        }



        /// <summary>
        /// ✅ НОВИЙ підхід: Конфігурація preload + швидкий перехід до гри
        /// </summary>
        public async UniTask EnterGameplayAsync(string[] selectedHeroArchetypes, string mapId = "default")
        {
            _logger.LogInfo("🎮 GameFlowManager: Перехід до гри через PreloadManager", "GameFlow");

            try
            {
                // 1. Показуємо LoadingScene
                await _sceneDispatcher.LoadSceneAsync("LoadingScene");
                _currentSceneName = "LoadingScene";
                await ShowLoadingUIAsync("Підготовка гри...");

              

                // 3. Зберігаємо дані для передачі
                _sceneDispatcher.SetSceneData("SelectedHeroArchetypes", selectedHeroArchetypes);
                _sceneDispatcher.SetSceneData("MapId", mapId);

                // 4. Більша затримка для завантаження гри
                await UniTask.Delay(1500);

                // 5. Переходимо до GameScene (PreloadManager автоматично завантажить)
                await _sceneDispatcher.LoadSceneAsync("GameScene");
                _currentSceneName = "GameScene";

                // 6. Зміна стану
                _gameStateMachine.ChangeState(GameStateType.Gameplay);

                _logger.LogInfo("✅ GameFlowManager: Гра готова з preload ресурсами", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка переходу до гри: {ex.Message}", "GameFlow", ex);
                await ReturnToLobbyAsync();
            }
        }

       
        /// <summary>
        /// Показує Loading UI з простою логікою
        /// </summary>
        private async UniTask ShowLoadingUIAsync(string message)
        {
            var parameters = new NavigationParameters();
            parameters.Add("Message", message);
            parameters.Add("ShowProgress", true);
            await _navigationService.SetupForSceneAsync("LoadingScene", parameters);

            // Мінімальна затримка для відображення UI
            await UniTask.DelayFrame(3);
        }



        /// <summary>
        /// ✅ НОВИЙ метод для повернення до головного меню
        /// </summary>
        public async UniTask ReturnToMainMenuAsync()
        {
            _logger.LogInfo("Повернення до головного меню", "GameFlow");

            try
            {
                await EnterMainMenuAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при поверненні до головного меню: {ex.Message}", "GameFlow", ex);
                throw;
            }
        }
        public async UniTask ReturnToLobbyAsync()
        {
            _logger.LogInfo("Повернення до лобі", "GameFlow");
            await EnterLobbyAsync();
        }
       
        /// <summary>
        /// Повертається з будь-якого стану до головного меню
        /// </summary>


        /// <summary>
        /// Перезавантажує поточну сцену
        /// </summary>
        public async UniTask ReloadCurrentSceneAsync()
        {
            _logger.LogInfo($"Перезавантаження поточної сцени: {_currentSceneName}", "GameFlow");

            try
            {
                if (string.IsNullOrEmpty(_currentSceneName))
                {
                    _currentSceneName = _sceneDispatcher.GetActiveScene();
                }

                await _sceneDispatcher.LoadSceneAsync(_currentSceneName);

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

        public async UniTask RestartGameAsync()
        {
            _logger.LogInfo("Перезапуск гри", "GameFlow");

            try
            {
                GameStateType previousState = _gameStateMachine.CurrentState;

                await _sceneDispatcher.LoadSceneAsync("LoadingScene");
                _currentSceneName = "LoadingScene";

                _gameStateMachine.ChangeState(GameStateType.Boot);

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
        /// Перехід до головного меню з врахуванням режиму
        /// </summary>
        public async UniTask EnterMainMenuAsync()
        {
            _logger.LogInfo("🏠 GameFlowManager: Перехід до головного меню", "GameFlow");

            try
            {
                // 1. Скидаємо режим гри до дефолтного
           
                _gameSettings.CurrentGameMode = GameMode.PvAI; // За замовчуванням

                // 2. Завантажуємо MainMenuScene
                await _sceneDispatcher.LoadSceneAsync("MainMenuScene");
                _currentSceneName = "MainMenuScene";

                // 3. Налаштовуємо навігацію
                var parameters = new NavigationParameters();
                parameters.Add("ShowWelcome", true);
                await _navigationService.SetupForSceneAsync("MainMenuScene", parameters);

                // 4. Зміна стану
                _gameStateMachine.ChangeState(GameStateType.MainMenu);

                _logger.LogInfo("✅ GameFlowManager: Головне меню готове", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка переходу до головного меню: {ex.Message}", "GameFlow", ex);
            }
        }

        /// <summary>
        /// Перехід до профілю гравця
        /// </summary>
        public async UniTask EnterProfileAsync()
        {
            _logger.LogInfo("👤 GameFlowManager: Перехід до профілю", "GameFlow");

            try
            {
                // 1. Показуємо LoadingScene
                await _sceneDispatcher.LoadSceneAsync("LoadingScene");
                _currentSceneName = "LoadingScene";
                await ShowLoadingUIAsync("Завантаження профілю...");

                // 2. Невелика затримка
                await UniTask.Delay(600);

                // 3. Переходимо до ProfileScene
                await _sceneDispatcher.LoadSceneAsync("ProfileScene");
                _currentSceneName = "ProfileScene";

                // 4. Передаємо дані профілю
                _sceneDispatcher.SetSceneData("PlayerId", "LocalPlayer");
                _sceneDispatcher.SetSceneData("ReturnTo", "MainMenu");

                // 5. Зміна стану
                _gameStateMachine.ChangeState(GameStateType.Profile);

                _logger.LogInfo("✅ GameFlowManager: Профіль готовий", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка переходу до профілю: {ex.Message}", "GameFlow", ex);
                await ReturnToMainMenuAsync();
            }
        }

        /// <summary>
        /// Звільнення ресурсів при знищенні об'єкту
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("GameFlowManager знищено", "GameFlow");
        }
        
    }

   
}
