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
            // Базові сервіси
            BindSingleton<IMythLogger, MythLogger>(container);
            BindSingleton<IEntityManager, EntityManager>(container);
            BindSingleton<IEventPool, EventPool>(container);
            BindSingleton<IEventBus, EventBus>(container);
            BindSingleton<IEcsWorld, EcsWorld>(container);
            BindSingleton<IStateMachine<GameStateType>, StateMachine<GameStateType>>(container);

            // Виправляємо реєстрацію SystemRegistry
            BindSingleton<ISystemRegistry, SystemRegistry>(container);
        }
    }
}
