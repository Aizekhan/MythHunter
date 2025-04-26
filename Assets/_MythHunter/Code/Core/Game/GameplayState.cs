using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Events.Domain;
using Cysharp.Threading.Tasks;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан ігрового процесу
    /// </summary>
    public class GameplayState : BaseState<GameStateType>
    {
        private ILogger _logger;
        private IEventBus _eventBus;
        
        public override GameStateType StateId => GameStateType.Game;
        
        public GameplayState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<ILogger>();
            _eventBus = container.Resolve<IEventBus>();
        }
        
        public override void Enter()
        {
            _logger.LogInfo("Entering gameplay state");
            
            // Публікуємо подію старту гри
            _eventBus.Publish(new GameStartedEvent
            {
                Timestamp = System.DateTime.UtcNow
            });
            
            // Асинхронна ініціалізація
            InitializeAsync().Forget();
        }
        
        private async UniTaskVoid InitializeAsync()
        {
            _logger.LogInfo("Starting async gameplay initialization");

            // Приклад асинхронної ініціалізації
            await UniTask.Delay(100);
            
            _logger.LogInfo("Async gameplay initialization completed");
        }
        
        public override void Update()
        {
            // Логіка оновлення геймплею
        }
        
        public override void Exit()
        {
            _logger.LogInfo("Exiting gameplay state");
            
            // Публікуємо подію завершення гри
            _eventBus.Publish(new GameEndedEvent
            {
                IsVictory = false,
                Timestamp = System.DateTime.UtcNow
            });
        }
    }
}
