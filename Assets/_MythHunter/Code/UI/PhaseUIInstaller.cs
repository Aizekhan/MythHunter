// Файл: Assets/_MythHunter/Code/UI/PhaseUIInstaller.cs

using MythHunter.Core.DI;
using MythHunter.Core.Game;

using MythHunter.Events;
using MythHunter.UI.Presenters;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.UI
{
    /// <summary>
    /// Інсталятор UI для фаз
    /// </summary>
    public class PhaseUIInstaller : MonoBehaviour
    {
        [SerializeField] private PhaseView _phaseView;

        private PhasePresenter _phasePresenter;

        private async void Start()
        {
            if (_phaseView == null)
            {
                Debug.LogError("PhaseView not assigned in PhaseUIInstaller");
                return;
            }

            // Отримання залежностей
            var bootstrapper = FindFirstObjectByType<GameBootstrapper>();
            if (bootstrapper == null)
            {
                Debug.LogError("GameBootstrapper not found in scene");
                return;
            }

            var container = bootstrapper.Container;
            var eventBus = container.Resolve<IEventBus>();
            var logger = container.Resolve<IMythLogger>();

            // Створення презентера
            _phasePresenter = new PhasePresenter(_phaseView, eventBus, logger);

            // Ініціалізація презентера
            await _phasePresenter.InitializeAsync();
        }

        private void OnDestroy()
        {
            if (_phasePresenter != null)
            {
                _phasePresenter.Dispose();
                _phasePresenter = null;
            }
        }
    }
}
