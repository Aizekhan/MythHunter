// Шлях: Assets/_MythHunter/Code/Core/Installers/ResourceInstaller.cs
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

            // Реєстрація провайдерів ресурсів
            BindSingleton<DefaultResourceProvider, DefaultResourceProvider>(container);

            // Pool Manager для управління пулами об'єктів
            BindSingleton<IPoolManager, OptimizedPoolManager>(container);

            // Провайдер Addressables (якщо проект використовує Addressables)
            if (IsAddressablesAvailable())
            {
                BindSingleton<AddressablesProvider, AddressablesProvider>(container);
                BindInstance<IAddressablesProvider>(container, container.Resolve<AddressablesProvider>());
                logger.LogInfo("Addressables Provider зареєстровано", "Installer");
            }

            // Реєстрація основних сервісів
            BindSingleton<IResourceManager, ResourceManager>(container);
            BindSingleton<IResourceProvider, DefaultResourceProvider>(container);
            BindSingleton<ISceneLoader, SceneLoader>(container);

            logger.LogInfo("Встановлення залежностей ResourceSystem завершено", "Installer");
        }

        private bool IsAddressablesAvailable()
        {
            // Перевірка наявності системи Addressables
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name == "Unity.Addressables")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
