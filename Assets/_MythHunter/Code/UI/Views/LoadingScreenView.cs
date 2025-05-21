// Assets/_MythHunter/Code/UI/Views/LoadingScreenView.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MythHunter.Events.Domain.Loading;
using MythHunter.UI.Core;
using MythHunter.UI.Navigation;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// Реалізація представлення екрану завантаження
    /// </summary>
    public class LoadingScreenView : NavigableViewBase, ILoadingScreenView
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private GameObject _completionPanel;

        public void UpdateProgress(float progress, string status)
        {
            if (_progressBar != null)
                _progressBar.value = progress;

            if (!string.IsNullOrEmpty(status) && _statusText != null)
                _statusText.text = status;
        }

        public void UpdateLoadingStage(LoadingStage stage, string status)
        {
            if (_stageText != null)
                _stageText.text = $"Етап: {stage.ToString()}";

            if (!string.IsNullOrEmpty(status) && _statusText != null)
                _statusText.text = status;
        }

        public void ShowError(string errorMessage)
        {
            if (_errorText != null)
            {
                _errorText.text = errorMessage;
                _errorText.gameObject.SetActive(true);
            }
        }

        public void ShowCompletionScreen()
        {
            if (_completionPanel != null)
                _completionPanel.SetActive(true);
        }

        public override async UniTask OnViewCreatedAsync(NavigationParameters parameters)
        {
            await base.OnViewCreatedAsync(parameters);

            _errorText?.gameObject.SetActive(false);
            _completionPanel?.SetActive(false);
            _progressBar?.SetValueWithoutNotify(0);
            _statusText?.SetText("Початок завантаження...");
            _stageText?.SetText("");
        }
    }
}
