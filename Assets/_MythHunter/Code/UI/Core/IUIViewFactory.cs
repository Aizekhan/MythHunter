
// IUIViewFactory.cs
namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс фабрики представлень UI
    /// </summary>
    public interface IUIViewFactory
    {
        Cysharp.Threading.Tasks.UniTask<T> CreateViewAsync<T>(string prefabPath) where T : UnityEngine.Component, IView;
        void ReleaseView<T>(T view) where T : UnityEngine.Component, IView;
    }
}
