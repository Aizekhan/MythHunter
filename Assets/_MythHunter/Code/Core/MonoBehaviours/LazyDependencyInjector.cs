// Шлях: Assets/_MythHunter/Code/Core/MonoBehaviours/LazyDependencyInjector.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.Core.Game;

namespace MythHunter.Core.MonoBehaviours
{
    /// <summary>
    /// Компонент для автоматичної ін'єкції залежностей в LazyMonoBehaviour
    /// </summary>
    public class LazyDependencyInjector : MonoBehaviour
    {
        [SerializeField] private bool _scanOnSceneLoad = true;
        [SerializeField] private bool _scanOnStart = true;
        [SerializeField] private bool _logInjections = true;

        private IMythLogger _logger;
        private IDIContainer _container;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Отримуємо доступ до DI-контейнера
            if (GameBootstrapper.Instance != null)
            {
                InjectDependencies();

                if (_scanOnStart)
                {
                    ScanAndInjectInScene();
                }

                if (_scanOnSceneLoad)
                {
                    SceneManager.sceneLoaded += OnSceneLoaded;
                }
            }
        }

        /// <summary>
        /// Ін'єкція власних залежностей
        /// </summary>
        private void InjectDependencies()
        {
            try
            {
                if (GameBootstrapper.Instance != null)
                {
                    GameBootstrapper.Instance.InjectInto(this);

                    // Отримуємо необхідні сервіси
                    if (_container == null)
                    {
                        _container = GameBootstrapper.Instance.GetContainer();
                    }

                    if (_logger == null && _container != null)
                    {
                        _logger = _container.Resolve<IMythLogger>();
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error injecting dependencies into LazyDependencyInjector: {ex.Message}", this);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ScanAndInjectInScene();
        }

        private void OnDestroy()
        {
            if (_scanOnSceneLoad)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        /// <summary>
        /// Сканує поточну сцену і виконує ін'єкцію в LazyMonoBehaviour
        /// </summary>
        public void ScanAndInjectInScene()
        {
            if (GameBootstrapper.Instance == null)
            {
                if (_logger != null)
                {
                    _logger.LogWarning("LazyDependencyInjector: GameBootstrapper is not available", "DI");
                }
                return;
            }

            var monoBehaviours = FindObjectsByType<LazyMonoBehaviour>(FindObjectsSortMode.None);
            int injectedCount = 0;

            foreach (var component in monoBehaviours)
            {
                if (!component.AreDependenciesInjected)
                {
                    component.EnsureDependenciesInjected();
                    injectedCount++;
                }
            }

            if (_logInjections && _logger != null && injectedCount > 0)
            {
                _logger.LogInfo($"Injected dependencies into {injectedCount} LazyMonoBehaviour components", "DI");
            }
        }
    }
}
