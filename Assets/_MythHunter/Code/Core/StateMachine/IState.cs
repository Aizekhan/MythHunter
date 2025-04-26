using System;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Інтерфейс стану для дженериків
    /// </summary>
    public interface IState<TStateId> where TStateId : Enum
    {
        TStateId StateId
        {
            get;
        }
        void Enter();
        void Update();
        void Exit();
    }

}
