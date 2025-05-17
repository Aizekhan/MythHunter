using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;

namespace MythHunter.States
{
    public class LobbyState : BaseState<GameStateType>
    {
        private readonly IUIService _uiService;
        private readonly IMythLogger _logger;

        public LobbyState(IDIContainer container) : base(container)
        {
            _uiService = container.Resolve<IUIService>();
            _logger = container.Resolve<IMythLogger>();
        }

        public override async void Enter()
        {
            _logger.LogInfo("Entering LobbyState", "GameState");

            var view = await _uiService.ShowScreenAsync<UI.Views.LobbyView>("Lobby");
            if (view != null)
            {
                _logger.LogInfo("LobbyView created and shown", "LobbyState");
            }
            else
            {
                _logger.LogError("Failed to show LobbyView", "LobbyState");
            }
        }

        public override GameStateType StateId => GameStateType.Lobby;

        public override void Exit()
        {
            _logger.LogInfo("Exiting LobbyState", "GameState");
            _uiService.HideScreen<UI.Views.LobbyView>();
        }
    }
}
