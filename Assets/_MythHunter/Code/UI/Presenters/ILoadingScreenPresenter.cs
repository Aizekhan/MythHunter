// Assets/_MythHunter/Code/UI/Presenters/ILoadingScreenPresenter.cs
using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;
using MythHunter.UI.Views;

namespace MythHunter.UI.Presenters
{
    /// <summary>
    /// Інтерфейс презентера для екрану завантаження
    /// </summary>
    public interface ILoadingScreenPresenter : IPresenter
    {
        /// <summary>
        /// Встановлює представлення для презентера
        /// </summary>
        void SetView(ILoadingScreenView view);
    }
}
