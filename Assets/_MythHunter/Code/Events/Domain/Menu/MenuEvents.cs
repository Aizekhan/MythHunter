using MythHunter.Services.GameSettings;
using System;

namespace MythHunter.Events.Domain.Menu
{
    public struct GameModeSelectedEvent : IEvent
    {
        public GameMode SelectedMode;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    public struct MainMenuExitedEvent : IEvent
    {
        public GameMode SelectedMode;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }
}
