// Шлях: Assets/_MythHunter/Code/Core/Validation/DIValidator.cs
using UnityEngine;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Utils;

namespace MythHunter.Core.Validation
{
    public static class DIValidator
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ValidateSceneDI()
        {
            var logger = MythLoggerFactory.GetDefaultLogger();
            var injectableComponents = UnityApiUtils.FindObjectsOfType<MonoBehaviour>();

            foreach (var component in injectableComponents)
            {
                if (HasInjectAttributes(component))
                {
                    var bootstrapper = UnityApiUtils.FindFirstObjectOfType<GameBootstrapper>();
                    if (bootstrapper == null)
                    {
                        logger.LogWarning($"Component {component.GetType().Name} on {component.gameObject.name} has [Inject] attributes but no GameBootstrapper!", "DI");
                    }
                }
            }
        }

        private static bool HasInjectAttributes(MonoBehaviour component)
        {
            // Логіка перевірки атрибутів [Inject]
            return true;
        }
    }
}
