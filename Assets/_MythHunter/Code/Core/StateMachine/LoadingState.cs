// Assets/_MythHunter/Code/Core/Game/States/LoadingState.cs

using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
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

        public LoadingState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
            _navigationService = container.Resolve<INavigationService>();
        }

        public override GameStateType StateId => GameStateType.Loading;

        public override void Enter(GameStateType previousState)
        {
            _logger.LogInfo("⏳ LoadingState: Показуємо Loading UI", "LoadingState");

            // Тільки показуємо UI - вся логіка в GameFlowManager + PreloadManager
            ShowLoadingUIAsync().Forget();
        }

        private async UniTaskVoid ShowLoadingUIAsync()
        {
            var parameters = new NavigationParameters();
            await _navigationService.SetupForSceneAsync("LoadingScene", parameters);
        }

        public override void Exit()
        {
            _logger.LogInfo("LoadingState: Приховуємо Loading UI", "LoadingState");
            // PreloadManager сам керує завантаженням
        }
    }
}
