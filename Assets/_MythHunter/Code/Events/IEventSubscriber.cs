namespace MythHunter.Events
{
    /// <summary>
    /// Інтерфейс для підписників на події
    /// </summary>
    public interface IEventSubscriber
    {
        void SubscribeToEvents();
        void UnsubscribeFromEvents();
    }
}