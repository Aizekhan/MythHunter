// Assets/_MythHunter/Code/UI/Views/ILobbyView.cs
using System.Collections.Generic;
using MythHunter.UI.Models;
using MythHunter.UI.Core;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// Інтерфейс представлення лоббі
    /// </summary>
    public interface ILobbyView : IView
    {
        /// <summary>
        /// Створює картки героїв на екрані з доступних героїв
        /// </summary>
        void PopulateHeroCards(List<HeroCardModel> heroes);

        /// <summary>
        /// Оновлює вибраних героїв
        /// </summary>
        void UpdateSelectedHeroes(List<HeroCardModel> selectedHeroes);

        /// <summary>
        /// Оновлює відображення залишку мани
        /// </summary>
        void UpdateMana(int remainingMana, int totalMana);

        /// <summary>
        /// Оновлює таймер
        /// </summary>
        void UpdateTimer(float remainingTime, float totalTime);

        /// <summary>
        /// Показує повідомлення про помилку
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Показує інформацію про готовність чи вибір іншого гравця
        /// </summary>
        void ShowPlayerStatus(int playerIndex, bool isReady);

        /// <summary>
        /// Показує повідомлення про початок гри
        /// </summary>
        void ShowGameStartingMessage();
    }
}
