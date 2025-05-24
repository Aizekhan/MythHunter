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

namespace MythHunter.UI.Presenters
{
    public class LobbyPresenter : ILobbyPresenter, IEventSubscriber
    {
        // Додаємо властивість IsInitialized
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
        // Додаємо семафор для уникнення повторних ініціалізацій
        private readonly object _initializationLock = new object();
        private bool _isInitializing = false;

        private readonly ILobbySystem _lobbySystem;
        private readonly IHeroSelectionSystem _heroSelectionSystem;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly ILobbyModel _model;
        private readonly IGameFlowManager _gameFlowManager;
        private readonly IGameSettingsService _gameSettings;
        private readonly IHeroCardService _heroCardService;
        private readonly ITimerSystem _timerSystem;
        private ILobbyView _view;
        private bool _isSubscribed = false;
        private int _currentPlayerIndex = 0;
        private const float SELECTION_TIME_LIMIT = 300f;
        private readonly List<HeroCardUI> _createdHeroCards = new();
        private readonly List<HeroCardUI> _selectedHeroCards = new();
        private readonly INavigationService _navigationService;

        [Inject]
        public LobbyPresenter(
            IEventBus eventBus,
            IMythLogger logger,
            ILobbySystem lobbySystem,
            IHeroSelectionSystem heroSelectionSystem,
            ILobbyModel model,
            IGameFlowManager gameFlowManager,
            IGameSettingsService gameSettings,
            IHeroCardService heroCardService,
            INavigationService navigationService,
            ITimerSystem timerSystem)
        {
            _eventBus = eventBus;
            _logger = logger;
            _lobbySystem = lobbySystem;
            _heroSelectionSystem = heroSelectionSystem;
            _model = model;
            _gameFlowManager = gameFlowManager;
            _gameSettings = gameSettings;
            _heroCardService = heroCardService;
            _navigationService = navigationService;
            _timerSystem = timerSystem;
        }

        public async UniTask InitializeAsync()
        {
            if (_isInitialized)
                return;

            lock (_initializationLock)
            {
                if (_isInitializing)
                    return;
                _isInitializing = true;
            }

            try
            {
                SubscribeToEvents();
                await _heroCardService.InitializeAsync();
                await _heroSelectionSystem.LoadAvailableHeroes();

                _isInitialized = true;
                _logger.LogInfo("LobbyPresenter ініціалізовано", "UI");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при ініціалізації LobbyPresenter: {ex.Message}", "UI", ex);
            }
            finally
            {
                lock (_initializationLock)
                {
                    _isInitializing = false;
                }
            }
        }

        public void Initialize(ILobbyView view)
        {
            if (_isInitialized)
            {
                _logger.LogWarning("LobbyPresenter вже ініціалізовано", "UI");
                return;
            }

            _view = view;

            UniTask.Create(async () => {
                try
                {
                    // ✅ Перевіряємо view
                    if (_view?.HeroCardsContainer == null)
                    {
                        _logger.LogError("❌ HeroCardsContainer is null!", "UI");

                        // Чекаємо до 3 секунд
                        for (int i = 0; i < 30; i++)
                        {
                            await UniTask.Delay(100);
                            if (_view?.HeroCardsContainer != null)
                                break;
                        }

                        if (_view?.HeroCardsContainer == null)
                        {
                            _logger.LogError("❌ HeroCardsContainer так і не ініціалізувався!", "UI");
                            return;
                        }
                    }

                    await InitializeAsync();

                    // ✅ ТІЛЬКИ один виклик
                    await PopulateHeroCardsAsync();

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка в ініціалізації: {ex.Message}", "UI", ex);
                }
            });
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
            var selectedHeroes = _lobbySystem.GetSelectedHeroes();
            await _gameFlowManager.EnterGameplayAsync(selectedHeroes.ToArray());
        }

        #region Event Handlers

        private void OnHeroSelectedEvent(HeroSelectedEvent evt)
        {
            if (_view == null)
            {
                _logger.LogWarning("⚠️ OnHeroSelectedEvent: _view is null", "UI");
                return;
            }

            UniTask.Create(async () => {
                try
                {
                    await UpdateSelectedHeroesAsync();

                    // ✅ ПЕРЕВІРЯЄМО _view ПЕРЕД КОЖНИМ ВИКЛИКОМ
                    if (_view != null)
                    {
                        _view.UpdateMana(evt.RemainingMana, 4);
                    }

                    await UpdateHeroCardsStateAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Помилка в OnHeroSelectedEvent: {ex.Message}", "UI", ex);
                }
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
                _view.UpdateTimer(evt.RemainingTime, SELECTION_TIME_LIMIT);
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
            // 🔥 ЛОГУЄМО НА САМОМУ ПОЧАТКУ
            _logger.LogInfo($"🎴 PopulateHeroCardsAsync ПОЧАТОК: _view = {_view?.GetType().Name ?? "NULL"}", "UI");
            if (_view == null)
            {
                _logger.LogError("❌ _view is null! Спочатку потрібно викликати Initialize(view)", "UI");
                return;
            }
            // 🔥 КРИТИЧНІ ПЕРЕВІРКИ НА ПОЧАТКУ
            if (_heroCardService == null)
            {
                _logger.LogError("❌ _heroCardService is null", "UI");
                return;
            }

            if (_view == null)
            {
                _logger.LogError("❌ PopulateHeroCardsAsync: _view is null!", "UI");
                return;
            }

            if (_view.HeroCardsContainer == null)
            {
                _logger.LogError("❌ _view.HeroCardsContainer is null", "UI");

                // 🔥 ЧЕКАЄМО З ДОДАТКОВИМИ ПЕРЕВІРКАМИ
                for (int i = 0; i < 50; i++)
                {
                    await UniTask.Delay(100);

                    if (_view == null)
                    {
                        _logger.LogError("❌ _view стало null під час очікування!", "UI");
                        return;
                    }

                    if (_view.HeroCardsContainer != null)
                    {
                        _logger.LogInfo("✅ HeroCardsContainer з'явився після очікування", "UI");
                        break;
                    }
                }

                if (_view?.HeroCardsContainer == null)
                {
                    _logger.LogError("❌ HeroCardsContainer так і не ініціалізувався!", "UI");
                    return;
                }
            }

            // Решта коду з додатковими перевірками...
            var heroes = GetAvailableHeroes();
            _logger.LogInfo($"🎴 Створюємо {heroes.Count} карток героїв", "UI");

            foreach (var hero in heroes)
            {
                // ✅ ПЕРЕВІРЯЄМО _view НА КОЖНІЙ ІТЕРАЦІЇ
                if (_view == null)
                {
                    _logger.LogError("❌ _view стало null під час створення карток!", "UI");
                    break;
                }

                try
                {
                    var card = await _heroCardService.CreateCardAsync(hero, _view.HeroCardsContainer);
                    if (card != null)
                    {
                        card.OnHeroSelected += OnHeroSelected;
                        _createdHeroCards.Add(card);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Помилка створення картки для {hero.ArchetypeId}: {ex.Message}", "UI", ex);
                }
            }

            _logger.LogInfo($"✅ PopulateHeroCardsAsync ЗАВЕРШЕНО: створено {_createdHeroCards.Count} карток", "UI");
        }

        private async UniTask UpdateSelectedHeroesAsync()
        {
            _heroCardService.ReturnAll(_selectedHeroCards);
            _selectedHeroCards.Clear();

            var selectedHeroes = GetSelectedHeroes();
            foreach (var hero in selectedHeroes)
            {
                var card = await _heroCardService.CreateCardAsync(hero, _view.SelectedHeroesContainer, false);
                if (card != null)
                {
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
                    _heroCardService.ReturnCard(card);
                    _createdHeroCards.Remove(card);
                    continue;
                }

                card.SetInteractable(model.IsSelectable);
                current.Remove(card.ArchetypeId);
            }

            foreach (var model in current.Values)
            {
                var card = await _heroCardService.CreateCardAsync(model, _view.HeroCardsContainer);
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

            // ✅ ОЧИЩУЄМО РЕСУРСИ
            try
            {
                if (_heroCardService != null)
                {
                    _heroCardService.ReturnAll(_createdHeroCards);
                    _heroCardService.ReturnAll(_selectedHeroCards);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Помилка при очищенні карток: {ex.Message}", "UI");
            }

            _createdHeroCards.Clear();
            _selectedHeroCards.Clear();

            // ✅ ОБНУЛЯЄМО _view В КІНЦІ
            _view = null;

            _logger?.LogInfo("✅ LobbyPresenter disposed", "UI");
        }

        #endregion
    }
}
