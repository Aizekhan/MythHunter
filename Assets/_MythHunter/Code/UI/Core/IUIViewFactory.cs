// Шлях: Assets/_MythHunter/Code/UI/Core/IUIViewFactory.cs
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс фабрики представлень UI, що працює виключно через ViewId
    /// </summary>
    public interface IUIViewFactory
    {
        /// <summary>
        /// Створює представлення за його ідентифікатором
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <returns>Створене представлення</returns>
        UniTask<IView> CreateViewAsync(ViewId viewId);

        /// <summary>
        /// Створює представлення з об'єктного пулу за його ідентифікатором
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <returns>Створене представлення з пулу</returns>
        UniTask<IView> CreateViewFromPoolAsync(ViewId viewId);

        /// <summary>
        /// Повертає представлення в пул
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <param name="view">Представлення для повернення в пул</param>
        void ReturnViewToPool(ViewId viewId, IView view);

        /// <summary>
        /// Вивільняє представлення, знищуючи його
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <param name="view">Представлення для знищення</param>
        void ReleaseView(ViewId viewId, IView view);
    }
}
