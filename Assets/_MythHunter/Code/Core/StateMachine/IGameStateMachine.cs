namespace MythHunter.Core.Game
{
    public interface IGameStateMachine
    {
        void Initialize();
        void Update();
        void ChangeState(GameStateType newState);
        void ChangeState(GameStateType newState, object context);
        GameStateType CurrentState
        {
            get;
        }
      
    }
}
