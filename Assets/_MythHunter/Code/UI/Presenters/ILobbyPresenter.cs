// Assets/_MythHunter/Code/UI/Presenters/ILobbyPresenter.cs

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.UI.Models;
using MythHunter.UI.Views;

namespace MythHunter.UI.Presenters
{
    public interface ILobbyPresenter
    {
        void Initialize(ILobbyView view);
        UniTask InitializeAsync();
        void Dispose();

        // Методи обробки подій
        void OnHeroSelected(string archetypeId);
        void OnSelectionConfirmed();
        UniTask StartGameAsync();

        // Методи отримання даних
        List<HeroCardModel> GetAvailableHeroes();
        List<HeroCardModel> GetSelectedHeroes();
        int GetRemainingMana();
        float GetRemainingTime();
    }
}
