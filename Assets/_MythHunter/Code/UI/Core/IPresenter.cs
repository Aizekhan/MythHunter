using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс базового Presenter для MVP
    /// </summary>
    public interface IPresenter
    {
        UniTask InitializeAsync();
        void Dispose();
    }
}