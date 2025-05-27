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
using MythHunter.UI.Services;
using MythHunter.UI.Navigation;
using MythHunter.Systems.Core;
using UnityEngine;
using MythHunter.Systems.AI;
using MythHunter.Core.SceneManagement;

namespace MythHunter.UI.Presenters
{
    public class LobbyPresenter : ILobbyPresenter, IEventSubscriber
    {
        // Додаємо властивість IsInitialized
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        private readonly ILobbySystem _lobbySystem;
        private readonly IHeroSelectionSystem _heroSelectionSystem;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly ILobbyModel _model;
        private readonly IGameFlowManager _gameFlowManager;
        private readonly IGameSettingsService _gameSettings;
        private readonly ITimerSystem _timerSystem;
        private readonly INavigationService _navigationService;
        private readonly IDIContainer _container;

        // ✅ ЄДИНА ЗАЛЕЖНІСТЬ для роботи з UI
        private readonly IUIViewFactory _uiViewFactory;

        private ILobbyView _view;
        private bool _isSubscribed = false;
        private int _currentPlayerIndex = 0;

        private readonly List<HeroCardUI> _createdHeroCards = new();
        private readonly List<HeroCardUI> _selectedHeroCards = new();
        [Inject]
        private readonly ISimpleLobbyAI _aiSystem;
        [Inject]
        public LobbyPresenter(
            IEventBus eventBus,
            IMythLogger logger,
            ILobbySystem lobbySystem,
            IHeroSelectionSystem heroSelectionSystem,
            ILobbyModel model,
            IGameFlowManager gameFlowManager,
            IGameSettingsService gameSettings,
            IUIViewFactory uiViewFactory,        // ✅ ЄДИНА UI ЗАЛЕЖНІСТЬ
            INavigationService navigationService,
            ITimerSystem timerSystem,
            IDIContainer container)
        {
            _eventBus = eventBus;
            _logger = logger;
            _lobbySystem = lobbySystem;
            _heroSelectionSystem = heroSelectionSystem;
            _model = model;
            _gameFlowManager = gameFlowManager;
            _gameSettings = gameSettings;
            _uiViewFactory = uiViewFactory;      // ✅ ЄДИНИЙ СПОСІБ РОБОТИ З UI
            _navigationService = navigationService;
            _timerSystem = timerSystem;
            _container = container;
        }

        public async UniTask InitializeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                SubscribeToEvents();
                await _heroSelectionSystem.LoadAvailableHeroes();
                _isInitialized = true;

                _logger.LogInfo("✅ LobbyPresenter ініціалізовано успішно", "UI");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка при ініціалізації LobbyPresenter: {ex.Message}", "UI", ex);
            }
        }

        public void Initialize(ILobbyView view)
        {
            _view = view;

            // ✅ ОТРИМУЄМО РЕЖИМ ГРИ З SceneDispatcher
            var sceneDispatcher = _container.Resolve<ISceneDispatcher>();
            var gameMode = sceneDispatcher.GetSceneData<GameMode>("GameMode", GameMode.PvAI);

            // ✅ Налаштовуємо UI для режиму
            _view.ConfigureForGameMode(gameMode);

            UniTask.Create(async () => {
                try
                {
                    if (_view?.HeroCardsContainer == null)
                    {
                        // Чекаємо ініціалізацію
                        for (int i = 0; i < 30; i++)
                        {
                            await UniTask.Delay(100);
                            if (_view?.HeroCardsContainer != null)
                                break;
                        }
                    }

                    await InitializeAsync();
                    await PopulateHeroCardsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Помилка в ініціалізації: {ex.Message}", "UI", ex);
                }
            });
        }




        private void ConfigureUIForGameMode()
        {
            if (_view == null)
                return;

            switch (_gameSettings.CurrentGameMode)
            {
                case GameMode.PvAI:
                    // Показуємо тільки 1 плеєра, AI невидимий
                    _view.ShowPlayerStatus(0, false); // Людина
                                                      // Ховаємо UI другого гравця
                    break;

                case GameMode.LocalPvP:
                    // Показуємо обох гравців
                    _view.ShowPlayerStatus(0, false);
                    _view.ShowPlayerStatus(1, false);
                    break;

                case GameMode.OnlinePvP:
                    // Поки що як LocalPvP, пізніше додамо мережевий функціонал
                    _view.ShowPlayerStatus(0, false);
                    _view.ShowPlayerStatus(1, false);
                    break;
            }
        }
        // ✅ НОВИЙ метод для AI подій
        private void OnAIHeroSelected(HeroSelectedEvent evt)
        {
            if (evt.PlayerIndex == 1 && _gameSettings.IsAIEnabled) // AI гравець
            {
                _view?.ShowAIStatus($"AI обрав: {evt.ArchetypeId}", false);

                // Якщо AI закінчив з маною, показуємо готовність до старту
                if (evt.RemainingMana == 0)
                {
                    _view?.ShowAIStatus("AI готовий до гри!", false);

                    // Автоматично показуємо кнопку старту
                    UniTask.Create(async () => {
                        await UniTask.Delay(1000);
                        if (_lobbySystem.AreAllPlayersReady())
                        {
                            _view?.ShowGameStartingMessage();
                        }
                    });
                }
            }
        }


        public async UniTask InitializeLobbyAsync(int playerCount)
        {
            if (!_isInitialized)
                await InitializeAsync();

            // Ініціалізуємо лобі, якщо воно ще не ініціалізоване
            if (!_lobbySystem.IsInitialized)
            {
                _lobbySystem.InitializeLobby(playerCount);
                _logger.LogInfo($"Lobby ініціалізовано з {playerCount} гравцями", "UI");
            }

            // Оновлюємо картки героїв
            await PopulateHeroCardsAsync();
        }

        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            // ✅ ПІДПИСУЄМОСЯ НА НОВІ ПОДІЇ TimerSystem
            _eventBus.Subscribe<TimerUpdatedEvent>(OnTimerUpdated);
            _eventBus.Subscribe<TimerCompletedEvent>(OnTimerCompleted);

            // ✅ ТАКОЖ ПІДПИСУЄМОСЯ НА СТАРУ ПОДІЮ (для сумісності)
            _eventBus.Subscribe<SelectionTimerUpdatedEvent>(OnSelectionTimerUpdated);

            _eventBus.Subscribe<LobbyStateEnteredEvent>(OnLobbyStateEntered);
            _eventBus.Subscribe<LobbyInitializedEvent>(OnLobbyInitialized);
            _eventBus.Subscribe<HeroSelectedEvent>(OnHeroSelectedEvent);
            _eventBus.Subscribe<SelectionConfirmedEvent>(OnSelectionConfirmedEvent);
            _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

            _isSubscribed = true;
            _logger.LogInfo("LobbyPresenter підписаний на події", "UI");
        }

        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            try
            {
                // ✅ ВІДПИСУЄМОСЯ ВІД НОВИХ ПОДІЙ
                _eventBus.Unsubscribe<TimerUpdatedEvent>(OnTimerUpdated);
                _eventBus.Unsubscribe<TimerCompletedEvent>(OnTimerCompleted);

                // ✅ ВІДПИСУЄМОСЯ ВІД СТАРОЇ ПОДІЇ
                _eventBus.Unsubscribe<SelectionTimerUpdatedEvent>(OnSelectionTimerUpdated);

                _eventBus.Unsubscribe<LobbyStateEnteredEvent>(OnLobbyStateEntered);
                _eventBus.Unsubscribe<LobbyInitializedEvent>(OnLobbyInitialized);
                _eventBus.Unsubscribe<HeroSelectedEvent>(OnHeroSelectedEvent);
                _eventBus.Unsubscribe<SelectionConfirmedEvent>(OnSelectionConfirmedEvent);
                _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);

                _isSubscribed = false;
                _logger.LogInfo("✅ LobbyPresenter відписався від подій", "UI");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка при відписці від подій: {ex.Message}", "UI", ex);
            }
        }

        public void OnHeroSelected(string archetypeId)
        {
            if (!_lobbySystem.SelectHero(archetypeId))
            {
                _view?.ShowError("Не вдалося вибрати героя. Перевірте мана або доступність героя.");
            }
        }

        public void OnSelectionConfirmed()
        {
            int remainingMana = _lobbySystem.GetRemainingManaForCurrentPlayer();

            if (remainingMana > 0)
            {
                _view?.ShowError("Використайте всю ману перед підтвердженням");
                return;
            }

            _lobbySystem.ConfirmSelection();
        }

        // ✅ ОНОВЛЕНИЙ StartGameAsync з урахуванням режиму
        public async UniTask StartGameAsync()
        {
            switch (_gameSettings.CurrentGameMode)
            {
                case GameMode.PvAI:
                    // В AI режимі достатньо готовності одного гравця
                    if (_lobbySystem.GetRemainingManaForCurrentPlayer() > 0)
                    {
                        _view?.ShowError("Спочатку оберіть всіх героїв");
                        return;
                    }
                    break;

                case GameMode.LocalPvP:
                case GameMode.OnlinePvP:
                    // В PvP режимах потрібна готовність всіх
                    if (!_lobbySystem.AreAllPlayersReady())
                    {
                        _view?.ShowError("Не всі гравці готові");
                        return;
                    }
                    break;
            }

            _view?.ShowGameStartingMessage();

            // Публікуємо подію початку гри
            _eventBus.Publish(new GameStartRequestEvent
            {
                Timestamp = DateTime.UtcNow
            });
            await UniTask.CompletedTask;
        }

        #region Event Handlers

        // ✅ ОНОВЛЕНИЙ обробник вибору героя з автоматичним confirm для AI
        private void OnHeroSelectedEvent(HeroSelectedEvent evt)
        {
            if (_view == null)
                return;

            UniTask.Create(async () => {
                try
                {
                    await UpdateSelectedHeroesAsync();

                    if (_view != null)
                    {
                        _view.UpdateMana(evt.RemainingMana, _gameSettings.ManaPerPlayer);
                    }

                    await UpdateHeroCardsStateAsync();

                    // ✅ АВТОМАТИЧНИЙ CONFIRM В AI РЕЖИМІ
                    if (_gameSettings.IsAIEnabled && evt.PlayerIndex == 0) // Гравець-людина
                    {
                        // Перевіряємо, чи закінчилась мана
                        if (evt.RemainingMana == 0)
                        {
                            _logger.LogInfo("🤖 Мана гравця закінчилась, автоматично підтверджуємо", "UI");

                            // Затримка для плавності
                            await UniTask.Delay(500);

                            // Автоматично підтверджуємо вибір гравця
                            OnSelectionConfirmed();

                            // Показуємо статус AI
                            _view.ShowAIStatus("AI обирає героїв...", true);

                            // AI почне діяти автоматично через події
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Помилка в OnHeroSelectedEvent: {ex.Message}", "UI", ex);
                }
            });
        }
        private void OnSelectionConfirmedEvent(SelectionConfirmedEvent evt)
        {
            _view?.ShowPlayerStatus(evt.PlayerIndex, true);

            if (_lobbySystem.AreAllPlayersReady())
            {
                _view?.ShowGameStartingMessage();
            }
            else
            {
                UniTask.Create(async () => {
                    await PopulateHeroCardsAsync();
                    await UpdateSelectedHeroesAsync();
                    _view?.UpdateMana(_lobbySystem.GetRemainingManaForCurrentPlayer(), 4);
                });
            }
        }

        // ✅ НОВИЙ метод для подій TimerSystem
        private void OnTimerUpdated(TimerUpdatedEvent evt)
        {
            // Обробляємо тільки таймери категорії "Lobby" або з назвою "LobbySelection"
            if ((evt.Category == "Lobby" || evt.TimerName == "LobbySelection") && _view != null)
            {
                _view.UpdateTimer(evt.RemainingTime, 300f);
                _logger.LogDebug($"Timer updated: {evt.RemainingTime:F1}s remaining", "UI");
            }
        }

        // ✅ НОВИЙ метод для завершення таймера
        private void OnTimerCompleted(TimerCompletedEvent evt)
        {
            if ((evt.Category == "Lobby" || evt.TimerName == "LobbySelection") && _view != null)
            {
                _view.ShowError("Час вибору закінчився!");
                _logger.LogInfo("Lobby selection timer completed", "UI");
            }
        }

        // ✅ СТАРИЙ метод (для зворотної сумісності)
        private void OnSelectionTimerUpdated(SelectionTimerUpdatedEvent evt)
        {
            if (_view == null)
            {
                _logger.LogWarning("⚠️ OnSelectionTimerUpdated: _view is null", "UI");
                return;
            }

            try
            {
                _view.UpdateTimer(evt.RemainingTime, _gameSettings.SelectionTimeLimit);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ OnSelectionTimerUpdated error: {ex.Message}", "UI", ex);
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            // ✅ Перевіряємо стан перед викликом
            if (evt.NewState == GameStateType.Lobby && _view != null && _isInitialized)
            {
                UniTask.Create(async () => {
                    try
                    {
                        // Додаткова перевірка перед викликом
                        if (_view != null && this != null)
                        {
                            await PopulateHeroCardsAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Помилка в OnGameStateChanged: {ex.Message}", "UI", ex);
                    }
                });
            }
        }

        private void OnLobbyStateEntered(LobbyStateEnteredEvent evt)
        {
            _logger.LogInfo("Отримано подію LobbyStateEnteredEvent", "UI");

            // ✅ НЕ викликаємо ініціалізацію тут, якщо вже ініціалізовано
            if (_isInitialized)
            {
                _logger.LogInfo("LobbyPresenter вже ініціалізовано", "UI");
                return;
            }
        }

        private void OnLobbyInitialized(LobbyInitializedEvent evt)
        {
            // ✅ НЕ викликаємо PopulateHeroCardsAsync тут
            _view?.UpdateMana(evt.ManaPerPlayer, evt.ManaPerPlayer);
            _view?.UpdateTimer(evt.SelectionTimeLimit, evt.SelectionTimeLimit);
        }

        #endregion

        #region Private Methods

        private async UniTask PopulateHeroCardsAsync()
        {
            if (_view?.HeroCardsContainer == null)
            {
                _logger.LogError("❌ HeroCardsContainer is null у PopulateHeroCardsAsync!", "UI");
                return;
            }

            // Очищуємо старі картки
            foreach (var card in _createdHeroCards.ToList())
            {
                if (card != null)
                {
                    card.OnHeroSelected -= OnHeroSelected;
                    ReturnHeroCardToPool(card);
                }
            }
            _createdHeroCards.Clear();

            // Створюємо нові
            var heroes = GetAvailableHeroes();
            _logger.LogInfo($"🎴 Створюємо {heroes.Count} карток героїв", "UI");

            foreach (var hero in heroes)
            {
                var card = await CreateHeroCardFromFactory(hero, _view.HeroCardsContainer);
                if (card != null)
                {
                    card.OnHeroSelected += OnHeroSelected;
                    _createdHeroCards.Add(card);
                }
            }

            _logger.LogInfo($"✅ Створено {_createdHeroCards.Count} карток героїв", "UI");
        }

        /// <summary>
        /// ✅ АРХІТЕКТУРНО ПРАВИЛЬНЕ створення HeroCard через UIViewFactory
        /// </summary>
        private async UniTask<HeroCardUI> CreateHeroCardFromFactory(HeroCardModel model, Transform parent)
        {
            try
            {
                // ✅ ЄДИНИЙ спосіб створення UI через фабрику
                var view = await _uiViewFactory.CreateViewAsync(ViewId.HeroCard);
                var card = view as HeroCardUI;

                if (card != null)
                {
                    // Налаштовуємо картку
                    card.transform.SetParent(parent, false);

                    // Ін'єктуємо залежності, якщо потрібно
                    _container.InjectDependencies(card);

                    // Налаштовуємо дані
                    card.Setup(model);

                    _logger.LogDebug($"✅ Створено картку для героя: {model.Name}", "UI");
                }
                else
                {
                    _logger.LogError($"❌ View не є HeroCardUI: {view?.GetType().Name}", "UI");
                }

                return card;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка створення картки героя {model.Name}: {ex.Message}", "UI", ex);
                return null;
            }
        }

        /// <summary>
        /// ✅ АРХІТЕКТУРНО ПРАВИЛЬНЕ повернення HeroCard через UIViewFactory
        /// </summary>
        private void ReturnHeroCardToPool(HeroCardUI card)
        {
            if (card == null)
                return;

            try
            {
                // ✅ ЄДИНИЙ спосіб повернення UI через фабрику
                _uiViewFactory.ReturnViewToPool(ViewId.HeroCard, card);
                _logger.LogDebug("✅ Картку повернено через UIViewFactory", "UI");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка при поверненні картки через фабрику: {ex.Message}", "UI", ex);

                // ✅ Запасний варіант - пряме знищення
                if (card?.gameObject != null)
                {
                    UnityEngine.Object.Destroy(card.gameObject);
                    _logger.LogWarning("⚠️ Картку знищено напряму як запасний варіант", "UI");
                }
            }
        }

        private async UniTask UpdateSelectedHeroesAsync()
        {
            // ✅ Очищуємо вибрані картки через фабрику
            foreach (var card in _selectedHeroCards.ToList())
            {
                if (card != null)
                {
                    ReturnHeroCardToPool(card);
                }
            }
            _selectedHeroCards.Clear();

            if (_view?.SelectedHeroesContainer == null)
            {
                _logger.LogWarning("⚠️ SelectedHeroesContainer is null", "UI");
                return;
            }

            var selectedHeroes = GetSelectedHeroes();
            foreach (var hero in selectedHeroes)
            {
                var card = await CreateHeroCardFromFactory(hero, _view.SelectedHeroesContainer);
                if (card != null)
                {
                    card.SetInteractable(false); // Вибрані картки не інтерактивні
                    _selectedHeroCards.Add(card);
                }
            }
        }

        private async UniTask UpdateHeroCardsStateAsync()
        {
            var current = GetAvailableHeroes().ToDictionary(x => x.ArchetypeId);

            foreach (var card in _createdHeroCards.ToList())
            {
                if (card == null || !current.TryGetValue(card.ArchetypeId, out var model))
                {
                    if (card != null)
                        card.OnHeroSelected -= OnHeroSelected;
                    ReturnHeroCardToPool(card);
                    _createdHeroCards.Remove(card);
                    continue;
                }

                card.SetInteractable(model.IsSelectable);
                current.Remove(card.ArchetypeId);
            }

            // Додаємо нові картки для героїв, які з'явились
            foreach (var model in current.Values)
            {
                var card = await CreateHeroCardFromFactory(model, _view.HeroCardsContainer);
                if (card != null)
                {
                    card.OnHeroSelected += OnHeroSelected;
                    _createdHeroCards.Add(card);
                }
            }
        }

        #endregion

        #region Public Methods

        public List<HeroCardModel> GetAvailableHeroes()
        {
            var allHeroIds = _heroSelectionSystem.GetHeroesByCategory().SelectMany(x => x.Value).ToList();
            var selected = _lobbySystem.GetSelectedHeroes();

            return allHeroIds
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
                    IsSelectable = !selected.Contains(info.ArchetypeId) &&
                                   _lobbySystem.GetRemainingManaForCurrentPlayer() >= info.ManaCost &&
                                   _heroSelectionSystem.CanSelectHero(info.ArchetypeId, _currentPlayerIndex, _lobbySystem.GetRemainingManaForCurrentPlayer()),
                    IsSelected = selected.Contains(info.ArchetypeId)
                })
                .ToList();
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

        public int GetRemainingMana() => _lobbySystem.GetRemainingManaForCurrentPlayer();

        public float GetRemainingTime() => _lobbySystem.GetRemainingSelectionTime();

        public async UniTask OpenHeroSelectorAsync()
        {
            var parameters = new NavigationParameters();
            parameters.Add("PlayerCount", _gameSettings.PlayerCount);
            parameters.Add("ManaPerPlayer", _gameSettings.ManaPerPlayer);

            // Використовуємо ViewId замість типу
            await _navigationService.NavigateToAsync(ViewId.HeroCardSelector, parameters, TransitionType.SlideLeft);
        }

        public async UniTask<bool> ShowConfirmationAsync(string message)
        {
            var parameters = new NavigationParameters();
            parameters.Add("Message", message);

            // Використовуємо ViewId
            bool result = await _navigationService.ShowModalAsync<bool>(
                ViewId.ConfirmDialog,
                parameters
            );

            return result;
        }

        public void Dispose()
        {
            _logger?.LogInfo("🗑️ LobbyPresenter.Dispose() викликано", "UI");

            // ✅ СПОЧАТКУ ВІДПИСУЄМОСЯ
            UnsubscribeFromEvents();

            // ✅ ОЧИЩУЄМО РЕСУРСИ через UIViewFactory
            try
            {
                // Очищуємо створені картки
                foreach (var card in _createdHeroCards.ToList())
                {
                    if (card != null)
                    {
                        card.OnHeroSelected -= OnHeroSelected;
                        ReturnHeroCardToPool(card);
                    }
                }
                _createdHeroCards.Clear();

                // Очищуємо вибрані картки
                foreach (var card in _selectedHeroCards.ToList())
                {
                    if (card != null)
                    {
                        ReturnHeroCardToPool(card);
                    }
                }
                _selectedHeroCards.Clear();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Помилка при очищенні карток: {ex.Message}", "UI");
            }

            // ✅ ОБНУЛЯЄМО _view В КІНЦІ
            _view = null;
            _isInitialized = false;

            _logger?.LogInfo("✅ LobbyPresenter disposed", "UI");
        }

        #endregion
    }
}
