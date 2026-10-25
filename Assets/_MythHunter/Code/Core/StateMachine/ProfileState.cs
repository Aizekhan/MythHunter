// Assets/_MythHunter/Code/Core/StateMachine/ProfileState.cs
using Cysharp.Threading.Tasks;
using RPGFramework.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.UI.Navigation;
using MythHunter.Utils.Logging;
using System;
using System.Threading;

namespace MythHunter.States
{
    public class ProfileState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly INavigationService _navigationService;

        // EPIC 08: Track cancellation token for state entry
        private CancellationTokenSource _entryCts;

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

            // EPIC 08: Create cancellation token for this state entry
            _entryCts?.Cancel();
            _entryCts = new CancellationTokenSource();
            EnterProfileAsync(previousState, _entryCts.Token).Forget();
        }

        /// <summary>
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        private async UniTaskVoid EnterProfileAsync(GameStateType previousState, CancellationToken cancellationToken = default)
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
                await _navigationService.SetupForSceneAsync("ProfileScene", parameters, cancellationToken);

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

            // EPIC 08: Cancel any ongoing async operations
            _entryCts?.Cancel();
            _entryCts?.Dispose();
            _entryCts = null;
        }
    }
}
