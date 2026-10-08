using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.MonoBehaviours;
using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Services;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroCardUI : LazyMonoBehaviour, IHeroCardUI, IView
{
    [Header("Main Hero Info")]
    [SerializeField] private Image _heroIconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _raceText;

    [Header("Level and Mana")]
    [SerializeField] private TextMeshProUGUI _levelText; // В кружечку
    [SerializeField] private TextMeshProUGUI _manaCostText; // Вартість в мані
    [SerializeField] private GameObject _levelCircle; // Фон для рівня

    [Header("Class Indicators")]
    [SerializeField] private GameObject _archerIcon; // Значок лука для лучників
    [SerializeField] private GameObject _mageIcon; // Значок для магів
    [SerializeField] private GameObject _warriorIcon; // Значок для воїнів

    [Header("Selection State")]
    [SerializeField] private Button _selectButton;
    [SerializeField] private GameObject _selectedIndicator;
    [SerializeField] private Image _cardBackground;

    [Header("Visual States")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = Color.yellow;
    [SerializeField] private Color _disabledColor = Color.gray;

    [Header("Fallbacks")]
    [SerializeField] private Sprite _defaultIcon;

    private string _archetypeId;

    [Inject] private IMythLogger _logger;
    [Inject] private ISpriteService _spriteService;

    public string ArchetypeId => _archetypeId;
    public event Action<string> OnHeroSelected;

    // ✅ Реалізація IView
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    protected override void OnInitialized()
    {
        base.OnInitialized();
        SetupButton();
    }

    private void SetupButton()
    {
        if (_selectButton)
        {
            _selectButton.onClick.AddListener(OnSelectButtonClicked);
        }
    }

    /// <summary>
    /// Налаштовує картку героя з новими даними
    /// </summary>
    public void Setup(HeroCardModel model)
    {
        if (model == null)
        {
            _logger?.LogError("Setup викликано з null моделлю", "HeroCardUI");
            return;
        }

        _archetypeId = model.ArchetypeId;

        // Основна інформація
        SetHeroName(model.Name);
        SetHeroRace(model.Race);
        SetHeroLevel(model.Level);
        SetManaCost(model.ManaCost);

        // Класові індикатори
        SetClassIndicators(model.Class);

        // Стан картки
        SetSelectionState(model.IsSelected, model.IsSelectable);

        // Завантаження іконки
        LoadHeroIcon(model.IconPath);

        _logger?.LogDebug($"Hero card setup: {model.Name} (Level {model.Level}, Cost {model.ManaCost})", "HeroCardUI");
    }

    private void SetHeroName(string heroName)
    {
        if (_nameText)
            _nameText.text = string.IsNullOrEmpty(heroName) ? _archetypeId : heroName;
    }

    private void SetHeroRace(string race)
    {
        if (_raceText)
            _raceText.text = race;
    }

    private void SetHeroLevel(int level)
    {
        if (_levelText)
            _levelText.text = level.ToString();
    }

    private void SetManaCost(int manaCost)
    {
        if (_manaCostText)
            _manaCostText.text = manaCost.ToString();
    }

    private void SetClassIndicators(string heroClass)
    {
        // Ховаємо всі індикатори
        _archerIcon?.SetActive(false);
        _mageIcon?.SetActive(false);
        _warriorIcon?.SetActive(false);

        // Показуємо потрібний
        switch (heroClass.ToLower())
        {
            case "archer":
            case "ranger":
                _archerIcon?.SetActive(true);
                break;
            case "mage":
            case "wizard":
                _mageIcon?.SetActive(true);
                break;
            case "warrior":
            case "tank":
                _warriorIcon?.SetActive(true);
                break;
        }
    }

    private void SetSelectionState(bool isSelected, bool isSelectable)
    {
        // Індикатор вибраності
        if (_selectedIndicator)
            _selectedIndicator.SetActive(isSelected);

        // Інтерактивність кнопки
        if (_selectButton)
            _selectButton.interactable = isSelectable && !isSelected;

        // Колір фону
        if (_cardBackground)
        {
            if (isSelected)
                _cardBackground.color = _selectedColor;
            else if (!isSelectable)
                _cardBackground.color = _disabledColor;
            else
                _cardBackground.color = _normalColor;
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (_selectButton)
            _selectButton.interactable = interactable;
    }

    private async UniTaskVoid LoadHeroIcon(string iconPath)
    {
        if (string.IsNullOrEmpty(iconPath) || _spriteService == null)
        {
            SetDefaultIcon();
            return;
        }

        try
        {
            var sprite = await _spriteService.GetSpriteAsync(iconPath, _defaultIcon);
            if (this != null && _heroIconImage != null)
            {
                _heroIconImage.sprite = sprite;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Помилка завантаження іконки {iconPath}: {ex.Message}", "HeroCardUI");
            SetDefaultIcon();
        }
    }

    private void SetDefaultIcon()
    {
        if (_heroIconImage && _defaultIcon)
            _heroIconImage.sprite = _defaultIcon;
    }

    public void Reset()
    {
        OnHeroSelected = null;
        _archetypeId = string.Empty;

        // Скидаємо UI елементи
        SetHeroName("");
        SetHeroRace("");
        SetHeroLevel(1);
        SetManaCost(1);
        SetClassIndicators("");
        SetSelectionState(false, true);
        SetDefaultIcon();
    }

    private void OnSelectButtonClicked()
    {
        OnHeroSelected?.Invoke(_archetypeId);
    }

    private void OnDestroy()
    {
        if (_selectButton)
            _selectButton.onClick.RemoveListener(OnSelectButtonClicked);
    }
}
