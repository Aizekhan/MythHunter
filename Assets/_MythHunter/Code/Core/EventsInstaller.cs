using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Debugging;

namespace MythHunter.Core
{
    /// <summary>
    /// Інсталятор для подійної системи
    /// </summary>
    public class EventsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            // Реєстрація пулу подій
            container.RegisterSingleton<IEventPool, EventPool>();

            // Реєстрація шини подій
            container.RegisterSingleton<IEventBus, EventBus>();

            // Реєстрація черги подій
            container.RegisterSingleton<IEventQueue, EventQueue>();

            // Реєстрація логеру подій для відлагодження
            container.Register<EventLogger, EventLogger>();
        }
    }
}
