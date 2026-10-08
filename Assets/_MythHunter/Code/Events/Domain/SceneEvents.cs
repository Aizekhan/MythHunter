using System;
using MythHunter.Events;

namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Подія перезавантаження сцени
    /// </summary>
    public struct SceneReloadedEvent : IEvent
    {
        public string SceneName;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }
}
