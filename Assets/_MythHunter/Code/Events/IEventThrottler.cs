// Assets/_MythHunter/Code/Events/Core/IEventThrottler.cs
namespace MythHunter.Events
{
    /// <summary>
    /// Інтерфейс для обмежувача частоти подій
    /// </summary>
    public interface IEventThrottler
    {
        /// <summary>
        /// Реєструє обмеження частоти для події
        /// </summary>
        /// <typeparam name="TEvent">Тип події</typeparam>
        /// <param name="minInterval">Мінімальний інтервал між подіями в секундах</param>
        /// <param name="mode">Режим обмеження</param>
        /// <param name="useUnscaledTime">Використовувати нескальований час</param>
        void RegisterThrottle<TEvent>(float minInterval, ThrottleMode mode = ThrottleMode.DropIntermediate, bool useUnscaledTime = false) where TEvent : struct, IEvent;

        /// <summary>
        /// Публікує подію з урахуванням обмеження частоти
        /// </summary>
        /// <typeparam name="TEvent">Тип події</typeparam>
        /// <param name="eventData">Дані події</param>
        void PublishThrottled<TEvent>(TEvent eventData) where TEvent : struct, IEvent;

        /// <summary>
        /// Перевіряє, чи можна опублікувати подію за заданим іменем та інтервалом
        /// </summary>
        /// <param name="name">Ім'я події</param>
        /// <param name="interval">Інтервал між подіями</param>
        /// <returns>true, якщо можна опублікувати</returns>
        bool CheckThrottle(string name, float interval);

        /// <summary>
        /// Оновлення обмежувача подій
        /// </summary>
        void Update();
    }

    /// <summary>
    /// Режим обмеження частоти подій
    /// </summary>
    public enum ThrottleMode
    {
        /// <summary>
        /// Відкидати проміжні події, публікувати тільки першу та останню в інтервалі
        /// </summary>
        DropIntermediate,

        /// <summary>
        /// Публікувати тільки першу подію в інтервалі, інші відкидати
        /// </summary>
        First,

        /// <summary>
        /// Публікувати тільки останню подію в інтервалі, інші відкидати
        /// </summary>
        Last
    }
}
