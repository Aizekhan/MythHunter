// Шлях: Assets/_MythHunter/Code/UI/Views/HeroCardUI.cs
using System;
using MythHunter.UI.Models;
using MythHunter.Utils.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// UI компонент картки героя
    /// </summary>
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

        public event Action<string> OnHeroSelected;
        private IMythLogger _logger;
        // В класі HeroCardUI.cs
        public void Setup(HeroCardModel model)
        {
            _archetypeId = model.ArchetypeId;

            _nameText.text = string.IsNullOrEmpty(model.Name) ? model.ArchetypeId : model.Name;
            _descriptionText.text = model.Description;
            _raceClassText.text = $"{model.Race} - {model.Class}";
            _manaCostText.text = $"Вартість: {model.ManaCost}";

            // Завантаження іконки або встановлення за замовчуванням
            Sprite icon = null;

            if (!string.IsNullOrEmpty(model.IconPath))
            {
                icon = UnityEngine.Resources.Load<Sprite>(model.IconPath);
                _logger.LogInfo($"Спроба завантажити іконку за шляхом: {model.IconPath}, результат: {(icon != null ? "успішно" : "невдача")}", "HeroCardUI");
            }

            if (icon == null)
            {
                // Завантаження іконки за замовчуванням
                if (_defaultIcon == null)
                {
                    _defaultIcon = UnityEngine.Resources.Load<Sprite>("UI/Icons/default_hero");
                    _logger.LogInfo($"Завантаження іконки за замовчуванням: {(_defaultIcon != null ? "успішно" : "невдача")}", "HeroCardUI");
                }
                icon = _defaultIcon;
            }

            _iconImage.sprite = icon;
            _selectedIndicator.SetActive(model.IsSelected);
            SetInteractable(model.IsSelectable);
            _selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        public void SetInteractable(bool interactable)
        {
            _selectButton.interactable = interactable;
        }

        private void OnSelectButtonClicked()
        {
            OnHeroSelected?.Invoke(_archetypeId);
        }

        private void OnDestroy()
        {
            _selectButton.onClick.RemoveListener(OnSelectButtonClicked);
        }
    }
}
