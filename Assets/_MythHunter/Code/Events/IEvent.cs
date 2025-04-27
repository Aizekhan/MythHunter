namespace MythHunter.Events
{
    /// <summary>
    /// Базовий інтерфейс для подій
    /// </summary>
    public interface IEvent
    {
        string GetEventId();
    }
}