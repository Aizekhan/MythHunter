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
using System;

namespace MythHunter.States
{
    public class LobbyState : BaseState<GameStateType>
    {
        private readonly IUIService _uiService;
        private readonly IMythLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IGameFlowManager _gameFlowManager;
        private readonly INavigationService _navigationService;

        public LobbyState(IDIContainer container) : base(container)
        {
            _uiService = container.Resolve<IUIService>();
            _logger = container.Resolve<IMythLogger>();
            _eventBus = container.Resolve<IEventBus>();
            _gameFlowManager = container.Resolve<IGameFlowManager>();
            _navigationService = container.Resolve<INavigationService>();
        }

        public override async void Enter(GameStateType previousState)
        {
            _logger.LogInfo("Вхід у LobbyState", "GameState");

            // Публікуємо подію зміни стану гри
            _eventBus.Publish(new GameStateChangedEvent
            {
                PreviousState = previousState,
                NewState = GameStateType.Lobby,
                Timestamp = DateTime.UtcNow
            });

            // Асинхронно налаштовуємо навігацію для сцени
            var parameters = new NavigationParameters();
            parameters.Add("PreviousState", previousState.ToString());
            await _navigationService.SetupForSceneAsync("LobbyScene", parameters);

            // Публікуємо подію входу в Lobby
            _eventBus.Publish(new LobbyStateEnteredEvent { Timestamp = DateTime.UtcNow });
            _logger.LogInfo("LobbyStateEnteredEvent опубліковано", "LobbyState");
        }

        public override GameStateType StateId => GameStateType.Lobby;

        public override void Exit()
        {
            _logger.LogInfo("Exiting LobbyState", "GameState");
            _uiService.HideScreen(ViewId.Lobby);
        }

        // Метод для переходу в ігровий режим (викликається з презентера лобі)
        public async UniTask StartGame(string[] selectedHeroArchetypes)
        {
            _logger.LogInfo("Starting game from lobby", "LobbyState");

            try
            {
                await _gameFlowManager.EnterGameplayAsync(selectedHeroArchetypes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error entering gameplay: {ex.Message}", "LobbyState", ex);
            }
        }
    }
}
