using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.Events;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Presenters
{
    public class InventoryPresenter : IInventoryPresenter, IEventSubscriber
    {
        private readonly IInventoryModel _model;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private IInventoryView _view;

        [Inject]
        public InventoryPresenter(IInventoryModel model, IEventBus eventBus, IMythLogger logger)
        {
            _model = model;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async UniTask InitializeAsync()
        {
            SubscribeToEvents();
            _logger.LogInfo("InventoryPresenter initialized", "UI");
            await UniTask.CompletedTask;
        }

        public void SetView(IInventoryView view)
        {
            _view = view;
            UpdateView();
        }

        public void SelectItem(int index)
        {
            if (index >= 0 && index < _model.Items.Count)
            {
                _model.SelectedItemIndex = index;
                UpdateView();
            }
        }

        public void UseSelectedItem()
        {
            if (_model.SelectedItemIndex >= 0)
            {
                _model.UseItem(_model.SelectedItemIndex);
                UpdateView();
            }
        }

        public void DropSelectedItem()
        {
            if (_model.SelectedItemIndex >= 0)
            {
                _model.RemoveItem(_model.SelectedItemIndex);
                UpdateView();
            }
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("InventoryPresenter disposed", "UI");
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
                _view.UpdateItems(_model.Items);
                _view.UpdateSelection(_model.SelectedItemIndex);
            }
        }
    }
}
