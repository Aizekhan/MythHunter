using System;
using MythHunter.Core.DI;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Базовий клас для станів з підтримкою enum
    /// </summary>
    public abstract class BaseState<TStateEnum> : IState<TStateEnum> where TStateEnum : Enum
    {
        protected readonly IDIContainer Container;
        
        public abstract TStateEnum StateId { get; }
        
        protected BaseState(IDIContainer container)
        {
            Container = container;
        }
        
        public virtual void Enter() { }
        
        public virtual void Update() { }
        
        public virtual void Exit() { }
    }
}