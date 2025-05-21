// Assets/_MythHunter/Code/Systems/Loading/LoadingPhaseProvider.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain.Loading;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Loading
{
    /// <summary>
    /// Провайдер стадій завантаження, який надає інформацію про поточний етап завантаження
    /// </summary>
    public class LoadingPhaseProvider : IPhaseProvider
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private string _currentPhaseId = "None";
        private readonly List<Action<string, string>> _callbacks = new List<Action<string, string>>();
        private bool _isSubscribed = false;

        [Inject]
        public LoadingPhaseProvider(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<LoadingStartedEvent>(OnLoadingStarted);
            _eventBus.Subscribe<LoadingProgressEvent>(OnLoadingProgress);
            _eventBus.Subscribe<LoadingCompletedEvent>(OnLoadingCompleted);
            _eventBus.Subscribe<LoadingErrorEvent>(OnLoadingError);

            _isSubscribed = true;
        }

        private void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<LoadingStartedEvent>(OnLoadingStarted);
            _eventBus.Unsubscribe<LoadingProgressEvent>(OnLoadingProgress);
            _eventBus.Unsubscribe<LoadingCompletedEvent>(OnLoadingCompleted);
            _eventBus.Unsubscribe<LoadingErrorEvent>(OnLoadingError);

            _isSubscribed = false;
        }

        private void OnLoadingStarted(LoadingStartedEvent evt)
        {
            UpdateCurrentPhase("LoadingStarted");
        }

        private void OnLoadingProgress(LoadingProgressEvent evt)
        {
            UpdateCurrentPhase($"Loading_{evt.Stage}");
        }

        private void OnLoadingCompleted(LoadingCompletedEvent evt)
        {
            UpdateCurrentPhase("LoadingCompleted");
        }

        private void OnLoadingError(LoadingErrorEvent evt)
        {
            UpdateCurrentPhase("LoadingError");
        }

        private void UpdateCurrentPhase(string newPhaseId)
        {
            if (_currentPhaseId == newPhaseId)
                return;

            string oldPhaseId = _currentPhaseId;
            _currentPhaseId = newPhaseId;

            // Сповіщаємо про зміну фази
            foreach (var callback in _callbacks.ToArray())
            {
                try
                {
                    callback.Invoke(oldPhaseId, newPhaseId);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка в обробнику зміни фази: {ex.Message}", "LoadingPhase", ex);
                }
            }
        }

        public string GetCurrentPhaseId() => _currentPhaseId;

        public bool IsCurrentPhase(string phaseId) => _currentPhaseId == phaseId;

        public void SubscribeToPhaseChange(Action<string, string> onPhaseChanged)
        {
            if (!_callbacks.Contains(onPhaseChanged))
                _callbacks.Add(onPhaseChanged);
        }

        public void UnsubscribeFromPhaseChange(Action<string, string> onPhaseChanged)
        {
            _callbacks.Remove(onPhaseChanged);
        }

        public string[] GetAllPhaseIds()
        {
            return new string[]
            {
                "None",
                "LoadingStarted",
                "Loading_PreparingResources",
                "Loading_LoadingHeroPrefabs",
                "Loading_LoadingMapData",
                "Loading_InitializingPools",
                "Loading_CreatingEntities",
                "Loading_SettingUpSystems",
                "Loading_FinalSetup",
                "LoadingCompleted",
                "LoadingError"
            };
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
            _callbacks.Clear();
        }
    }
}
