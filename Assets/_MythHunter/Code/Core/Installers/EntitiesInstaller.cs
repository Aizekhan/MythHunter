// Шлях: Assets/_MythHunter/Code/Core/Installers/EntitiesInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Entities;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Entities.Archetypes;
using MythHunter.Systems.Gameplay;

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
            var entityManager = container.Resolve<IEntityManager>();
            var eventBus = container.Resolve<IEventBus>();

            // Реєстрація кешу компонентів
            var componentCacheRegistry = new ComponentCacheRegistry(entityManager, logger);
            container.RegisterInstance<ComponentCacheRegistry>(componentCacheRegistry);

            // Реєстрація фабрики компонентів
            var componentFactory = new ComponentFactory(entityManager, logger);
            container.RegisterInstance<ComponentFactory>(componentFactory);

            // Реєстрація реєстру шаблонів архетипів
            var archetypeTemplateRegistry = new ArchetypeTemplateRegistry(entityManager, logger);
            container.RegisterInstance<ArchetypeTemplateRegistry>(archetypeTemplateRegistry);

            // Реєстрація системи архетипів
            var archetypeSystem = new ArchetypeSystem(
     entityManager,
     logger,
     eventBus,
     archetypeTemplateRegistry);
            container.RegisterInstance<ArchetypeSystem>(archetypeSystem);

            // Реєстрація фабрики сутностей
            var entityFactory = new EntityFactory(
                entityManager,
                archetypeSystem,
                logger
            );
            container.RegisterInstance<EntityFactory>(entityFactory);

            // Реєстрація системи створення сутностей
            var entitySpawnSystem = new EntitySpawnSystem(
                entityFactory,
                archetypeSystem,
                eventBus,
                logger
            );
            container.RegisterInstance<IEntitySpawnSystem>(entitySpawnSystem);

            // Реєстрація компонентів у SystemRegistry
            var systemRegistry = container.Resolve<Systems.Core.SystemRegistry>();
            systemRegistry.RegisterSystem(archetypeSystem);
            systemRegistry.RegisterSystem(entitySpawnSystem);

            logger.LogInfo("Встановлення залежностей Entities System завершено", "Installer");
        }
    }
}
