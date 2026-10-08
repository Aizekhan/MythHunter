// Assets/_MythHunter/Code/Events/Domain/Profile/ProfileEvents.cs
using System;

namespace MythHunter.Events.Domain.Profile
{
    /// <summary>
    /// Подія входу в профіль
    /// </summary>
    public struct ProfileOpenedEvent : IEvent
    {
        public string PlayerId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія закриття профілю
    /// </summary>
    public struct ProfileClosedEvent : IEvent
    {
        public string PlayerId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія зміни налаштувань в профілі
    /// </summary>
    public struct ProfileSettingsChangedEvent : IEvent
    {
        public string SettingName;
        public object NewValue;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
