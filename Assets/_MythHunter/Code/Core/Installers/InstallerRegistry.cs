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
        // 1. Ядро системи
        new CoreInstaller(),

        // 2. Система подій
        new EventsInstaller(),
        
        // 3. Система ресурсів
        new ResourceInstaller(),
        // 3. Система серіалізації компонентів
        new SerializationInstaller(), 
        // 4. Система пулінгу об'єктів
        new PoolSystemInstaller(),
        
        // 5. Мережева система
        new NetworkingInstaller(),
        
         // 7. Система сутностей (перемістили в кінець!)
        new EntitiesInstaller(),

        // 6. UI система
        new UIInstaller(),


        // 8. Ігрова система
        new GameplayInstaller(),
        
       
        
        // 9. Інструменти відлагодження
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
