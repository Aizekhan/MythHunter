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
using System;
using MythHunter.Core.Game;

namespace MythHunter.UI.Presenters
{
    /// <summary>
    /// Презентер лоббі
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
        private readonly IGameFlowManager _gameFlowManager;
        private ILobbyView _view;
        private bool _isSubscribed = false;
        private int _currentPlayerIndex = 0; // Додайте це поле
                                           
        [Inject]
        public LobbyPresenter(
            IEventBus eventBus,
            IMythLogger logger,
            ILobbySystem lobbySystem,
            IHeroSelectionSystem heroSelectionSystem,
            ILobbyModel model,
            IUIViewFactory viewFactory,
            IViewConfigRegistry viewConfigRegistry,
            IGameFlowManager gameFlowManager // Додали GameFlowManager
        )
        {
            _eventBus = eventBus;
            _logger = logger;
            _lobbySystem = lobbySystem;
            _heroSelectionSystem = heroSelectionSystem;
            _model = model;
            _viewFactory = viewFactory;
            _viewConfigRegistry = viewConfigRegistry;
            _gameFlowManager = gameFlowManager; // Зберігаємо посилання
        }

        public void Initialize(ILobbyView view)
        {
            if (view == null)
            {
                _logger.LogError("[LobbyPresenter] Спроба ініціалізувати з null view", "Lobby");
                return;
            }

            _view = view;
            SubscribeToEvents();
            _logger.LogInfo("[LobbyPresenter] Ініціалізовано з представленням", "Lobby");

            // Переконаймося, що HeroSelectionSystem завантажив героїв
            if (_heroSelectionSystem == null)
            {
                _logger.LogError("[LobbyPresenter] _heroSelectionSystem є null", "Lobby");
            }
            else
            {
                // Запускаємо Load асинхронно
                UniTask.Create(async () =>
                {
                    await _heroSelectionSystem.LoadAvailableHeroes();
                    _logger.LogInfo("[LobbyPresenter] Героїв асинхронно завантажено", "Lobby");
                });
            }
        }

        public async UniTask InitializeAsync()
        {
            SubscribeToEvents();
            _logger.LogInfo("LobbyPresenter initialized async", "Lobby");
            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("LobbyPresenter disposed", "Presenter");
        }

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

        public void StartLobby(int playerCount)
        {
            _logger.LogInfo($"[LobbyPresenter] StartLobby викликано для {playerCount} гравців", "Lobby");

            // Спочатку ініціалізуємо систему лобі
            _lobbySystem.InitializeLobby(playerCount);

            // Явно оновлюємо UI з актуальними даними
            if (_view != null)
            {
                // Отримуємо доступних героїв і оновлюємо картки
                var availableHeroes = GetAvailableHeroes();
                _logger.LogInfo($"[LobbyPresenter] Отримано {availableHeroes.Count} героїв", "Lobby");

                _view.PopulateHeroCards(availableHeroes);
                _view.UpdateMana(GetRemainingMana(), 4);
            }
            else
            {
                _logger.LogError("[LobbyPresenter] View не ініціалізовано у StartLobby", "Lobby");
            }
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

            // Отримуємо список вибраних героїв
            var selectedHeroes = _lobbySystem.GetSelectedHeroes();

            // Використовуємо GameFlowManager для переходу до ігрової сцени
            await _gameFlowManager.EnterGameplayAsync(selectedHeroes.ToArray());
        }

        public List<HeroCardModel> GetAvailableHeroes()
        {
            if (_heroSelectionSystem == null)
            {
                _logger.LogError("[LobbyPresenter] _heroSelectionSystem є null в GetAvailableHeroes", "Lobby");
                return new List<HeroCardModel>();
            }

            try
            {
                // Отримуємо всі доступні архетипи героїв
                var allHeroIds = _heroSelectionSystem.GetHeroesByCategory()
                    .SelectMany(category => category.Value)
                    .ToList();

                if (allHeroIds.Count == 0)
                {
                    _logger.LogWarning("[LobbyPresenter] Отримано 0 архетипів героїв", "Lobby");
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

                _logger.LogInfo($"[LobbyPresenter] GetAvailableHeroes повертає {result.Count} героїв", "Lobby");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[LobbyPresenter] Помилка в GetAvailableHeroes: {ex.Message}", "Lobby");
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

        public float GetRemainingTime() => 300f;

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
            // Замінюємо повну перегенерацію на оновлення стану
            _view.UpdateHeroCardsState(GetAvailableHeroes());
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
        // Також модифікуйте метод обробки зміни гравця (якщо він є):
        private void OnPlayerChanged(int newPlayerIndex)
        {
            _currentPlayerIndex = newPlayerIndex;
            // Оновлення UI...
        }
    }
}
