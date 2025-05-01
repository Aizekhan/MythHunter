using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Debugging;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для оновленої подійної системи
    /// </summary>
    public class EventsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Event System", "Installer");

            // Реєстрація пулу подій з підтримкою пріоритетів
            BindSingleton<IEventPool, EventPool>(container);

            // Реєстрація шини подій
            BindSingleton<IEventBus, EventBus>(container);

            // Реєстрація черги подій
            BindSingleton<IEventQueue, AsyncEventQueue>(container);

            // Реєстрація оптимізатора обробки подій
            BindSingleton<PrioritizedEventProcessing, PrioritizedEventProcessing>(container);

            // Реєстрація логеру подій для відлагодження
            Bind<EventLogger, EventLogger>(container);

            logger.LogInfo("Event System installed", "Installer");
        }
    }
}
