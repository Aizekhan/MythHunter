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

            // Реєстрація клієнтської частини
            BindSingleton<INetworkClient, NetworkClient>(container);
            BindSingleton<IClientNetworkSystem, ClientNetworkSystem>(container);

            // Реєстрація серверної частини
            BindSingleton<INetworkServer, NetworkServer>(container);
            BindSingleton<IServerNetworkSystem, ServerNetworkSystem>(container);

            // Current shipped application has no explicit scene/menu mode selector yet.
            // Keep synchronization disabled by default; Game Composition Root may replace
            // these exact registrations with an explicit mode and the selected role service.
            BindInstance(container, EventNetworkMode.Offline);

            logger.LogInfo("Встановлення залежностей NetworkingSystem завершено", "Installer");
        }
    }
}
