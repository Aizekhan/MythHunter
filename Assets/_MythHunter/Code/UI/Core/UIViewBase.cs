// Шлях: Assets/_MythHunter/Code/UI/Core/UIViewBase.cs
using MythHunter.Core.DI;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.UI.Core
{
    public abstract class UIViewBase : LazyMonoBehaviour, IView
    {
        [Inject] protected IUISystem _uiSystem;
        [Inject] protected IViewConfigRegistry _viewConfigRegistry;
        protected bool _isRegistered = false;
        protected ViewId _viewId = ViewId.None;
     

        // Реалізація властивості gameObject з інтерфейсу IView
        UnityEngine.GameObject IView.gameObject => this.gameObject;

        protected override void OnInitialized()
        {
            FindViewIdAndRegister();
        }

        /// <summary>
        /// Знаходить ViewId для цього представлення та реєструє його в UI системі
        /// </summary>
        protected virtual void FindViewIdAndRegister()
        {
            if (_uiSystem == null || _viewConfigRegistry == null || _isRegistered)
                return;

            // Шукаємо ViewId за типом компонента
            var configs = _viewConfigRegistry.GetAll();
            foreach (var config in configs)
            {
                // Перевіряємо ім'я префабу (останню частину шляху)
                string prefabName = GetPrefabNameFromPath(config.prefabPath);
                string typeName = GetType().Name;

                // Якщо ім'я префабу містить ім'я типу або навпаки, це може бути наше представлення
                if ((prefabName != null && prefabName.Contains(typeName)) ||
                    (config.viewId.ToString() == typeName))
                {
                    _viewId = config.viewId;
                    break;
                }
            }

            // Якщо не знайдено, використовуємо None
            if (_viewId == ViewId.None)
            {
                UnityEngine.Debug.LogWarning($"Не вдалося знайти ViewId для {GetType().Name}. Використовується ViewId.None ");
            }

            // Реєструємо представлення з використанням знайденого ViewId
            _uiSystem.RegisterView(_viewId, this);
            _isRegistered = true;
        }

        /// <summary>
        /// Отримує ім'я префабу з шляху
        /// </summary>
        private string GetPrefabNameFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            // Отримуємо останню частину шляху
            int lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex >= 0 && lastSlashIndex < path.Length - 1)
            {
                return path.Substring(lastSlashIndex + 1);
            }

            return path;
        }

        protected virtual void OnDestroy()
        {
            if (_uiSystem != null && _isRegistered)
            {
                _uiSystem.UnregisterView(_viewId);
                _isRegistered = false;
            }
        }

        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);
    }
}
