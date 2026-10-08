// Шлях: Assets/_MythHunter/Code/Core/MonoBehaviours/LazyDependencyInjector.cs
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.MonoBehaviours
{
    /// <summary>
    /// Ін'єктор залежностей для LazyMonoBehaviour компонентів
    /// </summary>
    public class LazyDependencyInjector : MonoBehaviour, IDependencyInjector
    {
        [SerializeField] private bool _scanOnSceneLoad = true;
        [SerializeField] private bool _scanOnStart = true;
        [SerializeField] private bool _logInjections = true;

        private IDIContainer _container;
        private IMythLogger _logger;
        private bool _isInitialized = false;

        /// <summary>
        /// Ініціалізує ін'єктор залежностей
        /// </summary>
        public void Initialize(IDIContainer container, IMythLogger logger)
        {
            _container = container;
            _logger = logger;
            _isInitialized = true;

            if (_logInjections)
                _logger.LogInfo("LazyDependencyInjector ініціалізовано", "DI");

            // Початкове сканування, якщо налаштовано
            if (_scanOnStart)
                ScanAndInjectInScene();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (_scanOnSceneLoad)
                SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_isInitialized)
                ScanAndInjectInScene();
        }

        private void OnDestroy()
        {
            if (_scanOnSceneLoad)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Сканує сцену та ін'єктує залежності в LazyMonoBehaviour компоненти
        /// </summary>
        public void ScanAndInjectInScene()
        {
            if (!_isInitialized)
            {
                UnityEngine.Debug.LogError("LazyDependencyInjector не ініціалізовано. Використайте метод Initialize перед викликом ScanAndInjectInScene.");
                return;
            }

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
                _logger.LogInfo($"Ін'єктовано залежності у {injectedCount} LazyMonoBehaviour компонентів", "DI");
        }

        /// <summary>
        /// Ін'єктує залежності в об'єкт
        /// </summary>
        public void InjectDependencies(object instance)
        {
            if (!_isInitialized)
            {
                UnityEngine.Debug.LogError("LazyDependencyInjector не ініціалізовано. Використайте метод Initialize перед викликом InjectDependencies.");
                return;
            }

            _container.InjectDependencies(instance);
        }

        /// <summary>
        /// Ін'єктує залежності у всі MonoBehaviour компоненти на сцені
        /// </summary>
        public void InjectDependenciesInScene()
        {
            ScanAndInjectInScene();
        }
    }
}
