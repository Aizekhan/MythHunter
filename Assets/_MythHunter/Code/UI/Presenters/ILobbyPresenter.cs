// Assets/_MythHunter/Code/UI/Presenters/ILobbyPresenter.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.UI.Models;
using MythHunter.UI.Views;

namespace MythHunter.UI.Presenters
{
    /// <summary>
    /// Інтерфейс презентера лоббі
    /// </summary>
    public interface ILobbyPresenter : IDisposable
    {
        /// <summary>
        /// Ініціалізує презентер з відповідним View
        /// </summary>
        void Initialize(ILobbyView view);

        /// <summary>
        /// Запускає лоббі з вказаною кількістю гравців
        /// </summary>
        void StartLobby(int playerCount);

        /// <summary>
        /// Обробляє вибір героя
        /// </summary>
        void OnHeroSelected(string archetypeId);

        /// <summary>
        /// Підтверджує вибір героїв
        /// </summary>
        void OnSelectionConfirmed();

        /// <summary>
        /// Починає гру
        /// </summary>
        UniTask StartGameAsync();

        /// <summary>
        /// Завантажує доступних героїв
        /// </summary>
        List<HeroCardModel> GetAvailableHeroes();

        /// <summary>
        /// Завантажує вибраних героїв
        /// </summary>
        List<HeroCardModel> GetSelectedHeroes();

        /// <summary>
        /// Отримує залишок мани
        /// </summary>
        int GetRemainingMana();

        /// <summary>
        /// Отримує час, що залишився для вибору
        /// </summary>
        float GetRemainingTime();
    }
}
