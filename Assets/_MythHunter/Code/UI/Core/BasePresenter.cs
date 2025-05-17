using Cysharp.Threading.Tasks;
using MythHunter.Events;
using MythHunter.Utils.Logging;

namespace MythHunter.UI.Core
{
    public abstract class BasePresenter<TView> : IPresenter, IEventSubscriber where TView : IView
    {
        protected readonly IEventBus _eventBus;
        protected readonly IMythLogger _logger;
        protected TView _view;
        protected bool _isSubscribed = false;

        protected BasePresenter(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public virtual void Initialize(TView view)
        {
            _view = view;
            SubscribeToEvents();
            _logger.LogInfo($"{GetType().Name} initialized", "UI");
        }

        public virtual async UniTask InitializeAsync()
        {
            SubscribeToEvents();
            _logger.LogInfo($"{GetType().Name} initialized async", "UI");
            await UniTask.CompletedTask;
        }

        public virtual void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo($"{GetType().Name} disposed", "UI");
        }

        public virtual void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            OnSubscribeToEvents();
            _isSubscribed = true;
        }

        public virtual void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            OnUnsubscribeFromEvents();
            _isSubscribed = false;
        }

        protected virtual void OnSubscribeToEvents()
        {
        }
        protected virtual void OnUnsubscribeFromEvents()
        {
        }
    }
}
