using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.Game;
using MythHunter.Systems.Core;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Systems.Gameplay;
using MythHunter.Entities;
using MythHunter.Entities.Archetypes;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для основних ігрових систем
    /// </summary>
    public class GameplayInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей GameplaySystem...", "Installer");

            // Отримання існуючих залежностей
            var entityManager = container.Resolve<IEntityManager>();
            var eventBus = container.Resolve<IEventBus>();

            // Реєстрація системного реєстру, якщо його ще немає
            SystemRegistry systemRegistry;
            if (!container.IsRegistered<SystemRegistry>())
            {
                systemRegistry = new SystemRegistry(logger);
                container.RegisterInstance<SystemRegistry>(systemRegistry);
            }
            else
            {
                systemRegistry = container.Resolve<SystemRegistry>();
            }

            // Створення та реєстрація основних ігрових систем

            // Фазова система
            var phaseSystem = new PhaseSystem(eventBus, logger);
            container.RegisterInstance<IPhaseSystem>(phaseSystem);
            systemRegistry.RegisterSystem(phaseSystem);

            // Система руху

            // Бойова система

            // AI система

            // Система рун

            // Система архетипів та сутностей
            var archetypeTemplateRegistry = new ArchetypeTemplateRegistry(entityManager, logger);

            // Створюємо необхідні компоненти в правильному порядку
            var archetypeRegistry = new ArchetypeRegistry(logger);

            var archetypeDetector = new ArchetypeDetector(
                entityManager,
                archetypeTemplateRegistry,
                archetypeRegistry,
                logger);

            // Створюємо систему архетипів з правильними аргументами
            var archetypeSystem = new ArchetypeSystem(
                archetypeRegistry,
                archetypeDetector,
                archetypeTemplateRegistry,
                logger,
                eventBus);

            var entityFactory = new EntityFactory(entityManager, archetypeSystem, logger);
            var entitySpawnSystem = new EntitySpawnSystem(entityFactory, archetypeSystem, eventBus, logger);

            // Реєструємо системи
            container.RegisterInstance<ArchetypeSystem>(archetypeSystem);
            container.RegisterInstance<IEntitySpawnSystem>(entitySpawnSystem);
            systemRegistry.RegisterSystem(archetypeSystem);
            systemRegistry.RegisterSystem(entitySpawnSystem);

            logger.LogInfo("Встановлення залежностей GameplaySystem завершено", "Installer");
        }
    }
}
