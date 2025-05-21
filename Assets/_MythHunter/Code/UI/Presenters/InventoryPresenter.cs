using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.Events;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Presenters
{
    public class InventoryPresenter : BasePresenter, IInventoryPresenter
    {
        private readonly IInventoryModel _model;
        // Додаємо властивість для типізованого доступу до представлення
        private IInventoryView _typedView => _view as IInventoryView;

        [Inject]
        public InventoryPresenter(IInventoryModel model, IEventBus eventBus, IMythLogger logger)
            : base(eventBus, logger)
        {
            _model = model;

            // Встановлюємо ViewId (необхідно перевірити наявність у ViewId)
            _viewId = ViewId.Inventory;
        }

        public override async UniTask InitializeAsync()
        {
            await base.InitializeAsync();
            _logger.LogInfo("InventoryPresenter initialized", "UI");
        }

        // Метод для зворотної сумісності
        public void SetView(IInventoryView view)
        {
            Initialize(view, _viewId);
        }

        // Перевизначення методу з BasePresenter
        public override void Initialize(IView view, ViewId viewId = ViewId.None)
        {
            base.Initialize(view, viewId);
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

        // Перевизначення замість імплементації
        protected override void OnSubscribeToEvents()
        {
            // Підписка на події
        }

        protected override void OnUnsubscribeFromEvents()
        {
            // Відписка від подій
        }

        private void UpdateView()
        {
            if (_typedView != null)
            {
                _typedView.UpdateItems(_model.Items);
                _typedView.UpdateSelection(_model.SelectedItemIndex);
            }
        }
    }
}
