// Шлях: Assets/_MythHunter/Code/UI/Navigation/NavigationService.cs

using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.UI.Core;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Реалізація сервісу навігації між UI екранами
    /// </summary>
    public class NavigationService : INavigationService, IEventSubscriber, IDisposable
    {
        private readonly IUIService _uiService;
        private readonly IScreenTransition _transition;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;
        private readonly IViewConfigRegistry _viewConfigRegistry;
        // Стек навігації для зберігання історії екранів
        private readonly Stack<NavigationEntry> _navigationStack = new Stack<NavigationEntry>();

        // Поточне модальне вікно (якщо є)
        private object _currentModal;

        // TaskCompletionSource для очікування результату від модального вікна
        private object _modalTcs;

        // Чи підписаний на події
        private bool _isSubscribed;

        // Структура для збереження інформації про екран у стеку
        private struct NavigationEntry
        {
            public string ScreenId;
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

        /// <summary>
        /// Підписка на події
        /// </summary>
        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            _isSubscribed = true;
        }

        /// <summary>
        /// Відписка від подій
        /// </summary>
        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            _isSubscribed = false;
        }

        /// <summary>
        /// Обробник зміни стану гри
        /// </summary>
        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            // Очищаємо стек навігації при зміні стану гри
            if (evt.PreviousState != evt.NewState)
            {
                ClearStackAsync().Forget();
            }
        }

        /// <summary>
        /// Навігація до нового екрану
        /// </summary>
        public async UniTask<TView> NavigateToAsync<TView>(string screenId, NavigationParameters parameters = null, TransitionType transition = TransitionType.Default)
            where TView : Component, IView
        {
            _logger.LogInfo($"Навігація до екрану: {screenId}", "Navigation");

            try
            {
                // Отримуємо поточний екран (якщо є)
                IView currentView = _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;

                // Створюємо новий екран
                var viewConfig = _viewConfigRegistry.Get(screenId);
                if (viewConfig == null)
                {
                    _logger.LogError($"ViewConfig не знайдено: {screenId}", "Navigation");
                    return null;
                }

                Type viewType = Type.GetType(viewConfig.ViewTypeName);
                if (viewType == null)
                {
                    _logger.LogError($"Тип не знайдено: {viewConfig.ViewTypeName}", "Navigation");
                    return null;
                }

                var newView = await _uiService.ShowScreenAsync(viewType, viewConfig.PrefabPath);
                if (newView == null)
                {
                    _logger.LogError($"Не вдалося створити екран: {screenId}", "Navigation");
                    return null;
                }

                // Викликаємо метод OnViewNavigatedFrom для поточного екрану
                if (currentView is INavigableView currentNavigableView)
                {
                    await currentNavigableView.OnViewNavigatedFromAsync();
                }

                // Відтворюємо анімацію переходу
                await _transition.PlayTransitionAsync(
                    currentView?.gameObject,
                    newView.gameObject,
                    transition,
                    true
                );

                // Викликаємо методи життєвого циклу для нового екрану
                if (newView is INavigableView newNavigableView)
                {
                    await newNavigableView.OnViewCreatedAsync(parameters ?? new NavigationParameters());
                    await newNavigableView.OnViewNavigatedToAsync(parameters ?? new NavigationParameters());
                }

                // Додаємо новий екран у стек навігації
                _navigationStack.Push(new NavigationEntry
                {
                    ScreenId = screenId,
                    View = newView,
                    Parameters = parameters ?? new NavigationParameters()
                });

                return newView;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при навігації до екрану {screenId}: {ex.Message}", "Navigation", ex);
                return null;
            }
        }

        /// <summary>
        /// Повернення до попереднього екрану
        /// </summary>
        public async UniTask<IView> GoBackAsync(NavigationParameters parameters = null)
        {
            if (_navigationStack.Count <= 1)
            {
                _logger.LogWarning("Неможливо повернутися назад - стек навігації порожній", "Navigation");
                return null;
            }

            _logger.LogInfo("Повернення до попереднього екрану", "Navigation");

            try
            {
                // Видаляємо поточний екран зі стеку
                var currentEntry = _navigationStack.Pop();
                IView currentView = currentEntry.View;

                // Отримуємо попередній екран
                var previousEntry = _navigationStack.Peek();
                IView previousView = previousEntry.View;

                // Викликаємо метод OnViewNavigatedFrom для поточного екрану
                if (currentView is INavigableView currentNavigableView)
                {
                    await currentNavigableView.OnViewNavigatedFromAsync();
                }

                // Відтворюємо анімацію переходу
                await _transition.PlayTransitionAsync(
                    currentView?.gameObject,
                    previousView?.gameObject,
                    TransitionType.Default,
                    false
                );

                // Викликаємо метод OnViewNavigatedTo для попереднього екрану
                if (previousView is INavigableView previousNavigableView)
                {
                    await previousNavigableView.OnViewNavigatedToAsync(parameters ?? previousEntry.Parameters);
                }

                // Видаляємо поточний екран
                if (currentView is INavigableView navigableCurrentView)
                {
                    await navigableCurrentView.OnViewDestroyedAsync();
                }

                // Ховаємо поточний екран
                currentView.Hide();

                return previousView;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при поверненні до попереднього екрану: {ex.Message}", "Navigation", ex);
                return null;
            }
        }

        /// <summary>
        /// Повернення до кореневого екрану
        /// </summary>
        public async UniTask<IView> GoToRootAsync(NavigationParameters parameters = null)
        {
            if (_navigationStack.Count <= 1)
            {
                _logger.LogInfo("Вже знаходимось на кореневому екрані", "Navigation");
                return _navigationStack.Count > 0 ? _navigationStack.Peek().View : null;
            }

            _logger.LogInfo("Повернення до кореневого екрану", "Navigation");

            try
            {
                // Отримуємо поточний і кореневий екрани
                var currentEntry = _navigationStack.Peek();
                IView currentView = currentEntry.View;

                // Зберігаємо кореневий екран
                NavigationEntry rootEntry = default;

                // Викликаємо метод OnViewNavigatedFrom для поточного екрану
                if (currentView is INavigableView currentNavigableView)
                {
                    await currentNavigableView.OnViewNavigatedFromAsync();
                }

                // Видаляємо всі екрани, крім кореневого
                while (_navigationStack.Count > 1)
                {
                    var entry = _navigationStack.Pop();

                    if (_navigationStack.Count == 1)
                    {
                        // Залишився кореневий екран
                        rootEntry = _navigationStack.Peek();
                    }

                    // Видаляємо екран
                    if (entry.View != rootEntry.View)
                    {
                        if (entry.View is INavigableView navigableView)
                        {
                            await navigableView.OnViewDestroyedAsync();
                        }
                        entry.View.Hide();
                    }
                }

                // Отримуємо кореневий екран
                IView rootView = rootEntry.View;

                // Відтворюємо анімацію переходу
                await _transition.PlayTransitionAsync(
                    currentView?.gameObject,
                    rootView?.gameObject,
                    TransitionType.Default,
                    false
                );

                // Викликаємо метод OnViewNavigatedTo для кореневого екрану
                if (rootView is INavigableView rootNavigableView)
                {
                    await rootNavigableView.OnViewNavigatedToAsync(parameters ?? rootEntry.Parameters);
                }

                return rootView;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при поверненні до кореневого екрану: {ex.Message}", "Navigation", ex);
                return null;
            }
        }

        /// <summary>
        /// Заміна поточного екрану без додавання в стек
        /// </summary>
        public async UniTask<TView> ReplaceCurrentAsync<TView>(string screenId, NavigationParameters parameters = null)
            where TView : Component, IView
        {
            _logger.LogInfo($"Заміна поточного екрану на: {screenId}", "Navigation");

            try
            {
                // Якщо стек порожній, просто додаємо новий екран
                if (_navigationStack.Count == 0)
                {
                    return await NavigateToAsync<TView>(screenId, parameters);
                }

                // Отримуємо поточний екран
                var currentEntry = _navigationStack.Pop();
                IView currentView = currentEntry.View;

                // Створюємо новий екран
                var newView = await _uiService.ShowScreenAsync<TView>(screenId);
                if (newView == null)
                {
                    _logger.LogError($"Не вдалося створити екран: {screenId}", "Navigation");

                    // Повертаємо поточний екран назад у стек
                    _navigationStack.Push(currentEntry);
                    return null;
                }

                // Викликаємо метод OnViewNavigatedFrom для поточного екрану
                if (currentView is INavigableView currentNavigableView)
                {
                    await currentNavigableView.OnViewNavigatedFromAsync();
                }

                // Відтворюємо анімацію переходу
                await _transition.PlayTransitionAsync(
                    currentView?.gameObject,
                    newView.gameObject,
                    TransitionType.Default,
                    true
                );

                // Викликаємо методи життєвого циклу для нового екрану
                if (newView is INavigableView newNavigableView)
                {
                    await newNavigableView.OnViewCreatedAsync(parameters ?? new NavigationParameters());
                    await newNavigableView.OnViewNavigatedToAsync(parameters ?? new NavigationParameters());
                }

                // Видаляємо поточний екран
                if (currentView is INavigableView navigableCurrentView)
                {
                    await navigableCurrentView.OnViewDestroyedAsync();
                }
                currentView.Hide();

                // Додаємо новий екран у стек навігації
                _navigationStack.Push(new NavigationEntry
                {
                    ScreenId = screenId,
                    View = newView,
                    Parameters = parameters ?? new NavigationParameters()
                });

                return newView;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при заміні поточного екрану: {ex.Message}", "Navigation", ex);
                return null;
            }
        }

        /// <summary>
        /// Показ модального вікна з очікуванням результату
        /// </summary>
        public async UniTask<TResult> ShowModalAsync<TView, TResult>(string modalId, NavigationParameters parameters = null)
            where TView : Component, IModalView<TResult>
        {
            _logger.LogInfo($"Показ модального вікна: {modalId}", "Navigation");

            try
            {
                // Створюємо TaskCompletionSource для очікування результату
                var tcs = new UniTaskCompletionSource<TResult>();
                _modalTcs = tcs;

                // Створюємо модальне вікно
                var modalView = await _uiService.ShowScreenAsync<TView>(modalId);
                if (modalView == null)
                {
                    _logger.LogError($"Не вдалося створити модальне вікно: {modalId}", "Navigation");
                    throw new InvalidOperationException($"Не вдалося створити модальне вікно: {modalId}");
                }

                _currentModal = modalView;

                // Ініціалізуємо модальне вікно
                await modalView.InitializeAsync(parameters ?? new NavigationParameters());

                // Встановлюємо callback для завершення
                modalView.SetCompletionCallback(result =>
                {
                    // Закриваємо модальне вікно
                    modalView.Hide();

                    // Завершуємо TaskCompletionSource з результатом
                    tcs.TrySetResult(result);

                    _currentModal = null;
                    _modalTcs = null;
                });

                // Очікуємо на результат
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при показі модального вікна {modalId}: {ex.Message}", "Navigation", ex);
                throw;
            }
        }

        /// <summary>
        /// Закриття модального вікна з результатом
        /// </summary>
        public void CloseModal<TResult>(TResult result = default)
        {
            if (_currentModal == null || _modalTcs == null)
            {
                _logger.LogWarning("Неможливо закрити модальне вікно - немає активного модального вікна", "Navigation");
                return;
            }

            try
            {
                // Перевіряємо типи
                if (_modalTcs is UniTaskCompletionSource<TResult> tcs)
                {
                    // Закриваємо модальне вікно
                    if (_currentModal is IView view)
                    {
                        view.Hide();
                    }

                    // Завершуємо TaskCompletionSource з результатом
                    tcs.TrySetResult(result);

                    _currentModal = null;
                    _modalTcs = null;

                    _logger.LogInfo("Модальне вікно закрито", "Navigation");
                }
                else
                {
                    _logger.LogWarning($"Неспівпадіння типів при закритті модального вікна. Очікувалось: {_modalTcs.GetType()}, отримано: {typeof(TResult)}", "Navigation");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при закритті модального вікна: {ex.Message}", "Navigation", ex);
            }
        }

        /// <summary>
        /// Очищення всього стеку навігації
        /// </summary>
        public async UniTask ClearStackAsync()
        {
            _logger.LogInfo("Очищення стеку навігації", "Navigation");

            try
            {
                // Закриваємо модальне вікно, якщо є
                if (_currentModal != null)
                {
                    if (_currentModal is IView view)
                    {
                        view.Hide();
                    }

                    _currentModal = null;
                    _modalTcs = null;
                }

                // Видаляємо всі екрани зі стеку
                while (_navigationStack.Count > 0)
                {
                    var entry = _navigationStack.Pop();

                    // Викликаємо метод OnViewDestroyed перед знищенням
                    if (entry.View is INavigableView navigableView)
                    {
                        await navigableView.OnViewDestroyedAsync();
                    }

                    // Ховаємо екран
                    entry.View.Hide();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при очищенні стеку навігації: {ex.Message}", "Navigation", ex);
            }
        }

        /// <summary>
        /// Отримання поточного екрану
        /// </summary>
        public IView GetCurrentScreen()
        {
            if (_navigationStack.Count == 0)
                return null;

            return _navigationStack.Peek().View;
        }

        /// <summary>
        /// Підготовка до зміни сцени - закриття всіх екранів
        /// </summary>
        public async UniTask PrepareForSceneChangeAsync()
        {
            _logger.LogInfo("Підготовка до зміни сцени", "Navigation");

            // Очищаємо стек навігації
            await ClearStackAsync();
        }

        /// <summary>
        /// Встановлення початкового екрану для сцени
        /// </summary>
        public async UniTask<TView> SetInitialScreen<TView>(string prefabPath, NavigationParameters parameters = null)
     where TView : UnityEngine.Component, IView
        {
            _logger.LogInfo($"Встановлення початкового екрану: {prefabPath}", "Navigation");

            // Очищаємо стек навігації
            await ClearStackAsync();

            // Створюємо новий екран
            var newView = await _uiService.ShowScreenAsync<TView>(prefabPath);
            if (newView == null)
            {
                _logger.LogError($"Не вдалося створити початковий екран: {prefabPath}", "Navigation");
                return null;
            }

            // Викликаємо методи життєвого циклу для нового екрану (якщо підтримуються)
            if (newView is INavigableView navigableView)
            {
                await navigableView.OnViewCreatedAsync(parameters ?? new NavigationParameters());
                await navigableView.OnViewNavigatedToAsync(parameters ?? new NavigationParameters());
            }

            // Додаємо новий екран у стек навігації
            _navigationStack.Push(new NavigationEntry
            {
                ScreenId = prefabPath, // Використовуємо шлях як ідентифікатор
                View = newView,
                Parameters = parameters ?? new NavigationParameters()
            });

            return newView;
        }

        /// <summary>
        /// Перевірка, чи є екрани в стеку
        /// </summary>
        public bool HasScreensInStack()
        {
            return _navigationStack.Count > 0;
        }

        /// <summary>
        /// Звільнення ресурсів
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromEvents();

            // Очищаємо стек навігації
            ClearStackAsync().Forget();
        }

        public async UniTask SetupForSceneAsync(string sceneName, NavigationParameters parameters = null)
        {
            _logger.LogInfo($"Налаштування навігації для сцени: {sceneName}", "Navigation");

            // Очистити стек навігації
            await ClearStackAsync();

            // Визначити, який екран завантажити залежно від сцени
            string initialScreenId = GetInitialScreenForScene(sceneName);
            if (string.IsNullOrEmpty(initialScreenId))
            {
                _logger.LogWarning($"Не знайдено початковий екран для сцени: {sceneName}", "Navigation");
                return;
            }

            // Додати параметри сцени до навігаційних параметрів
            var navParams = parameters ?? new NavigationParameters();
            navParams.Add("SceneName", sceneName);

            // Завантажити відповідний екран
            await LoadInitialScreenAsync(initialScreenId, navParams);
        }

        // Приватний метод для визначення початкового екрану сцени
        private string GetInitialScreenForScene(string sceneName)
        {
            switch (sceneName.ToLower())
            {
                case "lobbyscene":
                case "mainmenu":
                    return "Lobby";
                case "gamescene":
                    return "GameUI";
                case "loadingscene":
                    return "LoadingUI";
                default:
                    return null;
            }
        }

        // Приватний метод для завантаження початкового екрана
        private async UniTask LoadInitialScreenAsync(string screenId, NavigationParameters parameters)
        {
            var viewConfigRegistry = _container.Resolve<IViewConfigRegistry>();
            var viewConfig = viewConfigRegistry.Get(screenId);

            if (viewConfig == null)
            {
                _logger.LogError($"ViewConfig не знайдено: {screenId}", "Navigation");
                return;
            }

            Type viewType = Type.GetType(viewConfig.ViewTypeName);
            if (viewType == null)
            {
                _logger.LogError($"Не вдалося отримати Type з ViewTypeName: {viewConfig.ViewTypeName}", "Navigation");
                return;
            }

            var view = await _uiService.ShowScreenAsync(viewType, viewConfig.PrefabPath);
            if (view is INavigableView navigableView)
            {
                await navigableView.OnViewCreatedAsync(parameters ?? new NavigationParameters());
                await navigableView.OnViewNavigatedToAsync(parameters ?? new NavigationParameters());
            }
        }


        public void OnSceneChanged(string previousScene, string newScene)
        {
            _logger.LogInfo($"Зміна сцени: {previousScene} -> {newScene}", "Navigation");

            // Налаштування для нової сцени
            var parameters = new NavigationParameters();
            parameters.Add("PreviousScene", previousScene);

            // Асинхронно налаштовуємо навігацію для нової сцени
            SetupForSceneAsync(newScene, parameters).Forget();
        }
    }
}
