// Шлях: Assets/_MythHunter/Code/Core/Installers/EntitiesInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Entities;
using MythHunter.Entities.Archetypes;
using MythHunter.Events;
using MythHunter.Systems.Core;
using MythHunter.Systems.Gameplay;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для системи сутностей та архетипів
    /// </summary>
    public class EntitiesInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей Entities System...", "Installer");

            // ComponentCacheRegistry
            BindSingleton<ComponentCacheRegistry, ComponentCacheRegistry>(container);

            // ComponentFactory
            BindSingleton<ComponentFactory, ComponentFactory>(container);

            // ArchetypeTemplateRegistry
            BindSingleton<IArchetypeTemplateRegistry, ArchetypeTemplateRegistry>(container);

            // ArchetypeRegistry
            BindSingleton<IArchetypeRegistry, ArchetypeRegistry>(container);

            // ArchetypeDetector
            BindSingleton<IArchetypeDetector, ArchetypeDetector>(container);

            // ArchetypeSystem
            BindSingleton<IArchetypeSystem, ArchetypeSystem>(container);

            // EntityFactory
            BindSingleton<IEntityFactory, EntityFactory>(container);

            // EntitySpawnSystem
            BindSingleton<IEntitySpawnSystem, EntitySpawnSystem>(container);

            // Реєстрація в SystemRegistry
            var systemRegistry = container.Resolve<ISystemRegistry>();
            systemRegistry.RegisterSystem(container.Resolve<IArchetypeSystem>());
            systemRegistry.RegisterSystem(container.Resolve<IEntitySpawnSystem>());

            logger.LogInfo("Встановлення залежностей Entities System завершено", "Installer");
        }
    }
}
