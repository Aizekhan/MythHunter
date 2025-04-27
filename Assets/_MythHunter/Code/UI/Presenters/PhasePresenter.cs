// Файл: Assets/_MythHunter/Code/UI/Presenters/PhasePresenter.cs

using Cysharp.Threading.Tasks;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.UI.Core;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;

namespace MythHunter.UI.Presenters
{
    /// <summary>
    /// Презентер для фаз гри
    /// </summary>
    public class PhasePresenter : IPresenter, IEventSubscriber
    {
        private readonly PhaseView _view;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        public PhasePresenter(PhaseView view, IEventBus eventBus, IMythLogger logger)
        {
            _view = view;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async UniTask InitializeAsync()
        {
            // Підписка на події View
            _view.OnNextPhaseRequested += HandleNextPhaseRequested;

            // Підписка на системні події
            SubscribeToEvents();

            // Показ View
            _view.Show();

            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            // Відписка від подій View
            _view.OnNextPhaseRequested -= HandleNextPhaseRequested;

            // Відписка від системних подій
            UnsubscribeFromEvents();

            // Приховати View
            _view.Hide();
        }

        private void HandleNextPhaseRequested()
        {
            _eventBus.Publish(new RequestNextPhaseEvent
            {
                CurrentPhase = GamePhase.None, // Системи обробить запит правильно
                Timestamp = System.DateTime.UtcNow
            });
        }

        public void SubscribeToEvents()
        {
            _eventBus.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
            _eventBus.Subscribe<PhaseStartedEvent>(OnPhaseStarted);
            _eventBus.Subscribe<PhaseTimerUpdatedEvent>(OnPhaseTimerUpdated);
        }

        public void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<PhaseChangedEvent>(OnPhaseChanged);
            _eventBus.Unsubscribe<PhaseStartedEvent>(OnPhaseStarted);
            _eventBus.Unsubscribe<PhaseTimerUpdatedEvent>(OnPhaseTimerUpdated);
        }

        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            _logger.LogDebug($"Phase changed from {evt.PreviousPhase} to {evt.CurrentPhase}");
        }

        private void OnPhaseStarted(PhaseStartedEvent evt)
        {
            _view.SetPhase(evt.Phase, evt.Duration);
            _logger.LogDebug($"Phase started: {evt.Phase}, Duration: {evt.Duration}");
        }

        private void OnPhaseTimerUpdated(PhaseTimerUpdatedEvent evt)
        {
            _view.UpdateTimer(evt.RemainingTime);
        }
    }
}
