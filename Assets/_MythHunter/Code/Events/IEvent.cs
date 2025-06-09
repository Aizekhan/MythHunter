namespace MythHunter.Events
{
    /// <summary>
    /// Базовий інтерфейс для подій
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Отримує унікальний ідентифікатор події
        /// </summary>
        string GetEventId();

        /// <summary>
        /// Отримує пріоритет події
        /// </summary>
        EventPriority GetPriority();
    }
}
