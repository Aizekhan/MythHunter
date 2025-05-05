// Шлях: Assets/_MythHunter/Code/Core/MonoBehaviours/DependencyInjector.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.MonoBehaviours
{
    /// <summary>
    /// Компонент для пошуку та ін'єкції залежностей в MonoBehaviour
    /// </summary>
    public class DependencyInjector : MonoBehaviour
    {
        [SerializeField] private bool _searchOnAwake = true;
        [SerializeField] private bool _searchOnSceneLoad = true;
        [SerializeField] private bool _logInjections = true;

        private IDIContainer _container;
        private IMythLogger _logger;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (GameBootstrapper.Instance != null)
            {
                // Отримуємо логер без контейнера
                if (_logger == null)
                {
                    _logger = MythLoggerFactory.GetDefaultLogger();
                }

                if (_searchOnAwake)
                {
                    InjectDependenciesInScene();
                }

                if (_searchOnSceneLoad)
                {
                    SceneManager.sceneLoaded += OnSceneLoaded;
                }
            }
        }

        private void OnDestroy()
        {
            if (_searchOnSceneLoad)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InjectDependenciesInScene();
        }

        /// <summary>
        /// Виконує пошук MonoBehaviour з атрибутом [Inject] і виконує ін'єкцію
        /// </summary>
        public void InjectDependenciesInScene()
        {
            if (GameBootstrapper.Instance == null)
            {
                if (_logger != null)
                {
                    _logger.LogError("DependencyInjector: GameBootstrapper is not available", "DI");
                }
                return;
            }

            var injectables = new List<MonoBehaviour>();

            // Використовуємо сучасний API для пошуку об'єктів
            var gameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            foreach (var go in gameObjects)
            {
                var components = go.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    if (HasInjectAttribute(component))
                    {
                        injectables.Add(component);
                    }
                }
            }

            foreach (var injectable in injectables)
            {
                GameBootstrapper.Instance.InjectInto(injectable);
            }

            if (_logInjections && _logger != null)
            {
                _logger.LogInfo($"Injected dependencies into {injectables.Count} components", "DI");
            }
        }

        /// <summary>
        /// Перевіряє, чи має компонент атрибут [Inject]
        /// </summary>
        private bool HasInjectAttribute(MonoBehaviour component)
        {
            if (component == null)
                return false;

            // Отримуємо тип компонента
            var type = component.GetType();

            // Перевіряємо методи
            var methods = type.GetMethods(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(InjectAttribute), true);
                if (attributes.Length > 0)
                {
                    return true;
                }
            }

            // Перевіряємо поля
            var fields = type.GetFields(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(InjectAttribute), true);
                if (attributes.Length > 0)
                {
                    return true;
                }
            }

            // Перевіряємо властивості
            var properties = type.GetProperties(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(typeof(InjectAttribute), true);
                if (attributes.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
