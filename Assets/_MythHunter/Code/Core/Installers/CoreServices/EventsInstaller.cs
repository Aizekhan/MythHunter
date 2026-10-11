// Шлях: Assets/_MythHunter/Code/Core/Installers/EventsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Debugging;
using MythHunter.Events.Network;
using MythHunter.Networking.Core;
using MythHunter.Systems.Core;
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

            // Спочатку реєструємо допоміжні компоненти
            BindSingleton<IEventThrottler, EventThrottler>(container);
            BindSingleton<IEventThrottlerUpdateSystem, EventThrottlerUpdateSystem>(container);


            BindSingleton<IEventBatcher, EventBatcher>(container);
            BindSingleton<IEventStore, EventStore>(container);


            // Select the event bus from an explicit composition policy rather than
            // inferring the gameplay role from exact DI registration keys.
            var networkMode = container.Resolve<EventNetworkMode>();
            if (networkMode == EventNetworkMode.Offline)
            {
                logger.LogInfo("Event network synchronization disabled; using EventBus", "Installer");
                BindSingleton<IEventBus, EventBus>(container);
            }
            else
            {
                logger.LogInfo($"Event network synchronization enabled ({networkMode}); using NetworkEventBus", "Installer");
                // Register and resolve exactly one network bus, then alias the same
                // singleton through IEventBus so SystemRegistry and network consumers
                // cannot receive separate bus instances.
                BindSingleton<INetworkEventBus, NetworkEventBus>(container);
                BindInstance<IEventBus>(container, container.Resolve<INetworkEventBus>());
            }

            // Only now resolve the registry, so it captures the selected implementation.
            var systemRegistry = container.Resolve<ISystemRegistry>();
            BindSingleton<ITimerSystem, TimerSystem>(container);
            systemRegistry.RegisterSystem(container.Resolve<ITimerSystem>());

            Bind<EventLogger, EventLogger>(container);

            logger.LogInfo("Event System installed", "Installer");
        }
    }
}
