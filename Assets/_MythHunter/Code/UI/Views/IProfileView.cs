// Assets/_MythHunter/Code/UI/Views/IProfileView.cs
using MythHunter.UI.Core;
using UnityEngine;

namespace MythHunter.UI.Views
{
    public interface IProfileView : IView
    {
        // Навігація
        void ShowBackButton(bool show);

        // Вкладки
        void ShowHeroesTab();
        void ShowStatsTab();
        void ShowSettingsTab();
        void ShowAchievementsTab();

        // Дані гравця
        void SetPlayerName(string playerName);
        void SetPlayerLevel(int level);
        void SetPlayerExp(int currentExp, int maxExp);

        // Контейнери для контенту
        Transform HeroesContainer
        {
            get;
        }
        Transform StatsContainer
        {
            get;
        }
        Transform SettingsContainer
        {
            get;
        }
        Transform AchievementsContainer
        {
            get;
        }

        // Статус
        void ShowLoadingState(bool isLoading);
        void ShowError(string error);

        // ✅ НОВІ методи для роботи з контентом
        void SetHeroesCount(int total, int owned);
        void SetPlayerStats(int gamesPlayed, float winRate, string favoriteHero, int totalPlayTimeMinutes);
        void SetAchievementPoints(int points);
        void LoadSettings(bool soundEnabled, float volume, bool notificationsEnabled, int languageIndex);
    }
}
