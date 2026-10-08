// Assets/_MythHunter/Code/UI/Presenters/IProfilePresenter.cs
using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;
using MythHunter.UI.Views;

namespace MythHunter.UI.Presenters
{
    public interface IProfilePresenter : IPresenter
    {
        // Навігація
        void OnBackClicked();
        void Initialize(IProfileView view);
        // Вкладки
        void OnHeroesTabClicked();
        void OnStatsTabClicked();
        void OnSettingsTabClicked();
        void OnAchievementsTabClicked();

        // Дані
        UniTask LoadPlayerDataAsync();
        UniTask LoadHeroesAsync();
        UniTask SaveSettingsAsync();
    }
}
