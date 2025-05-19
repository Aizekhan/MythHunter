namespace MythHunter.Core.Game
{
    public interface IGameStateMachine
    {
        void Initialize();
        void Update();
        void ChangeState(GameStateType newState);
        GameStateType CurrentState
        {
            get;
        }
    }
}
