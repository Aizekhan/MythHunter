using System;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Інтерфейс машини станів з підтримкою enum
    /// </summary>
    public interface IStateMachine<TStateEnum> where TStateEnum : Enum
    {
        void RegisterState(TStateEnum stateId, IState<TStateEnum> state);
        void UnregisterState(TStateEnum stateId);
        bool SetState(TStateEnum stateId);
        void Update();
        TStateEnum CurrentState { get; }
        void AddTransition(TStateEnum fromStateId, TStateEnum toStateId);
        bool CanTransition(TStateEnum fromStateId, TStateEnum toStateId);
    }
}