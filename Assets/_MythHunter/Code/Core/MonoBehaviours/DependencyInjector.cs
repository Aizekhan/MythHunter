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
            // Зберігаємо цей об'єкт між сценами
            DontDestroyOnLoad(gameObject);

            // Отримуємо контейнер
            if (GameBootstrapper.Instance != null)
            {
                _container = GameBootstrapper.Instance.GetContainerInternal();
                _logger = _container.Resolve<IMythLogger>();

                if (_searchOnAwake)
                {
                    InjectDependenciesInScene();
                }

                if (_searchOnSceneLoad)
                {
                    SceneManager.sceneLoaded += OnSceneLoaded;
                }
            }
            else
            {
                var fallbackLogger = MythLoggerFactory.GetDefaultLogger();
                fallbackLogger.LogError("DependencyInjector cannot find GameBootstrapper instance", "DI");
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
            if (_container == null)
            {
                if (_logger != null)
                {
                    _logger.LogError("DependencyInjector: Container is not available", "DI");
                }
                else
                {
                    _logger.LogError("DependencyInjector: Container is not available");
                }
                return;
            }

            // Використовуємо MonoBehaviour[] для отримання всіх компонентів на сцені
            var injectables = new List<MonoBehaviour>();

            // Знаходимо у активній сцені
            var gameObjects = FindObjectsOfType<GameObject>();

            foreach (var go in gameObjects)
            {
                var components = go.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    // Перевіряємо, чи має компонент атрибут [Inject]
                    if (HasInjectAttribute(component))
                    {
                        injectables.Add(component);
                    }
                }
            }

            // Виконуємо ін'єкцію
            foreach (var injectable in injectables)
            {
                _container.InjectInto(injectable, _logInjections ? _logger : null);
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
