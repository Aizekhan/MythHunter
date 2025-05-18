// Файл: Assets/_MythHunter/Code/Core/Game/LobbyState.cs
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Events.Domain;
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

        public LobbyState(IDIContainer container) : base(container)
        {
            _uiService = container.Resolve<IUIService>();
            _logger = container.Resolve<IMythLogger>();
            _eventBus = container.Resolve<IEventBus>();
        }

        public override async void Enter(GameStateType previousState)
        {
            _logger.LogInfo("Entering LobbyState", "GameState");

            // Отримання реєстру систем
            var systemRegistry = Container.Resolve<ISystemRegistry>();

            // Ініціалізуємо всі системи з категорією OnDemand
            systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.OnDemand);

            // Публікуємо подію зміни стану гри
            _eventBus.Publish(new GameStateChangedEvent
            {
                PreviousState = previousState,
                NewState = GameStateType.Lobby,
                Timestamp = DateTime.UtcNow
            });

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
