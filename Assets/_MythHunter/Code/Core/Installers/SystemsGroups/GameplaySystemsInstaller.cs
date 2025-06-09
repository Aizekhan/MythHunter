// Шлях: Assets/_MythHunter/Code/Core/Installers/SystemsGroups/GameplaySystemsInstaller.cs

using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.Game;
using MythHunter.Events.Domain;
using MythHunter.Game.Systems.Phase;
using MythHunter.Systems.Combat;
using MythHunter.Systems.Core;
using MythHunter.Systems.Groups;
using MythHunter.Systems.Movement;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;

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
            logger.LogInfo("Встановлення ігрових систем (Gameplay)...", "Installer");

            var systemRegistry = container.Resolve<ISystemRegistry>();
           
          
            BindSingleton<IPhaseSystem, PhaseSystem>(container);


            BindSingleton<IPathfindingSystem, PathfindingSystem>(container);
            BindSingleton<IMovementSystem, MovementSystem>(container);
            BindSingleton<IVisibilitySystem, VisibilitySystem>(container);

            BindSingleton<ICombatDetectionSystem, CombatDetectionSystem>(container);
            BindSingleton<ICombatAbilitySystem, CombatAbilitySystem>(container);
            BindSingleton<ICombatSystem, CombatSystem>(container);
            // Отримання зареєстрованих інстансів
            var phaseProvider = container.Resolve<IPhaseProvider>();
            var phaseSystem = container.Resolve<IPhaseSystem>();

            // 🟢 Реєстрація групи фазової системи
            var phaseGroup = systemRegistry.RegisterGroupWithCategory<SystemGroup>(
                "PhaseSystems",
                SystemPriorities.Phase,
                SystemInitializationCategory.Gameplay,
                logger
            );
            phaseGroup.AddSystem(phaseSystem);
        

            // Група планування
            var planningGroup = systemRegistry.RegisterPhaseSystemGroup(
                "Planning",
                SystemPriorities.Planning,
                logger,
                phaseProvider,
                new[] { GamePhase.Planning }
            );

            // Група руху
            var movementGroup = systemRegistry.RegisterPhaseSystemGroup(
                "Movement",
                SystemPriorities.Active,
                logger,
                phaseProvider,
                new[] { GamePhase.Active }
            );

            movementGroup.AddSystem(container.Resolve<IPathfindingSystem>());
            movementGroup.AddSystem(container.Resolve<IMovementSystem>());
            movementGroup.AddSystem(container.Resolve<IVisibilitySystem>());

            // Група бою
            var combatGroup = systemRegistry.RegisterPhaseSystemGroup(
                "Combat",
                SystemPriorities.Active + 10,
                logger,
                phaseProvider,
                new[] { GamePhase.Active }
            );

            combatGroup.AddSystem(container.Resolve<ICombatDetectionSystem>());
            combatGroup.AddSystem(container.Resolve<ICombatAbilitySystem>());
            combatGroup.AddSystem(container.Resolve<ICombatSystem>());

            logger.LogInfo("Геймплейні системи встановлено успішно", "Installer");
        }
    }
}
