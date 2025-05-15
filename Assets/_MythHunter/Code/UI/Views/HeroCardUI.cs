// Assets/_MythHunter/Code/UI/Views/HeroCardUI.cs
using System;
using MythHunter.UI.Models;
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

        private string _archetypeId;

        public event Action<string> OnHeroSelected;

        public void Setup(HeroCardModel model)
        {
            _archetypeId = model.ArchetypeId;

            _nameText.text = model.Name;
            _descriptionText.text = model.Description;
            _raceClassText.text = $"{model.Race} - {model.Class}";
            _manaCostText.text = $"Вартість: {model.ManaCost}";

            // Завантажуємо зображення
            if (!string.IsNullOrEmpty(model.IconPath))
            {
                Sprite icon = UnityEngine.Resources.Load<Sprite>(model.IconPath);

                if (icon != null)
                {
                    _iconImage.sprite = icon;
                }
            }

            // Налаштовуємо відображення вибраної картки
            _selectedIndicator.SetActive(model.IsSelected);

            // Налаштовуємо інтерактивність
            SetInteractable(model.IsSelectable);

            // Додаємо слухача подій кнопки
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
            // Видаляємо слухача подій
            _selectButton.onClick.RemoveListener(OnSelectButtonClicked);
        }
    }
}
