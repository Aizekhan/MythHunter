// Шлях: Assets/_MythHunter/Code/UI/Views/LobbyView.cs
// ... (залишено без змін)

// Шлях: Assets/_MythHunter/Code/UI/Presenters/LobbyPresenter.cs
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Systems.Lobby;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;

namespace MythHunter.UI.Presenters
{
    /// <summary>
    /// Презентер лоббі з повною підтримкою IViewFactory та ViewConfigRegistry
    /// </summary>
    public class LobbyPresenter : ILobbyPresenter, IEventSubscriber
    {
        private readonly ILobbySystem _lobbySystem;
        private readonly IHeroSelectionSystem _heroSelectionSystem;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly ILobbyModel _model;
        private readonly IUIViewFactory _viewFactory;
        private readonly IViewConfigRegistry _viewConfigRegistry;
        private readonly IEventThrottler _eventThrottler;
        private ILobbyView _view;
        private bool _isSubscribed = false;

        private ILobbyPresenter _presenter;
        [Inject]
        public LobbyPresenter(
    ILobbySystem lobbySystem,
    IHeroSelectionSystem heroSelectionSystem,
    IEventBus eventBus,
    IMythLogger logger,
    ILobbyModel model,
    IUIViewFactory viewFactory,
    IViewConfigRegistry viewConfigRegistry

)
        {
            _lobbySystem = lobbySystem;
            _heroSelectionSystem = heroSelectionSystem;
            _eventBus = eventBus;
            _logger = logger;
            _model = model;
            _viewFactory = viewFactory;
            _viewConfigRegistry = viewConfigRegistry;
            
        }

        public  void Initialize(ILobbyView view)
        {
            _view = view;
            SubscribeToEvents();

        }

        public void StartLobby(int playerCount)
        {
            _lobbySystem.InitializeLobby(playerCount);

            // ❌ Більше НЕ створюй _view вручну
            if (_view == null)
            {
                _logger.LogWarning("LobbyView is not initialized via Construct()", "LobbyPresenter");
                return;
            }

            _view.PopulateHeroCards(GetAvailableHeroes());
            _view.UpdateMana(GetRemainingMana(), 4);
        }

        public void OnHeroSelected(string archetypeId)
        {
            if (_lobbySystem.SelectHero(archetypeId))
            {
                _view.UpdateSelectedHeroes(GetSelectedHeroes());
                _view.UpdateMana(GetRemainingMana(), 4);
                _view.PopulateHeroCards(GetAvailableHeroes());
            }
            else
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
            if (await _lobbySystem.StartGameAsync())
            {
                _logger.LogInfo("Game started successfully", "LobbyUI");
            }
            else
            {
                _view.ShowError("Не вдалося почати гру");
            }
        }

        public List<HeroCardModel> GetAvailableHeroes()
        {
            return _lobbySystem.GetAvailableHeroes()
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
                    IsSelectable = true,
                    IsSelected = false
                }).ToList();
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

        public float GetRemainingTime() => 300f;

        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<LobbyInitializedEvent>(OnLobbyInitialized);
            _eventBus.Subscribe<HeroSelectedEvent>(OnHeroSelectedEvent);
            _eventBus.Subscribe<SelectionConfirmedEvent>(OnSelectionConfirmedEvent);
            _eventBus.Subscribe<SelectionTimerUpdatedEvent>(OnTimerUpdated);
           

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

            _isSubscribed = false;
        }

        private void OnLobbyInitialized(LobbyInitializedEvent evt)
        {
            _view.PopulateHeroCards(GetAvailableHeroes());
            _view.UpdateMana(evt.ManaPerPlayer, evt.ManaPerPlayer);
            _view.UpdateTimer(evt.SelectionTimeLimit, evt.SelectionTimeLimit);
        }

        private void OnHeroSelectedEvent(HeroSelectedEvent evt)
        {
            _view.UpdateSelectedHeroes(GetSelectedHeroes());
            _view.UpdateMana(evt.RemainingMana, 4);
            _view.PopulateHeroCards(GetAvailableHeroes());
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
                _view.PopulateHeroCards(GetAvailableHeroes());
                _view.UpdateSelectedHeroes(GetSelectedHeroes());
                _view.UpdateMana(GetRemainingMana(), 4);
            }
        }

        private void OnTimerUpdated(SelectionTimerUpdatedEvent evt)
        {
            _view.UpdateTimer(evt.RemainingTime, 300f);
        }
    }
}
