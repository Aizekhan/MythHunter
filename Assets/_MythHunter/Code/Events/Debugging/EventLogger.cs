using System;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.Events.Debugging
{
    /// <summary>
    /// Логер подій для відлагодження
    /// </summary>
    public class EventLogger : IEventSubscriber
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger _logger;
        private bool _isEnabled = false;
        
        [Inject]
        public EventLogger(IEventBus eventBus, ILogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }
        
        public void Enable()
        {
            if (!_isEnabled)
            {
                SubscribeToEvents();
                _isEnabled = true;
                _logger.LogInfo("Event logger enabled");
            }
        }
        
        public void Disable()
        {
            if (_isEnabled)
            {
                UnsubscribeFromEvents();
                _isEnabled = false;
                _logger.LogInfo("Event logger disabled");
            }
        }
        
        public void SubscribeToEvents()
        {
            // Підписка на всі події (можна замінити на конкретний список)
            _eventBus.Subscribe<IEvent>(OnAnyEvent);
        }
        
        public void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<IEvent>(OnAnyEvent);
        }
        
        private void OnAnyEvent(IEvent evt)
        {
            _logger.LogDebug($"Event: {evt.GetType().Name}, ID: {evt.GetEventId()}");
        }
    }
}