using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;
using System;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан Loading
    /// </summary>
    public class LoadingState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        
        public override GameStateType StateId => GameStateType.Loading;
        
        public LoadingState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
        }
        
        public override void Enter()
        {
            _logger.LogInfo("Entering Loading state", "GameState");
            
            // Асинхронна ініціалізація
            InitializeAsync().Forget();
        }
        
        private async UniTaskVoid InitializeAsync()
        {
            try
            {
                // Приклад асинхронної ініціалізації
                await UniTask.Delay(100);
                
                _logger.LogInfo("Loading state initialized asynchronously", "GameState");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Loading initialization: {ex.Message}", "GameState", ex);
            }
        }
        
        public override void Update()
        {
            // Логіка оновлення Loading стану
        }
        
        public override void Exit()
        {
            _logger.LogInfo("Exiting Loading state", "GameState");
        }
    }
}