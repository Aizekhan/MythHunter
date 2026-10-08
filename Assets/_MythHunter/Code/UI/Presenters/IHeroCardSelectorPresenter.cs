// Шлях: Assets/_MythHunter/Code/UI/Presenters/IHeroCardSelectorPresenter.cs
using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;

namespace MythHunter.UI.Presenters
{
    public interface IHeroCardSelectorPresenter : IPresenter
    {
        UniTask InitializeWithHero(string archetypeId);
        void ConfirmHeroSelection();
        void GoBackToLobby();
    }
}
