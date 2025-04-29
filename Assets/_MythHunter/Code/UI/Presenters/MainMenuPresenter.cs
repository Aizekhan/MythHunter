using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.Events;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Presenters
{
    public class MainMenuPresenter : IMainMenuPresenter, IEventSubscriber
    {
        private readonly IMainMenuModel _model;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private IMainMenuView _view;

        [Inject]
        public MainMenuPresenter(IMainMenuModel model, IEventBus eventBus, IMythLogger logger)
        {
            _model = model;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async UniTask InitializeAsync()
        {
            SubscribeToEvents();
            _logger.LogInfo("MainMenuPresenter initialized", "UI");
            await UniTask.CompletedTask;
        }

        public void SetView(IMainMenuView view)
        {
            _view = view;
            UpdateView();
        }

        public void OnPlayClicked()
        {
            _logger.LogInfo("Play button clicked", "UI");
            // TODO: Publish event to start the game
        }

        public void OnSettingsClicked()
        {
            _logger.LogInfo("Settings button clicked", "UI");
            // TODO: Show settings
        }

        public void OnExitClicked()
        {
            _logger.LogInfo("Exit button clicked", "UI");
            // TODO: Exit game logic
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("MainMenuPresenter disposed", "UI");
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
                _view.SetTitle(_model.Title);
                _view.SetPlayButtonEnabled(_model.IsPlayButtonEnabled);
                _view.SetSettingsButtonEnabled(_model.IsSettingsButtonEnabled);
                _view.SetExitButtonEnabled(_model.IsExitButtonEnabled);
            }
        }
    }
}
