// Assets/_MythHunter/Code/Core/Installers/Gameplay/HeroSystemInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Entities.Archetypes;
using MythHunter.Entities.Heroes;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    public class HeroSystemInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Hero systems...", "Installer");

            // Реєструємо реєстр архетипів героїв
            BindSingleton<IHeroArchetypeRegistry, HeroArchetypeRegistry>(container);

            // Реєструємо фабрику героїв
            BindSingleton<IHeroFactory, HeroFactory>(container);

            logger.LogInfo("Hero systems installed successfully", "Installer");
        }
    }
}
