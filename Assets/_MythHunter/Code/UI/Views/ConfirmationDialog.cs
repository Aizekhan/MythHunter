// Шлях: Assets/_MythHunter/Code/UI/Views/ConfirmationDialog.cs
using MythHunter.UI.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Views
{
    public class ConfirmationDialog : ModalViewBase<bool>
    {
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        public override async UniTask InitializeAsync(NavigationParameters parameters)
        {
            string message = parameters.GetValue<string>("Message", "Ви впевнені?");
            if (_messageText != null)
                _messageText.text = message;
            await UniTask.CompletedTask;
        }

        private void OnConfirmClicked()
        {
            Complete(true);
        }

        private void OnCancelClicked()
        {
            Complete(false);
        }

        private new void OnDestroy()

        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            _cancelButton.onClick.RemoveListener(OnCancelClicked);
        }
    }
}
