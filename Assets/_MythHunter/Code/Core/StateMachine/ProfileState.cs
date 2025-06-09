// Assets/_MythHunter/Code/Core/StateMachine/ProfileState.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.UI.Navigation;
using MythHunter.Utils.Logging;
using System;

namespace MythHunter.States
{
    public class ProfileState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly INavigationService _navigationService;

        public ProfileState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
            _eventBus = container.Resolve<IEventBus>();
            _navigationService = container.Resolve<INavigationService>();
        }

        public override GameStateType StateId => GameStateType.Profile;

        public override void Enter(GameStateType previousState)
        {
            _logger.LogInfo("👤 ProfileState: Вхід в профіль", "ProfileState");
            EnterProfileAsync(previousState).Forget();
        }

        private async UniTaskVoid EnterProfileAsync(GameStateType previousState)
        {
            try
            {
                // Публікуємо подію зміни стану
                _eventBus.Publish(new GameStateChangedEvent
                {
                    PreviousState = previousState,
                    NewState = GameStateType.Profile,
                    Timestamp = DateTime.UtcNow
                });

                // Налаштовуємо навігацію для профілю
                var parameters = new NavigationParameters();
                parameters.Add("PreviousState", previousState.ToString());
                await _navigationService.SetupForSceneAsync("ProfileScene", parameters);

                _logger.LogInfo("✅ ProfileState готовий", "ProfileState");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка ProfileState: {ex.Message}", "ProfileState", ex);
            }
        }

        public override void Exit()
        {
            _logger.LogInfo("ProfileState: Вихід з профілю", "ProfileState");
        }
    }
}
