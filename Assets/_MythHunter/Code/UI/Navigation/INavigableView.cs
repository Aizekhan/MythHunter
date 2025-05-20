// Шлях: Assets/_MythHunter/Code/UI/Navigation/INavigableView.cs

using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Розширення інтерфейсу IView для підтримки навігації
    /// </summary>
    public interface INavigableView : IView
    {
        /// <summary>
        /// Викликається при першому створенні представлення
        /// </summary>
        UniTask OnViewCreatedAsync(NavigationParameters parameters);

        /// <summary>
        /// Викликається коли представлення стає активним під час навігації
        /// </summary>
        UniTask OnViewNavigatedToAsync(NavigationParameters parameters);

        /// <summary>
        /// Викликається при навігації з цього представлення на інше
        /// </summary>
        UniTask OnViewNavigatedFromAsync();

        /// <summary>
        /// Викликається перед знищенням представлення
        /// </summary>
        UniTask OnViewDestroyedAsync();
    }
}
