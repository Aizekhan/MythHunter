using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан Loading
    /// </summary>
    public class LoadingState : BaseState<GameStateType>
    {
        private ILogger _logger;
        
        public override GameStateType StateId => GameStateType.Loading;
        
        public LoadingState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<ILogger>();
        }
        
        public override void Enter()
        {
            _logger.LogInfo("Entering Loading state");
            
            // Асинхронна ініціалізація
            InitializeAsync().Forget();
        }
        
        private async UniTaskVoid InitializeAsync()
        {
            // Приклад асинхронної ініціалізації
            await UniTask.Delay(100);
            
            _logger.LogInfo("Loading state initialized asynchronously");
        }
        
        public override void Update()
        {
            // Логіка оновлення Loading стану
        }
        
        public override void Exit()
        {
            _logger.LogInfo("Exiting Loading state");
        }
    }
}