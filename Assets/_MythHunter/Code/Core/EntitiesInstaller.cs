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

            // Реєстрація кешу компонентів
            var entityManager = container.Resolve<IEntityManager>();
            var componentCacheRegistry = new ComponentCacheRegistry(entityManager, logger);
            container.RegisterInstance<ComponentCacheRegistry>(componentCacheRegistry);

            // Реєстрація реєстру архетипів
            var entityArchetypeRegistry = new EntityArchetypeRegistry(entityManager);
            container.RegisterInstance<EntityArchetypeRegistry>(entityArchetypeRegistry);

            // Реєстрація системи архетипів (після реєстру)
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
                entityArchetypeRegistry, // Використовуємо EntityArchetypeRegistry замість ArchetypeSystem
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
