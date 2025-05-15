// Assets/_MythHunter/Code/UI/Presenters/LobbyPresenter.cs
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Systems.Lobby;
using MythHunter.UI.Models;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;

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

        private ILobbyView _view;
        private bool _isSubscribed = false;

        [Inject]
        public LobbyPresenter(
            ILobbySystem lobbySystem,
            IHeroSelectionSystem heroSelectionSystem,
            IEventBus eventBus,
            IMythLogger logger)
        {
            _lobbySystem = lobbySystem;
            _heroSelectionSystem = heroSelectionSystem;
            _eventBus = eventBus;
            _logger = logger;
        }

        public void Initialize(ILobbyView view)
        {
            _view = view;
            SubscribeToEvents();
        }

        public void StartLobby(int playerCount)
        {
            _lobbySystem.InitializeLobby(playerCount);

            // Відображаємо доступних героїв
            _view.PopulateHeroCards(GetAvailableHeroes());

            // Оновлюємо UI мани
            _view.UpdateMana(GetRemainingMana(), 4); // 4 - стандартна кількість мани

            _logger.LogInfo($"Lobby UI initialized for {playerCount} players", "LobbyUI");
        }

        public void OnHeroSelected(string archetypeId)
        {
            if (_lobbySystem.SelectHero(archetypeId))
            {
                // Оновлюємо UI
                _view.UpdateSelectedHeroes(GetSelectedHeroes());
                _view.UpdateMana(GetRemainingMana(), 4);
                _view.PopulateHeroCards(GetAvailableHeroes());
            }
            else
            {
                _view.ShowError("Не вдалося вибрати героя. Перевірте наявність мани та доступність героя.");
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
            var availableHeroes = _lobbySystem.GetAvailableHeroes();
            var heroModels = new List<HeroCardModel>();

            foreach (var archetypeId in availableHeroes)
            {
                var heroInfo = _heroSelectionSystem.GetHeroInfo(archetypeId);

                if (heroInfo != null)
                {
                    heroModels.Add(new HeroCardModel
                    {
                        ArchetypeId = heroInfo.ArchetypeId,
                        Name = heroInfo.Name,
                        Description = heroInfo.Description,
                        Race = heroInfo.Race,
                        Class = heroInfo.Class,
                        ManaCost = heroInfo.ManaCost,
                        IconPath = heroInfo.IconPath,
                        IsSelected = false,
                        IsSelectable = true
                    });
                }
            }

            return heroModels;
        }

        public List<HeroCardModel> GetSelectedHeroes()
        {
            var selectedHeroes = _lobbySystem.GetSelectedHeroes();
            var heroModels = new List<HeroCardModel>();

            foreach (var archetypeId in selectedHeroes)
            {
                var heroInfo = _heroSelectionSystem.GetHeroInfo(archetypeId);

                if (heroInfo != null)
                {
                    heroModels.Add(new HeroCardModel
                    {
                        ArchetypeId = heroInfo.ArchetypeId,
                        Name = heroInfo.Name,
                        Description = heroInfo.Description,
                        Race = heroInfo.Race,
                        Class = heroInfo.Class,
                        ManaCost = heroInfo.ManaCost,
                        IconPath = heroInfo.IconPath,
                        IsSelected = true,
                        IsSelectable = false
                    });
                }
            }

            return heroModels;
        }

        public int GetRemainingMana()
        {
            // Отримуємо через доступні героїв, оскільки нам потрібен контекст поточного гравця
            var heroInfos = _heroSelectionSystem.GetHeroesByCategory().Values.SelectMany(x => x).ToList();
            var availableHeroes = _lobbySystem.GetAvailableHeroes();

            // Знаходимо героя з максимальною вартістю, якого ще можна вибрати
            int maxAvailableCost = 0;

            foreach (var archetypeId in availableHeroes)
            {
                var heroInfo = _heroSelectionSystem.GetHeroInfo(archetypeId);

                if (heroInfo != null && heroInfo.ManaCost > maxAvailableCost)
                {
                    maxAvailableCost = heroInfo.ManaCost;
                }
            }

            // Якщо немає доступних героїв, повертаємо 0
            if (availableHeroes.Count == 0)
            {
                foreach (var archetypeId in heroInfos)
                {
                    var heroInfo = _heroSelectionSystem.GetHeroInfo(archetypeId);

                    // Перевіряємо героїв зі зростаючою вартістю
                    for (int manaCost = 1; manaCost <= 4; manaCost++)
                    {
                        if (heroInfo != null && heroInfo.ManaCost == manaCost && !availableHeroes.Contains(archetypeId))
                        {
                            // Якщо знайшли героя, якого не можна вибрати, це означає, що залишилось менше мани
                            return manaCost - 1;
                        }
                    }
                }

                return 0;
            }

            return maxAvailableCost;
        }

        public float GetRemainingTime()
        {
            // У справжній реалізації це значення можна було б отримати з компонента LobbyStateComponent
            // Для прикладу, повертаємо фіксоване значення
            return 300f; // 5 хвилин
        }

        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<LobbyInitializedEvent>(OnLobbyInitialized);
            _eventBus.Subscribe<HeroSelectedEvent>(OnHeroSelectEvent);
            _eventBus.Subscribe<SelectionConfirmedEvent>(OnSelectionConfirmedEvent);
            _eventBus.Subscribe<SelectionTimerUpdatedEvent>(OnSelectionTimerUpdated);

            _isSubscribed = true;
        }

        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<LobbyInitializedEvent>(OnLobbyInitialized);
            _eventBus.Unsubscribe<HeroSelectedEvent>(OnHeroSelectEvent);
            _eventBus.Unsubscribe<SelectionConfirmedEvent>(OnSelectionConfirmedEvent);
            _eventBus.Unsubscribe<SelectionTimerUpdatedEvent>(OnSelectionTimerUpdated);

            _isSubscribed = false;
        }

        private void OnLobbyInitialized(LobbyInitializedEvent evt)
        {
            // Оновлюємо UI при ініціалізації лоббі
            _view.PopulateHeroCards(GetAvailableHeroes());
            _view.UpdateMana(evt.ManaPerPlayer, evt.ManaPerPlayer);
            _view.UpdateTimer(evt.SelectionTimeLimit, evt.SelectionTimeLimit);
        }

        private void OnHeroSelectEvent(HeroSelectedEvent evt)
        {
            // Оновлюємо UI при виборі героя
            if (_view != null)
            {
                _view.UpdateSelectedHeroes(GetSelectedHeroes());
                _view.UpdateMana(evt.RemainingMana, 4);
                _view.PopulateHeroCards(GetAvailableHeroes());
            }
        }

        private void OnSelectionConfirmedEvent(SelectionConfirmedEvent evt)
        {
            // Оновлюємо статус гравця
            if (_view != null)
            {
                _view.ShowPlayerStatus(evt.PlayerIndex, true);

                // Якщо всі гравці готові, показуємо повідомлення про початок гри
                if (_lobbySystem.AreAllPlayersReady())
                {
                    _view.ShowGameStartingMessage();
                }
                else
                {
                    // Якщо вибір гравця завершено, оновлюємо доступних героїв для наступного гравця
                    _view.PopulateHeroCards(GetAvailableHeroes());
                    _view.UpdateSelectedHeroes(GetSelectedHeroes());
                    _view.UpdateMana(GetRemainingMana(), 4);
                }
            }
        }

        private void OnSelectionTimerUpdated(SelectionTimerUpdatedEvent evt)
        {
            // Оновлюємо таймер
            if (_view != null)
            {
                _view.UpdateTimer(evt.RemainingTime, 300f); // 5 хвилин
            }
        }
    }
}
