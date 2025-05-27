// Assets/_MythHunter/Code/Systems/AI/SimpleLobbyAI.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain.Lobby;
using MythHunter.Services.GameSettings;
using MythHunter.Systems.Core;
using MythHunter.Systems.Lobby;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.AI
{
    public interface ISimpleLobbyAI : ISystem
    {
        bool IsEnabled
        {
            get; set;
        }
        void StartAIBehavior();
        void StopAIBehavior();
    }

    public class SimpleLobbyAI : SystemBase, ISimpleLobbyAI
    {
        private readonly ILobbySystem _lobbySystem;
        private readonly IHeroSelectionSystem _heroSelection;
        private readonly IGameSettingsService _gameSettings;

        private bool _isEnabled = false;
        private bool _isRunning = false;
        private readonly System.Random _random = new();

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        [Inject]
        public SimpleLobbyAI(
            ILobbySystem lobbySystem,
            IHeroSelectionSystem heroSelection,
            IGameSettingsService gameSettings,
            IEventBus eventBus,
            IMythLogger logger)
            : base(logger, eventBus)
        {
            _lobbySystem = lobbySystem;
            _heroSelection = heroSelection;
            _gameSettings = gameSettings;
        }

        public override void Initialize()
        {
            base.Initialize();

            // Активуємо AI тільки в PvAI режимі
            _isEnabled = _gameSettings.IsAIEnabled;
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<LobbyInitializedEvent>(OnLobbyInitialized);
            Subscribe<HeroSelectedEvent>(OnHeroSelected);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<LobbyInitializedEvent>(OnLobbyInitialized);
            Unsubscribe<HeroSelectedEvent>(OnHeroSelected);
        }

        private void OnLobbyInitialized(LobbyInitializedEvent evt)
        {
            if (_isEnabled && !_isRunning)
            {
                _logger.LogInfo("🤖 AI активовано для лобі", "AI");
                StartAIBehavior();
            }
        }

        private void OnHeroSelected(HeroSelectedEvent evt)
        {
            // Якщо гравець зробив вибір, AI робить свій через деякий час
            if (_isEnabled && _isRunning && evt.PlayerIndex == 0) // Гравець 0 - людина
            {
                DelayedAIAction().Forget();
            }
        }

        public void StartAIBehavior()
        {
            if (!_isEnabled || _isRunning)
                return;

            _isRunning = true;
            _logger.LogInfo("🤖 AI починає роботу", "AI");

            // Починаємо AI логіку через 3 секунди
            DelayedAIStart().Forget();
        }

        public void StopAIBehavior()
        {
            _isRunning = false;
            _logger.LogInfo("🤖 AI зупинено", "AI");
        }

        private async UniTaskVoid DelayedAIStart()
        {
            await UniTask.Delay(3000); // 3 секунди на роздуми

            if (!_isRunning)
                return;

            await PerformAISelection();
        }

        private async UniTaskVoid DelayedAIAction()
        {
            await UniTask.Delay(_random.Next(1000, 3000)); // 1-3 секунди

            if (!_isRunning)
                return;

            await PerformAISelection();
        }

        private async UniTask PerformAISelection()
        {
            try
            {
                var availableHeroes = _heroSelection.GetHeroesByCategory()
                    .SelectMany(x => x.Value).ToList();

                int remainingMana = _lobbySystem.GetRemainingManaForCurrentPlayer();

                // AI стратегія: спочатку дорогі герої, потім дешеві
                var possibleHeroes = availableHeroes
                    .Select(id => _heroSelection.GetHeroInfo(id))
                    .Where(info => info != null && info.ManaCost <= remainingMana)
                    .OrderByDescending(info => info.ManaCost) // Спочатку дорогі
                    .ToList();

                if (possibleHeroes.Count > 0)
                {
                    // Вибираємо з топ-3 найдорожчих (елемент випадковості)
                    var topChoices = possibleHeroes.Take(3).ToList();
                    var selectedHero = topChoices[_random.Next(topChoices.Count)];

                    _logger.LogInfo($"🤖 AI вибирає: {selectedHero.Name} (вартість: {selectedHero.ManaCost})", "AI");

                    _lobbySystem.SelectHero(selectedHero.ArchetypeId);

                    await UniTask.Delay(500); // Короткая пауза

                    // Перевіряємо, чи потрібно ще вибирати
                    remainingMana = _lobbySystem.GetRemainingManaForCurrentPlayer();
                    if (remainingMana > 0)
                    {
                        DelayedAIAction().Forget(); // Продовжуємо вибір
                    }
                    else
                    {
                        // Мана закінчилась - підтверджуємо вибір
                        await UniTask.Delay(1000);
                        _lobbySystem.ConfirmSelection();
                        _logger.LogInfo("🤖 AI підтвердив вибір", "AI");
                        StopAIBehavior();
                    }
                }
                else
                {
                    // Немає доступних героїв - підтверджуємо те, що є
                    _logger.LogInfo("🤖 AI не може вибрати більше героїв, підтверджує", "AI");
                    _lobbySystem.ConfirmSelection();
                    StopAIBehavior();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"🤖 Помилка AI: {ex.Message}", "AI", ex);
            }
        }
    }
}
