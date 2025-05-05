// Файл: Assets/_MythHunter/Code/Core/MonoBehaviours/DependencyScope.cs
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using UnityEngine;

namespace MythHunter.Core.MonoBehaviours
{
    /// <summary>
    /// Компонент, який надає контекст DI для нащадків
    /// </summary>
    public class DependencyScope : MonoBehaviour
    {
        [Header("Dependency Injection")]
        [SerializeField] private bool _injectChildren = true;
        [SerializeField] private bool _injectOnStart = true;

        private IDIContainer _container;

        private void Awake()
        {
            if (_injectOnStart && GameBootstrapper.Instance != null)
            {
                InjectComponents();
            }
        }

       

        /// <summary>
        /// Ін'єктує залежності в компоненти
        /// </summary>
        public void InjectComponents()
        {
            if (GameBootstrapper.Instance == null)
                return;

            if (_injectChildren)
            {
                var components = GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var component in components)
                {
                    if (component != this)
                    {
                        GameBootstrapper.Instance.InjectInto(component);
                    }
                }
            }
            else
            {
                var components = GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    if (component != this)
                    {
                        GameBootstrapper.Instance.InjectInto(component);
                    }
                }
            }
        }

        /// <summary>
        /// Додає компонент і відразу інжектує залежності
        /// </summary>
        public T AddInjectedComponent<T>() where T : MonoBehaviour
        {
            T component = gameObject.AddComponent<T>();
            _container?.InjectDependencies(component);
            return component;
        }
    }
}
