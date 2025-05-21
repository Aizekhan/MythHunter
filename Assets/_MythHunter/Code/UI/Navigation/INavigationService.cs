// Шлях: Assets/_MythHunter/Code/UI/Navigation/INavigationService.cs

using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;
using UnityEngine;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Сервіс для навігації між UI екранами з підтримкою історії, анімацій та передачі параметрів
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Налаштування початкової навігації при завантаженні сцени
        /// </summary>
        UniTask SetupForSceneAsync(string sceneName, NavigationParameters parameters = null);

        /// <summary>
        /// Отримання сповіщення про зміну сцени
        /// </summary>
        void OnSceneChanged(string previousScene, string newScene);

        /// <summary>
        /// Навігація до нового екрану за його типом (найкращий варіант)
        /// </summary>
        UniTask<TView> NavigateToAsync<TView>(NavigationParameters parameters = null, TransitionType transition = TransitionType.Default)
            where TView : Component, IView;

        /// <summary>
        /// Навігація до нового екрану з додаванням у стек історії (з використанням enum)
        /// </summary>
        UniTask<TView> NavigateToAsync<TView>(ViewId viewId, NavigationParameters parameters = null, TransitionType transition = TransitionType.Default)
            where TView : Component, IView;

        /// <summary>
        /// Повернення до попереднього екрану
        /// </summary>
        UniTask<IView> GoBackAsync(NavigationParameters parameters = null);

        /// <summary>
        /// Повернення до кореневого екрану (очищення всього стеку)
        /// </summary>
        UniTask<IView> GoToRootAsync(NavigationParameters parameters = null);

        /// <summary>
        /// Заміна поточного екрану без додавання в стек (за типом)
        /// </summary>
        UniTask<TView> ReplaceCurrentAsync<TView>(NavigationParameters parameters = null)
            where TView : Component, IView;

        /// <summary>
        /// Заміна поточного екрану без додавання в стек (з використанням enum)
        /// </summary>
        UniTask<TView> ReplaceCurrentAsync<TView>(ViewId viewId, NavigationParameters parameters = null)
            where TView : Component, IView;

        /// <summary>
        /// Показ модального вікна з очікуванням результату (за типом)
        /// </summary>
        UniTask<TResult> ShowModalAsync<TView, TResult>(NavigationParameters parameters = null)
            where TView : Component, IModalView<TResult>;

        /// <summary>
        /// Показ модального вікна з очікуванням результату (з використанням enum)
        /// </summary>
        UniTask<TResult> ShowModalAsync<TView, TResult>(ViewId viewId, NavigationParameters parameters = null)
            where TView : Component, IModalView<TResult>;

        /// <summary>
        /// Закриття модального вікна з результатом
        /// </summary>
        void CloseModal<TResult>(TResult result = default);

        /// <summary>
        /// Очищення всього стеку навігації
        /// </summary>
        UniTask ClearStackAsync();

        /// <summary>
        /// Отримання поточного екрану
        /// </summary>
        IView GetCurrentScreen();

        /// <summary>
        /// Підготовка до зміни сцени - закриття всіх екранів
        /// </summary>
        UniTask PrepareForSceneChangeAsync();

        /// <summary>
        /// Встановлення початкового екрану для сцени (за типом)
        /// </summary>
        UniTask<TView> SetInitialScreen<TView>(NavigationParameters parameters = null)
            where TView : Component, IView;

        /// <summary>
        /// Встановлення початкового екрану для сцени (з використанням enum)
        /// </summary>
        UniTask<TView> SetInitialScreen<TView>(ViewId viewId, NavigationParameters parameters = null)
            where TView : Component, IView;

        /// <summary>
        /// Перевірка, чи є екрани в стеку
        /// </summary>
        bool HasScreensInStack();

        /// <summary>
        /// Отримання ViewId для вказаного типу View
        /// </summary>
        ViewId GetViewIdForType<TView>() where TView : Component, IView;
    }
}
