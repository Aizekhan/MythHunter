using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан завантаження гри
    /// </summary>
    public class BootState : IState<GameStateType>
    {
        private readonly IDIContainer _container;
        private readonly ILogger _logger;

        public BootState(IDIContainer container)
        {
            _container = container;
            _logger = container.Resolve<ILogger>();
        }

        public GameStateType StateId => GameStateType.Boot;

        public void Enter()
        {
            _logger.LogInfo("Entering Boot State");
            // Ініціалізація систем
            // Завантаження основних ресурсів
        }

        public void Update()
        {
            // Тут можна виконувати логіку переходу до MainMenu
            // Наприклад, перевірка завершення завантаження
        }

        public void Exit()
        {
            _logger.LogInfo("Exiting Boot State");
        }
    }
}
