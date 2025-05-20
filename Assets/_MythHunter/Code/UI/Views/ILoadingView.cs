// Assets/_MythHunter/Code/UI/Views/ILoadingView.cs
using MythHunter.UI.Core;

namespace MythHunter.UI.Views
{
    public interface ILoadingView : IView
    {
        void UpdateProgress(float progress, string status = null);
        void SetTitle(string title);
        void ShowRandomTip();
    }
}
