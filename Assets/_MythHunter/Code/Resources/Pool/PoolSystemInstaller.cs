using MythHunter.Core.DI;
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;

namespace MythHunter.Resources
{
    /// <summary>
    /// Інсталятор для системи пулінгу об'єктів
    /// </summary>
    public class PoolSystemInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Pool System", "Installer");

            // Реєструємо оптимізований менеджер пулів
            BindSingleton<IPoolManager, OptimizedPoolManager>(container);

            // Реєструємо фабрику для GameObjectPool
            container.RegisterFactory<GameObjectPool>((c) => {
                // Створюємо порожній пул для подальшого налаштування
                return new GameObjectPool(
                    UnityEngine.Resources.Load<UnityEngine.GameObject>("DefaultPoolPrefab"),
                    10,
                    null,
                    null,
                    null,
                    c.Resolve<IMythLogger>()
                );
            });

            logger.LogInfo("Pool System installed", "Installer");
        }
    }
}
