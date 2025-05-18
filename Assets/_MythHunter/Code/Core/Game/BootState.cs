using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;
using System;
using MythHunter.Core.SceneManagement;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан Boot
    /// </summary>
    public class BootState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        
        public override GameStateType StateId => GameStateType.Boot;
        
        public BootState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
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
                var sceneDispatcher = Container.Resolve<ISceneDispatcher>();
                await sceneDispatcher.LoadSceneAsync("LobbyScene");

                Container.Resolve<IGameStateMachine>().ChangeState(GameStateType.Lobby);
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
