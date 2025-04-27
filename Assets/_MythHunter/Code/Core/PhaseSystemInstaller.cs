// Файл: Assets/_MythHunter/Code/Core/PhaseSystemInstaller.cs

using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Data.ScriptableObjects;
using MythHunter.Systems.Phase;
using UnityEngine;

namespace MythHunter.Core
{
    /// <summary>
    /// Інсталятор системи фаз для DI
    /// </summary>
    public class PhaseSystemInstaller : MonoBehaviour
    {
        [SerializeField] private PhaseConfig _phaseConfig;

        private void Awake()
        {
            if (_phaseConfig == null)
            {
                Debug.LogError("PhaseConfig not assigned in PhaseSystemInstaller");
                return;
            }

            // Отримання DI контейнера
            var bootstrapper = FindFirstObjectByType<GameBootstrapper>();

            if (bootstrapper == null)
            {
                Debug.LogError("GameBootstrapper not found in scene");
                return;
            }

            // Реєстрація конфігурації фаз
            var container = bootstrapper.Container;
            container.RegisterInstance(_phaseConfig);

            // Реєстрація системи фаз
            var systemRegistry = container.Resolve<Systems.Core.SystemRegistry>();
            var phaseSystem = new PhaseSystem(
                container.Resolve<Events.IEventBus>(),
                container.Resolve<Utils.Logging.ILogger>(),
                _phaseConfig
            );

            systemRegistry.RegisterSystem(phaseSystem);

            Debug.Log("PhaseSystem registered successfully");
        }
    }
}
