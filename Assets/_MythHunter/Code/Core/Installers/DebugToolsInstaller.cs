// DebugToolsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Debug.Profiling;
using MythHunter.Debug.UI;
using MythHunter.Networking.Security;
using MythHunter.Data.Serialization;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для інструментів відлагодження та розширених систем
    /// </summary>
    public class DebugToolsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей DebugTools...", "Installer");

            // Серіалізація з версіонуванням
            container.RegisterSingleton<VersionedSerializer, VersionedSerializer>();

            // Мережева безпека
            container.RegisterSingleton<INetworkSecurityProvider, NetworkSecurityProvider>();

            // Профілювання
            container.RegisterSingleton<SystemProfiler, SystemProfiler>();

            // Панель відлагодження (не реєструємо як сінглтон, бо це MonoBehaviour)
            // Панель буде створена в сцені як GameObject з компонентом

            logger.LogInfo("Встановлення залежностей DebugTools завершено", "Installer");
        }
    }
}
