using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Systems.Core;
using MythHunter.UI.Core;
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

        public LobbyState(IDIContainer container) : base(container)
        {
            _uiService = container.Resolve<IUIService>();
            _logger = container.Resolve<IMythLogger>();
            _eventBus = container.Resolve<IEventBus>();
            _gameFlowManager = container.Resolve<IGameFlowManager>();
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

            // Асинхронно ініціалізуємо UI з невеликою затримкою, щоб системи встигли підписатися
            await UniTask.Delay(100);

            // Потім публікуємо подію входу в Lobby
            _eventBus.Publish(new LobbyStateEnteredEvent { Timestamp = DateTime.UtcNow });
            _logger.LogInfo("LobbyStateEnteredEvent опубліковано", "LobbyState");

            // Показуємо UI, якщо потрібно
            if (!_uiService.IsScreenActive<UI.Views.LobbyView>())
            {
                var view = await _uiService.ShowScreenAsync<UI.Views.LobbyView>("Lobby");
                if (view != null)
                {
                    _logger.LogInfo("LobbyView створено і показано", "LobbyState");
                }
                else
                {
                    _logger.LogError("Не вдалося показати LobbyView", "LobbyState");
                }
            }
        }

        public override GameStateType StateId => GameStateType.Lobby;

        public override void Exit()
        {
            _logger.LogInfo("Exiting LobbyState", "GameState");
            _uiService.HideScreen<UI.Views.LobbyView>();
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
