using MythHunter.UI.Core;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Presenters
{
    public interface IGameplayUIPresenter : IPresenter
    {
        void UpdatePhaseInfo(int phase, float timeRemaining);
        void UpdateRuneValue(int value);
    }
}
