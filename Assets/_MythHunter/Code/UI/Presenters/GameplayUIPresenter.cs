// Шлях: Assets/_MythHunter/Code/UI/Presenters/GameplayUIPresenter.cs
using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;
using System;

namespace MythHunter.UI.Presenters
{
    /// <summary>
    /// Презентер для ігрового інтерфейсу
    /// </summary>
    public class GameplayUIPresenter : BasePresenter, IGameplayUIPresenter
    {
        private readonly IGameplayUIModel _model;
      

        private bool _isInitialized = false;
        protected IGameplayUIView GameplayView => base._view as IGameplayUIView;
        [Inject]
        public GameplayUIPresenter(IGameplayUIModel model, IEventBus eventBus, IMythLogger logger)
    : base(eventBus, logger)
        {
            _model = model;
            base._viewId = ViewId.GameplayUI;
        }

        /// <summary>
        /// Ініціалізує презентер асинхронно
        /// </summary>
        public override async UniTask InitializeAsync()
        {
            if (_isInitialized)
                return;

            await base.InitializeAsync();
            _isInitialized = true;
            _logger.LogInfo("GameplayUIPresenter initialized", "UI");
        }

        /// <summary>
        /// Встановлює представлення для презентера
        /// </summary>
        public void SetView(IGameplayUIView view)
        {
            base.Initialize(view, _viewId);
            UpdateView();
        }

        /// <summary>
        /// Оновлює інформацію про фазу гри
        /// </summary>
        public void UpdatePhaseInfo(int phase, float timeRemaining)
        {
            _model.CurrentPhase = phase;
            _model.PhaseTimeRemaining = timeRemaining;
            UpdateView();
        }

        /// <summary>
        /// Оновлює значення руни
        /// </summary>
        public void UpdateRuneValue(int value)
        {
            _model.RuneValue = value;
            _model.IsRunePhaseActive = true;
            UpdateView();
        }

        /// <summary>
        /// Вивільняє ресурси презентера
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _view = null;
            _logger.LogInfo("GameplayUIPresenter disposed", "UI");
        }

        /// <summary>
        /// Підписується на події фази та руни
        /// </summary>
        protected override void OnSubscribeToEvents()
        {
            _eventBus.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
            _eventBus.Subscribe<PhaseUpdateEvent>(OnPhaseUpdate);
            _eventBus.Subscribe<RuneRolledEvent>(OnRuneRolled);
        }

        /// <summary>
        /// Відписується від подій
        /// </summary>
        protected override void OnUnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<PhaseChangedEvent>(OnPhaseChanged);
            _eventBus.Unsubscribe<PhaseUpdateEvent>(OnPhaseUpdate);
            _eventBus.Unsubscribe<RuneRolledEvent>(OnRuneRolled);
        }

        /// <summary>
        /// Обробник події зміни фази
        /// </summary>
        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            _model.CurrentPhase = (int)evt.CurrentPhase;
            _model.IsRunePhaseActive = evt.CurrentPhase == GamePhase.Rune;
            UpdateView();
        }

        /// <summary>
        /// Обробник події оновлення фази
        /// </summary>
        private void OnPhaseUpdate(PhaseUpdateEvent evt)
        {
            _model.CurrentPhase = (int)evt.Phase;
            _model.PhaseTimeRemaining = evt.RemainingTime;
            UpdateView();
        }

        /// <summary>
        /// Обробник події кидання руни
        /// </summary>
        private void OnRuneRolled(RuneRolledEvent evt)
        {
            _model.RuneValue = evt.Value;
            _model.IsRunePhaseActive = true;
            UpdateView();
        }

        /// <summary>
        /// Оновлює представлення відповідно до моделі
        /// </summary>
        private void UpdateView()
        {
            if (GameplayView == null)
                return;

            GameplayView.UpdatePhaseInfo(_model.CurrentPhase, _model.PhaseTimeRemaining);

            if (_model.IsRunePhaseActive)
            {
                GameplayView.ShowRuneValue(_model.RuneValue);
            }
            else
            {
                GameplayView.HideRuneValue();
            }
        }
    }
}
