using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан головного меню
    /// </summary>
    public class MainMenuState : IState<GameStateType>
    {
        private readonly IDIContainer _container;
        private readonly ILogger _logger;

        public MainMenuState(IDIContainer container)
        {
            _container = container;
            _logger = container.Resolve<ILogger>();
        }

        public GameStateType StateId => GameStateType.MainMenu;

        public void Enter()
        {
            _logger.LogInfo("Entering Main Menu State");
            // Активація UI головного меню
        }

        public void Update()
        {
            // Обробка логіки головного меню
        }

        public void Exit()
        {
            _logger.LogInfo("Exiting Main Menu State");
            // Деактивація UI головного меню
        }
    }
}
