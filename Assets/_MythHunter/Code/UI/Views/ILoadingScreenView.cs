// Assets/_MythHunter/Code/UI/Views/ILoadingScreenView.cs
using MythHunter.Events.Domain.Loading;
using MythHunter.UI.Core;
using MythHunter.UI.Navigation;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// Інтерфейс для представлення екрану завантаження
    /// </summary>
    public interface ILoadingScreenView : INavigableView
    {
        /// <summary>
        /// Оновлює прогрес завантаження
        /// </summary>
        void UpdateProgress(float progress, string status);

        /// <summary>
        /// Оновлює інформацію про етап завантаження
        /// </summary>
        void UpdateLoadingStage(LoadingStage stage, string status);

        /// <summary>
        /// Показує помилку завантаження
        /// </summary>
        void ShowError(string errorMessage);

        /// <summary>
        /// Показує екран завершення завантаження
        /// </summary>
        void ShowCompletionScreen();
    }
}
