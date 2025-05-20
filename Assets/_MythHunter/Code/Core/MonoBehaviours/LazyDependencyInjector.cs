// LazyDependencyInjector.cs — без GameBootstrapper.Instance
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.MonoBehaviours
{
    public class LazyDependencyInjector : MonoBehaviour
    {
        [SerializeField] private bool _scanOnSceneLoad = true;
        [SerializeField] private bool _scanOnStart = true;
        [SerializeField] private bool _logInjections = true;

        [Inject] private IDIContainer _container;
        [Inject] private IMythLogger _logger;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (_scanOnStart)
                ScanAndInjectInScene();

            if (_scanOnSceneLoad)
                SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ScanAndInjectInScene();
        }

        private void OnDestroy()
        {
            if (_scanOnSceneLoad)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void ScanAndInjectInScene()
        {
            var monoBehaviours = FindObjectsByType<LazyMonoBehaviour>(FindObjectsSortMode.None);
            int injectedCount = 0;

            foreach (var component in monoBehaviours)
            {
                if (!component.AreDependenciesInjected)
                {
                    component.InjectWith(_container);
                    injectedCount++;
                }
            }

            if (_logInjections && injectedCount > 0)
                _logger.LogInfo($"Injected dependencies into {injectedCount} LazyMonoBehaviour components", "DI");
        }
    }
}
