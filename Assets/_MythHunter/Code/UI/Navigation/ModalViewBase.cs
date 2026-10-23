// Шлях: Assets/_MythHunter/Code/UI/Navigation/ModalViewBase.cs

using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;
using System;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Базовий клас для модальних вікон
    /// </summary>
    public abstract class ModalViewBase<TResult> : UIViewBase, IModalView<TResult>
    {
        protected Action<TResult> CompletionCallback;

        public virtual async UniTask InitializeAsync(NavigationParameters parameters)
        {
            await UniTask.CompletedTask;
        }

        public void SetCompletionCallback(Action<TResult> callback)
        {
            CompletionCallback = callback;
        }

        protected void Complete(TResult result)
        {
            CompletionCallback?.Invoke(result);
        }
    }
}
