using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.Events;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Presenters
{
    public class GameplayUIPresenter : IGameplayUIPresenter, IEventSubscriber
    {
        private readonly IGameplayUIModel _model;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private IGameplayUIView _view;

        [Inject]
        public GameplayUIPresenter(IGameplayUIModel model, IEventBus eventBus, IMythLogger logger)
        {
            _model = model;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async UniTask InitializeAsync()
        {
            SubscribeToEvents();
            _logger.LogInfo("GameplayUIPresenter initialized", "UI");
            await UniTask.CompletedTask;
        }

        public void SetView(IGameplayUIView view)
        {
            _view = view;
            UpdateView();
        }

        public void UpdatePhaseInfo(int phase, float timeRemaining)
        {
            _model.CurrentPhase = phase;
            _model.PhaseTimeRemaining = timeRemaining;
            UpdateView();
        }

        public void UpdateRuneValue(int value)
        {
            _model.RuneValue = value;
            _model.IsRunePhaseActive = true;
            UpdateView();
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("GameplayUIPresenter disposed", "UI");
        }

        public void SubscribeToEvents()
        {
            // Subscribe to necessary events
        }

        public void UnsubscribeFromEvents()
        {
            // Unsubscribe from events
        }

        private void UpdateView()
        {
            if (_view != null)
            {
                _view.UpdatePhaseInfo(_model.CurrentPhase, _model.PhaseTimeRemaining);
                if (_model.IsRunePhaseActive)
                {
                    _view.ShowRuneValue(_model.RuneValue);
                }
                else
                {
                    _view.HideRuneValue();
                }
            }
        }
    }
}
