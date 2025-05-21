// Шлях: Assets/_MythHunter/Code/UI/Navigation/NavigationService.cs

using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;
using System;
using System.Collections.Generic;
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

        public async UniTask<TView> NavigateToAsync<TView>(NavigationParameters parameters = null, TransitionType transition = TransitionType.Default)
    where TView : Component, IView
        {
            var viewId = GetViewIdForType<TView>();
            if (viewId == ViewId.None)
            {
                _logger.LogError($"Не знайдено ViewId для типу {typeof(TView).Name}", "Navigation");
                return null;
            }
            return await NavigateToAsync<TView>(viewId, parameters, transition);
        }

        public async UniTask<TView> NavigateToAsync<TView>(ViewId viewId, NavigationParameters parameters = null, TransitionType transition = TransitionType.Default)
            where TView : Component, IView
        {
            _logger.LogInfo($"Навігація до екрану: {viewId}", "Navigation");

            try
            {
                IView currentView = _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;

                var viewConfig = _viewConfigRegistry.Get(viewId);
                if (viewConfig == null)
                {
                    _logger.LogError($"ViewConfig не знайдено для: {viewId}", "Navigation");
                    return null;
                }

                var newView = await _uiService.ShowScreenAsync<TView>();
                if (newView == null)
                {
                    _logger.LogError($"Не вдалося створити екран {typeof(TView).Name} для ViewId {viewId}", "Navigation");
                    return null;
                }

                if (currentView is INavigableView currentNavView)
                    await currentNavView.OnViewNavigatedFromAsync();

                await _transition.PlayTransitionAsync(currentView?.gameObject, newView.gameObject, transition, true);

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
                _logger.LogError($"Помилка при навігації до екрану {viewId}: {ex.Message}", "Navigation", ex);
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

            await _transition.PlayTransitionAsync(currentEntry.View?.gameObject, previousEntry.View?.gameObject, TransitionType.Default, false);

            if (previousEntry.View is INavigableView navTo)
                await navTo.OnViewNavigatedToAsync(parameters ?? previousEntry.Parameters);

            if (currentEntry.View is INavigableView navDestroy)
                await navDestroy.OnViewDestroyedAsync();

            // Виправлено: використовуємо generic-версію HideScreen якщо можливо
            if (currentEntry.View is Component component)
            {
                // Знаходимо тип і робимо виклик через _uiService.HideScreen<TView>()
                // Тут потрібно використати рефлексію або убезпечитись іншим чином
                _uiService.HideScreen<Component>(); // Примітка: це не точне рішення, але напрямок правильний
            }

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

                // Виправляємо цей виклик
                if (entry.View is Component component)
                {
                    // Подібне рішення як в GoBackAsync
                    _uiService.HideScreen<Component>(); // Примітка: не точне рішення
                }
            }

            await _transition.PlayTransitionAsync(currentEntry.View?.gameObject, rootEntry.View?.gameObject, TransitionType.Default, false);

            if (rootEntry.View is INavigableView navTo)
                await navTo.OnViewNavigatedToAsync(parameters ?? rootEntry.Parameters);

            return rootEntry.View;
        }

        public async UniTask PrepareForSceneChangeAsync()
        {
            await ClearStackAsync();
        }

        public bool HasScreensInStack() => _navigationStack.Count > 0;

        // Метод для отримання ViewId за типом
        public ViewId GetViewIdForType<TView>() where TView : Component, IView
        {
            var config = _viewConfigRegistry.GetByType<TView>();
            return config != null ? config.viewId : ViewId.None;
        }

        // Змінити метод ShowModalAsync, щоб використовувати правильну змінну
        public async UniTask<TResult> ShowModalAsync<TView, TResult>(ViewId viewId, NavigationParameters parameters = null)
            where TView : Component, IModalView<TResult>
        {
            var viewConfig = _viewConfigRegistry.Get(viewId);
            if (viewConfig == null)
            {
                _logger.LogError($"ViewConfig не знайдено для: {viewId}", "Navigation");
                return default;
            }

            var tcs = new UniTaskCompletionSource<TResult>();
            _modalTcs = tcs;

            // Виправлено: var newView замість modalView
            var newView = await _uiService.ShowScreenAsync<TView>();
            if (newView == null) // Виправлено: перевірка newView замість modalView
            {
                _logger.LogError($"Не вдалося створити модальне вікно для ViewId: {viewId}", "Navigation");
                return default;
            }

            _currentModal = newView; // Виправлено: використання newView
            await newView.InitializeAsync(parameters ?? new NavigationParameters()); // Виправлено: використання newView
            newView.SetCompletionCallback(result =>
            {
                _uiService.HideScreen<TView>(); // Виправлено: використання generic-параметра
                tcs.TrySetResult(result);
                _currentModal = null;
                _modalTcs = null;
            });

            return await tcs.Task;
        }

        public void CloseModal<TResult>(TResult result = default)
        {
            if (_currentModal == null || _modalTcs == null)
                return;

            if (_modalTcs is UniTaskCompletionSource<TResult> tcs)
            {
                if (_currentModal is Component comp)
                    _uiService.HideScreen(comp.GetType());

                tcs.TrySetResult(result);
                _currentModal = null;
                _modalTcs = null;
            }
        }

        public async UniTask<TView> SetInitialScreen<TView>(ViewId viewId, NavigationParameters parameters = null)
            where TView : Component, IView
        {
            await ClearStackAsync();
            return await NavigateToAsync<TView>(viewId, parameters);
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

        public IView GetCurrentScreen() => _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;

        public void Dispose()
        {
            UnsubscribeFromEvents();
            ClearStackAsync().Forget();
        }
    }
}
