// Шлях: Assets/_MythHunter/Code/UI/Core/IUIViewFactory.cs
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Спрощений інтерфейс фабрики представлень UI
    /// Всі представлення створюються з пулів через PreloadManager
    /// </summary>
    public interface IUIViewFactory
    {
        /// <summary>
        /// Створює представлення за його ідентифікатором (з пулу)
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <returns>Створене представлення</returns>
        UniTask<IView> CreateViewAsync(ViewId viewId);

        /// <summary>
        /// Повертає представлення в пул
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <param name="view">Представлення для повернення в пул</param>
        void ReturnViewToPool(ViewId viewId, IView view);

        /// <summary>
        /// Вивільняє представлення, знищуючи його (рідко використовується)
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <param name="view">Представлення для знищення</param>
        void ReleaseView(ViewId viewId, IView view);

        /// <summary>
        /// Застарілий метод - використовуйте CreateViewAsync
        /// </summary>
        [System.Obsolete("Використовуйте CreateViewAsync - всі View тепер створюються з пулів")]
        UniTask<IView> CreateViewFromPoolAsync(ViewId viewId);
    }
}
