using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Машина станів гри
    /// </summary>
    public class GameStateMachine : IGameStateMachine   
    {
        private readonly IStateMachine<GameStateType> _stateMachine;
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;
        
        public GameStateMachine(IDIContainer container)
        {
            _container = container;
            _logger = container.Resolve<IMythLogger>();
            _stateMachine = new StateMachine<GameStateType>();
        }
        
        public void Initialize()
        {
            // Реєстрація станів
            _stateMachine.RegisterState(GameStateType.Boot, new BootState(_container));
            _stateMachine.RegisterState(GameStateType.MainMenu, new MainMenuState(_container));
            _stateMachine.RegisterState(GameStateType.Loading, new LoadingState(_container));
            _stateMachine.RegisterState(GameStateType.Game, new GameplayState(_container));
            
            // Налаштування переходів
            _stateMachine.AddTransition(GameStateType.Boot, GameStateType.MainMenu);
            _stateMachine.AddTransition(GameStateType.MainMenu, GameStateType.Loading);
            _stateMachine.AddTransition(GameStateType.Loading, GameStateType.Game);
            _stateMachine.AddTransition(GameStateType.Game, GameStateType.MainMenu);
            
            // Перехід до початкового стану
            _stateMachine.SetState(GameStateType.Boot);
            
            _logger.LogInfo($"Initialized GameStateMachine with initial state: {GameStateType.Boot}");
        }
        
        public void Update()
        {
            _stateMachine.Update();
        }
        
        public void ChangeState(GameStateType newState)
        {
            _stateMachine.SetState(newState);
        }
        
        public GameStateType CurrentState => _stateMachine.CurrentState;
    }
}
