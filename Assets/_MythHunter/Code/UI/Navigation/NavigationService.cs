// Шлях: Assets/_MythHunter/Code/UI/Navigation/NavigationService.cs

using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MythHunter.UI.Navigation
{
    public class NavigationService : INavigationService, IEventSubscriber, IDisposable
    {
        private readonly IUIService _uiService;
        private readonly IScreenTransition _transition;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;
        private readonly IViewConfigRegistry _viewConfigRegistry;

        private readonly Stack<NavigationEntry> _navigationStack = new();

        private object _currentModal;
        private object _modalTcs;
        private bool _isSubscribed;

        private struct NavigationEntry
        {
            public ViewId ViewId;
            public IView View;
            public NavigationParameters Parameters;
        }

        [Inject]
        public NavigationService(
            IUIService uiService,
            IScreenTransition transition,
            IEventBus eventBus,
            IMythLogger logger,
            IDIContainer container,
            IViewConfigRegistry viewConfigRegistry)
        {
            _uiService = uiService;
            _transition = transition;
            _eventBus = eventBus;
            _logger = logger;
            _container = container;
            _viewConfigRegistry = viewConfigRegistry;
            SubscribeToEvents();
        }

        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;
            _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            _isSubscribed = true;
        }

        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;
            _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            _isSubscribed = false;
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            if (evt.PreviousState != evt.NewState)
                ClearStackAsync().Forget();
        }

      

        public async UniTask<IView> NavigateToAsync(ViewId viewId, NavigationParameters parameters = null, TransitionType transition = TransitionType.Default)
        {
            _logger.LogInfo($"Навігація до екрану: {viewId}", "Navigation");
            var config = _viewConfigRegistry.Get(viewId);
            if (config == null)
            {
                _logger.LogError($"ViewConfig не знайдено для: {viewId}", "Navigation");
                return null;
            }
            try
            {
                IView currentView = _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;

                var viewConfig = _viewConfigRegistry.Get(viewId);
                if (viewConfig == null)
                {
                    _logger.LogError($"ViewConfig не знайдено для: {viewId}", "Navigation");
                    return null;
                }

                var newView = await _uiService.ShowScreenAsync(viewId);
                if (newView == null)
                {
                    _logger.LogError($"Не вдалося створити екран для ViewId {viewId}", "Navigation");
                    return null;
                }

                if (currentView is INavigableView currentNavView)
                    await currentNavView.OnViewNavigatedFromAsync();

                await _transition.PlayTransitionAsync(
                      (currentView as Component)?.gameObject,
                      (newView as Component)?.gameObject,
                       transition,
                              true
                                                   );

                if (newView is INavigableView navView)
                {
                    await navView.OnViewCreatedAsync(parameters ?? new NavigationParameters());
                    await navView.OnViewNavigatedToAsync(parameters ?? new NavigationParameters());
                }

                _navigationStack.Push(new NavigationEntry
                {
                    ViewId = viewId,
                    View = newView,
                    Parameters = parameters ?? new NavigationParameters()
                });

                return newView;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при навігації до екрану {viewId}: {ex.Message}", "Navigation");
                return null;
            }
        }

        public async UniTask<IView> GoBackAsync(NavigationParameters parameters = null)
        {
            if (_navigationStack.Count <= 1)
                return null;

            var currentEntry = _navigationStack.Pop();
            var previousEntry = _navigationStack.Peek();

            if (currentEntry.View is INavigableView navFrom)
                await navFrom.OnViewNavigatedFromAsync();

            await _transition.PlayTransitionAsync(
     (currentEntry.View as Component)?.gameObject,
     (previousEntry.View as Component)?.gameObject,
     TransitionType.Default,
     false
 );

            if (previousEntry.View is INavigableView navTo)
                await navTo.OnViewNavigatedToAsync(parameters ?? previousEntry.Parameters);

            if (currentEntry.View is INavigableView navDestroy)
                await navDestroy.OnViewDestroyedAsync();

            // Ховаємо поточне представлення
            _uiService.HideScreen(currentEntry.ViewId);

            return previousEntry.View;
        }

        public async UniTask<IView> GoToRootAsync(NavigationParameters parameters = null)
        {
            if (_navigationStack.Count <= 1)
                return _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;

            var currentEntry = _navigationStack.Peek();
            NavigationEntry rootEntry = default;

            if (currentEntry.View is INavigableView navFrom)
                await navFrom.OnViewNavigatedFromAsync();

            while (_navigationStack.Count > 1)
            {
                var entry = _navigationStack.Pop();
                if (_navigationStack.Count == 1)
                    rootEntry = _navigationStack.Peek();

                if (entry.View is INavigableView navDestroy)
                    await navDestroy.OnViewDestroyedAsync();

                // Деактивуємо об'єкт
                if (entry.View is Component component)
                {
                    component.gameObject.SetActive(false);
                }
            }

            await _transition.PlayTransitionAsync(
                currentEntry.View?.gameObject,
                rootEntry.View?.gameObject,
                TransitionType.Default,
                false
            );

            if (rootEntry.View is INavigableView navTo)
                await navTo.OnViewNavigatedToAsync(parameters ?? rootEntry.Parameters);

            return rootEntry.View;
        }

        public async UniTask<TResult> ShowModalAsync<TResult>(ViewId viewId, NavigationParameters parameters = null)
        {
            var viewConfig = _viewConfigRegistry.Get(viewId);
            if (viewConfig == null)
            {
                _logger.LogError($"ViewConfig не знайдено для: {viewId}", "Navigation");
                return default;
            }

            var tcs = new UniTaskCompletionSource<TResult>();
            _modalTcs = tcs;

            var newView = await _uiService.ShowScreenAsync(viewId);
            if (newView == null)
            {
                _logger.LogError($"Не вдалося створити модальне вікно для ViewId: {viewId}", "Navigation");
                return default;
            }

            if (newView is IModalView<TResult> modalView)
            {
                _currentModal = modalView;
                await modalView.InitializeAsync(parameters ?? new NavigationParameters());
                modalView.SetCompletionCallback(result =>
                {
                    _uiService.HideScreen(viewId);
                    tcs.TrySetResult(result);
                    _currentModal = null;
                    _modalTcs = null;
                });

                return await tcs.Task;
            }
            else
            {
                _logger.LogError($"View для ViewId {viewId} не реалізує IModalView<{typeof(TResult).Name}>", "Navigation");
                _uiService.HideScreen(viewId);
                return default;
            }
        }

        public async UniTask ClearStackAsync()
        {
            while (_navigationStack.Count > 0)
            {
                var entry = _navigationStack.Pop();
                if (entry.View is INavigableView navView)
                    await navView.OnViewDestroyedAsync();
                entry.View.Hide();
            }
        }

        public async UniTask SetupForSceneAsync(string sceneName, NavigationParameters parameters)
        {
            await ClearStackAsync();

            // Перетворюємо назву сцени у ViewId
            ViewId initialScreenId = sceneName switch
            {
                "LobbyScene" => ViewId.Lobby,
                "GameScene" => ViewId.GameplayUI,
                "MainMenuScene" => ViewId.MainMenu,
                "LoadingScene" => ViewId.Loading,
                _ => ViewId.None
            };

            if (initialScreenId != ViewId.None)
            {
                await NavigateToAsync(initialScreenId, parameters);
            }
        }

        public async UniTask PrepareForSceneChangeAsync()
        {
            await ClearStackAsync();
        }

        public bool HasScreensInStack() => _navigationStack.Count > 0;

        public IView GetCurrentScreen() => _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;

        public void Dispose()
        {
            UnsubscribeFromEvents();
            ClearStackAsync().Forget();
        }
    }
}
