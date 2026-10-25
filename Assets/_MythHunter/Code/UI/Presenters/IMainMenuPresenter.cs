using MythHunter.UI.Core;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Presenters
{
    public interface IMainMenuPresenter : IPresenter
    {
        void OnSettingsClicked();
        void OnExitClicked();

        // ✅ НОВІ методи для режимів гри
        void OnOnlinePvPClicked();
        void OnLocalPvPClicked();
        void OnPvAIClicked();

        void OnProfileClicked();
    }
}
