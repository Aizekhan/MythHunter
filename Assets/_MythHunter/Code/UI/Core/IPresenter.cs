namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс базового Presenter для MVP
    /// </summary>
    public interface IPresenter
    {
        void Initialize();
        void Dispose();
    }
}