// Файл: Assets/_MythHunter/Code/Events/Debugging/EventLogger.cs
using System;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.Events.Debugging
{
    /// <summary>
    /// Логер подій для відлагодження
    /// </summary>
    public class EventLogger : EventHandlerBase
    {
        private bool _isEnabled = false;
        private Action<IEvent, Type> _debugHandler;

        [Inject]
        public EventLogger(IEventBus eventBus, IMythLogger logger) : base(eventBus, logger)
        {
            _debugHandler = OnAnyEvent;
        }

        public void Enable()
        {
            if (!_isEnabled)
            {
                DebugEventMiddleware.Subscribe(_debugHandler);
                _isEnabled = true;
                _logger.LogInfo("Event logger enabled", "EventLogger");
            }
        }

        public void Disable()
        {
            if (_isEnabled)
            {
                DebugEventMiddleware.Unsubscribe(_debugHandler);
                _isEnabled = false;
                _logger.LogInfo("Event logger disabled", "EventLogger");
            }
        }

        private void OnAnyEvent(IEvent evt, Type eventType)
        {
            _logger.LogDebug($"Event: {eventType.Name}, ID: {evt.GetEventId()}", "EventDebug");
        }

        public override void Dispose()
        {
            Disable();
            base.Dispose();
        }
    }
}
