
// IUIViewFactory.cs
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс фабрики представлень UI
    /// </summary>
    public interface IUIViewFactory
    {
        // Фабрика тепер працює тільки з ViewId
        UniTask<IView> CreateViewAsync(ViewId viewId);
        void ReleaseView(ViewId viewId, IView view);
    }
}
