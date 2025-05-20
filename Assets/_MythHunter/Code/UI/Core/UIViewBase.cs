// Assets/_MythHunter/Code/UI/Core/UIViewBase.cs
using MythHunter.Core.DI;
using MythHunter.Core.MonoBehaviours;
using UnityEngine;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Базовий клас для усіх UI View з автоматичною реєстрацією в IUISystem через DI
    /// </summary>
    public abstract class UIViewBase : LazyMonoBehaviour, IView
    {
        [Inject] protected IUISystem _uiSystem;
        protected bool _isRegistered = false;

        protected override void OnInitialized()
        {
            RegisterWithUISystem();
        }

        protected virtual void RegisterWithUISystem()
        {
            if (_uiSystem != null && !_isRegistered)
            {
                _uiSystem.RegisterView(this);
                _isRegistered = true;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_uiSystem != null && _isRegistered)
            {
                _uiSystem.UnregisterView(this);
                _isRegistered = false;
            }
        }

        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);
    }
}
