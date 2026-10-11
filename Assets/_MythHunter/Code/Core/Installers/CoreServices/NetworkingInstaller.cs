// Шлях: Assets/_MythHunter/Code/Core/Installers/NetworkingInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Networking.Core;
using MythHunter.Networking.Client;
using MythHunter.Networking.Server;
using MythHunter.Networking.Serialization;
using MythHunter.Events.Network;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для мережевої системи
    /// </summary>
    public class NetworkingInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей NetworkingSystem...", "Installer");

            // Реєстрація загальних мережевих сервісів
            BindSingleton<INetworkSerializer, BinaryNetworkSerializer>(container);
            BindSingleton<INetworkMessageHandlerRegistry, NetworkMessageHandlerRegistry>(container);

            // Keep the default safe for existing scenes, which previously selected EventBus.
            // GameBootstrapper may set an explicit mode before running InstallerRegistry.
            if (!container.IsRegistered<EventNetworkMode>())
                BindInstance(container, EventNetworkMode.Offline);

            var networkMode = container.Resolve<EventNetworkMode>();
            if (!System.Enum.IsDefined(typeof(EventNetworkMode), networkMode))
            {
                logger.LogError($"Unsupported event network mode value: {(int)networkMode}; falling back to Offline.", "Installer");
                networkMode = EventNetworkMode.Offline;
                BindInstance(container, networkMode);
            }

            // Register role-specific services first, then expose the selected SAME singleton
            // through INetworkSystem. Never construct a second service as the base interface.
            BindSingleton<INetworkClient, NetworkClient>(container);
            BindSingleton<IClientNetworkSystem, ClientNetworkSystem>(container);

            BindSingleton<INetworkServer, NetworkServer>(container);
            BindSingleton<IServerNetworkSystem, ServerNetworkSystem>(container);

            switch (networkMode)
            {
                case EventNetworkMode.Client:
                    BindInstance<INetworkSystem>(container, container.Resolve<IClientNetworkSystem>());
                    break;
                case EventNetworkMode.Server:
                    BindInstance<INetworkSystem>(container, container.Resolve<IServerNetworkSystem>());
                    break;
                case EventNetworkMode.Offline:
                    // No INetworkSystem key is registered in offline mode.
                    break;
                default:
                    // The current transport has no tested dual-role host adapter.
                    // Fail closed to avoid applying client/server authority incorrectly.
                    logger.LogError($"Unsupported event network mode: {networkMode}. Falling back to Offline.", "Installer");
                    networkMode = EventNetworkMode.Offline;
                    BindInstance(container, networkMode);
                    break;
            }

            // Retain the existing message-authority service; it consumes both role-specific APIs.
            BindSingleton<NetworkAuthority, NetworkAuthority>(container);

            logger.LogInfo("Встановлення залежностей NetworkingSystem завершено", "Installer");
        }
    }
}
