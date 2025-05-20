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
            if (AutoInjectInAwake)
                UnityEngine.Debug.LogWarning($"LazyMonoBehaviour {name} requires LazyDependencyInjector to inject.");
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
