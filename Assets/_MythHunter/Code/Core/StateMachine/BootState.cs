using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;
using System;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан Boot
    /// </summary>
    public class BootState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        private readonly IGameFlowManager _gameFlowManager;

        public override GameStateType StateId => GameStateType.Boot;

        public BootState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
            _gameFlowManager = container.Resolve<IGameFlowManager>();
        }

        public override void Enter(GameStateType previousState)
        {
            _logger.LogInfo("Entering Boot state", "GameState");

            // Асинхронна ініціалізація
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            try
            {
                // ✅ ЗМІНЕНО: переходимо до MainMenu замість Lobby
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
        }
    }
}
