using MythHunter.UI.Core;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Presenters
{
    public interface IMainMenuPresenter : IPresenter
    {
        void OnPlayClicked();
        void OnSettingsClicked();
        void OnExitClicked();
    }
}
