// Шлях: Assets/_MythHunter/Code/Debug/UI/DebugDashboardInitializer.cs
using UnityEngine;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.Core.MonoBehaviours;

namespace MythHunter.Debug.UI
{
    /// <summary>
    /// Ініціалізатор для дашборду відлагодження
    /// </summary>
    public class DebugDashboardInitializer : InjectableMonoBehaviour
    {
        [Inject] private IMythLogger _logger;

        [SerializeField] private bool _createOnStart = true;

        protected override void OnInjectionsCompleted()
        {
            base.OnInjectionsCompleted();

          

            if (_createOnStart)
            {
                CreateDebugDashboard();
            }
        }

        /// <summary>
        /// Створює дашборд для відлагодження
        /// </summary>
        public void CreateDebugDashboard()
        {
            if (FindObjectOfType<DebugDashboard>() != null)
            {
                _logger?.LogWarning("Debug dashboard already exists", "Debug");
                return;
            }

            var dashboardObject = new GameObject("MythHunter_DebugDashboard");

            // Додаємо DependencyScope
            var scope = dashboardObject.AddComponent<DependencyScope>();

            // Додаємо компонент дашборду  
            var dashboard = dashboardObject.AddComponent<DebugDashboard>();

            DontDestroyOnLoad(dashboardObject);

            _logger?.LogInfo("Debug dashboard created", "Debug");
        }
    }
}
