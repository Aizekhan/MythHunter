// DependencyInjector.cs — без GameBootstrapper.Instance
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using System.Linq;

namespace MythHunter.Core.MonoBehaviours
{
    public class DependencyInjector : MonoBehaviour, IDependencyInjector
    {
        [SerializeField] private bool _searchOnAwake = true;
        [SerializeField] private bool _searchOnSceneLoad = true;
        [SerializeField] private bool _logInjections = true;

        [Inject] private IDIContainer _container;
        [Inject] private IMythLogger _logger;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (_searchOnAwake)
                InjectDependenciesInScene();

            if (_searchOnSceneLoad)
                SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (_searchOnSceneLoad)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InjectDependenciesInScene();
        }

        public void InjectDependenciesInScene()
        {
            var injectables = new List<MonoBehaviour>();
            var gameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            foreach (var go in gameObjects)
            {
                var components = go.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    if (HasInjectAttribute(component))
                        injectables.Add(component);
                }
            }

            foreach (var injectable in injectables)
            {
                _container.InjectDependencies(injectable);
            }

            if (_logInjections)
                _logger.LogInfo($"Injected dependencies into {injectables.Count} components", "DI");
        }

        private bool HasInjectAttribute(MonoBehaviour component)
        {
            var type = component.GetType();
            return type.GetFields().Any(f => f.IsDefined(typeof(InjectAttribute), true)) ||
                   type.GetProperties().Any(p => p.IsDefined(typeof(InjectAttribute), true)) ||
                   type.GetMethods().Any(m => m.IsDefined(typeof(InjectAttribute), true));
        }
    }
}
