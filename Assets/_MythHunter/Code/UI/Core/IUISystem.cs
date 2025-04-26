using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс системи UI
    /// </summary>
    public interface IUISystem
    {
        void ShowView<TView>() where TView : Component, IView;
        void HideView<TView>() where TView : Component, IView;
        UniTask<TView> ShowViewAsync<TView>(string prefabPath) where TView : Component, IView;
        void RegisterView<TView>(TView view) where TView : Component, IView;
        void UnregisterView<TView>(TView view) where TView : Component, IView;
        TView GetView<TView>() where TView : Component, IView;
        bool IsViewActive<TView>() where TView : Component, IView;
    }
}