// Assets/_MythHunter/Code/UI/Views/HeroCardUI.cs

using System;
using MythHunter.UI.Models;
using MythHunter.Utils.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MythHunter.Core.DI;
using MythHunter.UI.Services;
using Cysharp.Threading.Tasks;
using MythHunter.Core.MonoBehaviours;

namespace MythHunter.UI.Views
{
    public class HeroCardUI : LazyMonoBehaviour, IHeroCardUI
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _raceClassText;
        [SerializeField] private TextMeshProUGUI _manaCostText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _selectButton;
        [SerializeField] private GameObject _selectedIndicator;
        [SerializeField] private Sprite _defaultIcon;

        private string _archetypeId;

        [Inject] private IMythLogger _logger;
        [Inject] private ISpriteService _spriteService;

        public string ArchetypeId => _archetypeId;
        public event Action<string> OnHeroSelected;

        // Метод, який викликається після ін'єкції залежностей
        protected override void OnInitialized()
        {
            _logger.LogInfo("HeroCardUI ініціалізовано через LazyMonoBehaviour", "HeroCardUI");

            // Додаємо базову ініціалізацію кнопки
            if (_selectButton)
            {
                _selectButton.onClick.AddListener(OnSelectButtonClicked);
            }
        }

        public void Setup(HeroCardModel model)
        {
            _logger?.LogInfo($"🎴 Setup card: {model.Name}, IconPath: {model.IconPath}", "HeroCardUI");
            _logger?.LogInfo($"🎴 SpriteService is null: {_spriteService == null}", "HeroCardUI");
            if (model == null)
            {
                if (_logger != null)
                    _logger.LogError("Setup викликано з null моделлю", "HeroCardUI");
                return;
            }

            _archetypeId = model.ArchetypeId;

            // Встановлюємо текстові поля
            if (_nameText)
                _nameText.text = string.IsNullOrEmpty(model.Name) ? model.ArchetypeId : model.Name;
            if (_descriptionText)
                _descriptionText.text = model.Description;
            if (_raceClassText)
                _raceClassText.text = $"{model.Race} - {model.Class}";
            if (_manaCostText)
                _manaCostText.text = $"Вартість: {model.ManaCost}";

            // Асинхронне завантаження іконки
            if (!string.IsNullOrEmpty(model.IconPath) && _spriteService != null)
            {
                LoadIconAsync(model.IconPath).Forget();
            }
            else
            {
                if (_iconImage)
                    _iconImage.sprite = _defaultIcon;
            }

            if (_selectedIndicator)
                _selectedIndicator.SetActive(model.IsSelected);
            SetInteractable(model.IsSelectable);

            // Налаштування кнопки
            if (_selectButton)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(OnSelectButtonClicked);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (_selectButton != null)
                _selectButton.interactable = interactable;
        }

        public void Reset()
        {
            // Відписуємось від усіх евентів
            OnHeroSelected = null;

            // Скидаємо всі поля до початкових значень
            _archetypeId = string.Empty;

            // Скидаємо UI-елементи
            if (_nameText)
                _nameText.text = string.Empty;
            if (_descriptionText)
                _descriptionText.text = string.Empty;
            if (_raceClassText)
                _raceClassText.text = string.Empty;
            if (_manaCostText)
                _manaCostText.text = string.Empty;
            if (_iconImage && _defaultIcon)
                _iconImage.sprite = _defaultIcon;
            if (_selectedIndicator)
                _selectedIndicator.SetActive(false);

            // Скидаємо слухачі кнопки
            if (_selectButton)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(OnSelectButtonClicked);
            }

            if (_logger != null)
                _logger.LogInfo("HeroCardUI скинуто", "HeroCardUI");
        }

        private async UniTaskVoid LoadIconAsync(string iconPath)
        {
            _logger?.LogInfo($"🖼️ LoadIconAsync викликано для: {iconPath}", "HeroCardUI");

            if (_spriteService == null)
            {
                _logger?.LogError($"❌ SpriteService is null в LoadIconAsync!", "HeroCardUI");
                return;
            }

            var sprite = await _spriteService.GetSpriteAsync(iconPath, _defaultIcon);
            if (this != null && _iconImage != null)
            {
                _iconImage.sprite = sprite;
                _logger?.LogInfo($"✅ Іконка встановлена для: {iconPath}", "HeroCardUI");
            }
            else
            {
                _logger?.LogWarning($"⚠️ Об'єкт або _iconImage знищено під час завантаження: {iconPath}", "HeroCardUI");
            }
        }

        private void OnSelectButtonClicked()
        {
            OnHeroSelected?.Invoke(_archetypeId);
        }

        // Використовуємо стандартний метод Unity OnDestroy замість override
        // LazyMonoBehaviour, ймовірно, не перевизначає цей метод
        private void OnDestroy()
        {
            if (_selectButton)
                _selectButton.onClick.RemoveListener(OnSelectButtonClicked);

            if (_logger != null)
                _logger.LogInfo("HeroCardUI знищено", "HeroCardUI");
        }
        /// <summary>
        /// Налаштування картки з явною передачею SpriteService
        /// </summary>
        public void SetupWithSpriteService(HeroCardModel model, ISpriteService spriteService)
        {
            if (model == null)
            {
                if (_logger != null)
                    _logger.LogError("Setup викликано з null моделлю", "HeroCardUI");
                return;
            }

            // ✅ Явно встановлюємо SpriteService
            _spriteService = spriteService;

            _archetypeId = model.ArchetypeId;

            // Встановлюємо текстові поля
            if (_nameText)
                _nameText.text = string.IsNullOrEmpty(model.Name) ? model.ArchetypeId : model.Name;
            if (_descriptionText)
                _descriptionText.text = model.Description;
            if (_raceClassText)
                _raceClassText.text = $"{model.Race} - {model.Class}";
            if (_manaCostText)
                _manaCostText.text = $"Вартість: {model.ManaCost}";

            // ✅ Логування для дебагу
            _logger?.LogInfo($"🎴 SetupWithSpriteService: {model.Name}, IconPath: {model.IconPath}, SpriteService: {_spriteService != null}", "HeroCardUI");

            // Асинхронне завантаження іконки
            if (!string.IsNullOrEmpty(model.IconPath) && _spriteService != null)
            {
                LoadIconAsync(model.IconPath).Forget();
            }
            else
            {
                if (_iconImage)
                    _iconImage.sprite = _defaultIcon;

                if (string.IsNullOrEmpty(model.IconPath))
                    _logger?.LogWarning($"⚠️ IconPath порожній для {model.Name}", "HeroCardUI");
                if (_spriteService == null)
                    _logger?.LogWarning($"⚠️ SpriteService все ще null для {model.Name}", "HeroCardUI");
            }

            if (_selectedIndicator)
                _selectedIndicator.SetActive(model.IsSelected);
            SetInteractable(model.IsSelectable);

            // Налаштування кнопки
            if (_selectButton)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(OnSelectButtonClicked);
            }
        }

      
    }
}
