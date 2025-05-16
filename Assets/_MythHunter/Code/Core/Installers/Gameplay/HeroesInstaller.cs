// Path: Assets/_MythHunter/Code/Core/Installers/HeroesInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Entities.Archetypes;
using MythHunter.Entities.Heroes;
using MythHunter.Services.Heroes;
using MythHunter.Systems.Core;
using MythHunter.Systems.Heroes;
using MythHunter.Utils.Logging;

public class HeroesInstaller : DIInstaller
{
    public override void InstallBindings(IDIContainer container)
    {
        var logger = container.Resolve<IMythLogger>();
        logger.LogInfo("Installing Hero systems...", "Installer");

        BindSingleton<IHeroArchetypeRegistry, HeroArchetypeRegistry>(container);
        BindSingleton<IHeroFactory, HeroFactory>(container);
        BindSingleton<IHeroSystem, HeroSystem>(container);
        BindSingleton<IRaceClassBonusSystem, RaceClassBonusSystem>(container);
        BindSingleton<IHeroDataService, LocalHeroDataService>(container);

        var systemRegistry = container.Resolve<ISystemRegistry>();
        systemRegistry.RegisterSystem(container.Resolve<IRaceClassBonusSystem>());
        systemRegistry.RegisterSystem(container.Resolve<IHeroSystem>());

        logger.LogInfo("Hero systems installed successfully", "Installer");
    }
}
