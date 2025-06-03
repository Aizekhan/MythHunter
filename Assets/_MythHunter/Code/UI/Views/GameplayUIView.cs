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
    /// Розширене представлення ігрового інтерфейсу з новими геймплей елементами
    /// </summary>
    public class GameplayUIView : NavigableViewBase, IGameplayUIView
    {
        [Header("Існуючі UI елементи")]
        [SerializeField] private TextMeshProUGUI _phaseInfoText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private GameObject _runeValueContainer;
        [SerializeField] private TextMeshProUGUI _runeValueText;

        [Header("Панелі")]
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _heroStatsPanel;
        [SerializeField] private GameObject _objectivesPanel;

        [Header("🎮 НОВІ ГЕЙМПЛЕЙ ЕЛЕМЕНТИ")]
        [SerializeField] private Transform _actionPointsContainer;
        [SerializeField] private TextMeshProUGUI _actionPointsText;
        [SerializeField] private Slider _actionPointsSlider;

        [Header("Дії гравця")]
        [SerializeField] private Transform _availableActionsContainer;
        [SerializeField] private GameObject _actionButtonPrefab;
        [SerializeField] private ScrollRect _actionsScrollRect;

        [Header("Інформація про хід")]
        [SerializeField] private TextMeshProUGUI _turnIndicatorText;
        [SerializeField] private Image _currentPlayerIndicator;
        [SerializeField] private Color[] _playerColors = new Color[4];

        [Header("Статус гри")]
        [SerializeField] private GameObject _waitingForPlayerPanel;
        [SerializeField] private TextMeshProUGUI _waitingText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;

        // Кешування створених кнопок дій
        private readonly List<GameObject> _activeActionButtons = new List<GameObject>();

        // Події для взаємодії з презентером
        public event Action<string> OnActionButtonClicked;
        public event Action OnEndTurnClicked;

        // Існуючі методи IGameplayUIView
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

        public void ShowRuneValue(int value)
        {
            if (_runeValueContainer != null)
                _runeValueContainer.SetActive(true);

            if (_runeValueText != null)
                _runeValueText.text = $"Руна: {value}";
        }

        public void HideRuneValue()
        {
            if (_runeValueContainer != null)
                _runeValueContainer.SetActive(false);
        }

        // НОВІ МЕТОДИ для геймплею

        /// <summary>
        /// Оновлює відображення очків дій
        /// </summary>
        public void UpdateActionPoints(int current, int maximum)
        {
            if (_actionPointsText != null)
                _actionPointsText.text = $"Очки дій: {current}/{maximum}";

            if (_actionPointsSlider != null)
            {
                _actionPointsSlider.maxValue = maximum;
                _actionPointsSlider.value = current;
            }

            // Підсвічування при малій кількості очків
            if (_actionPointsText != null)
            {
                _actionPointsText.color = current <= 1 ? Color.red : Color.white;
            }
        }

        /// <summary>
        /// Показує доступні дії для поточного гравця
        /// </summary>
        public void ShowAvailableActions(List<ActionInfo> actions)
        {
            // Очищуємо попередні кнопки
            ClearActionButtons();

            if (_availableActionsContainer == null || _actionButtonPrefab == null)
                return;

            // Створюємо кнопки для доступних дій
            foreach (var action in actions)
            {
                var buttonObj = Instantiate(_actionButtonPrefab, _availableActionsContainer);
                var button = buttonObj.GetComponent<Button>();
                var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (button != null && buttonText != null)
                {
                    buttonText.text = action.DisplayName;
                    button.interactable = action.IsEnabled;

                    // Додаємо tooltip якщо є
                    if (!string.IsNullOrEmpty(action.Description))
                    {
                        // TODO: Додати tooltip компонент
                    }

                    // Підписуємся на клік
                    string actionId = action.ActionId; // Зберігаємо в локальній змінній для замикання
                    button.onClick.AddListener(() => OnActionButtonClicked?.Invoke(actionId));

                    _activeActionButtons.Add(buttonObj);
                }
            }

            // Оновлюємо скролл
            if (_actionsScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _actionsScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>
        /// Оновлює індикатор поточного гравця
        /// </summary>
        public void UpdateTurnIndicator(int currentPlayer, string playerName)
        {
            if (_turnIndicatorText != null)
            {
                _turnIndicatorText.text = $"Хід гравця: {playerName}";
            }

            if (_currentPlayerIndicator != null && currentPlayer >= 0 && currentPlayer < _playerColors.Length)
            {
                _currentPlayerIndicator.color = _playerColors[currentPlayer];
            }
        }

        /// <summary>
        /// Показує панель очікування
        /// </summary>
        public void ShowWaitingForPlayer(string message)
        {
            if (_waitingForPlayerPanel != null)
                _waitingForPlayerPanel.SetActive(true);

            if (_waitingText != null)
                _waitingText.text = message;
        }

        /// <summary>
        /// Ховає панель очікування
        /// </summary>
        public void HideWaitingForPlayer()
        {
            if (_waitingForPlayerPanel != null)
                _waitingForPlayerPanel.SetActive(false);
        }

        /// <summary>
        /// Показує екран завершення гри
        /// </summary>
        public void ShowGameOver(string message, bool isVictory)
        {
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(true);

            if (_gameOverText != null)
            {
                _gameOverText.text = message;
                _gameOverText.color = isVictory ? Color.green : Color.red;
            }
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
        }

        // ПРИВАТНІ МЕТОДИ

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

        // НАВІГАЦІЙНІ МЕТОДИ

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

            // Ініціалізуємо базовий стан UI
            HideRuneValue();
            HideWaitingForPlayer();
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(false);
        }

        protected override void OnDestroy()
        {
            ClearActionButtons();
            base.OnDestroy();
        }
    }


   
}
