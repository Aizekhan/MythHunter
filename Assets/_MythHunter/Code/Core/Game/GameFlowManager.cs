// Assets/_MythHunter/Code/Core/Game/GameFlowManager.cs

using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.SceneManagement;
using MythHunter.Entities.Archetypes;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Resources;
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

        [Inject]
        public GameFlowManager(
     IGameStateMachine gameStateMachine,
     ISceneDispatcher sceneDispatcher,
     IEventBus eventBus,
     IMythLogger logger,
     ISystemRegistry systemRegistry,
     INavigationService navigationService,
      IPreloadManager preloadManager,
      AutoPreloadConfigurator autoPreloadConfigurator
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

        private void OnGameStartRequested(GameStartRequestEvent evt)
        {
            _logger.LogInfo("Отримано запит на початок гри", "GameFlow");

            // Отримуємо системи для взаємодії з лоббі
            var systemRegistry = _systemRegistry;
            var lobbySystem = systemRegistry.GetSystem<ILobbySystem>();

            string[] selectedHeroes = null;
            if (lobbySystem != null)
            {
                selectedHeroes = lobbySystem.GetSelectedHeroes().ToArray();
                _logger.LogInfo($"Отримано {selectedHeroes.Length} вибраних героїв з Lobby", "GameFlow");
            }

            EnterGameplayAsync(selectedHeroes).Forget();
        }

        /// <summary>
        /// Запускає перехід від Boot до Lobby
        /// </summary>
        // Оновіть метод для переходу до лоббі:
        // Assets/_MythHunter/Code/Core/Game/GameFlowManager.cs

        /// <summary>
        /// ✅ НОВИЙ підхід: Конфігурація preload + швидкий перехід до лобі
        /// </summary>
        public async UniTask EnterLobbyAsync()
        {
            _logger.LogInfo("🚀 GameFlowManager: Перехід до лобі через PreloadManager", "GameFlow");

            try
            {
                // 1. Показуємо LoadingScene
                await _sceneDispatcher.LoadSceneAsync("LoadingScene");
                _currentSceneName = "LoadingScene";
                await ShowLoadingUIAsync("Підготовка лобі...");

                // 2. ✅ Конфігуруємо preload для лобі (БЕЗ хардкоду)
                ConfigureLobbyPreload();

                // 3. Невелика затримка для показу Loading UI
                await UniTask.Delay(800);

                // 4. Переходимо до LobbyScene (PreloadManager автоматично завантажить ресурси)
                await _sceneDispatcher.LoadSceneAsync("LobbyScene");
                _currentSceneName = "LobbyScene";

                // 5. Зміна стану (LobbyState сам ініціалізує системи)
                _gameStateMachine.ChangeState(GameStateType.Lobby);

                _logger.LogInfo("✅ GameFlowManager: Лобі готове з preload ресурсами", "GameFlow");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка переходу до лобі: {ex.Message}", "GameFlow", ex);
                await ReturnToMainMenuAsync();
            }
        }

        /// <summary>
        /// ✅ Конфігурація preload без хардкоду
        /// </summary>
        /// <summary>
        /// ✅ Конфігурація preload для лобі без хардкоду
        /// </summary>
        private void ConfigureLobbyPreload()
        {
            _logger.LogInfo("📋 Конфігурація preload для лобі", "GameFlow");

            // Найважливіші ресурси (високий пріоритет)
            _preloadManager.RegisterScenePreload<HeroArchetypeSO>("LobbyScene", "ScriptableObjects/Heroes", 100);
            _preloadManager.RegisterScenePreload<GameObject>("LobbyScene", "UI/Lobby/LobbyView", 90);

            // UI компоненти (середній пріоритет, з пулами)
            _preloadManager.RegisterScenePreload<GameObject>("LobbyScene", "UI/Lobby/HeroCardUI", 80, true, 20);
            _preloadManager.RegisterScenePreload<GameObject>("LobbyScene", "UI/Lobby/SelectedHeroCard", 70, true, 8);

            // Додаткові ресурси (низький пріоритет)
            _preloadManager.RegisterScenePreload<Sprite>("LobbyScene", "UI/Icons/hero_icons", 50);
            _preloadManager.RegisterScenePreload<AudioClip>("LobbyScene", "Audio/UI/lobby_sounds", 30);

            _logger.LogInfo("✅ Preload конфігурація для лобі зареєстрована", "GameFlow");
        }

        private void ConfigureGameplayPreload(string[] heroArchetypes)
        {
            // Реєструємо що потрібно завантажити для гри
            _preloadManager.RegisterScenePreload<GameObject>("GameScene", "UI/Game/GameplayUIView", 100);

            // Завантажуємо префаби вибраних героїв
            foreach (var archetypeId in heroArchetypes)
            {
                _preloadManager.RegisterScenePreload<GameObject>("GameScene", $"Prefabs/Heroes/{archetypeId}", 90, true, 5);
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

                // 2. ✅ Конфігуруємо preload для гри (динамічно)
                ConfigureGameplayPreload(selectedHeroArchetypes, mapId);

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
        /// ✅ Конфігурація preload для гри (динамічна, без хардкоду)
        /// </summary>
        private void ConfigureGameplayPreload(string[] selectedHeroArchetypes, string mapId)
        {
            _logger.LogInfo($"🎮 Конфігурація preload для гри: {selectedHeroArchetypes?.Length ?? 0} героїв, карта {mapId}", "GameFlow");

            // Базові ігрові ресурси (високий пріоритет)
            _preloadManager.RegisterScenePreload<GameObject>("GameScene", "UI/Game/GameplayUIView", 100);
            _preloadManager.RegisterScenePreload<GameObject>("GameScene", $"Prefabs/Maps/{mapId}", 95);

            // Завантажуємо префаби вибраних героїв (динамічно)
            if (selectedHeroArchetypes != null)
            {
                foreach (var archetypeId in selectedHeroArchetypes)
                {
                    _preloadManager.RegisterScenePreload<GameObject>("GameScene", $"Prefabs/Heroes/{archetypeId}", 90, true, 3);
                    _preloadManager.RegisterScenePreload<GameObject>("GameScene", $"Prefabs/Heroes/{archetypeId}_Effects", 70, true, 5);
                }
            }

            // Загальні ігрові ресурси (середній пріоритет)
            _preloadManager.RegisterScenePreload<GameObject>("GameScene", "Prefabs/Items/Chest", 60, true, 5);
            _preloadManager.RegisterScenePreload<GameObject>("GameScene", "Prefabs/Effects/CombatEffects", 50, true, 10);

            // Аудіо та інші ресурси (низький пріоритет)
            _preloadManager.RegisterScenePreload<AudioClip>("GameScene", "Audio/Game/combat_sounds", 40);
            _preloadManager.RegisterScenePreload<AudioClip>("GameScene", "Audio/Game/ambient_music", 30);

            _logger.LogInfo("✅ Preload конфігурація для гри зареєстрована", "GameFlow");
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



        public async UniTask ReturnToMainMenuAsync()
        {
            _logger.LogInfo("Повернення до головного меню", "GameFlow");

            try
            {
                await _sceneDispatcher.LoadSceneAsync("MainMenuScene");
                _currentSceneName = "MainMenuScene";
                _gameStateMachine.ChangeState(GameStateType.MainMenu);
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


        /// <summary>
        /// Запускає гру з самого початку (Boot)
        /// </summary>
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
        /// Звільнення ресурсів при знищенні об'єкту
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("GameFlowManager знищено", "GameFlow");
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
