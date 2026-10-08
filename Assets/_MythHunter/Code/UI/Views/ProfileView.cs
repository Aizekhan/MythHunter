// Assets/_MythHunter/Code/UI/Views/ProfileView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MythHunter.UI.Core;
using MythHunter.UI.Presenters;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.UI.Navigation;
using Cysharp.Threading.Tasks;
using MythHunter.Core.MonoBehaviours;

namespace MythHunter.UI.Views
{
    public class ProfileView : NavigableViewBase, IProfileView
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _playerLevelText;
        [SerializeField] private Slider _expSlider;
        [SerializeField] private TextMeshProUGUI _expText;
        [SerializeField] private Image _playerAvatarImage;

        [Header("Tab Buttons")]
        [SerializeField] private Button _heroesTabButton;
        [SerializeField] private Button _statsTabButton;
        [SerializeField] private Button _settingsTabButton;
        [SerializeField] private Button _achievementsTabButton;

        [Header("Tab Indicators")]
        [SerializeField] private GameObject _heroesTabIndicator;
        [SerializeField] private GameObject _statsTabIndicator;
        [SerializeField] private GameObject _settingsTabIndicator;
        [SerializeField] private GameObject _achievementsTabIndicator;

        [Header("Content Containers")]
        [SerializeField] private Transform _heroesContainer;
        [SerializeField] private Transform _statsContainer;
        [SerializeField] private Transform _settingsContainer;
        [SerializeField] private Transform _achievementsContainer;

        [Header("Content Panels")]
        [SerializeField] private GameObject _heroesPanel;
        [SerializeField] private GameObject _statsPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _achievementsPanel;

        [Header("Loading & Error")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private GameObject _errorPanel;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private Button _retryButton;

        [Header("Heroes Tab Content")]
        [SerializeField] private TextMeshProUGUI _heroesCountText;
        [SerializeField] private ScrollRect _heroesScrollRect;
        [SerializeField] private GridLayoutGroup _heroesGrid;

        [Header("Stats Tab Content")]
        [SerializeField] private TextMeshProUGUI _gamesPlayedText;
        [SerializeField] private TextMeshProUGUI _winRateText;
        [SerializeField] private TextMeshProUGUI _favoriteHeroText;
        [SerializeField] private TextMeshProUGUI _totalPlayTimeText;

        [Header("Settings Tab Content")]
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Toggle _notificationsToggle;
        [SerializeField] private TMP_Dropdown _languageDropdown;
        [SerializeField] private Button _saveSettingsButton;

        [Header("Achievements Tab Content")]
        [SerializeField] private TextMeshProUGUI _achievementPointsText;
        [SerializeField] private ScrollRect _achievementsScrollRect;
        [SerializeField] private Transform _achievementsListContainer;

        // Dependencies
        [Inject] private IProfilePresenter _presenter;
        [Inject] private IMythLogger _logger;

        // Properties
        public Transform HeroesContainer => _heroesContainer;
        public Transform StatsContainer => _statsContainer;
        public Transform SettingsContainer => _settingsContainer;
        public Transform AchievementsContainer => _achievementsContainer;

        // State
        private int _currentTabIndex = 0; // 0=Heroes, 1=Stats, 2=Settings, 3=Achievements

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ConnectToPresenter();
            SetupUI();
            ShowHeroesTab(); // За замовчуванням показуємо вкладку героїв
        }

        private void ConnectToPresenter()
        {
            if (_presenter == null)
            {
                var container = FindFirstObjectByType<LazyDependencyInjector>();
                if (container != null)
                {
                    var diContainer = container.GetComponent<IDIContainer>();
                    _presenter = diContainer?.Resolve<IProfilePresenter>();
                }
            }

            if (_presenter != null)
            {
                // ✅ ВАРІАНТ 1: Використовуємо специфічний метод
                _presenter.Initialize(this);

                // ✅ АБО ВАРІАНТ 2: Використовуємо базовий метод
                // _presenter.Initialize(this as IView, ViewId.Profile);

                _logger?.LogInfo("✅ ProfileView підключено до Presenter", "ProfileUI");
            }
            else
            {
                _logger?.LogError("❌ Не вдалося підключитися до ProfilePresenter", "ProfileUI");
            }
        }

        private void SetupUI()
        {
            // Налаштування кнопок
            _backButton?.onClick.AddListener(OnBackClicked);
            _retryButton?.onClick.AddListener(OnRetryClicked);

            // Налаштування вкладок
            _heroesTabButton?.onClick.AddListener(() => OnTabClicked(0));
            _statsTabButton?.onClick.AddListener(() => OnTabClicked(1));
            _settingsTabButton?.onClick.AddListener(() => OnTabClicked(2));
            _achievementsTabButton?.onClick.AddListener(() => OnTabClicked(3));

            // Налаштування налаштувань
            _saveSettingsButton?.onClick.AddListener(OnSaveSettingsClicked);
            _soundToggle?.onValueChanged.AddListener(OnSoundToggleChanged);
            _volumeSlider?.onValueChanged.AddListener(OnVolumeChanged);
            _notificationsToggle?.onValueChanged.AddListener(OnNotificationsToggleChanged);
            _languageDropdown?.onValueChanged.AddListener(OnLanguageChanged);

            // Початкові налаштування
            if (_titleText)
                _titleText.text = "Профіль гравця";
            ShowLoadingState(false);
            HideAllPanels();

            _logger?.LogInfo("✅ ProfileView UI налаштовано", "ProfileUI");
        }

        // ✅ РЕАЛІЗАЦІЯ IProfileView

        public void ShowBackButton(bool show)
        {
            if (_backButton != null)
                _backButton.gameObject.SetActive(show);
        }

        public void ShowHeroesTab()
        {
            SwitchToTab(0);
            _presenter?.OnHeroesTabClicked();
        }

        public void ShowStatsTab()
        {
            SwitchToTab(1);
            _presenter?.OnStatsTabClicked();
        }

        public void ShowSettingsTab()
        {
            SwitchToTab(2);
            _presenter?.OnSettingsTabClicked();
        }

        public void ShowAchievementsTab()
        {
            SwitchToTab(3);
            _presenter?.OnAchievementsTabClicked();
        }

        public void SetPlayerName(string playerName)
        {
            if (_playerNameText)
                _playerNameText.text = playerName;
        }

        public void SetPlayerLevel(int level)
        {
            if (_playerLevelText)
                _playerLevelText.text = $"Рівень {level}";
        }

        public void SetPlayerExp(int currentExp, int maxExp)
        {
            if (_expSlider)
            {
                _expSlider.value = (float)currentExp / maxExp;
            }

            if (_expText)
                _expText.text = $"{currentExp} / {maxExp} досвіду";
        }

        public void ShowLoadingState(bool isLoading)
        {
            if (_loadingPanel)
                _loadingPanel.SetActive(isLoading);

            if (isLoading && _loadingText)
                _loadingText.text = "Завантаження...";
        }

        public void ShowError(string error)
        {
            if (_errorPanel)
                _errorPanel.SetActive(true);

            if (_errorText)
                _errorText.text = error;

            _logger?.LogError($"ProfileView показує помилку: {error}", "ProfileUI");
        }

        // ✅ ДОДАТКОВІ МЕТОДИ ДЛЯ РОБОТИ З КОНТЕНТОМ

        /// <summary>
        /// Встановлює кількість героїв
        /// </summary>
        public void SetHeroesCount(int total, int owned)
        {
            if (_heroesCountText)
                _heroesCountText.text = $"Герої: {owned}/{total}";
        }

        /// <summary>
        /// Встановлює статистику гравця
        /// </summary>
        public void SetPlayerStats(int gamesPlayed, float winRate, string favoriteHero, int totalPlayTimeMinutes)
        {
            if (_gamesPlayedText)
                _gamesPlayedText.text = $"Ігор зіграно: {gamesPlayed}";

            if (_winRateText)
                _winRateText.text = $"Відсоток перемог: {winRate:F1}%";

            if (_favoriteHeroText)
                _favoriteHeroText.text = $"Улюблений герой: {favoriteHero}";

            if (_totalPlayTimeText)
            {
                int hours = totalPlayTimeMinutes / 60;
                int minutes = totalPlayTimeMinutes % 60;
                _totalPlayTimeText.text = $"Загальний час гри: {hours}г {minutes}хв";
            }
        }

        /// <summary>
        /// Встановлює очки досягнень
        /// </summary>
        public void SetAchievementPoints(int points)
        {
            if (_achievementPointsText)
                _achievementPointsText.text = $"Очки досягнень: {points}";
        }

        /// <summary>
        /// Завантажує налаштування
        /// </summary>
        public void LoadSettings(bool soundEnabled, float volume, bool notificationsEnabled, int languageIndex)
        {
            if (_soundToggle)
                _soundToggle.isOn = soundEnabled;

            if (_volumeSlider)
                _volumeSlider.value = volume;

            if (_notificationsToggle)
                _notificationsToggle.isOn = notificationsEnabled;

            if (_languageDropdown && languageIndex >= 0 && languageIndex < _languageDropdown.options.Count)
                _languageDropdown.value = languageIndex;
        }

        // ✅ ПРИВАТНІ МЕТОДИ

        private void SwitchToTab(int tabIndex)
        {
            _currentTabIndex = tabIndex;

            // Ховаємо всі панелі
            HideAllPanels();

            // Ховаємо всі індикатори
            HideAllTabIndicators();

            // Показуємо потрібну панель та індикатор
            switch (tabIndex)
            {
                case 0: // Heroes
                    _heroesPanel?.SetActive(true);
                    _heroesTabIndicator?.SetActive(true);
                    break;
                case 1: // Stats
                    _statsPanel?.SetActive(true);
                    _statsTabIndicator?.SetActive(true);
                    break;
                case 2: // Settings
                    _settingsPanel?.SetActive(true);
                    _settingsTabIndicator?.SetActive(true);
                    break;
                case 3: // Achievements
                    _achievementsPanel?.SetActive(true);
                    _achievementsTabIndicator?.SetActive(true);
                    break;
            }

            _logger?.LogInfo($"Переключено на вкладку {tabIndex}", "ProfileUI");
        }

        private void HideAllPanels()
        {
            _heroesPanel?.SetActive(false);
            _statsPanel?.SetActive(false);
            _settingsPanel?.SetActive(false);
            _achievementsPanel?.SetActive(false);
        }

        private void HideAllTabIndicators()
        {
            _heroesTabIndicator?.SetActive(false);
            _statsTabIndicator?.SetActive(false);
            _settingsTabIndicator?.SetActive(false);
            _achievementsTabIndicator?.SetActive(false);
        }

        // ✅ EVENT HANDLERS

        private void OnBackClicked()
        {
            _logger?.LogInfo("👈 Back button clicked", "ProfileUI");
            _presenter?.OnBackClicked();
        }

        private void OnRetryClicked()
        {
            _logger?.LogInfo("🔄 Retry button clicked", "ProfileUI");
            if (_errorPanel)
                _errorPanel.SetActive(false);
            _presenter?.LoadPlayerDataAsync().Forget();
        }

        private void OnTabClicked(int tabIndex)
        {
            if (tabIndex == _currentTabIndex)
                return; // Вже активна вкладка

            switch (tabIndex)
            {
                case 0:
                    ShowHeroesTab();
                    break;
                case 1:
                    ShowStatsTab();
                    break;
                case 2:
                    ShowSettingsTab();
                    break;
                case 3:
                    ShowAchievementsTab();
                    break;
            }
        }

        private void OnSaveSettingsClicked()
        {
            _logger?.LogInfo("💾 Save settings clicked", "ProfileUI");
            _presenter?.SaveSettingsAsync().Forget();
        }

        private void OnSoundToggleChanged(bool value)
        {
            _logger?.LogInfo($"🔊 Sound toggle: {value}", "ProfileUI");
            // TODO: Застосувати налаштування звуку
        }

        private void OnVolumeChanged(float value)
        {
            _logger?.LogInfo($"🔉 Volume changed: {value}", "ProfileUI");
            // TODO: Застосувати гучність
        }

        private void OnNotificationsToggleChanged(bool value)
        {
            _logger?.LogInfo($"🔔 Notifications toggle: {value}", "ProfileUI");
            // TODO: Застосувати налаштування сповіщень
        }

        private void OnLanguageChanged(int value)
        {
            _logger?.LogInfo($"🌍 Language changed: {value}", "ProfileUI");
            // TODO: Застосувати мову
        }

        // ✅ NAVIGATION VIEW BASE OVERRIDE

        public override async UniTask OnViewCreatedAsync(NavigationParameters parameters)
        {
            _logger?.LogInfo("👤 ProfileView створено", "ProfileUI");
            await base.OnViewCreatedAsync(parameters);

            // Завантажуємо дані гравця
            if (_presenter != null)
            {
                await _presenter.LoadPlayerDataAsync();
            }
        }

        public override async UniTask OnViewNavigatedToAsync(NavigationParameters parameters)
        {
            _logger?.LogInfo("👤 Навігація до ProfileView", "ProfileUI");
            await base.OnViewNavigatedToAsync(parameters);

            // Можна отримати параметри навігації
            string previousState = parameters?.GetValue<string>("PreviousState", "Unknown");
            _logger?.LogInfo($"Прийшли з стану: {previousState}", "ProfileUI");
        }

        public override async UniTask OnViewNavigatedFromAsync()
        {
            _logger?.LogInfo("👤 Навігація з ProfileView", "ProfileUI");
            await base.OnViewNavigatedFromAsync();
        }

        public override async UniTask OnViewDestroyedAsync()
        {
            _logger?.LogInfo("👤 ProfileView знищується", "ProfileUI");
            await base.OnViewDestroyedAsync();
        }

        // ✅ CLEANUP

        protected override void OnDestroy()
        {
            // Очищуємо всі listeners
            _backButton?.onClick.RemoveAllListeners();
            _retryButton?.onClick.RemoveAllListeners();

            _heroesTabButton?.onClick.RemoveAllListeners();
            _statsTabButton?.onClick.RemoveAllListeners();
            _settingsTabButton?.onClick.RemoveAllListeners();
            _achievementsTabButton?.onClick.RemoveAllListeners();

            _saveSettingsButton?.onClick.RemoveAllListeners();
            _soundToggle?.onValueChanged.RemoveAllListeners();
            _volumeSlider?.onValueChanged.RemoveAllListeners();
            _notificationsToggle?.onValueChanged.RemoveAllListeners();
            _languageDropdown?.onValueChanged.RemoveAllListeners();

            _logger?.LogInfo("🗑️ ProfileView очищено", "ProfileUI");
            base.OnDestroy();
        }
    }
}
