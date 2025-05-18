// Assets/_MythHunter/Code/Systems/Lobby/LobbySystem.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Components.Lobby;
using MythHunter.Events;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using MythHunter.Entities.Archetypes;
using MythHunter.Entities;
using MythHunter.Systems.Core;

namespace MythHunter.Systems.Lobby
{
    /// <summary>
    /// Система лоббі
    /// </summary>
  
    public class LobbySystem : SystemBase, ILobbySystem
    {
        private readonly IEntityManager _entityManager;
        private readonly IHeroSelectionSystem _heroSelectionSystem;
        private readonly IArchetypeSystem _archetypeSystem;
        private readonly IArchetypeTemplateRegistry _archetypeTemplateRegistry;
        private readonly IEventThrottler _eventThrottler;

        private int _currentPlayerIndex = 0;
        private bool _isInitialized = false;
        private float _selectionTimeLimit = 300f; // 5 хвилин
        private float _selectionTimeLeft = 0f;
        private int _lobbyEntityId = -1;
        private readonly List<int> _playerEntityIds = new List<int>();

        // Константи
        private const int DEFAULT_MANA_PER_PLAYER = 4;

        [Inject]
        public LobbySystem(
            IEntityManager entityManager,
            IHeroSelectionSystem heroSelectionSystem,
            IArchetypeSystem archetypeSystem,
            IArchetypeTemplateRegistry archetypeTemplateRegistry,
            IEventBus eventBus,
            IEventThrottler eventThrottler,
            IMythLogger logger)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;
            _heroSelectionSystem = heroSelectionSystem;
            _archetypeSystem = archetypeSystem;
            _archetypeTemplateRegistry = archetypeTemplateRegistry;
            _eventThrottler = eventThrottler;

            // Реєструємо обмеження для подій оновлення таймера (4 рази на секунду)
            _eventThrottler.RegisterThrottle<SelectionTimerUpdatedEvent>(0.25f);
        }

        public override void Initialize()
        {
            base.Initialize();
            _heroSelectionSystem.LoadAvailableHeroes();
        }

        public void InitializeLobby(int playerCount)
        {
            if (_isInitialized)
            {
                _logger.LogWarning("Lobby is already initialized", "Lobby");
                return;
            }

            // Створюємо сутність лоббі
            _lobbyEntityId = _entityManager.CreateEntity();

            // Додаємо компонент стану лоббі
            var lobbyState = new LobbyStateComponent
            {
                IsReady = false,
                RemainingMana = DEFAULT_MANA_PER_PLAYER,
                SelectedHeroIds = new int[0],
                SelectionTimeLeft = _selectionTimeLimit
            };

            _entityManager.AddComponent(_lobbyEntityId, lobbyState);

            // Створюємо сутності для гравців
            _playerEntityIds.Clear();

            for (int i = 0; i < playerCount; i++)
            {
                int playerEntityId = _entityManager.CreateEntity();

                var playerState = new LobbyStateComponent
                {
                    IsReady = false,
                    RemainingMana = DEFAULT_MANA_PER_PLAYER,
                    SelectedHeroIds = new int[0],
                    SelectionTimeLeft = _selectionTimeLimit
                };

                _entityManager.AddComponent(playerEntityId, playerState);
                _playerEntityIds.Add(playerEntityId);
            }

            _currentPlayerIndex = 0;
            _selectionTimeLeft = _selectionTimeLimit;
            _isInitialized = true;

            // Публікуємо подію ініціалізації лоббі
            Publish(new LobbyInitializedEvent
            {
                PlayerCount = playerCount,
                ManaPerPlayer = DEFAULT_MANA_PER_PLAYER,
                SelectionTimeLimit = _selectionTimeLimit,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Lobby initialized with {playerCount} players", "Lobby");
        }

        public override void Update(float deltaTime)
        {
            if (!_isInitialized)
                return;

            // Оновлюємо таймер вибору
            if (_selectionTimeLeft > 0)
            {
                _selectionTimeLeft -= deltaTime;

                // Обмежуємо частоту публікації події оновлення таймера
                _eventThrottler.PublishThrottled(new SelectionTimerUpdatedEvent
                {
                    RemainingTime = _selectionTimeLeft,
                    Timestamp = DateTime.UtcNow
                });

                // Якщо час вийшов, автоматично підтверджуємо вибір
                if (_selectionTimeLeft <= 0)
                {
                    for (int i = 0; i < _playerEntityIds.Count; i++)
                    {
                        int playerEntityId = _playerEntityIds[i];
                        var playerState = _entityManager.GetComponent<LobbyStateComponent>(playerEntityId);

                        if (!playerState.IsReady)
                        {
                            _currentPlayerIndex = i;
                            ConfirmSelection();
                        }
                    }
                }
            }
        }

        public bool SelectHero(string archetypeId)
        {
            if (!_isInitialized || _currentPlayerIndex >= _playerEntityIds.Count)
            {
                _logger.LogWarning("Cannot select hero: lobby not initialized or invalid player index", "Lobby");
                return false;
            }

            int playerEntityId = _playerEntityIds[_currentPlayerIndex];
            var playerState = _entityManager.GetComponent<LobbyStateComponent>(playerEntityId);

            // Перевіряємо, чи може гравець вибрати цього героя
            if (!_heroSelectionSystem.CanSelectHero(archetypeId, _currentPlayerIndex, playerState.RemainingMana))
            {
                return false;
            }

            var heroInfo = _heroSelectionSystem.GetHeroInfo(archetypeId);
            if (heroInfo == null)
            {
                _logger.LogWarning($"Cannot find hero info for archetype ID {archetypeId}", "Lobby");
                return false;
            }

            // Перевіряємо, чи існує архетип у системі
            if (!_archetypeTemplateRegistry.GetAllTemplateIds().Contains(archetypeId))
            {
                _logger.LogWarning($"Archetype ID {archetypeId} not found in registry", "Lobby");
                return false;
            }

            // Створюємо тимчасову сутність для вибору героя
            int heroEntityId = _entityManager.CreateEntity();

            var heroSelection = new HeroSelectionComponent
            {
                ArchetypeId = archetypeId,
                ManaCost = heroInfo.ManaCost,
                IsSelected = true,
                PlayerIndex = _currentPlayerIndex
            };

            _entityManager.AddComponent(heroEntityId, heroSelection);

            // Оновлюємо стан гравця
            playerState.RemainingMana -= heroInfo.ManaCost;

            // Додаємо ідентифікатор вибраного героя до списку
            var selectedHeroIds = new List<int>(playerState.SelectedHeroIds);
            selectedHeroIds.Add(heroEntityId);
            playerState.SelectedHeroIds = selectedHeroIds.ToArray();

            _entityManager.AddComponent(playerEntityId, playerState);

            // Публікуємо подію вибору героя
            Publish(new HeroSelectedEvent
            {
                PlayerIndex = _currentPlayerIndex,
                ArchetypeId = archetypeId,
                ManaCost = heroInfo.ManaCost,
                RemainingMana = playerState.RemainingMana,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Player {_currentPlayerIndex} selected hero {archetypeId} (cost: {heroInfo.ManaCost}, remaining mana: {playerState.RemainingMana})", "Lobby");

            return true;
        }

        public void ConfirmSelection()
        {
            if (!_isInitialized || _currentPlayerIndex >= _playerEntityIds.Count)
            {
                _logger.LogWarning("Cannot confirm selection: lobby not initialized or invalid player index", "Lobby");
                return;
            }

            int playerEntityId = _playerEntityIds[_currentPlayerIndex];
            var playerState = _entityManager.GetComponent<LobbyStateComponent>(playerEntityId);

            // Помічаємо гравця як готового
            playerState.IsReady = true;
            _entityManager.AddComponent(playerEntityId, playerState);

            // Отримуємо список вибраних героїв
            var selectedArchetypeIds = new List<string>();

            foreach (var heroEntityId in playerState.SelectedHeroIds)
            {
                if (_entityManager.HasComponent<HeroSelectionComponent>(heroEntityId))
                {
                    var heroSelection = _entityManager.GetComponent<HeroSelectionComponent>(heroEntityId);
                    selectedArchetypeIds.Add(heroSelection.ArchetypeId);
                }
            }

            // Публікуємо подію підтвердження вибору
            Publish(new SelectionConfirmedEvent
            {
                PlayerIndex = _currentPlayerIndex,
                SelectedArchetypeIds = selectedArchetypeIds.ToArray(),
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Player {_currentPlayerIndex} confirmed selection with {selectedArchetypeIds.Count} heroes", "Lobby");

            // Переходимо до наступного гравця
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _playerEntityIds.Count;

            // Перевіряємо, чи всі гравці готові
            if (AreAllPlayersReady())
            {
                // Публікуємо подію запиту на початок гри
                Publish(new GameStartRequestEvent
                {
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        public async UniTask<bool> StartGameAsync()
        {
            if (!_isInitialized)
            {
                _logger.LogWarning("Cannot start game: lobby not initialized", "Lobby");
                return false;
            }

            if (!AreAllPlayersReady())
            {
                _logger.LogWarning("Cannot start game: not all players are ready", "Lobby");
                return false;
            }

            // Збираємо всі вибрані архетипи героїв з усіх гравців
            var selectedArchetypes = new Dictionary<int, List<string>>();

            for (int i = 0; i < _playerEntityIds.Count; i++)
            {
                int playerEntityId = _playerEntityIds[i];
                var playerState = _entityManager.GetComponent<LobbyStateComponent>(playerEntityId);

                var archetypes = new List<string>();

                foreach (var heroEntityId in playerState.SelectedHeroIds)
                {
                    if (_entityManager.HasComponent<HeroSelectionComponent>(heroEntityId))
                    {
                        var heroSelection = _entityManager.GetComponent<HeroSelectionComponent>(heroEntityId);
                        archetypes.Add(heroSelection.ArchetypeId);
                    }
                }

                selectedArchetypes[i] = archetypes;
            }

            // Створюємо героїв через систему архетипів
            // Це буде викликано в ігровій сцені після її завантаження

            _logger.LogInfo("Starting game...", "Lobby");

            // Тут має бути код завантаження ігрової сцени
            // Наприклад, через ISceneDispatcher

            // Очікуємо завантаження сцени
            await UniTask.Delay(1000); // Імітація завантаження

            // Публікуємо подію початку гри з інформацією про вибраних героїв
            Publish(new GameStartedEvent
            {
                Timestamp = DateTime.UtcNow
            });

            // Після завантаження ігрової сцени створюємо героїв
            CreateSelectedHeroes(selectedArchetypes);

            _isInitialized = false;

            return true;
        }

        private void CreateSelectedHeroes(Dictionary<int, List<string>> selectedArchetypes)
        {
            // Для кожного гравця створюємо вибраних героїв
            foreach (var playerEntry in selectedArchetypes)
            {
                int playerIndex = playerEntry.Key;
                var archetypes = playerEntry.Value;

                _logger.LogInfo($"Creating {archetypes.Count} heroes for player {playerIndex}", "Lobby");

                foreach (var archetypeId in archetypes)
                {
                    try
                    {
                        // Створюємо героя з архетипу через ArchetypeSystem
                        int entityId = _archetypeSystem.CreateEntityFromArchetype(archetypeId, null);

                        if (entityId >= 0)
                        {
                            // Тут можна додати додаткові компоненти до створеного героя
                            // Наприклад, компонент власника (PlayerOwnerComponent)

                            _logger.LogInfo($"Created hero entity {entityId} with archetype {archetypeId} for player {playerIndex}", "Lobby");
                        }
                        else
                        {
                            _logger.LogError($"Failed to create hero entity with archetype {archetypeId} for player {playerIndex}", "Lobby");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error creating hero with archetype {archetypeId}: {ex.Message}", "Lobby", ex);
                    }
                }
            }
        }

        public List<string> GetAvailableHeroes()
        {
            if (!_isInitialized || _currentPlayerIndex >= _playerEntityIds.Count)
            {
                return new List<string>();
            }

            int playerEntityId = _playerEntityIds[_currentPlayerIndex];
            var playerState = _entityManager.GetComponent<LobbyStateComponent>(playerEntityId);

            var availableHeroes = new List<string>();
            var heroesByCategory = _heroSelectionSystem.GetHeroesByCategory();

            foreach (var category in heroesByCategory.Values)
            {
                foreach (var archetypeId in category)
                {
                    if (_heroSelectionSystem.CanSelectHero(archetypeId, _currentPlayerIndex, playerState.RemainingMana))
                    {
                        availableHeroes.Add(archetypeId);
                    }
                }
            }

            return availableHeroes;
        }

        public List<string> GetSelectedHeroes()
        {
            if (!_isInitialized || _currentPlayerIndex >= _playerEntityIds.Count)
            {
                return new List<string>();
            }

            int playerEntityId = _playerEntityIds[_currentPlayerIndex];
            var playerState = _entityManager.GetComponent<LobbyStateComponent>(playerEntityId);

            var selectedHeroes = new List<string>();

            foreach (var heroEntityId in playerState.SelectedHeroIds)
            {
                if (_entityManager.HasComponent<HeroSelectionComponent>(heroEntityId))
                {
                    var heroSelection = _entityManager.GetComponent<HeroSelectionComponent>(heroEntityId);
                    selectedHeroes.Add(heroSelection.ArchetypeId);
                }
            }

            return selectedHeroes;
        }

        public bool AreAllPlayersReady()
        {
            if (!_isInitialized)
                return false;

            foreach (var playerEntityId in _playerEntityIds)
            {
                var playerState = _entityManager.GetComponent<LobbyStateComponent>(playerEntityId);

                if (!playerState.IsReady)
                    return false;
            }

            return true;
        }
        public int GetRemainingManaForCurrentPlayer()
        {
            if (!_isInitialized || _currentPlayerIndex >= _playerEntityIds.Count)
                return 0;

            var playerEntityId = _playerEntityIds[_currentPlayerIndex];

            if (_entityManager.TryGetComponent<LobbyStateComponent>(playerEntityId, out var state))
            {
                return state.RemainingMana;
            }

            return 0;
        }
    }
}
