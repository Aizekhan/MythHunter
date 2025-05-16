using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.UI.Core;
using MythHunter.UI.Presenters;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;

namespace MythHunter.States
{
    /// <summary>
    /// Стан гри, відповідальний за запуск лоббі
    /// </summary>
    public class LobbyState : BaseState<GameStateType>
    {
        private readonly IViewConfigRegistry _viewConfigRegistry;
        private readonly IUIViewFactory _viewFactory;

        public LobbyState(IDIContainer container) : base(container)
        {
            _viewConfigRegistry = container.Resolve<IViewConfigRegistry>();
            _viewFactory = container.Resolve<IUIViewFactory>();
        }

        public override async void Enter()
        {
            var config = _viewConfigRegistry.Get("Lobby");
            await _viewFactory.CreateViewAsync<LobbyView>(config.PrefabPath);

            UnityEngine.Debug.Log("✅ LobbyView created from LobbyState");
        }

        public override GameStateType StateId => GameStateType.Lobby;
    }
}
