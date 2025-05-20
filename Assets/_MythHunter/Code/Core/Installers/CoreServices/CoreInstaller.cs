// Шлях: Assets/_MythHunter/Code/Core/Installers/CoreInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Core.SceneManagement;
using MythHunter.Resources.SceneManagement;
using MythHunter.Systems.Core;
using MythHunter.Systems.Phase;
using MythHunter.Services.GameSettings;
using MythHunter.Entities;
using MythHunter.Core.MonoBehaviours;

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
            BindSingleton<IDependencyInjector, DependencyInjector>(container);
            // Базові сервіси
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

            // 🟢 Реєстрація фазового провайдера та фазової системи
            BindSingleton<IPhaseProvider, GamePhaseProvider>(container);     // або EmergencyPhaseProvider

            BindSingleton<SceneLoader, SceneLoader>(container); // цей другорядний, використовується тільки в СценДиспечер (тому нема інтерфейса)
            BindSingleton<ISceneDispatcher, SceneDispatcher>(container);// 
            BindSingleton<IGameFlowManager, GameFlowManager>(container);

            BindSingleton<IGameSettingsService, GameSettingsService>(container);
      


            // Реєструємо розширення DI
            var diExtensionsInstaller = new DIExtensionsInstaller();
            diExtensionsInstaller.InstallBindings(container);
        }
    }
}
