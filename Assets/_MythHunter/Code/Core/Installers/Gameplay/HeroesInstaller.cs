// Assets/_MythHunter/Code/Core/Installers/HeroesInstaller.cs

using MythHunter.Core.DI;
using MythHunter.Entities.Heroes;
using MythHunter.Services.Heroes;
using MythHunter.Systems.Core;
using MythHunter.Systems.Heroes;

namespace MythHunter.Core.Installers
{
    public class HeroesInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            // Героєва фабрика
            BindSingleton<IHeroFactory, HeroFactory>(container);

            // Сервіс даних героїв (локальна реалізація для тестування)
            BindSingleton<IHeroDataService, LocalHeroDataService>(container);

            // Система расових і класових бонусів
            BindSingleton<RaceClassBonusSystem, RaceClassBonusSystem>(container);

            // Реєстрація системи в реєстрі систем
            var systemRegistry = container.Resolve<ISystemRegistry>();
            systemRegistry.RegisterSystem(container.Resolve<RaceClassBonusSystem>());
        }
    }
}
