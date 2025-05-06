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
            // Логер з GameBotstrapper.cs
            var logger = container.Resolve<IMythLogger>();
           
            // Реєстрація SystemRegistry
             BindSingleton<ISystemRegistry, SystemRegistry>(container);
            // Інші сервіси
            BindSingleton<IEntityManager, EntityManager>(container);
            
            BindSingleton<IEcsWorld, EcsWorld>(container);
            BindSingleton<IStateMachine<GameStateType>, StateMachine<GameStateType>>(container);
        }
    }
}
