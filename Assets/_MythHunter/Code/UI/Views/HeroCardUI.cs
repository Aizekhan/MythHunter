// Оновити Assets/_MythHunter/Code/UI/Views/HeroCardUI.cs

using System;
using MythHunter.UI.Models;
using MythHunter.Utils.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MythHunter.Core.DI;
using MythHunter.UI.Services;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Views
{
    public class HeroCardUI : MonoBehaviour
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
        public string ArchetypeId => _archetypeId;
        private IMythLogger _logger;
        private ISpriteService _spriteService;

        public event Action<string> OnHeroSelected;

        [Inject]
        public void Construct(IMythLogger logger, ISpriteService spriteService)
        {
            _logger = logger;
            _spriteService = spriteService;
            _logger.LogInfo("HeroCardUI initialized", "UI");
        }

        public void Setup(HeroCardModel model)
        {
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
                if (_logger != null)
                {
                    _logger.LogWarning($"Invalid icon path or sprite service for hero: {model.Name}", "HeroCardUI");
                }
            }

            if (_selectedIndicator)
                _selectedIndicator.SetActive(model.IsSelected);
            SetInteractable(model.IsSelectable);

            // Видаляємо старий обробник перед додаванням нового
            if (_selectButton)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(OnSelectButtonClicked);
            }
        }

        private async UniTaskVoid LoadIconAsync(string iconPath)
        {
            var sprite = await _spriteService.GetSpriteAsync(iconPath, _defaultIcon);
            if (this != null && _iconImage != null) // Перевірка на випадок, якщо об'єкт був знищений
            {
                _iconImage.sprite = sprite;
            }
        }

        public void SetInteractable(bool interactable)
        {
            _selectButton.interactable = interactable;
        }

        private void OnSelectButtonClicked()
        {
            if (_logger != null)
            {
                _logger.LogInfo($"Вибрано героя: {_archetypeId}", "HeroCardUI");
            }

            OnHeroSelected?.Invoke(_archetypeId);
        }

        private void OnDestroy()
        {
            _selectButton.onClick.RemoveListener(OnSelectButtonClicked);

            if (_logger != null)
            {
                _logger.LogInfo("HeroCardUI знищено", "HeroCardUI");
            }
        }
    }
}
