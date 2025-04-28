using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Systems.Core;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор базових сервісів ядра системи
    /// </summary>
    public class CoreInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            // Базові системи ядра
            BindSingleton<IMythLogger, MythLogger>(container);
            BindSingleton<IEventBus, EventBus>(container);

            // ECS компоненти
            BindSingleton<IEntityManager, EntityManager>(container);
            BindSingleton<IEcsWorld, EcsWorld>(container);

            // Машина станів
            BindSingleton<IStateMachine<GameStateType>, StateMachine<GameStateType>>(container);

            // Реєстрація систем оновлення
            var systemRegistry = new SystemRegistry();
            BindInstance(container, systemRegistry);
        }
    }
}
