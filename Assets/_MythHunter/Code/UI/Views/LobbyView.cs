// Assets/_MythHunter/Code/UI/Views/LobbyView.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.UI.Models;
using MythHunter.UI.Presenters;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MythHunter.Services.GameSettings;
using MythHunter.Core.Game;
using System.Linq;
namespace MythHunter.UI.Views
{
    /// <summary>
    /// Представлення лобі
    /// </summary>
    public class LobbyView : UIViewBase, ILobbyView
    {
        [SerializeField] private Transform _heroCardsContainer;
        [SerializeField] private Transform _selectedHeroesContainer;
        [SerializeField] private GameObject _heroCardPrefab;
        [SerializeField] private TextMeshProUGUI _manaText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private TextMeshProUGUI _gameStartingText;
        [SerializeField] private TextMeshProUGUI[] _playerStatusTexts;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _startGameButton;

        private ILobbyPresenter _presenter;
        private IGameSettingsService _settings;
        private IMythLogger _logger;
        private readonly List<HeroCardUI> _createdCards = new List<HeroCardUI>();
        private IUIViewFactory _uiViewFactory;
        protected override void Awake()
        {
            base.Awake();

            // Отримуємо логер для дебагу
            var bootstrapper = FindFirstObjectByType<GameBootstrapper>();
            if (bootstrapper != null)
            {
                var container = bootstrapper.GetContainer();
                if (container != null)
                {
                    _logger = container.Resolve<IMythLogger>();
                    _logger.LogInfo("Отримано логер через GameBootstrapper", "LobbyView");
                }
                else
                {
                    _logger = MythLoggerFactory.GetDefaultLogger();
                }
            }
            else
            {
                _logger = MythLoggerFactory.GetDefaultLogger();
            }

            // Перевірка компонентів
            if (_heroCardsContainer == null)
                _logger.LogError("_heroCardsContainer не вказано в інспекторі!", "LobbyView");
            if (_selectedHeroesContainer == null)
                _logger.LogError("_selectedHeroesContainer не вказано в інспекторі!", "LobbyView");
            if (_heroCardPrefab == null)
                _logger.LogError("_heroCardPrefab не вказано в інспекторі!", "LobbyView");

            // Додати слухачів подій для кнопок тут
            if (_confirmButton)
                _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            if (_startGameButton)
                _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        }
        private void Start()
        {
            _logger.LogInfo("Start викликано", "LobbyView");

            // Якщо ін'єкція вже відбулась, то ініціалізуємо
            if (_presenter != null)
            {
                InitializeView();
            }
            else
            {
                _logger.LogError("Presenter не був ін'єктований до Start()", "LobbyView");
            }
        }

      
        [Inject]
        public void Construct(ILobbyPresenter presenter, IGameSettingsService settings, IMythLogger logger, IUIViewFactory uiViewFactory)
        {
            _logger.LogInfo("Construct викликано", "LobbyView");
            _presenter = presenter;
            _settings = settings;
            _logger = logger;
            _uiViewFactory = uiViewFactory;

            // Ініціалізуємо презентер з представленням
            _presenter.Initialize(this);
            _logger.LogInfo("Presenter ініціалізовано", "LobbyView");

            // Якщо об'єкт уже активний, ініціалізуємо представлення
            if (gameObject.activeInHierarchy)
            {
                InitializeView();
            }
            // Інакше ініціалізація відбудеться у Start
        }
        private void OnEnable()
        {
            if (_presenter != null)
            {
                InitializeView();
            }
        }
        private void InitializeView()
        {
            _logger.LogInfo("InitializeView викликано", "LobbyView");

            if (_settings == null)
            {
                _logger.LogError("_settings не ін'єктовано!", "LobbyView");
                return;
            }

            // Скидаємо елементи інтерфейсу в початковий стан
            if (_errorText)
                _errorText.gameObject.SetActive(false);
            if (_gameStartingText)
                _gameStartingText.gameObject.SetActive(false);

            for (int i = 0; i < _playerStatusTexts.Length; i++)
            {
                if (_playerStatusTexts[i])
                    _playerStatusTexts[i].text = $"Гравець {i + 1}: Очікує вибору";
            }

            // Запускаємо лобі
            _presenter.StartLobby(_settings.PlayerCount);

            // ВАЖЛИВО: Тепер презентер відповідає за виклик PopulateHeroCards
            // Не викликайте PopulateHeroCards тут!

            _logger.LogInfo("View ініціалізовано", "LobbyView");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearHeroCards();

            // Відписуємось від евентів через презентер
            if (_presenter != null)
            {
                _presenter.Dispose();
            }

            if (_confirmButton)
                _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            if (_startGameButton)
                _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);

            ClearContainer(_heroCardsContainer);
            ClearContainer(_selectedHeroesContainer);

            _logger.LogInfo("OnDestroy викликано", "LobbyView");
        }

        public void UpdateHeroCardsState(List<HeroCardModel> heroes)
        {
            if (_createdCards == null || heroes == null)
            {
                _logger.LogWarning("_createdCards або heroes є null", "LobbyView");
                return;
            }

            foreach (var card in _createdCards)
            {
                if (card == null)
                    continue;

                // Шукаємо відповідну модель героя за ID
                var model = heroes.FirstOrDefault(h => h.ArchetypeId == card.ArchetypeId);
                if (model != null)
                {
                    // Оновлюємо стан картки
                    card.SetInteractable(model.IsSelectable);
                    // Встановлюємо видимість - завжди видима, навіть якщо не вибирається
                    card.gameObject.SetActive(true);
                }
                else
                {
                    // Якщо герой не знайдений у списку, однаково залишаємо його видимим
                    // але неактивним для вибору
                    card.SetInteractable(false);
                    card.gameObject.SetActive(true);
                }
            }
        }
        // Assets/_MythHunter/Code/UI/Views/LobbyView.cs
        public void PopulateHeroCards(List<HeroCardModel> heroes)
        {
            if (heroes == null || heroes.Count == 0)
            {
                _logger.LogWarning("PopulateHeroCards викликано з пустим списком героїв", "LobbyView");
                return;
            }

            _logger.LogInfo($"PopulateHeroCards викликано з {heroes.Count} героями", "LobbyView");

            try
            {
                ClearHeroCards();

                foreach (var hero in heroes)
                {
                    try
                    {
                        if (_heroCardPrefab == null)
                        {
                            _logger.LogError("_heroCardPrefab є null!", "LobbyView");
                            continue;
                        }

                        var card = Instantiate(_heroCardPrefab, _heroCardsContainer);
                        var ui = card.GetComponent<HeroCardUI>();

                        if (ui != null)
                        {
                            // Ін'єкція залежностей через GameBootstrapper
                            if (GameBootstrapper.Instance != null)
                            {
                                GameBootstrapper.Instance.RegisterForInjection(ui);
                                _logger.LogInfo($"Ін'єкція залежностей для картки героя {hero.Name}", "LobbyView");
                            }
                            else
                            {
                                _logger.LogWarning("GameBootstrapper.Instance є null!", "LobbyView");
                            }

                            ui.Setup(hero);
                            ui.OnHeroSelected += OnHeroCardSelected;
                            _createdCards.Add(ui);

                            _logger.LogInfo($"Створено картку для героя {hero.Name}", "LobbyView");
                        }
                        else
                        {
                            _logger.LogError("Не знайдено компонент HeroCardUI на префабі", "LobbyView");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Помилка при створенні картки героя: {ex.Message}", "LobbyView", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Критична помилка в PopulateHeroCards: {ex.Message}", "LobbyView", ex);
            }
        }
        private void ClearHeroCards()
        {
            // Відписуємось від подій і знищуємо всі картки
            foreach (var card in _createdCards)
            {
                if (card != null)
                {
                    card.OnHeroSelected -= OnHeroCardSelected;
                    Destroy(card.gameObject);
                }
            }

            _createdCards.Clear();

            // Додатково очищаємо контейнер
            foreach (Transform child in _heroCardsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public void UpdateSelectedHeroes(List<HeroCardModel> selectedHeroes)
        {
            ClearContainer(_selectedHeroesContainer);
            foreach (var hero in selectedHeroes)
            {
                var card = Instantiate(_heroCardPrefab, _selectedHeroesContainer);
                var ui = card.GetComponent<HeroCardUI>();
                if (ui != null)
                {
                    // Ось тут додаємо ін'єкцію
                    GameBootstrapper.Instance?.RegisterForInjection(ui);

                    ui.Setup(hero);
                    ui.SetInteractable(false);
                }
            }
        }

        public void UpdateMana(int remainingMana, int totalMana)
        {
            if (_manaText)
                _manaText.text = $"Мана: {remainingMana}/{totalMana}";
            else
                _logger.LogError("_manaText відсутній!", "LobbyView");
        }

        public void UpdateTimer(float remainingTime, float totalTime)
        {
            if (_timerText)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60);
                int seconds = Mathf.FloorToInt(remainingTime % 60);
                _timerText.text = $"Час: {minutes:00}:{seconds:00}";
            }
            else
                _logger.LogError("_timerText відсутній!", "LobbyView");
        }

        public void ShowError(string message)
        {
            _logger.LogError($"Помилка - {message}", "LobbyView");

            if (_errorText)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
                Invoke(nameof(HideError), 3f);
            }
            else
                _logger.LogError("_errorText відсутній!", "LobbyView");
        }

        public void ShowPlayerStatus(int index, bool isReady)
        {
            _logger.LogInfo($"ShowPlayerStatus - Гравець {index}, готовність: {isReady}", "LobbyView");

            if (_playerStatusTexts == null || _playerStatusTexts.Length == 0)
            {
                _logger.LogError("_playerStatusTexts відсутній!", "LobbyView");
                return;
            }

            if (index < _playerStatusTexts.Length && _playerStatusTexts[index] != null)
            {
                _playerStatusTexts[index].text = isReady ? $"Гравець {index + 1}: Готовий" : $"Гравець {index + 1}: Вибирає";
                _playerStatusTexts[index].color = isReady ? Color.green : Color.white;
            }
        }

        public void ShowGameStartingMessage()
        {
            _logger.LogInfo("ShowGameStartingMessage викликано", "LobbyView");

            if (_gameStartingText)
                _gameStartingText.gameObject.SetActive(true);
            if (_heroCardsContainer)
                _heroCardsContainer.gameObject.SetActive(false);
            if (_selectedHeroesContainer)
                _selectedHeroesContainer.gameObject.SetActive(false);
            if (_confirmButton)
                _confirmButton.gameObject.SetActive(false);
            if (_startGameButton)
                _startGameButton.gameObject.SetActive(false);
        }

        private void OnConfirmButtonClicked()
        {
            _logger.LogInfo("OnConfirmButtonClicked викликано", "LobbyView");

            if (_presenter != null)
            {
                _presenter.OnSelectionConfirmed();
            }
            else
            {
                _logger.LogError("_presenter відсутній при натисканні Confirm!", "LobbyView");
            }
        }

        private async void OnStartGameButtonClicked()
        {
            _logger.LogInfo("OnStartGameButtonClicked викликано", "LobbyView");
            if (_presenter != null)
                await _presenter.StartGameAsync();
            else
                _logger.LogError("_presenter відсутній!", "LobbyView");
        }

        private void OnHeroCardSelected(string archetypeId)
        {
            _logger.LogInfo($"OnHeroCardSelected викликано для {archetypeId}", "LobbyView");
            _presenter?.OnHeroSelected(archetypeId);
        }

        private void HideError()
        {
            if (_errorText)
                _errorText.gameObject.SetActive(false);
        }

        private void ClearContainer(Transform container)
        {
            if (container == null)
                return;

            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

    }
}
