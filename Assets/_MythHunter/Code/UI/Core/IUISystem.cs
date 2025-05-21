using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.UI.Navigation;
using System;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс системи UI
    /// </summary>
    public interface IUISystem
    {
        UniTask<TView> ShowViewAsync<TView>() where TView : Component, IView;
        void HideView<TView>();
        void RegisterView<TView>(TView view);
        TView GetView<TView>();
        bool IsViewActive<TView>();


    }
}
