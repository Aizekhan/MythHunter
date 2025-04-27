using UnityEngine;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Data.ScriptableObjects;
using MythHunter.Systems.Phase;
using System.Collections;

namespace MythHunter.Core
{
    /// <summary>
    /// Інсталятор системи фаз для DI
    /// </summary>
    public class PhaseSystemInstaller : MonoBehaviour
    {
        [SerializeField] private PhaseConfig _phaseConfig;

        private void Start()
        {
            StartCoroutine(InitializeWhenBootstrapperReady());
        }

        private IEnumerator InitializeWhenBootstrapperReady()
        {
            if (_phaseConfig == null)
            {
                Debug.LogError("[PhaseSystemInstaller] PhaseConfig not assigned!");
                yield break;
            }

            // Отримання GameBootstrapper
            var bootstrapper = FindFirstObjectByType<GameBootstrapper>();
            if (bootstrapper == null)
            {
                Debug.LogError("[PhaseSystemInstaller] GameBootstrapper not found in scene!");
                yield break;
            }

            // Чекаємо завершення ініціалізації GameBootstrapper
            yield return bootstrapper.WaitForInitializationCoroutine();

            if (!bootstrapper.IsInitialized)
            {
                Debug.LogError("[PhaseSystemInstaller] GameBootstrapper initialization failed!");
                yield break;
            }

            // Реєстрація конфігурації фаз
            var container = bootstrapper.Container;
            container.RegisterInstance(_phaseConfig);

            // Реєстрація системи фаз
            try
            {
                var systemRegistry = container.Resolve<Systems.Core.SystemRegistry>();
                var eventBus = container.Resolve<Events.IEventBus>();
                var logger = container.Resolve<Utils.Logging.IMythLogger>();

                var phaseSystem = new PhaseSystem(eventBus, logger, _phaseConfig);
                systemRegistry.RegisterSystem(phaseSystem);

                Debug.Log("[PhaseSystemInstaller] PhaseSystem registered successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PhaseSystemInstaller] Failed to register PhaseSystem: {ex.Message}");
            }
        }
    }
}
