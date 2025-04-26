using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан завантаження рівня
    /// </summary>
    public class LoadingState : IState<GameStateType>
    {
        private readonly IDIContainer _container;
        private readonly ILogger _logger;

        public LoadingState(IDIContainer container)
        {
            _container = container;
            _logger = container.Resolve<ILogger>();
        }

        public GameStateType StateId => GameStateType.Loading;

        public void Enter()
        {
            _logger.LogInfo("Entering Loading State");
            // Ініціалізація екрану завантаження
            // Початок асинхронного завантаження ресурсів
        }

        public void Update()
        {
            // Перевірка прогресу завантаження
            // Перехід до гри при завершенні
        }

        public void Exit()
        {
            _logger.LogInfo("Exiting Loading State");
            // Деактивація екрану завантаження
        }
    }
}
