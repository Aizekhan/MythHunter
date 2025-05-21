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
        private const int MaxNavigationDepth = 10; // Максимальна глибина стеку навігації

        private object _currentModal;
        private object _modalTcs;
        private bool _isSubscribed;

        private struct NavigationEntry
        {
            public ViewId ViewId;
            public IView View;
            public NavigationParameters Parameters;
            public DateTime NavigationTime; // Додано для відстеження часу навігації
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

        /// <summary>
        /// Навігація до представлення за його ViewId
        /// </summary>
        public async UniTask<IView> NavigateToAsync(ViewId viewId, NavigationParameters parameters = null, TransitionType transition = TransitionType.Default)
        {
            _logger.LogInfo($"Навігація до екрану: {viewId}", "Navigation");

            try
            {
                // Перевірка глибини стеку
                if (_navigationStack.Count >= MaxNavigationDepth)
                {
                    _logger.LogWarning($"Досягнуто максимальної глибини навігації ({MaxNavigationDepth}). Повернення до кореневого екрану.", "Navigation");
                    await GoToRootAsync();
                }

                var config = _viewConfigRegistry.Get(viewId);
                if (config == null)
                {
                    _logger.LogError($"ViewConfig не знайдено для: {viewId}", "Navigation");
                    return null;
                }

                IView currentView = _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;
                ViewId currentViewId = _navigationStack.Count > 0 ? _navigationStack.Peek().ViewId : ViewId.None;

                // Отримуємо оптимальний тип переходу
                TransitionType actualTransition = GetTransitionType(currentViewId, viewId, transition);

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
                    actualTransition,
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
                    Parameters = parameters ?? new NavigationParameters(),
                    NavigationTime = DateTime.UtcNow
                });

                return newView;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при навігації до екрану {viewId}: {ex.Message}", "Navigation", ex);
                return null;
            }
        }

        /// <summary>
        /// Навігація назад до попереднього екрану
        /// </summary>
        public async UniTask<IView> GoBackAsync(NavigationParameters parameters = null)
        {
            try
            {
                if (_navigationStack.Count <= 1)
                {
                    _logger.LogWarning("Спроба повернутися назад, коли в стеку лише один екран або стек порожній", "Navigation");
                    return null;
                }

                var currentEntry = _navigationStack.Pop();
                var previousEntry = _navigationStack.Peek();

                if (currentEntry.View is INavigableView navFrom)
                    await navFrom.OnViewNavigatedFromAsync();

                // Визначаємо тип переходу для повернення
                TransitionType backTransition = GetTransitionType(currentEntry.ViewId, previousEntry.ViewId, TransitionType.Default);

                await _transition.PlayTransitionAsync(
                    (currentEntry.View as Component)?.gameObject,
                    (previousEntry.View as Component)?.gameObject,
                    backTransition,
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
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при поверненні назад: {ex.Message}", "Navigation", ex);
                return null;
            }
        }

        /// <summary>
        /// Повернення до кореневого екрану стеку навігації
        /// </summary>
        public async UniTask<IView> GoToRootAsync(NavigationParameters parameters = null)
        {
            try
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
                    TransitionType.Fade, // Використовуємо затухання для повернення до кореневого екрану
                    false
                );

                if (rootEntry.View is INavigableView navTo)
                    await navTo.OnViewNavigatedToAsync(parameters ?? rootEntry.Parameters);

                return rootEntry.View;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при поверненні до кореневого екрану: {ex.Message}", "Navigation", ex);
                return null;
            }
        }

        /// <summary>
        /// Показ модального вікна з отриманням результату
        /// </summary>
        public async UniTask<TResult> ShowModalAsync<TResult>(ViewId viewId, NavigationParameters parameters = null)
        {
            try
            {
                // Перевірка, чи немає вже активного модального вікна
                if (_currentModal != null)
                {
                    _logger.LogWarning($"Спроба відкрити модальне вікно {viewId}, коли вже є активне модальне вікно", "Navigation");
                    return default;
                }

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

                    // Додаємо додаткову інформацію до параметрів
                    parameters ??= new NavigationParameters();
                    parameters.Add("IsModal", true);
                    parameters.Add("ParentViewId", _navigationStack.Count > 0 ? _navigationStack.Peek().ViewId : ViewId.None);

                    await modalView.InitializeAsync(parameters);
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
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при показі модального вікна {viewId}: {ex.Message}", "Navigation", ex);
                return default;
            }
        }

        /// <summary>
        /// Очищення стеку навігації
        /// </summary>
        public async UniTask ClearStackAsync()
        {
            try
            {
                while (_navigationStack.Count > 0)
                {
                    var entry = _navigationStack.Pop();
                    if (entry.View is INavigableView navView)
                        await navView.OnViewDestroyedAsync();
                    entry.View.Hide();
                }

                // Якщо є активне модальне вікно, також закриваємо його
                if (_currentModal != null)
                {
                    if (_currentModal is Component modalComponent)
                    {
                        modalComponent.gameObject.SetActive(false);
                    }
                    _currentModal = null;
                    _modalTcs = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при очищенні стеку навігації: {ex.Message}", "Navigation", ex);
            }
        }

        /// <summary>
        /// Налаштування навігації для нової сцени
        /// </summary>
        public async UniTask SetupForSceneAsync(string sceneName, NavigationParameters parameters)
        {
            try
            {
                await ClearStackAsync();

                // Перетворюємо назву сцени у ViewId
                ViewId initialScreenId = sceneName switch
                {
                    "LobbyScene" => ViewId.Lobby,
                    "GameScene" => ViewId.GameplayUI,
                    "MainMenuScene" => ViewId.MainMenu,
                    "LoadingScene" => ViewId.LoadingScreen,
                    _ => ViewId.None
                };

                if (initialScreenId != ViewId.None)
                {
                    // Додаємо інформацію про сцену до параметрів
                    parameters ??= new NavigationParameters();
                    parameters.Add("SceneName", sceneName);
                    parameters.Add("IsSceneRoot", true);

                    await NavigateToAsync(initialScreenId, parameters, TransitionType.None); // Без переходу для початкового екрану
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при налаштуванні навігації для сцени {sceneName}: {ex.Message}", "Navigation", ex);
            }
        }

        /// <summary>
        /// Підготовка до зміни сцени
        /// </summary>
        public async UniTask PrepareForSceneChangeAsync()
        {
            try
            {
                await ClearStackAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при підготовці до зміни сцени: {ex.Message}", "Navigation", ex);
            }
        }

        /// <summary>
        /// Перевірка наявності екранів у стеку
        /// </summary>
        public bool HasScreensInStack() => _navigationStack.Count > 0;

        /// <summary>
        /// Отримання поточного екрану
        /// </summary>
        public IView GetCurrentScreen() => _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;

        /// <summary>
        /// Отримання поточного ViewId
        /// </summary>
        public ViewId GetCurrentViewId() => _navigationStack.Count > 0 ? _navigationStack.Peek().ViewId : ViewId.None;

        /// <summary>
        /// Отримання поточної глибини стеку навігації
        /// </summary>
        public int GetNavigationDepth() => _navigationStack.Count;

        /// <summary>
        /// Перевірка можливості додавання нового екрану до стеку
        /// </summary>
        public bool CanNavigateDeeper() => _navigationStack.Count < MaxNavigationDepth;

        /// <summary>
        /// Перевірка можливості повернення назад
        /// </summary>
        public bool CanGoBack() => _navigationStack.Count > 1;

        /// <summary>
        /// Навігація з налаштуваннями анімації
        /// </summary>
        public async UniTask<IView> NavigateToWithAnimationAsync(
            ViewId viewId,
            NavigationParameters parameters = null,
            float animationDuration = 0.3f,
            AnimationCurve curve = null)
        {
            parameters ??= new NavigationParameters();
            parameters.Add("AnimationDuration", animationDuration);
            if (curve != null)
                parameters.Add("AnimationCurve", curve);

            return await NavigateToAsync(viewId, parameters);
        }

        /// <summary>
        /// Визначення оптимального типу переходу між представленнями
        /// </summary>
        private TransitionType GetTransitionType(ViewId fromViewId, ViewId toViewId, TransitionType defaultType)
        {
            // Спеціальні правила для переходів між конкретними екранами
            switch (fromViewId)
            {
                case ViewId.Lobby when toViewId == ViewId.HeroCardSelector:
                    return TransitionType.SlideLeft;
                case ViewId.HeroCardSelector when toViewId == ViewId.Lobby:
                    return TransitionType.SlideRight;
                case ViewId.MainMenu when toViewId == ViewId.Lobby:
                    return TransitionType.SlideUp;
                case ViewId.Lobby when toViewId == ViewId.MainMenu:
                    return TransitionType.SlideDown;
                case ViewId.None:
                    return TransitionType.Fade; // Для першого екрану - затухання
                default:
                    // Для модальних вікон використовуємо Scale
                    var toConfig = _viewConfigRegistry.Get(toViewId);
                    if (toConfig != null && toConfig.isPopup)
                        return TransitionType.Scale;

                    return defaultType;
            }
        }

        /// <summary>
        /// Звільнення ресурсів
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromEvents();
            ClearStackAsync().Forget();
        }
    }
}
