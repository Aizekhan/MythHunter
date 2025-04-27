using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан Boot
    /// </summary>
    public class BootState : BaseState<GameStateType>
    {
        private IMythLogger _logger;
        
        public override GameStateType StateId => GameStateType.Boot;
        
        public BootState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
        }
        
        public override void Enter()
        {
            _logger.LogInfo("Entering Boot state");
            
            // Асинхронна ініціалізація
            InitializeAsync().Forget();
        }
        
        private async UniTaskVoid InitializeAsync()
        {
            // Приклад асинхронної ініціалізації
            await UniTask.Delay(100);
            
            _logger.LogInfo("Boot state initialized asynchronously");
        }
        
        public override void Update()
        {
            // Логіка оновлення Boot стану
        }
        
        public override void Exit()
        {
            _logger.LogInfo("Exiting Boot state");
        }
    }
}