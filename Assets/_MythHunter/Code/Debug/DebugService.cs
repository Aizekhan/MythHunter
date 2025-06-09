// Шлях: Assets/_MythHunter/Code/Debug/DebugService.cs
using UnityEngine;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.Debug.Core;
using MythHunter.Debug.UI;

namespace MythHunter.Debug
{
    public class DebugService : IDebugService
    {
        private readonly IDIContainer _container;
        private readonly IMythLogger _logger;

        [Inject]
        public DebugService(IDIContainer container, IMythLogger logger)
        {
            _container = container;
            _logger = logger;
        }

        public void CreateDebugDashboard()
        {
            if (Object.FindFirstObjectByType<DebugDashboard>() != null)
            {
                _logger.LogWarning("Debug dashboard already exists", "Debug");
                return;
            }

            var dashboardObject = new GameObject("MythHunter_DebugDashboard");
            var dashboard = dashboardObject.AddComponent<DebugDashboard>();

            // Явно резолвимо залежності
            var toolFactory = _container.Resolve<DebugToolFactory>();

            // Ініціалізуємо компонент
            dashboard.Initialize(_logger, toolFactory);

            Object.DontDestroyOnLoad(dashboardObject);

            _logger.LogInfo("Debug dashboard created", "Debug");
        }
    }
}
