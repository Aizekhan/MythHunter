// Assets/_MythHunter/Code/Core/Game/States/LoadingState.cs

using System.Threading;
using Cysharp.Threading.Tasks;
using RPGFramework.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.UI.Navigation;
using MythHunter.Utils.Logging;


namespace MythHunter.States
{
    /// <summary>
    /// Стан для управління процесом завантаження гри
    /// </summary>
    // Assets/_MythHunter/Code/Core/StateMachine/LoadingState.cs
    public class LoadingState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        private readonly INavigationService _navigationService;

        // EPIC 08: Track cancellation token for state entry
        private CancellationTokenSource _entryCts;

        public LoadingState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
            _navigationService = container.Resolve<INavigationService>();
        }

        public override GameStateType StateId => GameStateType.Loading;

        public override void Enter(GameStateType previousState)
        {
            _logger.LogInfo("⏳ LoadingState: Показуємо Loading UI", "LoadingState");

            // EPIC 08: Create cancellation token for this state entry
            _entryCts?.Cancel();
            _entryCts = new CancellationTokenSource();
            ShowLoadingUIAsync(_entryCts.Token).Forget();
        }

        /// <summary>
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        private async UniTaskVoid ShowLoadingUIAsync(CancellationToken cancellationToken = default)
        {
            var parameters = new NavigationParameters();
            await _navigationService.SetupForSceneAsync("LoadingScene", parameters, cancellationToken);
        }

        public override void Exit()
        {
            _logger.LogInfo("LoadingState: Приховуємо Loading UI", "LoadingState");

            // EPIC 08: Cancel any ongoing async operations
            _entryCts?.Cancel();
            _entryCts?.Dispose();
            _entryCts = null;

            // PreloadManager сам керує завантаженням
        }
    }
}
