using MythHunter.Core.DI;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Базовий клас для станів
    /// </summary>
    public abstract class BaseState : IState
    {
        protected readonly IDIContainer Container;
        
        public abstract int StateId { get; }
        
        protected BaseState(IDIContainer container)
        {
            Container = container;
        }
        
        public virtual void Enter() { }
        
        public virtual void Update() { }
        
        public virtual void Exit() { }
    }
}