// Assets/_MythHunter/Code/UI/Views/IGameplayUIView.cs
using MythHunter.UI.Core;
using System.Collections.Generic;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// Розширений інтерфейс для ігрового UI
    /// </summary>
    public interface IGameplayUIView : IView
    {
        // Існуючі методи
        void UpdatePhaseInfo(int phase, float timeRemaining);
        void ShowRuneValue(int value);
        void HideRuneValue();

        // НОВІ методи для геймплею
        void UpdateActionPoints(int current, int maximum);
        void ShowAvailableActions(List<ActionInfo> actions);
        void UpdateTurnIndicator(int currentPlayer, string playerName);
        void ShowWaitingForPlayer(string message);
        void HideWaitingForPlayer();
        void ShowGameOver(string message, bool isVictory);
        void SetUIInteractable(bool interactable);

        // Події
        event System.Action<string> OnActionButtonClicked;
        event System.Action OnEndTurnClicked;
    }
}
