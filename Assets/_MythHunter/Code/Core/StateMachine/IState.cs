// Файл: Assets/_MythHunter/Code/Core/StateMachine/IState.cs
using System;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Інтерфейс базового стану для машини станів
    /// </summary>
    public interface IState<T> where T : Enum
    {
        T StateId
        {
            get;
        }

        /// <summary>
        /// Виконується при вході в стан
        /// </summary>
        /// <param name="previousState">Попередній стан або default</param>
        void Enter(T previousState);

        void Update();
        void Exit();
    }
}
