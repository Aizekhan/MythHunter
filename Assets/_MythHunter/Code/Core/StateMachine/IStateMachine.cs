using System;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Інтерфейс машини станів
    /// </summary>
    public interface IStateMachine<TStateId> where TStateId : Enum
    {
        void RegisterState(TStateId stateId, IState<TStateId> state);
        void UnregisterState(TStateId stateId);
        bool SetState(TStateId stateId);
        void Update();
        TStateId CurrentStateId
        {
            get;
        }
        void AddTransition(TStateId fromStateId, TStateId toStateId);
        bool CanTransition(TStateId fromStateId, TStateId toStateId);
    }

  
}
