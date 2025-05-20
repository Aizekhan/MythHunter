// Assets/_MythHunter/Code/UI/Views/LoadingView.cs
using MythHunter.UI.Core;
using MythHunter.UI.Navigation;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// Представлення екрану завантаження
    /// </summary>
    public class LoadingView : NavigableViewBase, ILoadingView
    {
        [Header("UI елементи")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private GameObject _tipContainer;
        [SerializeField] private TextMeshProUGUI _tipText;
        [SerializeField] private Image _backgroundImage;

        [Header("Налаштування")]
        [SerializeField] private float _minLoadTime = 1.5f;
        [SerializeField] private string[] _loadingTips;

        // Імплементація ILoadingView
        public void UpdateProgress(float progress, string status = null)
        {
            if (_progressBar != null)
                _progressBar.value = progress;

            if (!string.IsNullOrEmpty(status) && _statusText != null)
                _statusText.text = status;
        }

        public void SetTitle(string title)
        {
            if (_titleText != null)
                _titleText.text = title;
        }

        public void ShowRandomTip()
        {
            if (_tipContainer != null && _tipText != null && _loadingTips.Length > 0)
            {
                _tipContainer.SetActive(true);
                _tipText.text = _loadingTips[Random.Range(0, _loadingTips.Length)];
            }
        }

        // Імплементація INavigableView
        public override async UniTask OnViewCreatedAsync(NavigationParameters parameters)
        {
            await base.OnViewCreatedAsync(parameters);

            // Початкове налаштування
            UpdateProgress(0, "Підготовка...");
            ShowRandomTip();

            // Отримання параметра "Title" з навігаційних параметрів
            if (parameters.Contains("Title"))
                SetTitle(parameters.GetValue<string>("Title"));
            else
                SetTitle("Завантаження...");
        }

        public override async UniTask OnViewNavigatedToAsync(NavigationParameters parameters)
        {
            await base.OnViewNavigatedToAsync(parameters);

            // Отримання та обробка параметрів
            string targetScene = parameters.GetValue<string>("TargetScene", string.Empty);
            if (!string.IsNullOrEmpty(targetScene))
            {
                UpdateProgress(0, $"Підготовка до завантаження '{targetScene}'...");
            }
        }
    }
}
