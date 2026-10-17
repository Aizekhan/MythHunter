// Assets/_MythHunter/Code/UI/Presenters/ILobbyPresenter.cs

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Views;

namespace MythHunter.UI.Presenters
{
    public interface ILobbyPresenter : IPresenter
    {
        bool IsInitialized
        {
            get;
        }
        void Initialize(ILobbyView view);
        UniTask InitializeAsync();
        void Dispose();

        void OnHeroSelected(string archetypeId);
        void OnSelectionConfirmed();
        UniTask StartGameAsync();

        List<HeroCardModel> GetAvailableHeroes();
        int GetRemainingMana();
        float GetRemainingTime();
    }
}
