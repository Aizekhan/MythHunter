using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;

namespace MythHunter.States
{
    public class LobbyState : BaseState<GameStateType>
    {
        private readonly IUISystem _uiSystem;
        private readonly IViewConfigRegistry _viewConfigRegistry;
        private readonly IMythLogger _logger;

        public LobbyState(IDIContainer container) : base(container)
        {
            _uiSystem = container.Resolve<IUISystem>();
            _viewConfigRegistry = container.Resolve<IViewConfigRegistry>();
            _logger = container.Resolve<IMythLogger>();
        }

        public override async void Enter()
        {
            _logger.LogInfo("Entering LobbyState", "GameState");

            var config = _viewConfigRegistry.Get("Lobby");
            if (config != null)
            {
                await _uiSystem.ShowViewAsync<UI.Views.LobbyView>(config.PrefabPath);
                _logger.LogInfo("LobbyView created and shown", "LobbyState");
            }
            else
            {
                _logger.LogError("Failed to find LobbyView config", "LobbyState");
            }
        }

        public override GameStateType StateId => GameStateType.Lobby;

        public override void Exit()
        {
            _logger.LogInfo("Exiting LobbyState", "GameState");
            _uiSystem.HideView<UI.Views.LobbyView>();
        }
    }
}
