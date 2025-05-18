// Файл: Assets/_MythHunter/Code/Core/StateMachine/BaseState.cs
using System;
using MythHunter.Core.DI;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Базовий абстрактний клас для станів
    /// </summary>
    public abstract class BaseState<T> : IState<T> where T : Enum
    {
        protected readonly IDIContainer Container;

        protected BaseState(IDIContainer container)
        {
            Container = container;
        }

        public abstract T StateId
        {
            get;
        }

        /// <summary>
        /// Виконується при вході в стан
        /// </summary>
        /// <param name="previousState">Попередній стан або default</param>
        public virtual void Enter(T previousState)
        {
            // Базова реалізація
        }

        public virtual void Update()
        {
            // Базова реалізація
        }

        public virtual void Exit()
        {
            // Базова реалізація
        }
    }
}
