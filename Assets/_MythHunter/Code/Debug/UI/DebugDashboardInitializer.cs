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

            // Перевірте ініціалізацію
            var container = DIContainerProvider.GetContainer();
            if (container == null)
            {
                _logger?.LogError("DI Container not found", "Debug");
                return;
            }

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
            // Перевіряємо, чи не існує вже дашборд
            if (FindObjectOfType<DebugDashboard>() != null)
            {
                _logger?.LogWarning("Debug dashboard already exists", "Debug");
                return;
            }

            // Створюємо новий GameObject
            var dashboardObject = new GameObject("MythHunter_DebugDashboard");

            // Додаємо компонент дашборду
            var dashboard = dashboardObject.AddComponent<DebugDashboard>();

            // Переконуємося, що об'єкт не буде знищений при зміні сцени
            DontDestroyOnLoad(dashboardObject);

            _logger?.LogInfo("Debug dashboard created", "Debug");
        }
    }
}
