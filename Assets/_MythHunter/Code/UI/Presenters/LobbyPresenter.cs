// Assets/_MythHunter/Code/UI/Presenters/LobbyPresenter.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Systems.Lobby;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;
using MythHunter.Core.Game;
using MythHunter.Services.GameSettings;
using MythHunter.Resources.Pool;
using UnityEngine;

namespace MythHunter.UI.Presenters
{
    public class LobbyPresenter : ILobbyPresenter, IEventSubscriber
    {
        private readonly ILobbySystem _lobbySystem;
        private readonly IHeroSelectionSystem _heroSelectionSystem;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly ILobbyModel _model;
        private readonly IUIComponentFactory _componentFactory;
        private readonly IGameFlowManager _gameFlowManager;
        private readonly IGameSettingsService _gameSettings;
        private readonly IPoolManager _poolManager;

        private ILobbyView _view;
        private bool _isSubscribed = false;
        private int _currentPlayerIndex = 0;

        // Зберігаємо посилання на створені картки для управління
        private readonly List<HeroCardUI> _createdHeroCards = new List<HeroCardUI>();
        private readonly List<HeroCardUI> _selectedHeroCards = new List<HeroCardUI>();

        private const string HERO_CARD_POOL_KEY = "HeroCardUI";

        [Inject]
        public LobbyPresenter(
            IEventBus eventBus,
            IMythLogger logger,
            ILobbySystem lobbySystem,
            IHeroSelectionSystem heroSelectionSystem,
            ILobbyModel model,
            IUIComponentFactory componentFactory,
            IGameFlowManager gameFlowManager,
            IGameSettingsService gameSettings,
            IPoolManager poolManager)
        {
            _eventBus = eventBus;
            _logger = logger;
            _lobbySystem = lobbySystem;
            _heroSelectionSystem = heroSelectionSystem;
            _model = model;
            _componentFactory = componentFactory;
            _gameFlowManager = gameFlowManager;
            _gameSettings = gameSettings;
            _poolManager = poolManager;

            // Ініціалізація пулу карток
            InitializeCardPool();
        }

        public void Initialize(ILobbyView view)
        {
            _view = view;
            SubscribeToEvents();

            _logger.LogInfo("LobbyPresenter ініціалізовано", "LobbyPresenter");

            // Асинхронне завантаження героїв
            UniTask.Create(async () => {
                try
                {
                    // Завантажуємо героїв
                    await _heroSelectionSystem.LoadAvailableHeroes();
                    _logger.LogInfo("Героїв асинхронно завантажено", "LobbyPresenter");

                    // Ініціалізуємо лобі
                    if (!_lobbySystem.IsInitialized)
                    {
                        _lobbySystem.InitializeLobby(_gameSettings.PlayerCount);
                    }

                    // Оновлюємо UI з картками героїв
                    await PopulateHeroCardsAsync();

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка при завантаженні героїв: {ex.Message}", "LobbyPresenter");
                }
            });
        }

        public async UniTask InitializeAsync()
        {
            SubscribeToEvents();

            _logger.LogInfo("LobbyPresenter асинхронно ініціалізовано", "LobbyPresenter");

            // Завантажуємо героїв
            await _heroSelectionSystem.LoadAvailableHeroes();

            // Ініціалізуємо лобі, якщо потрібно
            if (!_lobbySystem.IsInitialized)
            {
                _lobbySystem.InitializeLobby(_gameSettings.PlayerCount);
            }
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();

            // Повертаємо всі картки в пул
            ReturnAllCardsToPool();

            _logger.LogInfo("LobbyPresenter disposed", "LobbyPresenter");
        }

        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<LobbyInitializedEvent>(OnLobbyInitialized);
            _eventBus.Subscribe<HeroSelectedEvent>(OnHeroSelectedEvent);
            _eventBus.Subscribe<SelectionConfirmedEvent>(OnSelectionConfirmedEvent);
            _eventBus.Subscribe<SelectionTimerUpdatedEvent>(OnTimerUpdated);
            _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            _eventBus.Subscribe<LobbyStateEnteredEvent>(OnLobbyStateEntered);

            _isSubscribed = true;
        }

        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<LobbyInitializedEvent>(OnLobbyInitialized);
            _eventBus.Unsubscribe<HeroSelectedEvent>(OnHeroSelectedEvent);
            _eventBus.Unsubscribe<SelectionConfirmedEvent>(OnSelectionConfirmedEvent);
            _eventBus.Unsubscribe<SelectionTimerUpdatedEvent>(OnTimerUpdated);
            _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            _eventBus.Unsubscribe<LobbyStateEnteredEvent>(OnLobbyStateEntered);

            _isSubscribed = false;
        }

        public void OnHeroSelected(string archetypeId)
        {
            if (_lobbySystem.SelectHero(archetypeId))
            {
                // Система вибрала героя успішно
                // Оновлення UI відбувається через обробку подій
            }
            else
            {
                // Сповіщаємо про помилку
                _view.ShowError("Не вдалося вибрати героя. Перевірте мана або доступність героя.");
            }
        }

        public void OnSelectionConfirmed()
        {
            _lobbySystem.ConfirmSelection();
        }

        public async UniTask StartGameAsync()
        {
            if (!_lobbySystem.AreAllPlayersReady())
            {
                _view.ShowError("Не всі гравці готові");
                return;
            }

            _view.ShowGameStartingMessage();

            // Отримуємо список вибраних героїв
            var selectedHeroes = _lobbySystem.GetSelectedHeroes();

            // Переходимо до гри через GameFlowManager
            await _gameFlowManager.EnterGameplayAsync(selectedHeroes.ToArray());
        }

        private void OnLobbyInitialized(LobbyInitializedEvent evt)
        {
            UniTask.Create(async () => {
                await PopulateHeroCardsAsync();
                _view.UpdateMana(evt.ManaPerPlayer, evt.ManaPerPlayer);
                _view.UpdateTimer(evt.SelectionTimeLimit, evt.SelectionTimeLimit);
            });
        }

        private void OnHeroSelectedEvent(HeroSelectedEvent evt)
        {
            UniTask.Create(async () => {
                // Оновлюємо вибраних героїв
                await UpdateSelectedHeroesAsync();

                // Оновлюємо ману
                _view.UpdateMana(evt.RemainingMana, 4);

                // Оновлюємо стан доступних карток
                await UpdateHeroCardsStateAsync();
            });
        }

        private void OnSelectionConfirmedEvent(SelectionConfirmedEvent evt)
        {
            _view.ShowPlayerStatus(evt.PlayerIndex, true);

            if (_lobbySystem.AreAllPlayersReady())
            {
                _view.ShowGameStartingMessage();
            }
            else
            {
                UniTask.Create(async () => {
                    await PopulateHeroCardsAsync();
                    await UpdateSelectedHeroesAsync();
                    _view.UpdateMana(_lobbySystem.GetRemainingManaForCurrentPlayer(), 4);
                });
            }
        }

        private void OnTimerUpdated(SelectionTimerUpdatedEvent evt)
        {
            _view.UpdateTimer(evt.RemainingTime, 300f);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            if (evt.NewState == GameStateType.Lobby)
            {
                UniTask.Create(async () => {
                    await PopulateHeroCardsAsync();
                });
            }
        }

        private void OnLobbyStateEntered(LobbyStateEnteredEvent evt)
        {
            // Ініціалізуємо лобі, якщо ще не ініціалізовано
            if (!_lobbySystem.IsInitialized)
            {
                _lobbySystem.InitializeLobby(_gameSettings.PlayerCount);
            }

            // Оновлюємо UI
            UniTask.Create(async () => {
                await PopulateHeroCardsAsync();
            });
        }

        // Метод створення пулу карток
        private void InitializeCardPool()
        {
            try
            {
                // Перевіряємо, чи існує пул
                if (!_poolManager.HasPool(HERO_CARD_POOL_KEY))
                {
                    // Асинхронно завантажуємо префаб картки
                    UniTask.Create(async () => {
                        var cardPrefab = await _componentFactory.CreateComponentAsync<HeroCardUI>("UI/Prefabs/HeroCardUI");
                        if (cardPrefab != null)
                        {
                            _poolManager.CreatePool<GameObject>(HERO_CARD_POOL_KEY, cardPrefab.gameObject, 20);
                            _logger.LogInfo("Створено пул HeroCardUI", "LobbyPresenter");
                        }
                        else
                        {
                            _logger.LogError("Не вдалося завантажити префаб HeroCardUI", "LobbyPresenter");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при ініціалізації пулу: {ex.Message}", "LobbyPresenter");
            }
        }

        // Методи роботи з картками героїв
        private async UniTask PopulateHeroCardsAsync()
        {
            var heroes = GetAvailableHeroes();
            if (heroes == null || heroes.Count == 0)
            {
                _logger.LogWarning("Немає героїв для відображення", "LobbyPresenter");
                return;
            }

            // Повертаємо всі картки в пул перед створенням нових
            ReturnAllCardsToPool();

            // Створюємо нові картки
            foreach (var hero in heroes)
            {
                var cardUI = await CreateHeroCardAsync(hero, _view.HeroCardsContainer);
                if (cardUI != null)
                {
                    _createdHeroCards.Add(cardUI);
                }
            }

            // Оновлюємо вибраних героїв
            await UpdateSelectedHeroesAsync();
        }

        private async UniTask UpdateHeroCardsStateAsync()
        {
            var heroes = GetAvailableHeroes();
            if (heroes == null || heroes.Count == 0)
                return;

            var heroesMap = heroes.ToDictionary(h => h.ArchetypeId, h => h);

            // Оновлюємо існуючі картки або видаляємо непотрібні
            foreach (var card in _createdHeroCards.ToList())
            {
                if (card == null)
                    continue;

                if (heroesMap.TryGetValue(card.ArchetypeId, out var model))
                {
                    // Оновлюємо стан картки
                    card.SetInteractable(model.IsSelectable);
                    // Видаляємо з мапи, щоб знати, які ще потрібно створити
                    heroesMap.Remove(card.ArchetypeId);
                }
                else
                {
                    // Повертаємо непотрібну картку в пул
                    ReturnCardToPool(card);
                    _createdHeroCards.Remove(card);
                }
            }

            // Створюємо нові картки для решти героїв
            foreach (var pair in heroesMap)
            {
                var cardUI = await CreateHeroCardAsync(pair.Value, _view.HeroCardsContainer);
                if (cardUI != null)
                {
                    _createdHeroCards.Add(cardUI);
                }
            }
        }

        private async UniTask UpdateSelectedHeroesAsync()
        {
            var selectedHeroes = GetSelectedHeroes();

            // Повертаємо всі старі картки в пул
            foreach (var card in _selectedHeroCards)
            {
                ReturnCardToPool(card);
            }
            _selectedHeroCards.Clear();

            // Створюємо нові картки для вибраних героїв
            foreach (var hero in selectedHeroes)
            {
                var cardUI = await CreateHeroCardAsync(hero, _view.SelectedHeroesContainer, false);
                if (cardUI != null)
                {
                    _selectedHeroCards.Add(cardUI);
                }
            }
        }

        // Допоміжні методи для роботи з пулом
        private async UniTask<HeroCardUI> CreateHeroCardAsync(HeroCardModel model, Transform parent, bool interactive = true)
        {
            try
            {
                // Отримуємо GameObject з пулу
                var cardObject = _poolManager.GetFromPool<GameObject>(HERO_CARD_POOL_KEY);
                if (cardObject == null)
                {
                    _logger.LogError("Не вдалося отримати картку з пулу", "LobbyPresenter");
                    return null;
                }

                // Налаштовуємо трансформацію
                cardObject.transform.SetParent(parent, false);

                // Отримуємо компонент HeroCardUI
                var cardUI = cardObject.GetComponent<HeroCardUI>();
                if (cardUI == null)
                {
                    _logger.LogError("GameObject з пулу не містить HeroCardUI", "LobbyPresenter");
                    _poolManager.ReturnToPool(HERO_CARD_POOL_KEY, cardObject);
                    return null;
                }

                // Скидаємо стан картки
                cardUI.Reset();

                // Налаштовуємо картку
                cardUI.Setup(model);

                // Підписуємося на подію вибору
                if (interactive)
                {
                    cardUI.OnHeroSelected += OnHeroSelected;
                }

                // Активуємо GameObject
                cardObject.SetActive(true);

                return cardUI;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при створенні картки героя: {ex.Message}", "LobbyPresenter");
                return null;
            }
        }

        private void ReturnCardToPool(HeroCardUI card)
        {
            if (card == null)
                return;

            try
            {
                // Відписуємося від подій
                card.OnHeroSelected -= OnHeroSelected;

                // Скидаємо стан
                card.Reset();

                // Деактивуємо GameObject
                card.gameObject.SetActive(false);

                // Повертаємо в пул
                _poolManager.ReturnToPool(HERO_CARD_POOL_KEY, card.gameObject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при поверненні картки в пул: {ex.Message}", "LobbyPresenter");
            }
        }

        private void ReturnAllCardsToPool()
        {
            // Повертаємо доступні картки
            foreach (var card in _createdHeroCards)
            {
                ReturnCardToPool(card);
            }
            _createdHeroCards.Clear();

            // Повертаємо вибрані картки
            foreach (var card in _selectedHeroCards)
            {
                ReturnCardToPool(card);
            }
            _selectedHeroCards.Clear();
        }

        // Методи для отримання даних
        // Продовження Assets/_MythHunter/Code/UI/Presenters/LobbyPresenter.cs

        public List<HeroCardModel> GetAvailableHeroes()
        {
            try
            {
                // Отримуємо всі доступні архетипи героїв
                var allHeroIds = _heroSelectionSystem.GetHeroesByCategory()
                    .SelectMany(category => category.Value)
                    .ToList();

                if (allHeroIds.Count == 0)
                {
                    _logger.LogWarning("Отримано 0 архетипів героїв", "LobbyPresenter");
                }

                // Отримуємо список вже вибраних героїв
                var selectedHeroIds = _lobbySystem.GetSelectedHeroes();

                // Для кожного архетипу створюємо модель
                var result = allHeroIds
                    .Select(id => _heroSelectionSystem.GetHeroInfo(id))
                    .Where(info => info != null)
                    .Select(info => new HeroCardModel
                    {
                        ArchetypeId = info.ArchetypeId,
                        Name = info.Name,
                        Description = info.Description,
                        Race = info.Race,
                        Class = info.Class,
                        ManaCost = info.ManaCost,
                        IconPath = info.IconPath,
                        IsSelectable = !selectedHeroIds.Contains(info.ArchetypeId) &&
                                      _lobbySystem.GetRemainingManaForCurrentPlayer() >= info.ManaCost &&
                                      _heroSelectionSystem.CanSelectHero(info.ArchetypeId, _currentPlayerIndex, _lobbySystem.GetRemainingManaForCurrentPlayer()),
                        IsSelected = selectedHeroIds.Contains(info.ArchetypeId)
                    })
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка в GetAvailableHeroes: {ex.Message}", "LobbyPresenter");
                return new List<HeroCardModel>();
            }
        }

        public List<HeroCardModel> GetSelectedHeroes()
        {
            return _lobbySystem.GetSelectedHeroes()
                .Select(id => _heroSelectionSystem.GetHeroInfo(id))
                .Where(info => info != null)
                .Select(info => new HeroCardModel
                {
                    ArchetypeId = info.ArchetypeId,
                    Name = info.Name,
                    Description = info.Description,
                    Race = info.Race,
                    Class = info.Class,
                    ManaCost = info.ManaCost,
                    IconPath = info.IconPath,
                    IsSelectable = false,
                    IsSelected = true
                }).ToList();
        }

        public int GetRemainingMana()
        {
            return _lobbySystem.GetRemainingManaForCurrentPlayer();
        }

        public float GetRemainingTime()
        {
            return 300f; // За замовчуванням 5 хвилин
        }
    }
}
