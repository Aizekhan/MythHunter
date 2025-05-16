// Шлях: Assets/_MythHunter/Code/Core/Installers/GameplayInstaller.cs

using MythHunter.Core.DI;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Systems.Core;
using MythHunter.Core.ECS;

using MythHunter.Game.Systems.Phase;
using MythHunter.Services.GameSettings;

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
            BindSingleton<IGameSettingsService, GameSettingsService>(container);
            // Реєстрація фазової системи з високим пріоритетом
            BindSingleton<IPhaseSystem, PhaseSystem>(container);

            // Реєстрація провайдера фаз - тепер тільки один тип!
            BindSingleton<IPhaseProvider, GamePhaseProvider>(container);

            // Отримання системного реєстру і провайдера фаз
            var systemRegistry = container.Resolve<ISystemRegistry>();
            var phaseProvider = container.Resolve<IPhaseProvider>();

            // Реєстрація базових систем
            systemRegistry.RegisterSystemWithPriority(container.Resolve<IPhaseSystem>(), SystemPriorities.Phase);

            // Реєстрація груп систем за фазами
            var movementGroup = systemRegistry.RegisterPhaseSystemGroup(
                "Movement",
                SystemPriorities.Active,
                logger,
                phaseProvider,
                "Movement");


            var planningGroup = systemRegistry.RegisterPhaseSystemGroup(
                "Planning",
                SystemPriorities.Planning,
                logger,
                phaseProvider,
                "Planning");

            // Тут можна зареєструвати інші системи в групах
            // movementGroup.AddSystem(container.Resolve<IMovementSystem>());
            // combatGroup.AddSystem(container.Resolve<ICombatSystem>());
            // planningGroup.AddSystem(container.Resolve<IPlanningSystem>());

            logger.LogInfo("Встановлення залежностей GameplaySystem завершено", "Installer");
        }
    }
}
