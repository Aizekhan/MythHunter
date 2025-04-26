using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан ігрового процесу
    /// </summary>
    public class GameplayState : IState<GameStateType>
    {
        private readonly IDIContainer _container;
        private readonly ILogger _logger;

        public GameplayState(IDIContainer container)
        {
            _container = container;
            _logger = container.Resolve<ILogger>();
        }

        public GameStateType StateId => GameStateType.Game;

        public void Enter()
        {
            _logger.LogInfo("Entering Game State");
            // Ініціалізація ігрових систем
            // Активація ігрового UI
        }

        public void Update()
        {
            // Обробка основної ігрової логіки
        }

        public void Exit()
        {
            _logger.LogInfo("Exiting Game State");
            // Деактивація ігрових систем
            // Збереження даних, якщо потрібно
        }
    }
}
