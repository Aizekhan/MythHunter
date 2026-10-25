namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс базового UI View
    /// </summary>
    public interface IView
    {
        void Show();
        void Hide();
        // Додаємо доступ до GameObject
        UnityEngine.GameObject gameObject
        {
            get;
        }
    }
}
