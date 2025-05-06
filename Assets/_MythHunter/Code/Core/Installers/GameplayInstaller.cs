// Файл: Assets/_MythHunter/Code/Core/Installers/GameplayInstaller.cs (оновлений)
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
            systemRegistry.RegisterSystemWithPriority(container.Resolve<IPhaseSystem>(), 100);

            // Реєстрація систем специфічних для певних фаз
            // Наприклад:
            // systemRegistry.RegisterPhaseSystem(container.Resolve<IMovementSystem>(), GamePhase.Movement);
            // systemRegistry.RegisterPhaseSystem(container.Resolve<ICombatSystem>(), GamePhase.Combat);
            // systemRegistry.RegisterPhaseSystem(container.Resolve<IPlanningSystem>(), GamePhase.Planning);

            logger.LogInfo("Встановлення залежностей GameplaySystem завершено", "Installer");
        }
    }
}
