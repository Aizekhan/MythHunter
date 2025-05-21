using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;
using UnityEngine;

namespace MythHunter.UI.Navigation
{
    public interface INavigationService
    {
        UniTask<TView> NavigateToAsync<TView>(ViewId viewId, NavigationParameters parameters = null, TransitionType transition = TransitionType.Default)
            where TView : Component, IView;

        UniTask<TView> NavigateToAsync<TView>(NavigationParameters parameters = null, TransitionType transition = TransitionType.Default)
            where TView : Component, IView;

        UniTask<TResult> ShowModalAsync<TView, TResult>(ViewId viewId, NavigationParameters parameters = null)
            where TView : Component, IModalView<TResult>;

        UniTask<IView> GoBackAsync(NavigationParameters parameters = null);

        UniTask<IView> GoToRootAsync(NavigationParameters parameters = null);

        UniTask ClearStackAsync();

        IView GetCurrentScreen();

        UniTask PrepareForSceneChangeAsync();

        bool HasScreensInStack();

        ViewId GetViewIdForType<TView>() where TView : Component, IView;
    }
}
