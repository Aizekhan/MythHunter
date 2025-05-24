// Assets/_MythHunter/Code/UI/Views/MinimalLoadingView.cs
using MythHunter.UI.Core;
using MythHunter.Events;
using MythHunter.Events.Domain.Loading;
using MythHunter.Events.Domain.Preload;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MythHunter.Core.DI;

namespace MythHunter.UI.Views
{
    public class MinimalLoadingView : UIViewBase, IEventSubscriber
    {
        [Header("UI Components")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Inject] private IEventBus _eventBus;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SubscribeToEvents();

            // Початкові значення
            SetProgress(0f, "Підготовка...");
            SetTitle("Завантаження");
        }

        public void SubscribeToEvents()
        {
            _eventBus?.Subscribe<LoadingProgressEvent>(OnLoadingProgress);
            _eventBus?.Subscribe<LoadingCompletedEvent>(OnLoadingCompleted);
            _eventBus?.Subscribe<PreloadProgressUpdatedEvent>(OnPreloadProgress);
            _eventBus?.Subscribe<PreloadCompletedEvent>(OnPreloadCompleted);
        }

        public void UnsubscribeFromEvents()
        {
            _eventBus?.Unsubscribe<LoadingProgressEvent>(OnLoadingProgress);
            _eventBus?.Unsubscribe<LoadingCompletedEvent>(OnLoadingCompleted);
            _eventBus?.Unsubscribe<PreloadProgressUpdatedEvent>(OnPreloadProgress);
            _eventBus?.Unsubscribe<PreloadCompletedEvent>(OnPreloadCompleted);
        }

        public void SetProgress(float progress, string status = "")
        {
            if (_progressBar)
                _progressBar.value = Mathf.Clamp01(progress);
            if (_statusText && !string.IsNullOrEmpty(status))
                _statusText.text = status;
        }

        public void SetTitle(string title)
        {
            if (_titleText)
                _titleText.text = title;
        }

        private void OnLoadingProgress(LoadingProgressEvent evt)
        {
            SetProgress(evt.Progress, evt.Status);
        }

        private void OnLoadingCompleted(LoadingCompletedEvent evt)
        {
            SetProgress(1f, "Завершено!");
        }

        private void OnPreloadProgress(PreloadProgressUpdatedEvent evt)
        {
            SetProgress(evt.Progress, $"Завантажуємо ресурси... ({evt.LoadedResources}/{evt.TotalResources})");
        }

        private void OnPreloadCompleted(PreloadCompletedEvent evt)
        {
            SetProgress(1f, "Ресурси завантажено!");
        }

        protected override void OnDestroy()
        {
            UnsubscribeFromEvents();
            base.OnDestroy();
        }
    }
}
