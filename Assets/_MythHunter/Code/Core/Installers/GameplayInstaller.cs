using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.Game;
using MythHunter.Systems.Core;
using MythHunter.Systems.Phase;
using MythHunter.Systems.Combat;
using MythHunter.Systems.Movement;
using MythHunter.Systems.AI;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.UI.Core;

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
                systemRegistry = new SystemRegistry();
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
            var movementSystem = new MovementSystem(entityManager, eventBus, logger);
            container.RegisterInstance<IMovementSystem>(movementSystem);
            systemRegistry.RegisterSystem(movementSystem);

            // Бойова система
            var combatSystem = new CombatSystem(entityManager, eventBus, logger);
            container.RegisterInstance<ICombatSystem>(combatSystem);
            systemRegistry.RegisterSystem(combatSystem);

            // AI система
            var aiSystem = new AISystem(entityManager, eventBus, logger);
            container.RegisterInstance<IAISystem>(aiSystem);
            systemRegistry.RegisterSystem(aiSystem);

            // Система рун
            var runeSystem = new RuneSystem(entityManager, eventBus, logger);
            container.RegisterInstance<IRuneSystem>(runeSystem);
            systemRegistry.RegisterSystem(runeSystem);

            logger.LogInfo("Встановлення залежностей GameplaySystem завершено", "Installer");
        }
    }
}
