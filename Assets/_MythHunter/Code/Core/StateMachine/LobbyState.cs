using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Systems.Core;
using MythHunter.UI.Core;
using MythHunter.UI.Navigation;
using MythHunter.Utils.Logging;
using MythHunter.Services.GameSettings;
using MythHunter.Systems.Lobby;
using MythHunter.UI.Presenters;
using System;
using System.Threading;

namespace MythHunter.States
{
    public class LobbyState : BaseState<GameStateType>
    {
        private readonly IUIService _uiService;
        private readonly IMythLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IGameFlowManager _gameFlowManager;
        private readonly INavigationService _navigationService;
        private readonly ISystemRegistry _systemRegistry;
        private readonly IGameSettingsService _gameSettings;

        // Додаємо семафор для уникнення паралельного входу
        private readonly SemaphoreSlim _enterSemaphore = new SemaphoreSlim(1, 1);
        private bool _isEntered = false;

        public LobbyState(IDIContainer container) : base(container)
        {
            _uiService = container.Resolve<IUIService>();
            _logger = container.Resolve<IMythLogger>();
            _eventBus = container.Resolve<IEventBus>();
            _gameFlowManager = container.Resolve<IGameFlowManager>();
            _navigationService = container.Resolve<INavigationService>();
            _systemRegistry = container.Resolve<ISystemRegistry>();
            _gameSettings = container.Resolve<IGameSettingsService>();
        }

        public override GameStateType StateId => GameStateType.Lobby;

        /// <summary>
        /// Вхід у стан - синхронний метод, що ініціює асинхронний процес
        /// </summary>
        public override void Enter(GameStateType previousState)
        {
            _logger.LogInfo("[LIFECYCLE] Початок входу в LobbyState", "GameState");

            // Запускаємо асинхронний вхід, не чекаючи його завершення
            EnterAsyncProcess(previousState).Forget();
        }

        /// <summary>
        /// Асинхронна реалізація входу в стан
        /// </summary>
        private async UniTaskVoid EnterAsyncProcess(GameStateType previousState)
        {
            try
            {
                // Захоплюємо семафор, щоб уникнути одночасного входу
                await _enterSemaphore.WaitAsync();

                if (_isEntered)
                {
                    _logger.LogWarning("Повторний вхід у LobbyState проігноровано", "GameState");
                    return;
                }

                _isEntered = true;

                // 1. Публікуємо подію зміни стану гри
                _eventBus.Publish(new GameStateChangedEvent
                {
                    PreviousState = previousState,
                    NewState = GameStateType.Lobby,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInfo("GameStateChangedEvent опубліковано", "LobbyState");

                // 2. Налаштовуємо навігацію для сцени
                var parameters = new NavigationParameters();
                parameters.Add("PreviousState", previousState.ToString());
                await _navigationService.SetupForSceneAsync("LobbyScene", parameters);

                _logger.LogInfo("Навігацію налаштовано", "LobbyState");

                // 3. Ініціалізуємо системи категорії Lobby
                await InitializeStateSystemsAsync();

                _logger.LogInfo("Системи лобі ініціалізовано", "LobbyState");

                // 4. Ініціалізуємо LobbySystem
                await InitializeLobbyAsync();

                _logger.LogInfo("LobbySystem ініціалізовано", "LobbyState");

                // 5. Публікуємо подію входу в Lobby ПІСЛЯ завершення всіх ініціалізацій
                _eventBus.Publish(new LobbyStateEnteredEvent
                {
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInfo("LobbyStateEnteredEvent опубліковано", "LobbyState");

                _logger.LogInfo("[LIFECYCLE] Завершено вхід у LobbyState", "GameState");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при вході в LobbyState: {ex.Message}", "GameState", ex);
            }
            finally
            {
                _enterSemaphore.Release();
            }
        }

        /// <summary>
        /// Ініціалізація систем для стану
        /// </summary>
        private async UniTask InitializeStateSystemsAsync()
        {
            _logger.LogInfo("Ініціалізація систем для стану Lobby", "LobbyState");

            try
            {
                // Ініціалізуємо системи категорії Lobby
                _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.Lobby);

                // Явно ініціалізуємо LobbyPresenter
                var lobbyPresenter = _container.Resolve<ILobbyPresenter>();
                await lobbyPresenter.InitializeAsync();

                // Даємо можливість Unity завершити оновлення UI
                await UniTask.DelayFrame(2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при ініціалізації систем Lobby: {ex.Message}", "LobbyState", ex);
                throw; // Ретрансляція винятку для обробки на вищому рівні
            }
        }

        /// <summary>
        /// Ініціалізація LobbySystem
        /// </summary>
        private async UniTask InitializeLobbyAsync()
        {
            try
            {
                var lobbySystem = _container.Resolve<ILobbySystem>();

                if (!lobbySystem.IsInitialized)
                {
                    _logger.LogInfo($"Ініціалізація LobbySystem з {_gameSettings.PlayerCount} гравцями", "LobbyState");
                    lobbySystem.InitializeLobby(_gameSettings.PlayerCount);
                }

                // Переконуємося, що LobbyPresenter також знає про ініціалізацію
                var lobbyPresenter = _container.Resolve<ILobbyPresenter>();
                await lobbyPresenter.InitializeLobbyAsync(_gameSettings.PlayerCount);

                // Даємо час для повної ініціалізації UI
                await UniTask.DelayFrame(3);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при ініціалізації LobbySystem: {ex.Message}", "LobbyState", ex);
                throw;
            }
        }

        /// <summary>
        /// Вихід зі стану
        /// </summary>
        public override void Exit()
        {
            _logger.LogInfo("[LIFECYCLE] Вихід з LobbyState", "GameState");

            _uiService.HideScreen(ViewId.Lobby);
            _isEntered = false;
        }

        /// <summary>
        /// Метод для переходу в ігровий режим (викликається з презентера лобі)
        /// </summary>
        public async UniTask StartGame(string[] selectedHeroArchetypes)
        {
            _logger.LogInfo("Запуск гри з лобі", "LobbyState");

            try
            {
                await _gameFlowManager.EnterGameplayAsync(selectedHeroArchetypes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при переході в ігровий режим: {ex.Message}", "LobbyState", ex);
            }
        }
    }
}
