// Assets/_MythHunter/Code/Core/Game/GameplayState.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.SceneManagement;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Gameplay;
using MythHunter.Utils.Logging;

namespace MythHunter.States
{
    /// <summary>
    /// Стан ігрового процесу
    /// </summary>
    public class GameplayState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IGameFlowManager _gameFlowManager;

        public GameplayState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
            _eventBus = container.Resolve<IEventBus>();
            _gameFlowManager = container.Resolve<IGameFlowManager>();
        }

        public override GameStateType StateId => GameStateType.Gameplay;

        public override async void Enter(GameStateType previousState)
        {
            _logger.LogInfo("Entering gameplay state");

            var dispatcher = _container.Resolve<ISceneDispatcher>();
            var eventBus = _container.Resolve<IEventBus>();
            var heroes = dispatcher.GetSceneData<List<string>>("SelectedHeroTypes");

            if (heroes != null)
            {
                foreach (var hero in heroes)
                {
                    eventBus.Publish(new SpawnCharacterEvent { CharacterName = hero });
                }
            }

            eventBus.Publish(new GameStartedEvent { Timestamp = DateTime.UtcNow });

            await UniTask.Delay(100);
        }

        public override void Exit()
        {
            _logger.LogInfo("Exiting gameplay state");
        }
    }
}
