using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Events.Domain;
using Cysharp.Threading.Tasks;
using System;
using MythHunter.Core.SceneManagement;
using MythHunter.Events.Domain.Gameplay;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан ігрового процесу
    /// </summary>
    public class GameplayState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IGameFlowManager _gameFlowManager;

        public override GameStateType StateId => GameStateType.Game;

        public GameplayState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
            _eventBus = container.Resolve<IEventBus>();
            _gameFlowManager = container.Resolve<IGameFlowManager>();
        }

        public override async void Enter(GameStateType previousState)
        {
            _logger.LogInfo("Entering gameplay state");

            var dispatcher = Container.Resolve<ISceneDispatcher>();
            var eventBus = Container.Resolve<IEventBus>();

            var heroes = SceneDispatcher.GetSceneData<string[]>("SelectedHeroArchetypes");

            if (heroes != null)
            {
                foreach (var hero in heroes)
                {
                    eventBus.Publish(new SpawnCharacterEvent { CharacterName = hero });
                }
            }

            eventBus.Publish(new GameStartedEvent { Timestamp = DateTime.UtcNow });

            // Додатково можна активувати HUD, тощо
            await UniTask.Delay(100); // якщо потрібно дочекатись ECS
        }
        private async UniTaskVoid InitializeAsync()
        {
            _logger.LogInfo("Starting async gameplay initialization");

            // Приклад асинхронної ініціалізації
            await UniTask.Delay(100);

            _logger.LogInfo("Async gameplay initialization completed");
        }

        public override void Update()
        {
            // Логіка оновлення геймплею
        }

        public override void Exit()
        {
            _logger.LogInfo("Exiting gameplay state");

            // Публікуємо подію завершення гри
            _eventBus.Publish(new GameEndedEvent
            {
                IsVictory = false,
                Timestamp = DateTime.UtcNow
            });
        }

        // Метод для виходу до головного меню (можливо, викликається з кнопки паузи)
        public async UniTask ExitToMainMenu()
        {
            _logger.LogInfo("Exiting gameplay to main menu");

            try
            {
                await _gameFlowManager.ReturnToMainMenuAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error returning to main menu: {ex.Message}", "GameplayState", ex);
            }
        }
    }
}
