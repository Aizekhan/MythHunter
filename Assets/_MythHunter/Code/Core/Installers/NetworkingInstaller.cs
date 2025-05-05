// Шлях: Assets/_MythHunter/Code/Core/Installers/NetworkingInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Networking.Core;
using MythHunter.Networking.Client;
using MythHunter.Networking.Server;
using MythHunter.Networking.Serialization;
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

            // Реєстрація авторитету мережевої системи
            BindSingleton<NetworkAuthority, NetworkAuthority>(container);

            logger.LogInfo("Встановлення залежностей NetworkingSystem завершено", "Installer");
        }
    }
}
