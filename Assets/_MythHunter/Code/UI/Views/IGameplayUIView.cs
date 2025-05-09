using MythHunter.UI.Core;

namespace MythHunter.UI.Views
{
    public interface IGameplayUIView : IView
    {
        void UpdatePhaseInfo(int phase, float timeRemaining);
        void ShowRuneValue(int value);
        void HideRuneValue();
    }
}
