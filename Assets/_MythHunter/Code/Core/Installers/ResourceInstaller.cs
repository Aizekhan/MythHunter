using MythHunter.Core.DI;
using MythHunter.Resources.Core;
using MythHunter.Resources.Pool;
using MythHunter.Resources.Providers;
using MythHunter.Resources.SceneManagement;
using MythHunter.Utils.Logging;
using UnityEngine;

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

            // Реєстрація основних сервісів
            BindSingleton<IResourceProvider, DefaultResourceProvider>(container);
            BindSingleton<ISceneLoader, SceneLoader>(container);

            // Провайдер Addressables (якщо проект використовує Addressables)
            if (IsAddressablesAvailable())
            {
                BindSingleton<IAddressablesProvider, AddressablesProvider>(container);
                logger.LogInfo("Addressables Provider зареєстровано", "Installer");
            }
            else
            {
                BindSingleton<IAddressablesProvider, FallbackResourceProvider>(container);
                logger.LogInfo("Addressables недоступні, використовується FallbackResourceProvider", "Installer");
            }

            // Pool Manager для управління пулами об'єктів
            BindSingleton<IPoolManager, OptimizedPoolManager>(container);

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
