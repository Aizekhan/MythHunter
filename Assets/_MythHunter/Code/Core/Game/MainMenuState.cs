using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;
using System;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан MainMenu
    /// </summary>
    public class MainMenuState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        
        public override GameStateType StateId => GameStateType.MainMenu;
        
        public MainMenuState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
        }
        
        public override void Enter()
        {
            _logger.LogInfo("Entering MainMenu state", "GameState");
            
            // Асинхронна ініціалізація
            InitializeAsync().Forget();
        }
        
        private async UniTaskVoid InitializeAsync()
        {
            try
            {
                // Приклад асинхронної ініціалізації
                await UniTask.Delay(100);
                
                _logger.LogInfo("MainMenu state initialized asynchronously", "GameState");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MainMenu initialization: {ex.Message}", "GameState", ex);
            }
        }
        
        public override void Update()
        {
            // Логіка оновлення MainMenu стану
        }
        
        public override void Exit()
        {
            _logger.LogInfo("Exiting MainMenu state", "GameState");
        }
    }
}