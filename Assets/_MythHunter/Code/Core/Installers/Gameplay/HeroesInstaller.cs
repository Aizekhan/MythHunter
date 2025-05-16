// Path: Assets/_MythHunter/Code/Core/Installers/HeroesInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Entities.Heroes;
using MythHunter.Services.Heroes;
using MythHunter.Systems.Core;
using MythHunter.Systems.Heroes;

public class HeroesInstaller : DIInstaller
{
    public override void InstallBindings(IDIContainer container)
    {
        // Героєва фабрика
        BindSingleton<IHeroFactory, HeroFactory>(container);

        // Сервіс даних героїв
        BindSingleton<IHeroDataService, LocalHeroDataService>(container);

        // Система расових і класових бонусів
        BindSingleton<IRaceClassBonusSystem, RaceClassBonusSystem>(container);

        // Головна система героїв
        BindSingleton<IHeroSystem, HeroSystem>(container);

        // Реєстрація систем у реєстрі систем
        var systemRegistry = container.Resolve<ISystemRegistry>();
        systemRegistry.RegisterSystem(container.Resolve<IRaceClassBonusSystem>());
        systemRegistry.RegisterSystem(container.Resolve<IHeroSystem>());
    }
}
