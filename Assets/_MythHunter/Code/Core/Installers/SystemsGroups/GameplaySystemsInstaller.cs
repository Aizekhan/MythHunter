// Шлях: Assets/_MythHunter/Code/Core/Installers/GameplaySystemsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Systems.Core;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;
using MythHunter.Systems.Movement;
using MythHunter.Systems.Combat;
using MythHunter.Events.Domain;
using MythHunter.Core.ECS;
using MythHunter.Systems.Groups;
using MythHunter.Game.Systems.Phase;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для ігрових систем
    /// </summary>
    public class GameplaySystemsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення ігрових систем...", "Installer");

            // Системи фазування
            BindSingleton<IPhaseProvider, GamePhaseProvider>(container);
            BindSingleton<IPhaseSystem, PhaseSystem>(container);
            
            // Системи руху
            BindSingleton<IPathfindingSystem, PathfindingSystem>(container);
            BindSingleton<IMovementSystem, MovementSystem>(container);
            BindSingleton<IVisibilitySystem, VisibilitySystem>(container);

            // Системи бою
            BindSingleton<ICombatDetectionSystem, CombatDetectionSystem>(container);
            BindSingleton<ICombatAbilitySystem, CombatAbilitySystem>(container);
            BindSingleton<ICombatSystem, CombatSystem>(container);
            var systemRegistry = container.Resolve<ISystemRegistry>();
            var phaseProvider = container.Resolve<IPhaseProvider>();

            // Реєстрація групи фазових систем
            systemRegistry.RegisterGroupWithCategory<SystemGroup>(
                "PhaseSystems",
                SystemPriorities.Phase,
                SystemInitializationCategory.Gameplay,
                logger,
                new[] {
                    typeof(IPhaseSystem)
                }
            );

            // Реєстрація групи систем планування з відразу вказаними фазами
            systemRegistry.RegisterPhaseSystemGroup(
                "Planning",
                SystemPriorities.Planning,
                logger,
                phaseProvider,
                new[] { GamePhase.Planning }
            );

            // Реєстрація групи систем руху з відразу вказаними фазами
            var movementGroup = systemRegistry.RegisterPhaseSystemGroup(
                "Movement",
                SystemPriorities.Active,
                logger,
                phaseProvider,
                new[] { GamePhase.Active }
            );

            // Додавання систем руху до групи
            movementGroup.AddSystem(container.Resolve<IPathfindingSystem>());
            movementGroup.AddSystem(container.Resolve<IMovementSystem>());
            movementGroup.AddSystem(container.Resolve<IVisibilitySystem>());

            // Реєстрація групи систем бою з відразу вказаними фазами
            var combatGroup = systemRegistry.RegisterPhaseSystemGroup(
                "Combat",
                SystemPriorities.Active + 10,
                logger,
                phaseProvider,
                new[] { GamePhase.Active }
            );

            // Додавання систем бою до групи
            combatGroup.AddSystem(container.Resolve<ICombatDetectionSystem>());
            combatGroup.AddSystem(container.Resolve<ICombatAbilitySystem>());
            combatGroup.AddSystem(container.Resolve<ICombatSystem>());

            logger.LogInfo("Ігрові системи встановлено успішно", "Installer");
        }
    }
}
