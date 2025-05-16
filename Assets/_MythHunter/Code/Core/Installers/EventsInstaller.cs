// Файл: Assets/_MythHunter/Code/Core/Installers/EventsInstaller.cs
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
    // Шлях: Assets/_MythHunter/Code/Core/Installers/EventsInstaller.cs
    public class EventsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            var systemRegistry = container.Resolve<ISystemRegistry>();
            logger.LogInfo("Installing Event System", "Installer");

            // Спочатку реєструємо допоміжні компоненти
            BindSingleton<IEventThrottler, EventThrottler>(container);
            BindSingleton<IEventThrottlerUpdateSystem, EventThrottlerUpdateSystem>(container);
            systemRegistry.RegisterSystem(container.Resolve<IEventThrottlerUpdateSystem>());

            BindSingleton<IEventBatcher, EventBatcher>(container);
            BindSingleton<IEventStore, EventStore>(container);

            // Перевіряємо наявність мережевої системи
            bool hasNetworkSystem = container.IsRegistered<INetworkSystem>();

            if (hasNetworkSystem)
            {
                // Замінюємо простий EventBus на мережевий
                logger.LogInfo("Network system detected, using NetworkEventBus", "Installer");

                // Видаляємо стару реєстрацію (необов'язково, так як RegisterSingleton перезаписує)
                // container.Unregister<IEventBus>();

                // Реєструємо нову шину подій
                BindSingleton<IEventBus, NetworkEventBus>(container);
                BindSingleton<INetworkEventBus, NetworkEventBus>(container);
            }
            else
            {
                // Замінюємо простий EventBus на повний
                logger.LogInfo("No network system detected, using standard EventBus", "Installer");

                // Реєструємо стандартну шину подій
                BindSingleton<IEventBus, EventBus>(container);
            }

            Bind<EventLogger, EventLogger>(container);

            logger.LogInfo("Event System installed", "Installer");
        }
    }
}
