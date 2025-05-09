// Шлях: Assets/_MythHunter/Code/Core/Installers/DIExtensionsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Core.Validation;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для розширених можливостей DI
    /// </summary>
    public class DIExtensionsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення розширень DI...", "Installer");

            // Реєструємо валідатор залежностей
            Bind<DependencyValidator, DependencyValidator>(container);

            // Створюємо глобальний скоуп за замовчуванням
            var defaultScope = container.CreateScope("GlobalScope");
            container.SetCurrentScope(defaultScope);

            // Створюємо компонент для ін'єкції в LazyMonoBehaviour
            if (Object.FindFirstObjectByType<LazyDependencyInjector>() == null)
            {
                var injectorObject = new GameObject("MythHunter_LazyDependencyInjector");
                var injector = injectorObject.AddComponent<LazyDependencyInjector>();
                Object.DontDestroyOnLoad(injectorObject);

                logger.LogInfo("Created LazyDependencyInjector", "Installer");
            }

            logger.LogInfo("Розширення DI встановлено", "Installer");

            // Валідуємо залежності, якщо в режимі розробки
#if UNITY_EDITOR
            var validator = container.Resolve<DependencyValidator>();
            var issues = validator.ValidateAllDependencies();

            if (issues.Count > 0)
            {
                logger.LogWarning($"Виявлено {issues.Count} проблем з залежностями. Перевірте логи для деталей.", "Installer");
            }
#endif
        }
    }
}
