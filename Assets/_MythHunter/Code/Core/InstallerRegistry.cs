using MythHunter.Core.DI;

namespace MythHunter.Core
{
    /// <summary>
    /// Централізований реєстратор інсталерів для DI
    /// </summary>
    public static class InstallerRegistry
    {
        public static void RegisterInstallers(IDIContainer container)
        {
            // TODO: Wizard буде автоматично додавати сюди інсталери
            // container.Register<MyPanelInstaller>();
        }
    }
}