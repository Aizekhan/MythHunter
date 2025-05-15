using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Systems.Core;
using MythHunter.Core.SceneManagement;

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
           
            
            // Інші сервіси
            BindSingleton<IEntityManager, EntityManager>(container);

            BindSingleton<IEventPool, EventPool>(container);
            // Реєструємо тимчасовий EventBus, який буде перезаписаний
            // ВАЖЛИВО: Використовуємо просту версію, яка не має залежностей
            BindSingleton<IEventBus, SimpleEventBus>(container);

            BindSingleton<IGameStateMachine, GameStateMachine>(container);
            // Реєстрація SystemRegistry
            BindSingleton<ISystemRegistry, SystemRegistry>(container);
            BindSingleton<IEcsWorld, EcsWorld>(container);
            // Реєстрація менеджера життєвого циклу DI
            BindSingleton<IDILifecycleManager, DILifecycleManager>(container);
            BindSingleton<ISceneDispatcher, SceneDispatcher>(container);
            // Реєструємо розширення DI
            var diExtensionsInstaller = new DIExtensionsInstaller();
            diExtensionsInstaller.InstallBindings(container);
        }
    }
}
