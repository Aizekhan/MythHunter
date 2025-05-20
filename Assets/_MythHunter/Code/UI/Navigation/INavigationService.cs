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
        /// Навігація до нового екрану з додаванням у стек історії
        /// </summary>
        UniTask<TView> NavigateToAsync<TView>(string screenId, NavigationParameters parameters = null, TransitionType transition = TransitionType.Default)
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
        /// Заміна поточного екрану без додавання в стек
        /// </summary>
        UniTask<TView> ReplaceCurrentAsync<TView>(string screenId, NavigationParameters parameters = null)
            where TView : Component, IView;

        /// <summary>
        /// Показ модального вікна з очікуванням результату
        /// </summary>
        UniTask<TResult> ShowModalAsync<TView, TResult>(string modalId, NavigationParameters parameters = null)
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
        /// Встановлення початкового екрану для сцени
        /// </summary>
        UniTask<TView> SetInitialScreen<TView>(string screenId, NavigationParameters parameters = null)
            where TView : Component, IView;

        /// <summary>
        /// Перевірка, чи є екрани в стеку
        /// </summary>
        bool HasScreensInStack();
    }
}
