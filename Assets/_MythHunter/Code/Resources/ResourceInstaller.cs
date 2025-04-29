using MythHunter.Core.DI;
using MythHunter.Resources.Core;
using MythHunter.Resources.Pool;
using MythHunter.Resources.Providers;
using MythHunter.Utils.Logging;

namespace MythHunter.Resources
{
    /// <summary>
    /// Інсталятор для ресурсної системи та пулів об'єктів
    /// </summary>
    public class ResourceInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            // Логер має вже бути інстальований в CoreInstaller
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Resource System", "Installer");

            // Базовий менеджер пулів
            BindSingleton<IPoolManager, PoolManager>(container);

            // Встановлюємо правильний ресурсний провайдер
            // В залежності від налаштувань проекту
#if USE_ADDRESSABLES
                // Використовуємо Addressables, якщо вони доступні
                var resourceProvider = new AddressableResourceProvider(logger, container.Resolve<IPoolManager>());
                container.RegisterInstance<IResourceProvider>(resourceProvider);
                logger.LogInfo("Using Addressable Resource Provider", "Installer");
#else
            // Базовий провайдер ресурсів 
            BindSingleton<IResourceProvider, DefaultResourceProvider>(container);

            // Менеджер пулів
            BindSingleton<IPoolManager, PoolManager>(container);

            // Провайдер пулінгу як обгортка над базовим провайдером
            var baseProvider = container.Resolve<IResourceProvider>();
            var poolManager = container.Resolve<IPoolManager>();
          
            var poolProvider = new ResourcePoolProvider(baseProvider, poolManager, logger);
            // Реєструємо як окремий сервіс для явного використання
            container.RegisterInstance<ResourcePoolProvider>(poolProvider);

            // Додатково реєструємо фоллбек провайдер для Addressables API
            var fallbackProvider = new FallbackResourceProvider(logger,
                container.Resolve<DefaultResourceProvider>() as DefaultResourceProvider);
            container.RegisterInstance<IAddressablesProvider>(fallbackProvider);
#endif

            
        }
    }
}
