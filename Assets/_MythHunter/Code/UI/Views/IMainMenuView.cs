// Assets/_MythHunter/Code/UI/Views/IMainMenuView.cs
using MythHunter.UI.Core;

namespace MythHunter.UI.Views
{
    public interface IMainMenuView : IView
    {
        void SetTitle(string title);

        // ✅ СТАРІ методи (залишаємо)
   
        void SetSettingsButtonEnabled(bool enabled);
        void SetExitButtonEnabled(bool enabled);

        // ✅ НОВИЙ метод для профілю
        void SetProfileButtonEnabled(bool enabled);
        // ✅ НОВІ методи для режимів гри
        void SetOnlinePvPButtonEnabled(bool enabled);
        void SetLocalPvPButtonEnabled(bool enabled);
        void SetPvAIButtonEnabled(bool enabled);
    }
}
