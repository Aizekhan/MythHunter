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

            // Реєстрація основних мережевих сервісів
            BindSingleton<INetworkSystem, NetworkSystem>(container);
            BindSingleton<INetworkSerializer, BinaryNetworkSerializer>(container);

            // Клієнтська частина
            BindSingleton<INetworkClient, NetworkClient>(container);

            // Серверна частина
            BindSingleton<INetworkServer, NetworkServer>(container);
            // Кліентна частина
            BindSingleton<INetworkClient, NetworkClient>(container);
            // Обробники повідомлень
            BindSingleton<INetworkMessageHandlerRegistry, NetworkMessageHandlerRegistry>(container);

            logger.LogInfo("Встановлення залежностей NetworkingSystem завершено", "Installer");
        }
    }
}
