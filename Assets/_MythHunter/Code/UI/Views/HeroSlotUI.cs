// Assets/_MythHunter/Code/UI/Views/HeroSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MythHunter.UI.Models;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Core.DI;
using MythHunter.UI.Services;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// UI компонент для слота вибраного героя (P1/P2 слоти)
    /// </summary>
    public class HeroSlotUI : LazyMonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _heroIconImage;
        [SerializeField] private TextMeshProUGUI _heroNameText;
        [SerializeField] private GameObject _emptySlotIndicator;
        [SerializeField] private Button _slotButton;

        [Header("Visual States")]
        [SerializeField] private Color _emptySlotColor = Color.gray;
        [SerializeField] private Color _filledSlotColor = Color.white;
        [SerializeField] private Sprite _defaultIcon;

        [Inject] private ISpriteService _spriteService;

        // State
        private HeroCardModel _currentHero;
        private bool _isEmpty = true;

        // Events
        public System.Action<HeroSlotUI> OnSlotClicked;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SetupUI();
            SetEmptyState();
        }

        private void SetupUI()
        {
            // Створюємо базові UI елементи, якщо вони не призначені
            if (_backgroundImage == null)
            {
                _backgroundImage = gameObject.AddComponent<Image>();
            }

            if (_slotButton == null)
            {
                _slotButton = gameObject.AddComponent<Button>();
                _slotButton.onClick.AddListener(OnSlotButtonClicked);
            }

            // Налаштовуємо RectTransform
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = gameObject.AddComponent<RectTransform>();
        }

        /// <summary>
        /// Встановлює героя в слот
        /// </summary>
        public void SetHero(HeroCardModel heroModel)
        {
            _currentHero = heroModel;
            _isEmpty = false;

            UpdateVisualState();
            LoadHeroIcon();
        }

        /// <summary>
        /// Очищає слот
        /// </summary>
        public void ClearHero()
        {
            _currentHero = null;
            _isEmpty = true;
            SetEmptyState();
        }

        /// <summary>
        /// Отримує поточного героя
        /// </summary>
        public HeroCardModel GetHero()
        {
            return _currentHero;
        }

        /// <summary>
        /// Перевіряє чи слот порожній
        /// </summary>
        public bool IsEmpty => _isEmpty;

        private void SetEmptyState()
        {
            if (_heroIconImage != null)
                _heroIconImage.gameObject.SetActive(false);

            if (_heroNameText != null)
                _heroNameText.gameObject.SetActive(false);

            if (_emptySlotIndicator != null)
                _emptySlotIndicator.SetActive(true);

            if (_backgroundImage != null)
                _backgroundImage.color = _emptySlotColor;
        }

        private void UpdateVisualState()
        {
            if (_isEmpty)
            {
                SetEmptyState();
                return;
            }

            // Показуємо елементи героя
            if (_heroIconImage != null)
                _heroIconImage.gameObject.SetActive(true);

            if (_heroNameText != null)
            {
                _heroNameText.gameObject.SetActive(true);
                _heroNameText.text = _currentHero.Name;
            }

            if (_emptySlotIndicator != null)
                _emptySlotIndicator.SetActive(false);

            if (_backgroundImage != null)
                _backgroundImage.color = _filledSlotColor;
        }

        private async UniTaskVoid LoadHeroIcon()
        {
            if (_currentHero == null || string.IsNullOrEmpty(_currentHero.IconPath))
            {
                SetDefaultIcon();
                return;
            }

            try
            {
                var sprite = await _spriteService.GetSpriteAsync(_currentHero.IconPath, _defaultIcon);
                if (this != null && _heroIconImage != null)
                {
                    _heroIconImage.sprite = sprite;
                }
            }
            catch (System.Exception)
            {
                SetDefaultIcon();
            }
        }

        private void SetDefaultIcon()
        {
            if (_heroIconImage && _defaultIcon)
                _heroIconImage.sprite = _defaultIcon;
        }

        private void OnSlotButtonClicked()
        {
            OnSlotClicked?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (_slotButton)
                _slotButton.onClick.RemoveListener(OnSlotButtonClicked);
        }
    }
}
