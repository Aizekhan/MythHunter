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

namespace MythHunter.UI.Presenters
{
    public class LobbyPresenter : ILobbyPresenter, IEventSubscriber
    {
        private readonly ILobbySystem _lobbySystem;
        private readonly IHeroSelectionSystem _heroSelectionSystem;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly ILobbyModel _model;
        private readonly IGameFlowManager _gameFlowManager;
        private readonly IGameSettingsService _gameSettings;
        private readonly IHeroCardService _heroCardService;

        private ILobbyView _view;
        private bool _isSubscribed = false;
        private int _currentPlayerIndex = 0;

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
            INavigationService navigationService) // 👈 Додали
        {
            _eventBus = eventBus;
            _logger = logger;
            _lobbySystem = lobbySystem;
            _heroSelectionSystem = heroSelectionSystem;
            _model = model;
            _gameFlowManager = gameFlowManager;
            _gameSettings = gameSettings;
            _heroCardService = heroCardService;
            _navigationService = navigationService; // 👈 Ініціалізували
        }

        public async UniTask InitializeAsync()
        {
            SubscribeToEvents();
            await _heroCardService.InitializeAsync();
            await _heroSelectionSystem.LoadAvailableHeroes();

            if (!_lobbySystem.IsInitialized)
            {
                _lobbySystem.InitializeLobby(_gameSettings.PlayerCount);
            }
        }

        public void Initialize(ILobbyView view)
        {
            _view = view;
            SubscribeToEvents();
            UniTask.Create(async () => {
                await _heroCardService.InitializeAsync();
                await _heroSelectionSystem.LoadAvailableHeroes();

                if (!_lobbySystem.IsInitialized)
                {
                    _lobbySystem.InitializeLobby(_gameSettings.PlayerCount);
                }

                await PopulateHeroCardsAsync();
            });
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
            _heroCardService.ReturnAll(_createdHeroCards);
            _heroCardService.ReturnAll(_selectedHeroCards);
            _createdHeroCards.Clear();
            _selectedHeroCards.Clear();
        }

        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<LobbyStateEnteredEvent>(OnLobbyStateEntered);
            _eventBus.Subscribe<LobbyInitializedEvent>(OnLobbyInitialized);
            _eventBus.Subscribe<HeroSelectedEvent>(OnHeroSelectedEvent);
            _eventBus.Subscribe<SelectionConfirmedEvent>(OnSelectionConfirmedEvent);
            _eventBus.Subscribe<SelectionTimerUpdatedEvent>(OnTimerUpdated);
            _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

            _isSubscribed = true;
            _logger.LogInfo("LobbyPresenter підписаний на події", "UI");
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
                await UpdateSelectedHeroesAsync();
                _view.UpdateMana(evt.RemainingMana, 4);
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
            _logger.LogInfo("Отримано подію LobbyStateEnteredEvent", "UI");

            UniTask.Create(async () => {
                if (!_lobbySystem.IsInitialized)
                {
                    _lobbySystem.InitializeLobby(_gameSettings.PlayerCount);
                }
                await PopulateHeroCardsAsync();
            });
        }

        private async UniTask PopulateHeroCardsAsync()
        {
            _heroCardService.ReturnAll(_createdHeroCards);
            _createdHeroCards.Clear();

            var heroes = GetAvailableHeroes();
            foreach (var hero in heroes)
            {
                var card = await _heroCardService.CreateCardAsync(hero, _view.HeroCardsContainer);
                if (card != null)
                {
                    card.OnHeroSelected += OnHeroSelected;
                    _createdHeroCards.Add(card);
                }
            }
            await UpdateSelectedHeroesAsync();
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

        public float GetRemainingTime() => 300f;

        public async UniTask OpenHeroSelectorAsync()
        {
            var parameters = new NavigationParameters();
            parameters.Add("PlayerCount", _gameSettings.PlayerCount);
            parameters.Add("ManaPerPlayer", _gameSettings.ManaPerPlayer);

            await _navigationService.NavigateToAsync<HeroCardSelectorView>(
                screenId: "HeroCardSelector",
                parameters: parameters,
                transition: TransitionType.SlideLeft
            );
        }

        public async UniTask<bool> ShowConfirmationAsync(string message)
        {
            var parameters = new NavigationParameters();
            parameters.Add("Message", message);

            bool result = await _navigationService.ShowModalAsync<ConfirmationDialog, bool>(
                modalId: "ConfirmDialog",
                parameters: parameters
            );

            return result;
        }
    }
}
