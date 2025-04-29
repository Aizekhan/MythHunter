using MythHunter.UI.Core;

namespace MythHunter.UI.Views
{
    public interface IMainMenuView : IView
    {
        void SetTitle(string title);
        void SetPlayButtonEnabled(bool enabled);
        void SetSettingsButtonEnabled(bool enabled);
        void SetExitButtonEnabled(bool enabled);
    }
}
