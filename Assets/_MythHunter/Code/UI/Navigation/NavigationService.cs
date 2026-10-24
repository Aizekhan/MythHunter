// Шлях: Assets/_MythHunter/Code/UI/Navigation/NavigationService.cs

using Cysharp.Threading.Tasks;
using RPGFramework.DI;
using MythHunter.Core.Game;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Спрощений навігаційний сервіс - прямо працює з UIViewFactory
    /// </summary>
    public class NavigationService : INavigationService, IEventSubscriber, IDisposable
    {
        private readonly IUIViewFactory _viewFactory;
        private readonly IScreenTransition _transition;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly ISceneViewRegistry _sceneViewRegistry;

        private readonly Stack<NavigationEntry> _navigationStack = new();
        private const int MaxNavigationDepth = 10;

        private object _currentModal;
        private object _modalTcs;
        private bool _isSubscribed;

        private struct NavigationEntry
        {
            public ViewId ViewId;
            public IView View;
            public NavigationParameters Parameters;
            public DateTime NavigationTime;
        }

        [Inject]
        public NavigationService(
            IUIViewFactory viewFactory,
            IScreenTransition transition,
            IEventBus eventBus,
            IMythLogger logger,
            ISceneViewRegistry sceneViewRegistry)
        {
            _viewFactory = viewFactory;
            _transition = transition;
            _eventBus = eventBus;
            _logger = logger;
            _sceneViewRegistry = sceneViewRegistry;

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
            if (evt.PreviousState == evt.NewState)
                return;

            var previousBehavior = GetNavigationBehavior(evt.PreviousState);
            var newBehavior = GetNavigationBehavior(evt.NewState);

            bool shouldClear = previousBehavior == NavigationClearType.OnExit ||
                              newBehavior == NavigationClearType.Always;

            if (shouldClear)
            {
                // EPIC 08: Pass default cancellation token (fire-and-forget)
                ClearStackAsync(default).Forget();
            }
        }

        /// <summary>
        /// Основний метод навігації - спрощений
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        public async UniTask<IView> NavigateToAsync(ViewId viewId, NavigationParameters parameters = null, TransitionType transition = TransitionType.Default, CancellationToken cancellationToken = default)
        {
            _logger.LogInfo($"🧭 Навігація до: {viewId}", "Navigation");

            try
            {
                // EPIC 08: Check cancellation at start
                cancellationToken.ThrowIfCancellationRequested();

                // Перевірка глибини стеку
                if (_navigationStack.Count >= MaxNavigationDepth)
                {
                    _logger.LogWarning($"⚠️ Досягнуто максимальної глибини навігації ({MaxNavigationDepth})", "Navigation");
                    await GoToRootAsync(null, cancellationToken);
                }

                // Отримуємо поточний екран
                IView currentView = _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;
                ViewId currentViewId = _navigationStack.Count > 0 ? _navigationStack.Peek().ViewId : ViewId.None;

                // Створюємо новий екран через фабрику
                var newView = await _viewFactory.CreateViewAsync(viewId);
                if (newView == null)
                {
                    _logger.LogError($"❌ Не вдалося створити екран {viewId}", "Navigation");
                    return null;
                }

                // EPIC 08: Check cancellation before navigation
                cancellationToken.ThrowIfCancellationRequested();

                // Обробка навігації для старого екрану
                if (currentView is INavigableView currentNavView)
                    await currentNavView.OnViewNavigatedFromAsync();

                // Анімація переходу
                TransitionType actualTransition = GetOptimalTransition(currentViewId, viewId, transition);
                await _transition.PlayTransitionAsync(
                    (currentView as Component)?.gameObject,
                    (newView as Component)?.gameObject,
                    actualTransition,
                    true
                );

                // Обробка навігації для нового екрану
                if (newView is INavigableView navView)
                {
                    await navView.OnViewCreatedAsync(parameters ?? new NavigationParameters());
                    await navView.OnViewNavigatedToAsync(parameters ?? new NavigationParameters());
                }

                // Додаємо в стек
                _navigationStack.Push(new NavigationEntry
                {
                    ViewId = viewId,
                    View = newView,
                    Parameters = parameters ?? new NavigationParameters(),
                    NavigationTime = DateTime.UtcNow
                });

                _logger.LogInfo($"✅ Навігація до {viewId} завершена", "Navigation");
                return newView;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка навігації до {viewId}: {ex.Message}", "Navigation", ex);
                return null;
            }
        }

        /// <summary>
        /// Повернення назад - спрощене
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        public async UniTask<IView> GoBackAsync(NavigationParameters parameters = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // EPIC 08: Check cancellation at start
                cancellationToken.ThrowIfCancellationRequested();

                if (_navigationStack.Count <= 1)
                {
                    _logger.LogWarning("⚠️ Неможливо повернутися назад - стек майже порожній", "Navigation");
                    return null;
                }

                var currentEntry = _navigationStack.Pop();
                var previousEntry = _navigationStack.Peek();

                // Обробка навігації
                if (currentEntry.View is INavigableView navFrom)
                    await navFrom.OnViewNavigatedFromAsync();

                // Анімація переходу назад
                await _transition.PlayTransitionAsync(
                    (currentEntry.View as Component)?.gameObject,
                    (previousEntry.View as Component)?.gameObject,
                    TransitionType.SlideRight, // Завжди slide right для назад
                    false
                );

                if (previousEntry.View is INavigableView navTo)
                    await navTo.OnViewNavigatedToAsync(parameters ?? previousEntry.Parameters);

                // Очищення поточного екрану
                if (currentEntry.View is INavigableView navDestroy)
                    await navDestroy.OnViewDestroyedAsync();

                _viewFactory.ReturnViewToPool(currentEntry.ViewId, currentEntry.View);

                _logger.LogInfo($"🔙 Повернулися з {currentEntry.ViewId} до {previousEntry.ViewId}", "Navigation");
                return previousEntry.View;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка при поверненні назад: {ex.Message}", "Navigation", ex);
                return null;
            }
        }

        /// <summary>
        /// Повернення до кореневого екрану
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        public async UniTask<IView> GoToRootAsync(NavigationParameters parameters = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // EPIC 08: Check cancellation at start
                cancellationToken.ThrowIfCancellationRequested();

                if (_navigationStack.Count <= 1)
                    return _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;

                var currentEntry = _navigationStack.Peek();
                NavigationEntry rootEntry = default;

                if (currentEntry.View is INavigableView navFrom)
                    await navFrom.OnViewNavigatedFromAsync();

                // Очищуємо всі проміжні екрани
                while (_navigationStack.Count > 1)
                {
                    // EPIC 08: Check cancellation during loop
                    cancellationToken.ThrowIfCancellationRequested();

                    var entry = _navigationStack.Pop();
                    if (_navigationStack.Count == 1)
                        rootEntry = _navigationStack.Peek();

                    if (entry.View is INavigableView navDestroy)
                        await navDestroy.OnViewDestroyedAsync();

                    _viewFactory.ReturnViewToPool(entry.ViewId, entry.View);
                }

                // Анімація до кореневого екрану
                await _transition.PlayTransitionAsync(
                    currentEntry.View?.gameObject,
                    rootEntry.View?.gameObject,
                    TransitionType.Fade,
                    false
                );

                if (rootEntry.View is INavigableView navTo)
                    await navTo.OnViewNavigatedToAsync(parameters ?? rootEntry.Parameters);

                _logger.LogInfo($"🏠 Повернулися до кореневого екрану {rootEntry.ViewId}", "Navigation");
                return rootEntry.View;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка повернення до кореня: {ex.Message}", "Navigation", ex);
                return null;
            }
        }

        /// <summary>
        /// Модальні вікна - спрощені
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        public async UniTask<TResult> ShowModalAsync<TResult>(ViewId viewId, NavigationParameters parameters = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // EPIC 08: Check cancellation at start
                cancellationToken.ThrowIfCancellationRequested();

                if (_currentModal != null)
                {
                    _logger.LogWarning($"⚠️ Вже є активне модальне вікно", "Navigation");
                    return default;
                }

                var tcs = new UniTaskCompletionSource<TResult>();
                _modalTcs = tcs;

                var newView = await _viewFactory.CreateViewAsync(viewId);
                if (newView == null)
                {
                    _logger.LogError($"❌ Не вдалося створити модальне вікно {viewId}", "Navigation");
                    return default;
                }

                // EPIC 08: Check cancellation before showing modal
                cancellationToken.ThrowIfCancellationRequested();

                if (newView is IModalView<TResult> modalView)
                {
                    _currentModal = modalView;

                    parameters ??= new NavigationParameters();
                    parameters.Add("IsModal", true);

                    await modalView.InitializeAsync(parameters);
                    modalView.SetCompletionCallback(result =>
                    {
                        _viewFactory.ReturnViewToPool(viewId, newView);
                        tcs.TrySetResult(result);
                        _currentModal = null;
                        _modalTcs = null;
                    });

                    // EPIC 08: Await with cancellation support
                    return await tcs.Task.AttachExternalCancellation(cancellationToken);
                }
                else
                {
                    _logger.LogError($"❌ View {viewId} не реалізує IModalView<{typeof(TResult).Name}>", "Navigation");
                    _viewFactory.ReturnViewToPool(viewId, newView);
                    return default;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка модального вікна {viewId}: {ex.Message}", "Navigation", ex);
                return default;
            }
        }

        /// <summary>
        /// Налаштування для сцени - спрощене
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        public async UniTask SetupForSceneAsync(string sceneName, NavigationParameters parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInfo($"🎬 Налаштування навігації для сцени: {sceneName}", "Navigation");

                // EPIC 08: Pass cancellation token through
                await ClearStackAsync(cancellationToken);

                bool shouldSkipInitialView = parameters?.GetValue<bool>("DontCreateInitialView", false) ?? false;
                if (shouldSkipInitialView)
                {
                    _logger.LogInfo($"⏭️ Пропускаємо автоматичне створення View для {sceneName}", "Navigation");
                    return;
                }

                ViewId initialScreenId = _sceneViewRegistry.GetViewIdForScene(sceneName);
                if (initialScreenId != ViewId.None)
                {
                    parameters ??= new NavigationParameters();
                    parameters.Add("SceneName", sceneName);
                    parameters.Add("IsSceneRoot", true);

                    // EPIC 08: Pass cancellation token through
                    await NavigateToAsync(initialScreenId, parameters, TransitionType.None, cancellationToken);
                    _logger.LogInfo($"✅ Створено початковий View {initialScreenId} для сцени {sceneName}", "Navigation");
                }
                else
                {
                    _logger.LogInfo($"ℹ️ Для сцени {sceneName} не визначено початкового View", "Navigation");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка налаштування навігації для сцени {sceneName}: {ex.Message}", "Navigation", ex);
            }
        }

        /// <summary>
        /// Очищення стеку - спрощене
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        public async UniTask ClearStackAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                while (_navigationStack.Count > 0)
                {
                    // EPIC 08: Check cancellation during loop
                    cancellationToken.ThrowIfCancellationRequested();

                    var entry = _navigationStack.Pop();
                    if (entry.View != null)
                    {
                        if (entry.View is INavigableView navView)
                            await navView.OnViewDestroyedAsync();
                        _viewFactory.ReturnViewToPool(entry.ViewId, entry.View);
                    }
                }

                if (_currentModal != null && _currentModal is Component modalComponent)
                {
                    modalComponent.gameObject.SetActive(false);
                }
                _currentModal = null;
                _modalTcs = null;

                _logger.LogInfo("🧹 Стек навігації очищено", "Navigation");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка очищення стеку: {ex.Message}", "Navigation", ex);
            }
        }

        // Допоміжні методи та основні інтерфейсні методи
        /// <summary>
        /// EPIC 08: Added CancellationToken parameter
        /// </summary>
        public async UniTask PrepareForSceneChangeAsync(CancellationToken cancellationToken = default) => await ClearStackAsync(cancellationToken);
        public bool HasScreensInStack() => _navigationStack.Count > 0;
        public IView GetCurrentScreen() => _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;

        /// <summary>
        /// Оптимальний перехід між екранами
        /// </summary>
        private TransitionType GetOptimalTransition(ViewId fromViewId, ViewId toViewId, TransitionType defaultType)
        {
            return (fromViewId, toViewId) switch
            {
                (ViewId.Lobby, ViewId.HeroCardSelector) => TransitionType.SlideLeft,
                (ViewId.HeroCardSelector, ViewId.Lobby) => TransitionType.SlideRight,
                (ViewId.MainMenu, ViewId.Lobby) => TransitionType.SlideUp,
                (ViewId.Lobby, ViewId.MainMenu) => TransitionType.SlideDown,
                (ViewId.None, _) => TransitionType.Fade,
                _ => defaultType == TransitionType.Default ? TransitionType.Fade : defaultType
            };
        }

        private NavigationClearType GetNavigationBehavior(GameStateType state)
        {
            var field = typeof(GameStateType).GetField(state.ToString());
            if (field != null)
            {
                var attribute = field.GetCustomAttribute<NavigationBehaviorAttribute>();
                return attribute?.ClearType ?? NavigationClearType.Never;
            }
            return NavigationClearType.Never;
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
            // EPIC 08: Pass default cancellation token (fire-and-forget)
            ClearStackAsync(default).Forget();
        }
    }
}
