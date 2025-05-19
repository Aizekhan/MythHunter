// Assets/_MythHunter/Code/UI/Views/ILobbyView.cs

using System.Collections.Generic;
using MythHunter.UI.Models;
using MythHunter.UI.Core;
using UnityEngine;

namespace MythHunter.UI.Views
{
    public interface ILobbyView : IView
    {
        // Container properties
        Transform HeroCardsContainer
        {
            get;
        }
        Transform SelectedHeroesContainer
        {
            get;
        }

        // UI update methods (dumb container)
        void UpdateMana(int remainingMana, int totalMana);
        void UpdateTimer(float remainingTime, float totalTime);
        void ShowError(string message);
        void ShowPlayerStatus(int playerIndex, bool isReady);
        void ShowGameStartingMessage();
    }
}
