using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;
using UnityEngine;

namespace MythHunter.UI.Navigation
{
    public interface INavigationService
    {
        // Всі методи працюють тільки з ViewId
        UniTask<IView> NavigateToAsync(ViewId viewId, NavigationParameters parameters = null, TransitionType transition = TransitionType.Default);
        UniTask<TResult> ShowModalAsync<TResult>(ViewId viewId, NavigationParameters parameters = null);
        UniTask<IView> GoBackAsync(NavigationParameters parameters = null);
        UniTask<IView> GoToRootAsync(NavigationParameters parameters = null);
        UniTask ClearStackAsync();
        IView GetCurrentScreen();
        UniTask PrepareForSceneChangeAsync();
        bool HasScreensInStack();

        // Навігація при зміні сцени
        UniTask SetupForSceneAsync(string sceneName, NavigationParameters parameters);
    }
}
