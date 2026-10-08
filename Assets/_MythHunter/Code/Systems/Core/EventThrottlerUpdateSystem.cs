using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Система, яка викликає EventThrottler.Update() кожен кадр
    /// </summary>
    [SystemCategory(SystemInitializationCategory.OnBoot)]
    public class EventThrottlerUpdateSystem : SystemBase, ISystem, IEventThrottlerUpdateSystem
    {
        private readonly IEventThrottler _eventThrottler;

        [Inject]
        public EventThrottlerUpdateSystem(
            IEventThrottler eventThrottler,
            IMythLogger logger,
            IEventBus eventBus)
            : base(logger, eventBus)
        {
            _eventThrottler = eventThrottler;
        }

        public override void Update(float deltaTime)
        {
            _eventThrottler.Update();
        }
    }
}
