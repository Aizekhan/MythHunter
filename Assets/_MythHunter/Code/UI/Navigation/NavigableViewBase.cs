// Шлях: Assets/_MythHunter/Code/UI/Navigation/NavigableViewBase.cs

using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Базовий клас для представлень з підтримкою навігації
    /// </summary>
    public abstract class NavigableViewBase : UIViewBase, INavigableView
    {
        public virtual async UniTask OnViewCreatedAsync(NavigationParameters parameters)
        {
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnViewNavigatedToAsync(NavigationParameters parameters)
        {
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnViewNavigatedFromAsync()
        {
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnViewDestroyedAsync()
        {
            await UniTask.CompletedTask;
        }
    }
}
