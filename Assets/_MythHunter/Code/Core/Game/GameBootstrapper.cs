using UnityEngine;
using MythHunter.Core.DI;
using MythHunter.Events;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Початковий ініціалізатор гри
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            var container = new DIContainer();
            container.Register<IEventBus, EventBus>();

            InstallerRegistry.RegisterInstallers(container);

            Debug.Log("✅ GameBootstrapper: DI container initialized");
        }
    }
}