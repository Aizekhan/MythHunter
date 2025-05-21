using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.Events;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Presenters
{
    /// <summary>
    /// Презентер головного меню з підтримкою ViewId-архітектури
    /// </summary>
    public class MainMenuPresenter : BasePresenter, IMainMenuPresenter
    {
        private readonly IMainMenuModel _model;

        // Властивість для типізованого доступу до представлення
        private IMainMenuView _typedView => _view as IMainMenuView;

        [Inject]
        public MainMenuPresenter(IMainMenuModel model, IEventBus eventBus, IMythLogger logger)
            : base(eventBus, logger)
        {
            _model = model;

            // Встановлюємо ViewId (перевірте, що ViewId.MainMenu існує в енумерації)
            _viewId = ViewId.MainMenu;
        }

        public override async UniTask InitializeAsync()
        {
            await base.InitializeAsync();
            _logger.LogInfo("MainMenuPresenter initialized", "UI");
        }

        // Метод для зворотної сумісності
        public void SetView(IMainMenuView view)
        {
            Initialize(view, _viewId);
        }

        // Перевизначення методу з BasePresenter
        public override void Initialize(IView view, ViewId viewId = ViewId.None)
        {
            base.Initialize(view, viewId);
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

        // Перевизначення замість імплементації
        protected override void OnSubscribeToEvents()
        {
            // Підписка на необхідні події
            // Наприклад:
            // _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            // Відписка від подій
            // Наприклад:
            // _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void UpdateView()
        {
            if (_typedView != null)
            {
                _typedView.SetTitle(_model.Title);
                _typedView.SetPlayButtonEnabled(_model.IsPlayButtonEnabled);
                _typedView.SetSettingsButtonEnabled(_model.IsSettingsButtonEnabled);
                _typedView.SetExitButtonEnabled(_model.IsExitButtonEnabled);
            }
        }
    }
}
