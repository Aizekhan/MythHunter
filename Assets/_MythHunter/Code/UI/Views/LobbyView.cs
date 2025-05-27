// Assets/_MythHunter/Code/UI/Views/LobbyView.cs
using UnityEngine;
using UnityEngine.UI; // ✅ ДОДАНО для Button
using MythHunter.UI.Core;
using TMPro;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.UI.Presenters;
using Cysharp.Threading.Tasks;
using MythHunter.Services.GameSettings;
namespace MythHunter.UI.Views
{
    public class LobbyView : UIViewBase, ILobbyView
    {
        [Inject] private IMythLogger _logger;
        [Inject] private IDIContainer _container;

        [Header("Контейнери")]
        [SerializeField] private Transform _heroCardsContainer;
        [SerializeField] private Transform _selectedHeroesContainer;

        [Header("UI елементи")]
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _manaText;
        [SerializeField] private TMP_Text _errorText;

        [Header("Кнопки")] // ✅ НОВА СЕКЦІЯ
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _confirmButton;

        [SerializeField] private GameObject[] _playerReadyIndicators;
        [SerializeField] private GameObject _gameStartingPanel;


        [Header("Mode-Specific UI")]
        [SerializeField] private GameObject _aiModePanel;      // Панель для AI режиму
        [SerializeField] private GameObject _localPvPPanel;    // Панель для локального PvP
        [SerializeField] private GameObject _onlinePvPPanel;   // Панель для онлайн PvP
        [SerializeField] private TMP_Text _gameModeText;       // Показує поточний режим

        [Header("AI Mode UI")]
        [SerializeField] private TMP_Text _aiStatusText;       // "AI обирає героїв..."
        [SerializeField] private GameObject _aiThinkingIndicator; // Індикатор "AI думає"

        [Header("PvP Mode UI")]
        [SerializeField] private TMP_Text _currentPlayerText;  // "Хід гравця 1" / "Хід гравця 2"
        [SerializeField] private GameObject _waitingPanel;     // "Очікування іншого гравця"

        private ILobbyPresenter _presenter; // ✅ ПОСИЛАННЯ НА PRESENTER

        public Transform HeroCardsContainer => _heroCardsContainer;
        public Transform SelectedHeroesContainer => _selectedHeroesContainer;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // ✅ ДЕТАЛЬНЕ логування
            _logger?.LogInfo($"🎬 LobbyView ініціалізовано", "UI");
            _logger?.LogInfo($"HeroCardsContainer: {(_heroCardsContainer != null ? "✅ OK" : "❌ NULL")}", "UI");
            _logger?.LogInfo($"SelectedHeroesContainer: {(_selectedHeroesContainer != null ? "✅ OK" : "❌ NULL")}", "UI");

            if (_heroCardsContainer == null)
            {
                _logger?.LogError("⚠️ HeroCardsContainer не призначений в інспекторі!", "UI");
            }

            if (_selectedHeroesContainer == null)
            {
                _logger?.LogError("⚠️ SelectedHeroesContainer не призначений в інспекторі!", "UI");
            }

            // ✅ НАЛАШТОВУЄМО КНОПКИ
            SetupButtons();
            ConnectToPresenter();
        }

        // ✅ НОВИЙ МЕТОД для налаштування кнопок
        private void SetupButtons()
        {
            _logger?.LogInfo("🔘 Налаштування кнопок...", "UI");

            if (_startButton != null)
            {
                _startButton.onClick.RemoveAllListeners();
                _startButton.onClick.AddListener(OnStartButtonClicked);
                _logger?.LogInfo("✅ StartButton налаштовано", "UI");
            }
            else
            {
                _logger?.LogWarning("⚠️ StartButton не призначено в інспекторі!", "UI");
            }

            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveAllListeners();
                _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
                _logger?.LogInfo("✅ ConfirmButton налаштовано", "UI");
            }
            else
            {
                _logger?.LogWarning("⚠️ ConfirmButton не призначено в інспекторі!", "UI");
            }
        }

        // ✅ ОБРОБНИКИ КНОПОК
        private void OnStartButtonClicked()
        {
            _logger?.LogInfo("🚀 Start button clicked!", "UI");

            if (_presenter != null)
            {
                _presenter.StartGameAsync().Forget();
            }
            else
            {
                _logger?.LogError("❌ Presenter is null in OnStartButtonClicked!", "UI");
                ShowError("Presenter не ініціалізовано!");
            }
        }

        private void OnConfirmButtonClicked()
        {
            _logger?.LogInfo("✅ Confirm button clicked!", "UI");

            if (_presenter != null)
            {
                _presenter.OnSelectionConfirmed();
            }
            else
            {
                _logger?.LogError("❌ Presenter is null in OnConfirmButtonClicked!", "UI");
                ShowError("Presenter не ініціалізовано!");
            }
        }

        private void ConnectToPresenter()
        {
            try
            {
                _logger?.LogInfo("🔗 Починаємо зв'язування з LobbyPresenter...", "UI");

                if (_container == null)
                {
                    _logger?.LogError("❌ DIContainer не ін'єктовано в LobbyView!", "UI");
                    return;
                }

                var presenter = _container.Resolve<ILobbyPresenter>();
                if (presenter != null)
                {
                    _presenter = presenter; // ✅ ЗБЕРІГАЄМО ПОСИЛАННЯ
                    _logger?.LogInfo("✅ LobbyPresenter знайдено, викликаємо Initialize(this)", "UI");
                    presenter.Initialize(this);
                    _logger?.LogInfo("🎯 LobbyView успішно зв'язано з LobbyPresenter!", "UI");
                }
                else
                {
                    _logger?.LogError("❌ LobbyPresenter не знайдено в контейнері!", "UI");
                    ShowError("LobbyPresenter не знайдено!");
                }
            }
            catch (System.Exception ex)
            {
                _logger?.LogError($"❌ Помилка зв'язування presenter: {ex.Message}", "UI", ex);
                ShowError($"Помилка зв'язування: {ex.Message}");
            }
        }

        // ✅ ІСНУЮЧІ МЕТОДИ (з покращеним логуванням)
        public void ShowError(string message)
        {
            _logger?.LogInfo($"🚨 Показуємо помилку: {message}", "UI");

            if (_errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);

                // ✅ АВТОМАТИЧНО ПРИХОВУЄМО ЧЕРЕЗ 5 СЕКУНД
                Invoke(nameof(HideError), 5f);
            }
            else
            {
                _logger?.LogError("❌ _errorText is null!", "UI");
            }
        }

        private void HideError()
        {
            if (_errorText != null)
            {
                _errorText.gameObject.SetActive(false);
            }
        }

        public void UpdateTimer(float current, float max)
        {
            if (_timerText != null)
            {
                _timerText.text = $"{Mathf.CeilToInt(current)} сек";
            }
            else
            {
                _logger?.LogWarning("⚠️ _timerText is null в UpdateTimer!", "UI");
            }
        }

        public void UpdateMana(int current, int max)
        {
            if (_manaText != null)
            {
                _manaText.text = $"Мана: {current}/{max}";
            }
        }

        public void ShowPlayerStatus(int playerIndex, bool ready)
        {
            _logger?.LogInfo($"🎮 Показуємо статус гравця {playerIndex}: {ready}", "UI");

            if (playerIndex < 0 || playerIndex >= _playerReadyIndicators.Length)
                return;

            _playerReadyIndicators[playerIndex].SetActive(ready);
        }

        public void ShowGameStartingMessage()
        {
            _logger?.LogInfo("🎮 Показуємо повідомлення про початок гри", "UI");

            if (_gameStartingPanel != null)
            {
                _gameStartingPanel.SetActive(true);
            }
            else
            {
                _logger?.LogError("❌ _gameStartingPanel is null!", "UI");
            }
        }
        //
        /// <summary>
        /// Налаштовує UI для конкретного режиму гри
        /// </summary>
        public void ConfigureForGameMode(GameMode mode)
        {
            _logger?.LogInfo($"🎮 Налаштування LobbyView для режиму: {mode}", "UI");

            // Ховаємо всі панелі спочатку
            _aiModePanel?.SetActive(false);
            _localPvPPanel?.SetActive(false);
            _onlinePvPPanel?.SetActive(false);

            switch (mode)
            {
                case GameMode.PvAI:
                    _aiModePanel?.SetActive(true);
                    if (_gameModeText)
                        _gameModeText.text = "Гра з AI";
                    SetupAIMode();
                    break;

                case GameMode.LocalPvP:
                    _localPvPPanel?.SetActive(true);
                    if (_gameModeText)
                        _gameModeText.text = "Локальний PvP";
                    SetupLocalPvPMode();
                    break;

                case GameMode.OnlinePvP:
                    _onlinePvPPanel?.SetActive(true);
                    if (_gameModeText)
                        _gameModeText.text = "Онлайн PvP";
                    SetupOnlinePvPMode();
                    break;
            }
        }

        private void SetupAIMode()
        {
            // В AI режимі ховаємо деякі UI елементи
            if (_confirmButton)
                _confirmButton.gameObject.SetActive(false); // AI не потребує confirm
            if (_aiStatusText)
                _aiStatusText.text = "Оберіть своїх героїв";
            if (_aiThinkingIndicator)
                _aiThinkingIndicator.SetActive(false);
        }

        private void SetupLocalPvPMode()
        {
            // В локальному PvP показуємо всі елементи
            if (_confirmButton)
                _confirmButton.gameObject.SetActive(true);
            if (_currentPlayerText)
                _currentPlayerText.text = "Хід: Гравець 1";
        }

        private void SetupOnlinePvPMode()
        {
            // В онлайн PvP поки що як локальний
            SetupLocalPvPMode();
            // Пізніше додамо специфічний функціонал
        }

        /// <summary>
        /// Показує статус AI
        /// </summary>
        public void ShowAIStatus(string status, bool isThinking = false)
        {
            if (_aiStatusText)
                _aiStatusText.text = status;
            if (_aiThinkingIndicator)
                _aiThinkingIndicator.SetActive(isThinking);
        }

        /// <summary>
        /// Показує поточного гравця в PvP режимах
        /// </summary>
        public void ShowCurrentPlayer(int playerIndex)
        {
            if (_currentPlayerText)
            {
                _currentPlayerText.text = $"Хід: Гравець {playerIndex + 1}";
            }
        }

        /// <summary>
        /// Показує панель очікування
        /// </summary>
        public void ShowWaitingForPlayer(bool show, string message = "Очікування іншого гравця...")
        {
            if (_waitingPanel)
                _waitingPanel.SetActive(show);
            // Можна додати текст повідомлення
        }
        protected override void OnDestroy()
        {
            _logger?.LogInfo("🗑️ LobbyView.OnDestroy() викликано", "UI");

            // ✅ ОЧИЩУЄМО LISTENERS
            if (_startButton != null)
                _startButton.onClick.RemoveAllListeners();
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveAllListeners();

            _presenter = null;
            base.OnDestroy();
        }

        private void OnDisable()
        {
            _logger?.LogInfo("⏸️ LobbyView.OnDisable() викликано", "UI");
        }

        private void OnEnable()
        {
            _logger?.LogInfo("▶️ LobbyView.OnEnable() викликано", "UI");
        }
    }
}
