// Шлях: Assets/_MythHunter/Code/Core/Installers/GameplayInstaller.cs

using MythHunter.Core.DI;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Systems.Core;
using MythHunter.Events.Domain;
using MythHunter.Systems.Extensions;

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

            // Реєстрація фазової системи з високим пріоритетом
            BindSingleton<IPhaseSystem, PhaseSystem>(container);

            // Отримання системного реєстру
            var systemRegistry = container.Resolve<ISystemRegistry>();

            // Реєстрація базових систем
            systemRegistry.RegisterSystemWithPriority(container.Resolve<IPhaseSystem>(), SystemPriorities.Phase);

            // Реєстрація систем специфічних для певних фаз
            // Реєстрація груп систем за фазами - передаємо логер при створенні
            var movementGroup = systemRegistry.RegisterPhaseSystemGroup(
                "Movement",
                SystemPriorities.Movement,
                logger,
                GamePhase.Movement);

            var combatGroup = systemRegistry.RegisterPhaseSystemGroup(
                "Combat",
                SystemPriorities.Combat,
                logger,
                GamePhase.Combat);

            var planningGroup = systemRegistry.RegisterPhaseSystemGroup(
                "Planning",
                SystemPriorities.Planning,
                logger,
                GamePhase.Planning);

            // Додавання систем в групи (за наявності)
            // Наприклад:
            // if (container.IsRegistered<IMovementSystem>())
            //     movementGroup.AddSystem(container.Resolve<IMovementSystem>());

            logger.LogInfo("Встановлення залежностей GameplaySystem завершено", "Installer");
        }
    }
}
