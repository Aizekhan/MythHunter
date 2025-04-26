using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан MainMenu
    /// </summary>
    public class MainMenuState : BaseState<GameStateType>
    {
        private ILogger _logger;
        
        public override GameStateType StateId => GameStateType.MainMenu;
        
        public MainMenuState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<ILogger>();
        }
        
        public override void Enter()
        {
            _logger.LogInfo("Entering MainMenu state");
            
            // Асинхронна ініціалізація
            InitializeAsync().Forget();
        }
        
        private async UniTaskVoid InitializeAsync()
        {
            // Приклад асинхронної ініціалізації
            await UniTask.Delay(100);
            
            _logger.LogInfo("MainMenu state initialized asynchronously");
        }
        
        public override void Update()
        {
            // Логіка оновлення MainMenu стану
        }
        
        public override void Exit()
        {
            _logger.LogInfo("Exiting MainMenu state");
        }
    }
}