// Шлях: Assets/_MythHunter/Code/Core/Installers/EntitiesInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Entities;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Entities.Archetypes; // Для ArchetypeSystem

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

            // Отримуємо необхідні залежності
          
            var eventBus = container.Resolve<IEventBus>();

            // Реєстрація кешу компонентів
            var entityManager = container.Resolve<IEntityManager>();
            var componentCacheRegistry = new ComponentCacheRegistry(entityManager, logger);
            container.RegisterInstance<ComponentCacheRegistry>(componentCacheRegistry);

            // Реєстрація оптимізатора ECS
            var ecsOptimizer = new EcsOptimizer(entityManager, logger);
            container.RegisterInstance<EcsOptimizer>(ecsOptimizer);


            // Реєстрація фабрики компонентів
            var componentFactory = new ComponentFactory(entityManager, logger);
            container.RegisterInstance<ComponentFactory>(componentFactory);

            // Реєстрація реєстру шаблонів архетипів
            var archetypeTemplateRegistry = new ArchetypeTemplateRegistry(entityManager, logger);
            container.RegisterInstance<ArchetypeTemplateRegistry>(archetypeTemplateRegistry);

            // Реєстрація системи архетипів
            var archetypeSystem = new ArchetypeSystem(
                entityManager,
            eventBus,
                logger,
                archetypeTemplateRegistry
            );
            container.RegisterInstance<ArchetypeSystem>(archetypeSystem);

            // Реєстрація фабрики сутностей
            var entityFactory = new EntityFactory(
                entityManager,
                archetypeSystem,
                logger
            );
            container.RegisterInstance<EntityFactory>(entityFactory);

            // Реєстрація компонентів за допомогою SystemRegistry
            var systemRegistry = container.Resolve<Systems.Core.SystemRegistry>();
            systemRegistry.RegisterSystem(archetypeSystem);

            logger.LogInfo("Встановлення залежностей Entities System завершено", "Installer");
        }
    }
}
