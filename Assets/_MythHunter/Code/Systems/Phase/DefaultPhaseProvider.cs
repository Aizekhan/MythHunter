// Шлях: Assets/_MythHunter/Code/Core/ECS/DefaultPhaseProvider.cs

using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Реалізація провайдера фаз за замовчуванням
    /// </summary>
    public class DefaultPhaseProvider : IPhaseProvider, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        private string _currentPhase = string.Empty;
        private readonly List<Action<string, string>> _phaseChangeCallbacks = new List<Action<string, string>>();

        // Статичний масив усіх доступних фаз
        private static readonly string[] AllPhaseIds = new[]
        {
            "None",
            "Rune",
            "Planning",
            "Movement",
            "Combat",
            "Freeze"
            // Додайте інші фази тут
        };

        [Inject]
        public DefaultPhaseProvider(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;

            // Підписка на зміну фази
            _eventBus.Subscribe<PhaseChangedEvent>(OnPhaseChangedEvent);
        }

        public string GetCurrentPhaseId()
        {
            return _currentPhase;
        }

        public bool IsCurrentPhase(string phaseId)
        {
            return _currentPhase == phaseId;
        }

        public void SubscribeToPhaseChange(Action<string, string> onPhaseChanged)
        {
            if (!_phaseChangeCallbacks.Contains(onPhaseChanged))
            {
                _phaseChangeCallbacks.Add(onPhaseChanged);
            }
        }

        public void UnsubscribeFromPhaseChange(Action<string, string> onPhaseChanged)
        {
            _phaseChangeCallbacks.Remove(onPhaseChanged);
        }

        public string[] GetAllPhaseIds()
        {
            // Повертаємо копію масиву
            return AllPhaseIds.ToArray();
        }

        private void OnPhaseChangedEvent(PhaseChangedEvent evt)
        {
            // Збереження старої фази
            string previousPhase = _currentPhase;

            // Оновлення поточної фази (потрібно адаптувати до вашої системи)
            _currentPhase = evt.CurrentPhase.ToString();

            _logger.LogDebug($"Phase changed from '{previousPhase}' to '{_currentPhase}'", "Phase");

            // Виклик всіх колбеків
            foreach (var callback in _phaseChangeCallbacks)
            {
                try
                {
                    callback(previousPhase, _currentPhase);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in phase change callback: {ex.Message}", "Phase", ex);
                }
            }
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<PhaseChangedEvent>(OnPhaseChangedEvent);
            _phaseChangeCallbacks.Clear();
        }
    }
}
