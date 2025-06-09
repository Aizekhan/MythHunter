// LazyMonoBehaviour.cs — без доступу до GameBootstrapper.Instance
using UnityEngine;
using MythHunter.Core.DI;

namespace MythHunter.Core.MonoBehaviours
{
    public abstract class LazyMonoBehaviour : MonoBehaviour
    {
        private bool _injectionPerformed = false;
        private bool _awakePerformed = false;

        protected virtual bool AutoInjectInAwake => true;

        protected virtual void BeforeInjection()
        {
        }
        protected virtual void AfterInjection()
        {
        }
        protected virtual void OnInitialized()
        {
        }
        protected virtual void OnInject()
        {
        }

        protected virtual void Awake()
        {
            _awakePerformed = true;

            // Спробуємо знайти LazyDependencyInjector, якщо налаштовано автоматичну ін'єкцію
            if (AutoInjectInAwake && !_injectionPerformed)
            {
                var injector = FindFirstObjectByType<LazyDependencyInjector>();
                if (injector != null)
                {
                    // Якщо інжектор знайдений, виконуємо ін'єкцію
                    BeforeInjection();
                    injector.InjectDependencies(this);
                    _injectionPerformed = true;
                    AfterInjection();
                    OnInitialized();
                }
                else
                {
                    // Якщо інжектор не знайдений, виводимо попередження в лог
                    UnityEngine.Debug.LogWarning($"LazyMonoBehaviour {name} requires LazyDependencyInjector to inject. " +
                                    "Make sure LazyDependencyInjector exists in the scene and is initialized.");

                    // Тут ми НЕ викликаємо OnInitialized(), оскільки ін'єкція не відбулася
                }
            }
        }

        public void InjectWith(IDIContainer container)
        {
            if (_injectionPerformed)
                return;

            BeforeInjection();
            container.InjectDependencies(this);
            _injectionPerformed = true;
            AfterInjection();

            if (_awakePerformed)
                OnInitialized();
        }

        public bool AreDependenciesInjected => _injectionPerformed;
    }
}
