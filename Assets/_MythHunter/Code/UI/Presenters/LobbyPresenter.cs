// Assets/_MythHunter/Code/UI/Presenters/LobbyPresenter.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Systems.Lobby;
using MythHunter.UI.Controllers;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;
using MythHunter.Services.GameSettings;
using MythHunter.UI.Navigation;
using MythHunter.Core.Game;
using MythHunter.Events.Domain;

namespace MythHunter.UI.Presenters
{
    public class LobbyPresenter : ILobbyPresenter, IEventSubscriber
    {
        private readonly ILobbySystem _lobbySystem;
        private readonly IHeroSelectionSystem _heroSelectionSystem;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly IGameSettingsService _gameSettings;
        private readonly IUIViewFactory _uiViewFactory;
        private readonly INavigationService _navigationService;
        private readonly IGameFlowManager _gameFlowManager;
        private readonly IDIContainer _container;

        private HeroGridController _heroGridController;
        private LobbyView _view;
        private bool _isInitialized = false;
        private int _currentTurnPlayerIndex = 0;
        private Dictionary<int, List<string>> _playerSelections = new();

        public bool IsInitialized => _isInitialized;

        [Inject]
        public LobbyPresenter(
            ILobbySystem lobbySystem,
            IHeroSelectionSystem heroSelectionSystem,
            IEventBus eventBus,
            IMythLogger logger,
            IGameSettingsService gameSettings,
            IUIViewFactory uiViewFactory,
            INavigationService navigationService,
            IGameFlowManager gameFlowManager,
            IDIContainer container)
        {
            _lobbySystem = lobbySystem;
            _heroSelectionSystem = heroSelectionSystem;
            _eventBus = eventBus;
            _logger = logger;
            _gameSettings = gameSettings;
            _uiViewFactory = uiViewFactory;
            _navigationService = navigationService;
            _gameFlowManager = gameFlowManager;
            _container = container;

            _heroGridController = new HeroGridController(_uiViewFactory, _logger, _container);
        }

        public void Initialize(ILobbyView view)
        {
            _view = view as LobbyView;
            _heroGridController.Initialize(_view.HeroCardsContainer);
            _heroGridController.OnPageChanged += _view.UpdatePageInfo;
            _heroGridController.OnHeroSelected += OnHeroSelected;
        }

        public async UniTask InitializeAsync()
        {
            if (_isInitialized)
                return;

            SubscribeToEvents();

            await _heroSelectionSystem.LoadAvailableHeroes();

            _playerSelections[0] = new List<string>();
            _playerSelections[1] = new List<string>();

            _isInitialized = true;
            LoadAndDisplayHeroes();
        }

        public void SubscribeToEvents()
        {
            _eventBus.Subscribe<HeroSelectedEvent>(OnHeroSelectedEvent);
            _eventBus.Subscribe<SelectionConfirmedEvent>(OnSelectionConfirmed);
            _eventBus.Subscribe<TimerUpdatedEvent>(OnTimerUpdated);
        }
        public void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<HeroSelectedEvent>(OnHeroSelectedEvent);
            _eventBus.Unsubscribe<SelectionConfirmedEvent>(OnSelectionConfirmed);
            _eventBus.Unsubscribe<TimerUpdatedEvent>(OnTimerUpdated);
        }

        public void OnHeroSelected(string archetypeId)
        {
            if (!CanSelectHero(archetypeId, _currentTurnPlayerIndex))
                return;

            AddHeroToSelection(_currentTurnPlayerIndex, archetypeId);
            UpdateHeroDisplay();
            UpdatePlayerSlots();
            CheckPlayerSelectionComplete();
        }

        public void OnSelectionConfirmed()
        {
            SwitchToNextPlayer();
        }

        public async UniTask StartGameAsync()
        {
            if (_lobbySystem.AreAllPlayersReady())
            {
                var allSelected = _playerSelections.Values.SelectMany(x => x).ToArray();
                _gameFlowManager.EnterGameplayAsync(allSelected).Forget();
            }
            await UniTask.CompletedTask;
        }

        public List<HeroCardModel> GetAvailableHeroes() => BuildHeroModels();

        public int GetRemainingMana() => _lobbySystem.GetRemainingManaForCurrentPlayer();

        public float GetRemainingTime() => _lobbySystem.GetRemainingSelectionTime();

        public void Dispose()
        {
            UnsubscribeFromEvents();

            _heroGridController.Dispose();
            _playerSelections.Clear();
            _view = null;
        }

        private void OnHeroSelectedEvent(HeroSelectedEvent evt) => UpdateHeroDisplay();

        private void OnSelectionConfirmed(SelectionConfirmedEvent evt) => SwitchToNextPlayer();

        private void OnTimerUpdated(TimerUpdatedEvent evt)
        {
            if (evt.Category == "Lobby")
                _view?.UpdateTimer(evt.RemainingTime, evt.TotalTime);
        }

        private void LoadAndDisplayHeroes()
        {
            var heroes = BuildHeroModels();
            _heroGridController.SetHeroes(heroes);
            var (page, total) = _heroGridController.GetPageInfo();
            _view?.UpdatePageInfo(page, total);
        }

        private List<HeroCardModel> BuildHeroModels()
        {
            var ids = _heroSelectionSystem.GetHeroesByCategory().SelectMany(x => x.Value);
            var result = new List<HeroCardModel>();
            foreach (var id in ids)
            {
                var info = _heroSelectionSystem.GetHeroInfo(id);
                if (info == null)
                    continue;

                result.Add(new HeroCardModel
                {
                    ArchetypeId = info.ArchetypeId,
                    Name = info.Name,
                    Race = info.Race,
                    Class = info.Class,
                    IconPath = info.IconPath,
                    ManaCost = info.ManaCost,
                    IsSelectable = CanSelectHero(id, _currentTurnPlayerIndex),
                    IsSelected = IsHeroSelected(id)
                });
            }
            return result;
        }

        private void AddHeroToSelection(int playerIndex, string archetypeId)
        {
            _playerSelections[playerIndex].Add(archetypeId);
            _lobbySystem.SelectHero(archetypeId);
        }

        private bool IsHeroSelected(string id) => _playerSelections.Values.Any(x => x.Contains(id));

        private bool CanSelectHero(string id, int playerIndex)
        {
            if (IsHeroSelected(id))
                return false;
            if (playerIndex != _currentTurnPlayerIndex)
                return false;
            if (_playerSelections[playerIndex].Count >= 4)
                return false;
            return _heroSelectionSystem.CanSelectHero(id, playerIndex, GetRemainingMana());
        }

        private void UpdateHeroDisplay()
        {
            var updated = BuildHeroModels();
            _heroGridController.SetHeroes(updated);
            _heroGridController.RefreshCardStates();
        }

        private void UpdatePlayerSlots()
        {
            for (int p = 0; p < 2; p++)
            {
                if (!_playerSelections.TryGetValue(p, out var selected))
                    continue;
                for (int i = 0; i < 4; i++)
                {
                    if (i < selected.Count)
                    {
                        var info = _heroSelectionSystem.GetHeroInfo(selected[i]);
                        if (info != null)
                        {
                            _view?.SetHeroInSlot(p, i, new HeroCardModel
                            {
                                ArchetypeId = info.ArchetypeId,
                                Name = info.Name,
                                Race = info.Race,
                                Class = info.Class,
                                IconPath = info.IconPath,
                                Level = 1
                            });
                        }
                    }
                    else
                        _view?.ClearSlot(p, i);
                }
            }
        }

        private void CheckPlayerSelectionComplete()
        {
            if (GetRemainingMana() == 0)
                _lobbySystem.ConfirmSelection();
        }

        private void SwitchToNextPlayer()
        {
            _currentTurnPlayerIndex = (_currentTurnPlayerIndex + 1) % 2;
            UpdateHeroDisplay();

            if (_lobbySystem.AreAllPlayersReady())
                _gameFlowManager.EnterGameplayAsync(_playerSelections.Values.SelectMany(x => x).ToArray()).Forget();
        }
    }
}
