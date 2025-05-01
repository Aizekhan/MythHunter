using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Entities;
using MythHunter.Utils.Logging;
using MythHunter.Events;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для системи сутностей та архетипів
    /// </summary>
    public class EntitiesInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей Entities System...", "Installer");

            // Реєстрація кешу компонентів
            var entityManager = container.Resolve<IEntityManager>();
            var componentCacheRegistry = new ComponentCacheRegistry(entityManager, logger);
            container.RegisterInstance<ComponentCacheRegistry>(componentCacheRegistry);

            // Реєстрація системи архетипів
            var archetypeSystem = new ArchetypeSystem(
                entityManager,
                container.Resolve<IEventBus>(),
                logger,
                componentCacheRegistry
            );

            container.RegisterInstance<ArchetypeSystem>(archetypeSystem);

            // Реєстрація фабрики сутностей
            var entityFactory = new EntityFactory(
                entityManager,
                archetypeSystem,
                logger
            );

            container.RegisterInstance<EntityFactory>(entityFactory);

            // Реєстрація фабрики компонентів
            var componentFactory = new ComponentFactory(entityManager, logger);
            container.RegisterInstance<ComponentFactory>(componentFactory);

            logger.LogInfo("Встановлення залежностей Entities System завершено", "Installer");
        }
    }
}
