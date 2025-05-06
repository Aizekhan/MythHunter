// Шлях: Assets/_MythHunter/Code/Core/Installers/InstallerRegistry.cs
using MythHunter.Core.DI;
using MythHunter.Core.Installers;
using MythHunter.Utils.Logging;
using MythHunter.Resources;

namespace MythHunter.Core
{
    /// <summary>
    /// Реєстр всіх інсталяторів для DI
    /// </summary>
    public static class InstallerRegistry
    {
        public static void RegisterInstallers(IDIContainer container)
        {
            // Базовий логер (до ін'єкції)
            var logger = MythHunter.Utils.Logging.MythLoggerFactory.GetDefaultLogger();

            // Інсталятори в порядку залежностей
            var installers = new DIInstaller[]
   {
new CoreInstaller(),
new EventsInstaller(),
new ResourceInstaller(),
new SerializationInstaller(),
new UIInstaller(),           
new PoolSystemInstaller(),
new NetworkingInstaller(),
new EntitiesInstaller(),
new GameplayInstaller(),
new DebugToolsInstaller()
   };

            // Встановлюємо залежності для кожного інсталятора
            foreach (var installer in installers)
            {
                try
                {
                    installer.InstallBindings(container);
                }
                catch (System.Exception ex)
                {
                    logger.LogError($"Помилка встановлення {installer.GetType().Name}: {ex.Message}", "Installer", ex);
                }
            }

            // Аналіз залежностей для виявлення помилок
            container.AnalyzeDependencies();
        }
    }
}
