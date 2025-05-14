// Шлях: Assets/_MythHunter/Code/Core/Installers/CombatSystemInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Systems;
using MythHunter.Systems.Combat;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор бойової системи
    /// </summary>
    public class CombatSystemInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            var systemRegistry = container.Resolve<ISystemRegistry>();

            // Реєструємо системи як синглтони
            BindSingleton<ICombatDetectionSystem, CombatDetectionSystem>(container);
            BindSingleton<ICombatAbilitySystem, CombatAbilitySystem>(container);
            BindSingleton<ICombatSystem, CombatSystem>(container);

            // Отримуємо системи
            var combatDetectionSystem = container.Resolve<ICombatDetectionSystem>();
            var combatAbilitySystem = container.Resolve<ICombatAbilitySystem>();
            var combatSystem = container.Resolve<ICombatSystem>();

            // Реєструємо системи з пріоритетами
            // Використовуємо існуючі пріоритети, розташовуючи їх після руху
            systemRegistry.RegisterSystemWithPriority(combatDetectionSystem, SystemPriorities.Active + 1);
            systemRegistry.RegisterSystemWithPriority(combatAbilitySystem, SystemPriorities.Active + 2);
            systemRegistry.RegisterSystemWithPriority(combatSystem, SystemPriorities.Active + 3);

            logger.LogInfo("CombatSystemInstaller: Combat systems registered", "Installers");
        }
    }
}
