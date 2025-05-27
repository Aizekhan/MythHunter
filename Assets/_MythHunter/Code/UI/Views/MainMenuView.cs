// Assets/_MythHunter/Code/UI/Views/MainMenuView.cs
using UnityEngine;
using UnityEngine.UI;
using MythHunter.UI.Core;
using MythHunter.UI.Presenters;
using MythHunter.Core.DI;
using MythHunter.Core.MonoBehaviours;

namespace MythHunter.UI.Views
{
    public class MainMenuView : UIViewBase, IMainMenuView
    {
        [Header("Title")]
        [SerializeField] private TMPro.TextMeshProUGUI _titleText;

        [Header("Game Mode Buttons")]
        [SerializeField] private Button _onlinePvPButton;
        [SerializeField] private Button _localPvPButton;
        [SerializeField] private Button _pvAIButton;

        [Header("Other Buttons")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

    

        [Header("Profile Button")]
        [SerializeField] private Button _profileButton;

        [Inject] private IMainMenuPresenter _presenter;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ConnectToPresenter();
            SetupButtons();
        }

        private void ConnectToPresenter()
        {
            if (_presenter == null)
            {
                var container = FindFirstObjectByType<LazyDependencyInjector>()?.GetComponent<IDIContainer>();
                _presenter = container?.Resolve<IMainMenuPresenter>();
            }
        }

        private void SetupButtons()
        {
            // ✅ НОВІ кнопки режимів
            _onlinePvPButton?.onClick.AddListener(() => _presenter.OnOnlinePvPClicked());
            _localPvPButton?.onClick.AddListener(() => _presenter.OnLocalPvPClicked());
            _pvAIButton?.onClick.AddListener(() => _presenter.OnPvAIClicked());

            // ✅ НОВА кнопка профілю
            _profileButton?.onClick.AddListener(() => _presenter.OnProfileClicked());
            // ✅ ІНШІ кнопки
            _settingsButton?.onClick.AddListener(() => _presenter.OnSettingsClicked());
            _exitButton?.onClick.AddListener(() => _presenter.OnExitClicked());

        }

        // ✅ Реалізація IMainMenuView
        public void SetTitle(string title)
        {
            if (_titleText != null)
                _titleText.text = title;
        }

        public void SetOnlinePvPButtonEnabled(bool enabled)
        {
            if (_onlinePvPButton != null)
                _onlinePvPButton.interactable = enabled;
        }

        public void SetLocalPvPButtonEnabled(bool enabled)
        {
            if (_localPvPButton != null)
                _localPvPButton.interactable = enabled;
        }

        public void SetPvAIButtonEnabled(bool enabled)
        {
            if (_pvAIButton != null)
                _pvAIButton.interactable = enabled;
        }

        // Legacy методи
      

        public void SetSettingsButtonEnabled(bool enabled)
        {
            if (_settingsButton != null)
                _settingsButton.interactable = enabled;
        }
        // ✅ ДОДАЙТЕ НОВИЙ МЕТОД:
        public void SetProfileButtonEnabled(bool enabled)
        {
            if (_profileButton != null)
                _profileButton.interactable = enabled;
        }
        public void SetExitButtonEnabled(bool enabled)
        {
            if (_exitButton != null)
                _exitButton.interactable = enabled;
        }

        protected override void OnDestroy()
        {
            // Очищуємо listeners
            _onlinePvPButton?.onClick.RemoveAllListeners();
            _localPvPButton?.onClick.RemoveAllListeners();
            _pvAIButton?.onClick.RemoveAllListeners();
            _settingsButton?.onClick.RemoveAllListeners();
            _exitButton?.onClick.RemoveAllListeners();
          
            _profileButton?.onClick.RemoveAllListeners(); // ✅ ДОДАЙТЕ ЦЕ

            base.OnDestroy();
        }
    }
}
