// Шлях: Assets/_MythHunter/Code/UI/Core/BasePresenter.cs
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Events;
using MythHunter.Utils.Logging;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Базовий клас для презентерів, який не використовує типізацію представлень
    /// </summary>
    public abstract class BasePresenter : IPresenter, IEventSubscriber, IDisposable
    {
        protected readonly IEventBus _eventBus;
        protected readonly IMythLogger _logger;
        protected IView _view;
        protected bool _isSubscribed = false;
        protected ViewId _viewId = ViewId.None;

        protected BasePresenter(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        /// <summary>
        /// Ініціалізує презентер із представленням та його ідентифікатором
        /// </summary>
        /// <param name="view">Представлення</param>
        /// <param name="viewId">Ідентифікатор представлення</param>
        public virtual void Initialize(IView view, ViewId viewId = ViewId.None)
        {
            _view = view;
            _viewId = viewId;
            SubscribeToEvents();
            _logger.LogInfo($"{GetType().Name} initialized for ViewId {_viewId}", "UI");
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

    /// <summary>
    /// Застарілий типізований базовий презентер.
    /// Використовується для зворотної сумісності. Рекомендовано перейти на BasePresenter.
    /// </summary>
    [Obsolete("Використовуйте нетипізований BasePresenter для відповідності архітектурним принципам")]
    public abstract class BasePresenter<TView> : BasePresenter where TView : IView
    {
        protected new TView _view;

        protected BasePresenter(IEventBus eventBus, IMythLogger logger) : base(eventBus, logger)
        {
        }

        /// <summary>
        /// Ініціалізує презентер з типізованим представленням
        /// </summary>
        /// <param name="view">Типізоване представлення</param>
        public virtual void Initialize(TView view)
        {
            _view = view;
            base.Initialize(view);
        }

        /// <summary>
        /// Перевизначення базового методу для збереження зворотної сумісності
        /// </summary>
        public override void Initialize(IView view, ViewId viewId = ViewId.None)
        {
            if (view is TView typedView)
            {
                Initialize(typedView);
            }
            else
            {
                _logger.LogWarning($"Отримано представлення типу {view.GetType().Name}, але очікувався {typeof(TView).Name}", "UI");
                base.Initialize(view, viewId);
            }
        }
    }
}
