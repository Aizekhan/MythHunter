// Assets/_MythHunter/Code/Core/StateMachine/BaseState.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;

namespace MythHunter.Core.StateMachine
{
    public abstract class BaseState<T>
    {
        protected object _stateContext;
        protected readonly IDIContainer _container;

        protected BaseState(IDIContainer container)
        {
            _container = container;
        }

        public abstract T StateId
        {
            get;
        }

        public virtual void Enter(T previousState)
        {
        }


        // Додаємо асинхронний метод EnterAsync
        public virtual async UniTask EnterAsync(T previousState)
        {
            // За замовчуванням викликаємо синхронний метод
            Enter(previousState);
            await UniTask.CompletedTask;
        }

        public void SetStateContext(object context)
        {
            _stateContext = context;
        }

        protected TContext GetStateContext<TContext>() where TContext : class
        {
            return _stateContext as TContext;
        }

        public virtual void Exit()
        {
        }

        public virtual void Update()
        {
        }
    }
}
