// Шлях: Assets/_MythHunter/Code/UI/Navigation/IModalView.cs

using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Інтерфейс для модальних вікон, які повертають результат
    /// </summary>
    public interface IModalView<TResult> : IView
    {
        /// <summary>
        /// Ініціалізація модального вікна з параметрами
        /// </summary>
        UniTask InitializeAsync(NavigationParameters parameters);

        /// <summary>
        /// Встановлення callback для закриття модального вікна з результатом
        /// </summary>
        void SetCompletionCallback(System.Action<TResult> callback);
    }
}
