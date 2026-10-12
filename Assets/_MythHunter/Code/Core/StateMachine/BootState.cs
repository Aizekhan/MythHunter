using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан Boot
    /// </summary>
    public class BootState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        private readonly IGameFlowManager _gameFlowManager;

        // EPIC 08: Track cancellation token for state entry
        private CancellationTokenSource _entryCts;

        public override GameStateType StateId => GameStateType.Boot;

        public BootState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
            _gameFlowManager = container.Resolve<IGameFlowManager>();
        }

        public override void Enter(GameStateType previousState)
        {
            _logger.LogInfo("Entering Boot state", "GameState");

            // EPIC 08: Create cancellation token for this state entry
            _entryCts?.Cancel();
            _entryCts = new CancellationTokenSource();
            InitializeAsync(_entryCts.Token).Forget();
        }

        /// <summary>
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        private async UniTaskVoid InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // ✅ ЗМІНЕНО: переходимо до MainMenu замість Lobby
                // Note: EnterMainMenuAsync might need cancellation token support
                await _gameFlowManager.EnterMainMenuAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Boot error: {ex.Message}", "BootState", ex);
            }
        }

        public override void Update()
        {
            // Логіка оновлення Boot стану
        }

        public override void Exit()
        {
            _logger.LogInfo("Exiting Boot state", "GameState");

            // EPIC 08: Cancel any ongoing async operations
            _entryCts?.Cancel();
            _entryCts?.Dispose();
            _entryCts = null;
        }
    }
}
