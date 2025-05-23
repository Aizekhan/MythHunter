// Assets/_MythHunter/Code/UI/Presenters/ILobbyPresenter.cs

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.UI.Models;
using MythHunter.UI.Views;

namespace MythHunter.UI.Presenters
{
    public interface ILobbyPresenter
    {
        bool IsInitialized
        {
            get;
        }
        void Initialize(ILobbyView view);
        UniTask InitializeAsync();
        void Dispose();

        // Методи обробки подій
        void OnHeroSelected(string archetypeId);
        void OnSelectionConfirmed();
        UniTask StartGameAsync();
        // Додатковий метод для явного запуску ініціалізації лоббі
        UniTask InitializeLobbyAsync(int playerCount);
        // Методи отримання даних
        List<HeroCardModel> GetAvailableHeroes();
        List<HeroCardModel> GetSelectedHeroes();
        int GetRemainingMana();
        float GetRemainingTime();
    }
}
