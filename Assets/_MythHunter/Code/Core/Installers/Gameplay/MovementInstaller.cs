// Assets/_MythHunter/Code/Core/Installers/MovementInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events.Domain;
using MythHunter.Systems.Core;
using MythHunter.Systems.Movement;
using MythHunter.Utils.Logging;



namespace MythHunter.Core.Installers

{
    /// <summary>
    /// Інсталятор для систем руху
    /// </summary>
    public class MovementInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            // Отримуємо логер для інформаційних повідомлень
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Movement systems", "Installer");

            // Реєструємо системи руху
            BindSingleton<IPathfindingSystem, PathfindingSystem>(container);
            BindSingleton<IMovementSystem, MovementSystem>(container);
            BindSingleton<IVisibilitySystem, VisibilitySystem>(container);

            // Отримуємо реєстр систем
            var systemRegistry = container.Resolve<ISystemRegistry>();

            // Отримуємо провайдер фаз
            var phaseProvider = container.Resolve<IPhaseProvider>();

            // Реєструємо системи в реєстрі систем з пріоритетами
            systemRegistry.RegisterSystemWithPriority(
                container.Resolve<IPathfindingSystem>(),
                SystemPriorities.Core // Базовий пріоритет для системи пошуку шляху
            );

            systemRegistry.RegisterSystemWithPriority(
                container.Resolve<IVisibilitySystem>(),
                SystemPriorities.Active - 10 // Вище за рух, щоб оновлювати видимість перед рухом
            );

            systemRegistry.RegisterSystemWithPriority(
                container.Resolve<IMovementSystem>(),
                SystemPriorities.Active // Пріоритет для системи руху
            );

            // Налаштовуємо активні фази для системи руху
            var movementSystem = container.Resolve<IMovementSystem>() as IPhaseFilteredSystem;
            if (movementSystem != null)
            {
                movementSystem.SetActivePhases(new[] { GamePhase.Active });
            }

            logger.LogInfo("Movement systems installed successfully", "Installer");
        }
    }
}
