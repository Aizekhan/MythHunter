// Assets/_MythHunter/Code/UI/Views/IHeroCardUI.cs

using System;
using MythHunter.UI.Models;

namespace MythHunter.UI.Views
{
    public interface IHeroCardUI
    {
        string ArchetypeId
        {
            get;
        }

        // Методи налаштування
        void Setup(HeroCardModel model);
        void SetInteractable(bool interactable);
        void Reset();

        // Callback
        event Action<string> OnHeroSelected;
    }
}
