// Assets/_MythHunter/Code/UI/Presenters/LoadingScreenPresenter.cs
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain.Loading;
using MythHunter.UI.Core;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;

namespace MythHunter.UI.Presenters
{
    /// <summary>
    /// Презентер для екрану завантаження
    /// </summary>
    public class LoadingScreenPresenter : BasePresenter, IEventSubscriber
    {
        private ILoadingScreenView _loadingView;
        private bool _isSubscribed = false;

        [Inject]
        public LoadingScreenPresenter(IEventBus eventBus, IMythLogger logger)
            : base(eventBus, logger)
        {
            // Встановлюємо ViewId
            _viewId = ViewId.LoadingScreen;
        }

        public override async UniTask InitializeAsync()
        {
            await base.InitializeAsync();
            _logger.LogInfo("LoadingScreenPresenter ініціалізовано", "UI");
        }

        public void SetView(ILoadingScreenView view)
        {
            _loadingView = view;
            Initialize(view, _viewId);
        }

        public override void Initialize(IView view, ViewId viewId = ViewId.None)
        {
            base.Initialize(view, viewId);
            _loadingView = view as ILoadingScreenView;

            if (_loadingView == null)
            {
                _logger.LogError("Неможливо встановити представлення екрану завантаження", "UI");
                return;
            }

            // Підписуємося на події
            SubscribeToEvents();
        }

        protected override void OnSubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<LoadingStartedEvent>(OnLoadingStarted);
            _eventBus.Subscribe<LoadingProgressEvent>(OnLoadingProgress);
            _eventBus.Subscribe<LoadingCompletedEvent>(OnLoadingCompleted);
            _eventBus.Subscribe<LoadingErrorEvent>(OnLoadingError);

            _isSubscribed = true;
            _logger.LogInfo("LoadingScreenPresenter підписався на події", "LoadingUI"); // 🔥 ДОДАЛИ ЦЕ
        }

        protected override void OnUnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<LoadingStartedEvent>(OnLoadingStarted);
            _eventBus.Unsubscribe<LoadingProgressEvent>(OnLoadingProgress);
            _eventBus.Unsubscribe<LoadingCompletedEvent>(OnLoadingCompleted);
            _eventBus.Unsubscribe<LoadingErrorEvent>(OnLoadingError);

            _isSubscribed = false;
        }

        private void OnLoadingStarted(LoadingStartedEvent evt)
        {
            _logger.LogInfo("Початок завантаження...", "LoadingUI");

            if (_loadingView == null)
                return;

            _loadingView.UpdateProgress(0f, "Підготовка до завантаження...");
        }

        private void OnLoadingProgress(LoadingProgressEvent evt)
        {
            if (_loadingView == null)
                return;

            _loadingView.UpdateProgress(evt.Progress, evt.Status);
            _loadingView.UpdateLoadingStage(evt.Stage, evt.Status);
        }

        private void OnLoadingCompleted(LoadingCompletedEvent evt)
        {
            _logger.LogInfo("Завантаження завершено", "LoadingUI");

            if (_loadingView == null)
                return;

            _loadingView.UpdateProgress(1f, "Завантаження завершено");
            _loadingView.ShowCompletionScreen();
        }

        private void OnLoadingError(LoadingErrorEvent evt)
        {
            _logger.LogError($"Помилка завантаження: {evt.ErrorMessage}", "LoadingUI");

            if (_loadingView == null)
                return;

            _loadingView.ShowError(evt.ErrorMessage);
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _loadingView = null;
            base.Dispose();
        }
    }
}
