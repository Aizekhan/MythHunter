// Шлях: Assets/_MythHunter/Code/Core/Installers/NavigationInstaller.cs

using MythHunter.Core.DI;
using MythHunter.UI.Navigation;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для сервісів навігації між екранами
    /// </summary>
    public class NavigationInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            // Анімації переходів
            BindSingleton<IScreenTransition, ScreenTransition>(container);

            // Сервіс навігації
            BindSingleton<INavigationService, NavigationService>(container);
        }
    }
}
