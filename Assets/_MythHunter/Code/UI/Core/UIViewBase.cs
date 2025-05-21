// Assets/_MythHunter/Code/UI/Core/UIViewBase.cs
using MythHunter.Core.DI;
using MythHunter.Core.MonoBehaviours;
using UnityEngine;

namespace MythHunter.UI.Core
{
    public abstract class UIViewBase : LazyMonoBehaviour, IView
    {
        [Inject] protected IUISystem _uiSystem;
        [Inject] protected IViewConfigRegistry _viewConfigRegistry;
        protected bool _isRegistered = false;

        // Реалізація властивості gameObject з інтерфейсу IView
        UnityEngine.GameObject IView.gameObject => this.gameObject;

        protected override void OnInitialized()
        {
            RegisterWithUISystem();
        }

        protected virtual void RegisterWithUISystem()
        {
            if (_uiSystem != null && !_isRegistered)
            {
                // Отримуємо ViewId на основі типу компонента
                var config = _viewConfigRegistry.GetByType<UIViewBase>();
                if (config != null)
                {
                    _uiSystem.RegisterView(config.viewId, this);
                    _isRegistered = true;
                }
                else
                {
                    // Запасний варіант - реєструємо через типи (для зворотної сумісності)
                    _uiSystem.RegisterView(this);
                    _isRegistered = true;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (_uiSystem != null && _isRegistered)
            {
                var config = _viewConfigRegistry.GetByType<UIViewBase>();
                if (config != null)
                {
                    _uiSystem.UnregisterView(config.viewId);
                }
                else
                {
                    _uiSystem.UnregisterView(this);
                }
                _isRegistered = false;
            }
        }

        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);
    }
}
