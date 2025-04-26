using System;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Інтерфейс стану для машини станів
    /// </summary>
    public interface IState<TStateEnum> where TStateEnum : Enum
    {
        void Enter();
        void Update();
        void Exit();
        TStateEnum StateId { get; }
    }
}