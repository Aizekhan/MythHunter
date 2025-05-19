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
using MythHunter.Resources.Pool;
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

        //Пул система карток
        private const string HERO_CARD_POOL_KEY = "HeroCardUI";
        private IPoolManager _poolManager;
        private bool _isPoolInitialized = false;

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

                    // Отримуємо PoolManager через контейнер
                    _poolManager = container.Resolve<IPoolManager>();
                    if (_poolManager != null)
                    {
                        _logger.LogInfo("Отримано PoolManager через GameBootstrapper", "LobbyView");
                        InitializeCardPool();
                    }
                    else
                    {
                        _logger.LogWarning("Не вдалося отримати PoolManager", "LobbyView");
                    }
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
            ReturnAllCardsToPool();

            // Відписуємось від евентів через презентер
            if (_presenter != null)
            {
                _presenter.Dispose();
            }

            if (_confirmButton)
                _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            if (_startGameButton)
                _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);

            _logger.LogInfo("OnDestroy викликано", "LobbyView");
        }




        public void UpdateHeroCardsState(List<HeroCardModel> heroes)
        {
            if (_createdCards == null || heroes == null)
            {
                _logger.LogWarning("_createdCards або heroes є null", "LobbyView");
                return;
            }

            _logger.LogInfo($"UpdateHeroCardsState викликано з {heroes.Count} героями", "LobbyView");

            // Словник для швидкого пошуку моделей за ID
            var heroModelsDict = heroes.ToDictionary(h => h.ArchetypeId, h => h);

            // Список карток, які потрібно повернути в пул (не знайдені в оновленому списку)
            var cardsToRemove = new List<HeroCardUI>();

            // Оновлюємо існуючі картки
            foreach (var card in _createdCards)
            {
                if (card == null)
                    continue;

                if (heroModelsDict.TryGetValue(card.ArchetypeId, out var model))
                {
                    // Оновлюємо стан існуючої картки
                    card.SetInteractable(model.IsSelectable);

                    // Видаляємо з словника (щоб знати, які картки ще потрібно створити)
                    heroModelsDict.Remove(card.ArchetypeId);
                }
                else
                {
                    // Картка не знайдена в оновленому списку, відмічаємо для повернення в пул
                    cardsToRemove.Add(card);
                }
            }

            // Повертаємо непотрібні картки в пул
            foreach (var card in cardsToRemove)
            {
                ReturnCardToPool(card.gameObject);
                _createdCards.Remove(card);
            }

            // Створюємо нові картки для моделей, які не мають відповідних карток
            foreach (var heroEntry in heroModelsDict)
            {
                var hero = heroEntry.Value;
                var cardObject = GetCardFromPool();

                if (cardObject != null)
                {
                    cardObject.transform.SetParent(_heroCardsContainer, false);
                    cardObject.SetActive(true);

                    var ui = cardObject.GetComponent<HeroCardUI>();
                    if (ui != null)
                    {
                        GameBootstrapper.Instance?.RegisterForInjection(ui);
                        ui.Setup(hero);
                        ui.OnHeroSelected += OnHeroCardSelected;
                        _createdCards.Add(ui);

                        _logger.LogInfo($"Створено нову картку для героя {hero.Name}", "LobbyView");
                    }
                }
            }
        }
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
                // Повертаємо всі картки в пул перед створенням нових
                ReturnAllCardsToPool();

                foreach (var hero in heroes)
                {
                    try
                    {
                        // Отримуємо картку з пулу або створюємо нову
                        GameObject cardObject = GetCardFromPool();

                        if (cardObject == null)
                        {
                            _logger.LogError("Не вдалося отримати картку з пулу і створити нову", "LobbyView");
                            continue;
                        }

                        // Встановлюємо батьківський об'єкт і активуємо
                        cardObject.transform.SetParent(_heroCardsContainer, false);
                        cardObject.SetActive(true);

                        var ui = cardObject.GetComponent<HeroCardUI>();
                        if (ui != null)
                        {
                            // Ін'єкція залежностей через GameBootstrapper
                            if (GameBootstrapper.Instance != null)
                            {
                                GameBootstrapper.Instance.RegisterForInjection(ui);
                            }

                            ui.Setup(hero);
                            ui.OnHeroSelected += OnHeroCardSelected;
                            _createdCards.Add(ui);

                            _logger.LogInfo($"Отримано картку з пулу для героя {hero.Name}", "LobbyView");
                        }
                        else
                        {
                            _logger.LogError("Не знайдено компонент HeroCardUI на об'єкті з пулу", "LobbyView");
                            ReturnCardToPool(cardObject);
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
        //Пул карт
        private void InitializeCardPool()
        {
            if (_poolManager != null && _heroCardPrefab != null && !_isPoolInitialized)
            {
                try
                {
                    // Якщо пул вже існує, ми просто використовуємо його
                    if (!_poolManager.HasPool(HERO_CARD_POOL_KEY))
                    {
                        _poolManager.CreatePool<GameObject>(HERO_CARD_POOL_KEY, _heroCardPrefab, 20);
                        _logger.LogInfo($"Створено пул карток героїв з розміром 20", "LobbyView");
                    }
                    else
                    {
                        _logger.LogInfo("Пул карток героїв вже існує, використовуємо його", "LobbyView");
                    }

                    _isPoolInitialized = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка при ініціалізації пулу: {ex.Message}", "LobbyView");
                }
            }
        }
        private void ReturnAllCardsToPool()
        {
            // Повертаємо всі картки в пул
            foreach (var card in _createdCards)
            {
                if (card != null)
                {
                    ReturnCardToPool(card.gameObject);
                }
            }

            _createdCards.Clear();

            // Додатково перевіряємо, чи залишилися діти в контейнері
            foreach (Transform child in _heroCardsContainer)
            {
                if (child.gameObject != null)
                {
                    // Якщо залишилися діти, повертаємо їх у пул
                    ReturnCardToPool(child.gameObject);
                }
            }
        }
        private void ReturnCardToPool(GameObject cardObject)
        {
            if (cardObject == null)
                return;

            // Відключаємо подію для картки
            var ui = cardObject.GetComponent<HeroCardUI>();
            if (ui != null)
            {
                ui.OnHeroSelected -= OnHeroCardSelected;
            }

            // Деактивуємо об'єкт перед поверненням у пул
            cardObject.SetActive(false);

            // Повертаємо в пул
            if (_poolManager != null && _isPoolInitialized)
            {
                try
                {
                    _poolManager.ReturnToPool(HERO_CARD_POOL_KEY, cardObject);
                    _logger.LogInfo("Повернуто картку в пул", "LobbyView");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка при поверненні картки в пул: {ex.Message}", "LobbyView");
                    // Знищуємо об'єкт, якщо не вдалося повернути в пул
                    Destroy(cardObject);
                }
            }
            else
            {
                // Якщо пул не доступний, просто знищуємо об'єкт
                Destroy(cardObject);
            }
        }
        private GameObject GetCardFromPool()
        {
            // Спробуємо отримати об'єкт з пулу
            if (_poolManager != null && _isPoolInitialized)
            {
                try
                {
                    var cardObject = _poolManager.GetFromPool<GameObject>(HERO_CARD_POOL_KEY);
                    if (cardObject != null)
                    {
                        return cardObject;
                    }
                    else
                    {
                        _logger.LogWarning("Отримано null з пулу карток", "LobbyView");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка при отриманні картки з пулу: {ex.Message}", "LobbyView");
                }
            }

            // Якщо не вдалося отримати з пулу, створюємо новий об'єкт
            _logger.LogInfo("Створюємо нову картку героя, оскільки не вдалося отримати з пулу", "LobbyView");
            return Instantiate(_heroCardPrefab);
        }
        private void ClearHeroCards()
        {
            ReturnAllCardsToPool();
        }

        // Модифікуємо UpdateSelectedHeroes, щоб використовувати пул для вибраних героїв
        public void UpdateSelectedHeroes(List<HeroCardModel> selectedHeroes)
        {
            // Повертаємо всі картки з контейнера вибраних героїв у пул
            foreach (Transform child in _selectedHeroesContainer)
            {
                if (child.gameObject != null)
                {
                    ReturnCardToPool(child.gameObject);
                }
            }

            // Створюємо нові картки для вибраних героїв з пулу
            foreach (var hero in selectedHeroes)
            {
                var cardObject = GetCardFromPool();
                if (cardObject != null)
                {
                    cardObject.transform.SetParent(_selectedHeroesContainer, false);
                    cardObject.SetActive(true);

                    var ui = cardObject.GetComponent<HeroCardUI>();
                    if (ui != null)
                    {
                        GameBootstrapper.Instance?.RegisterForInjection(ui);
                        ui.Setup(hero);
                        ui.SetInteractable(false);
                    }
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
