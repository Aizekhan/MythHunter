// Assets/_MythHunter/Code/UI/Views/GameplayUIView.cs
using MythHunter.UI.Core;
using MythHunter.UI.Navigation;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// Оптимізоване представлення ігрового інтерфейсу для 2 гравців без скролінгу
    /// </summary>
    public class GameplayUIView : NavigableViewBase, IGameplayUIView
    {
        [Header("🔴 Top Panel - Фаза та Хід")]
        [SerializeField] private TextMeshProUGUI _phaseInfoText;         // "Фаза: Планування"
        [SerializeField] private TextMeshProUGUI _timerText;             // "Час: 25с"
        [SerializeField] private TextMeshProUGUI _turnIndicatorText;     // "Хід: Олександр"
        [SerializeField] private Image _currentPlayerIndicator;          // Кольоровий кружок гравця

        [Header("🎲 Center Area - Руна")]
        [SerializeField] private GameObject _runeValueContainer;         // Контейнер руни
        [SerializeField] private TextMeshProUGUI _runeValueText;         // "Руна: 4"

        [Header("⚡ Bottom Panel - Очки Дій")]
        [SerializeField] private TextMeshProUGUI _actionPointsText;      // "Очки дій: 2/3"
        [SerializeField] private Button _endTurnButton;                  // Кнопка завершення ходу

        [Header("🎮 Right Panel - Дії")]
        [SerializeField] private TextMeshProUGUI _actionsTitle;          // "Доступні дії:"
        [SerializeField] private Transform _availableActionsContainer;   // Контейнер кнопок дій
        [SerializeField] private GameObject _actionButtonPrefab;         // Префаб кнопки дії

        [Header("📊 Right Panel - Додаткові панелі")]
        [SerializeField] private GameObject _inventoryPanel;             // Панель інвентаря
        [SerializeField] private GameObject _heroStatsPanel;            // Панель статистик героя
        [SerializeField] private GameObject _objectivesPanel;           // Панель цілей

        [Header("⏳ Overlay - Статус")]
        [SerializeField] private CanvasGroup _statusOverlay;             // Загальний оверлей
        [SerializeField] private GameObject _waitingPanel;               // Панель очікування
        [SerializeField] private TextMeshProUGUI _waitingText;           // "Очікування іншого гравця..."
        [SerializeField] private GameObject _gameOverPanel;              // Панель завершення гри
        [SerializeField] private TextMeshProUGUI _gameOverText;          // "ПЕРЕМОГА!" / "ПОРАЗКА!"

        [Header("🎨 Налаштування")]
        [SerializeField] private Color _player1Color = Color.blue;       // Колір 1-го гравця
        [SerializeField] private Color _player2Color = Color.red;        // Колір 2-го гравця

        // Кешування створених кнопок дій
        private readonly List<GameObject> _activeActionButtons = new List<GameObject>();

        // Події для взаємодії з презентером
        public event Action<string> OnActionButtonClicked;
        public event Action OnEndTurnClicked;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SetupUIEvents();
            InitializeUIState();
        }

        // ===== ІСНУЮЧІ МЕТОДИ IGameplayUIView =====

        /// <summary>
        /// Оновлює відображення фази та таймера
        /// </summary>
        public void UpdatePhaseInfo(int phase, float timeRemaining)
        {
            if (_phaseInfoText != null)
            {
                string phaseName = GetPhaseNameById(phase);
                _phaseInfoText.text = $"Фаза: {phaseName}";
            }

            if (_timerText != null)
            {
                _timerText.text = $"Час: {Mathf.Ceil(timeRemaining)}с";
            }
        }

        /// <summary>
        /// Показує значення руни
        /// </summary>
        public void ShowRuneValue(int value)
        {
            if (_runeValueContainer != null)
                _runeValueContainer.SetActive(true);

            if (_runeValueText != null)
                _runeValueText.text = $"Руна: {value}";
        }

        /// <summary>
        /// Ховає руну
        /// </summary>
        public void HideRuneValue()
        {
            if (_runeValueContainer != null)
                _runeValueContainer.SetActive(false);
        }

        // ===== НОВІ МЕТОДИ ДЛЯ ГЕЙМПЛЕЮ =====

        /// <summary>
        /// Оновлює відображення очків дій
        /// </summary>
        public void UpdateActionPoints(int current, int maximum)
        {
            if (_actionPointsText != null)
            {
                _actionPointsText.text = $"Очки дій: {current}/{maximum}";

                // Підсвічування при малій кількості очків
                _actionPointsText.color = current <= 1 ? Color.red : Color.white;
            }
        }

        /// <summary>
        /// Показує доступні дії для поточного гравця (БЕЗ СКРОЛІНГУ)
        /// </summary>
        public void ShowAvailableActions(List<ActionInfo> actions)
        {
            // Очищуємо попередні кнопки
            ClearActionButtons();

            if (_availableActionsContainer == null || _actionButtonPrefab == null)
                return;

            // Оновлюємо заголовок
            if (_actionsTitle != null)
            {
                _actionsTitle.text = actions.Count > 0 ? "Доступні дії:" : "Немає дій";
            }

            // Створюємо кнопки для доступних дій (максимум 6-8 дій для зручності)
            foreach (var action in actions)
            {
                CreateActionButton(action);
            }
        }

        /// <summary>
        /// Оновлює індикатор поточного гравця
        /// </summary>
        public void UpdateTurnIndicator(int currentPlayer, string playerName)
        {
            if (_turnIndicatorText != null)
            {
                _turnIndicatorText.text = $"Хід: {playerName}";
            }

            if (_currentPlayerIndicator != null)
            {
                // Для 2 гравців: 0 = гравець 1 (синій), 1 = гравець 2 (червоний)
                _currentPlayerIndicator.color = currentPlayer == 0 ? _player1Color : _player2Color;
            }
        }

        /// <summary>
        /// Показує панель очікування
        /// </summary>
        public void ShowWaitingForPlayer(string message)
        {
            if (_statusOverlay != null)
                _statusOverlay.gameObject.SetActive(true);

            if (_waitingPanel != null)
                _waitingPanel.SetActive(true);

            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(false);

            if (_waitingText != null)
                _waitingText.text = message;

            // Плавна поява оверлею
            FadeInOverlay();
        }

        /// <summary>
        /// Ховає панель очікування
        /// </summary>
        public void HideWaitingForPlayer()
        {
            if (_waitingPanel != null)
                _waitingPanel.SetActive(false);

            // Ховаємо весь оверлей якщо нічого не показується
            if (_gameOverPanel == null || !_gameOverPanel.activeInHierarchy)
            {
                FadeOutOverlay();
            }
        }

        /// <summary>
        /// Показує екран завершення гри
        /// </summary>
        public void ShowGameOver(string message, bool isVictory)
        {
            if (_statusOverlay != null)
                _statusOverlay.gameObject.SetActive(true);

            if (_waitingPanel != null)
                _waitingPanel.SetActive(false);

            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(true);

            if (_gameOverText != null)
            {
                _gameOverText.text = message;
                _gameOverText.color = isVictory ? Color.green : Color.red;
            }

            // Плавна поява оверлею
            FadeInOverlay();
        }

        /// <summary>
        /// Увімкнути/вимкнути інтерактивність UI
        /// </summary>
        public void SetUIInteractable(bool interactable)
        {
            // Вимикаємо всі кнопки дій
            foreach (var buttonObj in _activeActionButtons)
            {
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                    button.interactable = interactable;
            }

            // Вимикаємо кнопку завершення ходу
            if (_endTurnButton != null)
                _endTurnButton.interactable = interactable;
        }

        // ===== ПРИВАТНІ МЕТОДИ =====

        /// <summary>
        /// Налаштовує події UI
        /// </summary>
        private void SetupUIEvents()
        {
            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.AddListener(() => OnEndTurnClicked?.Invoke());
            }
        }

        /// <summary>
        /// Ініціалізує початковий стан UI
        /// </summary>
        private void InitializeUIState()
        {
            // Ховаємо руну
            HideRuneValue();

            // Ховаємо оверлей
            if (_statusOverlay != null)
            {
                _statusOverlay.alpha = 0f;
                _statusOverlay.gameObject.SetActive(false);
            }

            // Встановлюємо початковий стан панелей
            if (_waitingPanel != null)
                _waitingPanel.SetActive(false);

            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(false);

            // Початкові значення
            UpdateActionPoints(3, 3);
            UpdateTurnIndicator(0, "Гравець 1");

            if (_actionsTitle != null)
                _actionsTitle.text = "Доступні дії:";
        }

        /// <summary>
        /// Створює кнопку дії
        /// </summary>
        private void CreateActionButton(ActionInfo action)
        {
            var buttonObj = Instantiate(_actionButtonPrefab, _availableActionsContainer);
            var button = buttonObj.GetComponent<Button>();

            // Налаштовуємо текст кнопки
            var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"{action.DisplayName} ({action.Cost} ОД)";
            }

            // Налаштовуємо іконку (якщо є)
            var buttonIcon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
            if (buttonIcon != null && action.Icon != null)
            {
                buttonIcon.sprite = action.Icon;
            }

            // Налаштовуємо інтерактивність
            if (button != null)
            {
                button.interactable = action.IsEnabled;

                // Підписуємося на клік
                string actionId = action.ActionId; // Зберігаємо в локальній змінній
                button.onClick.AddListener(() => OnActionButtonClicked?.Invoke(actionId));
            }

            _activeActionButtons.Add(buttonObj);
        }

        /// <summary>
        /// Очищає всі кнопки дій
        /// </summary>
        private void ClearActionButtons()
        {
            foreach (var buttonObj in _activeActionButtons)
            {
                if (buttonObj != null)
                {
                    var button = buttonObj.GetComponent<Button>();
                    button?.onClick.RemoveAllListeners();
                    Destroy(buttonObj);
                }
            }
            _activeActionButtons.Clear();
        }

        /// <summary>
        /// Плавна поява оверлею
        /// </summary>
        private void FadeInOverlay()
        {
            if (_statusOverlay == null)
                return;

            _statusOverlay.gameObject.SetActive(true);

            // Простий fade через LeanTween або DOTween (якщо є)
            // Поки що просто встановлюємо альфу
            _statusOverlay.alpha = 0.8f;

            // TODO: Додати плавну анімацію
            // LeanTween.alphaCanvas(_statusOverlay, 0.8f, 0.3f);
        }

        /// <summary>
        /// Плавне зникнення оверлею
        /// </summary>
        private void FadeOutOverlay()
        {
            if (_statusOverlay == null)
                return;

            // TODO: Додати плавну анімацію з callback
            // LeanTween.alphaCanvas(_statusOverlay, 0f, 0.3f).setOnComplete(() => {
            //     _statusOverlay.gameObject.SetActive(false);
            // });

            // Поки що просто ховаємо
            _statusOverlay.alpha = 0f;
            _statusOverlay.gameObject.SetActive(false);
        }

        /// <summary>
        /// Отримує назву фази за ID
        /// </summary>
        private string GetPhaseNameById(int phaseId)
        {
            return phaseId switch
            {
                0 => "Немає",
                1 => "Руна",
                2 => "Планування",
                3 => "Активна",
                4 => "Завмирання",
                _ => $"Невідома ({phaseId})",
            };
        }

        // ===== НАВІГАЦІЙНІ МЕТОДИ =====

        public override async UniTask OnViewCreatedAsync(NavigationParameters parameters)
        {
            await base.OnViewCreatedAsync(parameters);

            // Отримуємо дані з параметрів навігації
            if (parameters.Contains("SelectedHeroArchetypes"))
            {
                var selectedHeroes = parameters.GetValue<string[]>("SelectedHeroArchetypes");
                // TODO: Ініціалізувати UI на основі вибраних героїв
            }
        }

        public override async UniTask OnViewNavigatedToAsync(NavigationParameters parameters)
        {
            await base.OnViewNavigatedToAsync(parameters);

            // Ініціалізуємо базовий стан UI при навігації
            InitializeUIState();
        }

        protected override void OnDestroy()
        {
            // Очищуємо всі слухачі
            if (_endTurnButton != null)
                _endTurnButton.onClick.RemoveAllListeners();

            ClearActionButtons();
            base.OnDestroy();
        }
    }


   
   
}
