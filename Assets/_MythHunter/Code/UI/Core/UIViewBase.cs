// Шлях: Assets/_MythHunter/Code/UI/Core/UIViewBase.cs
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using UnityEngine;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Базова реалізація IView для MonoBehaviour з автоматичною реєстрацією в UISystem
    /// </summary>
    public abstract class UIViewBase : MonoBehaviour, IView
    {
        protected IUISystem _uiSystem;
        protected bool _isRegistered = false;

        protected virtual void Awake()
        {
            // Отримуємо UISystem через GameBootstrapper
            var bootstrapper = FindObjectOfType<GameBootstrapper>();
            if (bootstrapper != null)
            {
                var container = bootstrapper.GetContainer();
                if (container != null && container.IsRegistered<IUISystem>())
                {
                    _uiSystem = container.Resolve<IUISystem>();
                    RegisterWithUISystem();
                }
            }
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

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
