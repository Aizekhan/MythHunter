// Файл: Assets/_MythHunter/Code/Core/Installers/EventsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Debugging;
using MythHunter.Events.Network;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для оновленої подійної системи
    /// </summary>
    // Шлях: Assets/_MythHunter/Code/Core/Installers/EventsInstaller.cs
    public class EventsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Event System", "Installer");
            // Нові компоненти для оптимізації та діагностики
            BindSingleton<IEventThrottler, EventThrottler>(container);
            BindSingleton<IEventBatcher, EventBatcher>(container);
            BindSingleton<IEventStore, EventStore>(container);
            // Замінити реєстрацію базової шини подій на мережеву
            BindSingleton<IEventBus, NetworkEventBus>(container);
            BindSingleton<INetworkEventBus, NetworkEventBus>(container);

            Bind<EventLogger, EventLogger>(container);

            logger.LogInfo("Event System installed", "Installer");
        }
    }
}
