using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;
using System.Threading;
using UnityEngine;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// EPIC 08: Added CancellationToken parameters to async methods
    /// </summary>
    public interface INavigationService
    {
        // Всі методи працюють тільки з ViewId
        UniTask<IView> NavigateToAsync(ViewId viewId, NavigationParameters parameters = null, TransitionType transition = TransitionType.Default, CancellationToken cancellationToken = default);
        UniTask<TResult> ShowModalAsync<TResult>(ViewId viewId, NavigationParameters parameters = null, CancellationToken cancellationToken = default);
        UniTask<IView> GoBackAsync(NavigationParameters parameters = null, CancellationToken cancellationToken = default);
        UniTask<IView> GoToRootAsync(NavigationParameters parameters = null, CancellationToken cancellationToken = default);
        UniTask ClearStackAsync(CancellationToken cancellationToken = default);
        IView GetCurrentScreen();
        UniTask PrepareForSceneChangeAsync(CancellationToken cancellationToken = default);
        bool HasScreensInStack();

        // Навігація при зміні сцени
        UniTask SetupForSceneAsync(string sceneName, NavigationParameters parameters, CancellationToken cancellationToken = default);
    }
}
