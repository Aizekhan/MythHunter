// Файл: Assets/_MythHunter/Code/Core/Validation/DIValidator.cs
using UnityEngine;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.Core.Validation
{
    public static class DIValidator
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ValidateSceneDI()
        {
            var logger = MythLoggerFactory.GetDefaultLogger();
            var injectableComponents = GameObject.FindObjectsOfType<MonoBehaviour>();

            foreach (var component in injectableComponents)
            {
                if (HasInjectAttributes(component))
                {
                    var scope = DIContainerExtensions.FindNearestScope(component.gameObject);
                    if (scope == null)
                    {
                        logger.LogWarning($"Component {component.GetType().Name} on {component.gameObject.name} has [Inject] attributes but no DependencyScope!", "DI");
                    }
                }
            }
        }

        private static bool HasInjectAttributes(MonoBehaviour component)
        {
            // Спрощено для прикладу 
            return true;
        }
    }
}
