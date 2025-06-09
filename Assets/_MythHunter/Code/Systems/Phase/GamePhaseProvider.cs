// Шлях: Assets/_MythHunter/Code/Systems/Phase/GamePhaseProvider.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Phase
{
    /// <summary>
    /// Реалізація провайдера фаз на основі GamePhase
    /// </summary>
    public class GamePhaseProvider : IPhaseProvider, IEventSubscriber, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        private string _currentPhaseId = "None";
        private readonly List<Action<string, string>> _phaseChangeCallbacks = new List<Action<string, string>>();
        private bool _isSubscribed = false;

        // Статичний словник для перетворення GamePhase в string ID
        private static readonly Dictionary<GamePhase, string> PhaseToIdMap = new Dictionary<GamePhase, string>()
        {
            { GamePhase.None, "None" },
            { GamePhase.Rune, "Rune" },
            { GamePhase.Planning, "Planning" },
            { GamePhase.Active, "Active" }, 
            { GamePhase.Freeze, "Freeze" }
        };

        // Зворотнє відображення для конвертації string в GamePhase
        private static readonly Dictionary<string, GamePhase> IdToPhaseMap;

        // Статичний конструктор для ініціалізації зворотнього словника
        static GamePhaseProvider()
        {
            IdToPhaseMap = PhaseToIdMap.ToDictionary(pair => pair.Value, pair => pair.Key);
        }

        [Inject]
        public GamePhaseProvider(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;

            // Підписка на події здійснюється через метод SubscribeToEvents
            SubscribeToEvents();
            _logger.LogInfo("GamePhaseProvider initialized", "Phase");
        }

        /// <summary>
        /// Перетворює GamePhase на string ID
        /// </summary>
        public static string ConvertPhaseToId(GamePhase phase)
        {
            return PhaseToIdMap.TryGetValue(phase, out var id) ? id : "Unknown";
        }

        /// <summary>
        /// Перетворює string ID на GamePhase
        /// </summary>
        public static GamePhase ConvertIdToPhase(string phaseId)
        {
            return IdToPhaseMap.TryGetValue(phaseId, out var phase) ? phase : GamePhase.None;
        }

        public string GetCurrentPhaseId()
        {
            return _currentPhaseId;
        }

        public bool IsCurrentPhase(string phaseId)
        {
            return string.Equals(_currentPhaseId, phaseId, StringComparison.OrdinalIgnoreCase);
        }

        public void SubscribeToPhaseChange(Action<string, string> onPhaseChanged)
        {
            if (onPhaseChanged != null && !_phaseChangeCallbacks.Contains(onPhaseChanged))
            {
                _phaseChangeCallbacks.Add(onPhaseChanged);
                _logger.LogDebug($"Added phase change callback, total callbacks: {_phaseChangeCallbacks.Count}", "Phase");
            }
        }

        public void UnsubscribeFromPhaseChange(Action<string, string> onPhaseChanged)
        {
            if (onPhaseChanged != null && _phaseChangeCallbacks.Contains(onPhaseChanged))
            {
                _phaseChangeCallbacks.Remove(onPhaseChanged);
                _logger.LogDebug($"Removed phase change callback, remaining callbacks: {_phaseChangeCallbacks.Count}", "Phase");
            }
        }

        public string[] GetAllPhaseIds()
        {
            return PhaseToIdMap.Values.ToArray();
        }

        private void OnPhaseChangedEvent(PhaseChangedEvent evt)
        {
            string previousPhaseId = _currentPhaseId;
            _currentPhaseId = ConvertPhaseToId(evt.CurrentPhase);

            _logger.LogInfo($"Phase changed from '{previousPhaseId}' to '{_currentPhaseId}'", "Phase");

            // Виклик всіх колбеків
            foreach (var callback in new List<Action<string, string>>(_phaseChangeCallbacks))
            {
                try
                {
                    callback(previousPhaseId, _currentPhaseId);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in phase change callback: {ex.Message}", "Phase", ex);
                }
            }
        }

        // Реалізація IEventSubscriber
        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<PhaseChangedEvent>(OnPhaseChangedEvent);
            _isSubscribed = true;
            _logger.LogInfo("GamePhaseProvider subscribed to events", "Phase");
        }

        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<PhaseChangedEvent>(OnPhaseChangedEvent);
            _isSubscribed = false;
            _logger.LogInfo("GamePhaseProvider unsubscribed from events", "Phase");
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
            _phaseChangeCallbacks.Clear();
            _logger.LogInfo("GamePhaseProvider disposed", "Phase");
        }
    }
}
