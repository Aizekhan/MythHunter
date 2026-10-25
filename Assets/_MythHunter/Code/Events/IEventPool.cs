namespace MythHunter.Events
{
    /// <summary>
    /// Інтерфейс для пулу подій
    /// </summary>
    public interface IEventPool
    {
        /// <summary>
        /// Отримує об'єкт події з пулу або створює новий
        /// </summary>
        T Get<T>() where T : struct, IEvent;

        /// <summary>
        /// Повертає об'єкт події в пул
        /// </summary>
        void Return<T>(T eventObject) where T : struct, IEvent;

        /// <summary>
        /// Очищає пул
        /// </summary>
        void Clear();
    }
}
