using MythHunter.Core.DI;
using MythHunter.Resources.Core;
using MythHunter.Resources.Pool;
using MythHunter.Resources.Providers;
using MythHunter.Resources.SceneManagement;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для ресурсної системи
    /// </summary>
    public class ResourceInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей ResourceSystem...", "Installer");

            // DI автоматично створить з IMythLogger
            BindSingleton<IResourceProvider, DefaultResourceProvider>(container);

            // Pool Manager
            BindSingleton<IPoolManager, OptimizedPoolManager>(container);

            // Addressables — тепер без int в конструкторі
            if (IsAddressablesAvailable())
            {
                BindSingleton<IAddressablesProvider, AddressablesProvider>(container);
                logger.LogInfo("Addressables Provider зареєстровано", "Installer");
            }

            // Основні сервіси
            BindSingleton<IResourceManager, ResourceManager>(container);
            BindSingleton<ISceneLoader, SceneLoader>(container);

            logger.LogInfo("Встановлення залежностей ResourceSystem завершено", "Installer");
        }

        private bool IsAddressablesAvailable()
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name == "Unity.Addressables")
                    return true;
            }
            return false;
        }
    }
}
