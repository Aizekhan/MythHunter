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


            // Choose the final bus before resolving anything that depends on IEventBus.
            // SystemRegistry subscribes in its constructor, so resolving it first would
            // instantiate and cache the earlier SimpleEventBus registration.
            bool hasNetworkSystem = container.IsRegistered<INetworkSystem>();

            if (hasNetworkSystem)
            {
                logger.LogInfo("Network system detected, using NetworkEventBus", "Installer");
                BindSingleton<IEventBus, NetworkEventBus>(container);
                BindSingleton<INetworkEventBus, NetworkEventBus>(container);
            }
            else
            {
                logger.LogInfo("No network system detected, using standard EventBus", "Installer");
                BindSingleton<IEventBus, EventBus>(container);
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
