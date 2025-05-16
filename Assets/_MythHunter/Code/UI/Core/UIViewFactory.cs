// UIViewFactory.cs

using MythHunter.Core.DI;
using UnityEngine;
namespace MythHunter.UI.Core
{
    /// <summary>
    /// Фабрика представлень UI
    /// </summary>
    public class UIViewFactory : IUIViewFactory
    {
        private readonly MythHunter.Resources.Core.IResourceProvider _resourceProvider;
        private readonly MythHunter.Utils.Logging.IMythLogger _logger;

        [Inject]
        public UIViewFactory(MythHunter.Resources.Core.IResourceProvider resourceProvider, MythHunter.Utils.Logging.IMythLogger logger)
        {
            _resourceProvider = resourceProvider;
            _logger = logger;
        }

        public async Cysharp.Threading.Tasks.UniTask<T> CreateViewAsync<T>(string prefabPath) where T : UnityEngine.Component, IView
        {
            _logger.LogInfo($"[UIFactory] Creating view: {typeof(T).Name} from path {prefabPath}", "UI");
            try
            {
                var prefab = await _resourceProvider.LoadAsync<GameObject>(prefabPath);
                if (prefab == null)
                {
                    _logger.LogError($"[UIFactory] Failed to load UI prefab at path: {prefabPath}", "UI");
                    return null;
                }

                // 🧩 Визначаємо parent — GlobalCanvas (UIRoot)
                Transform parent = UIRoot.RootTransform;
                if (parent == null)
                {
                    _logger.LogError("[UIFactory] UIRoot.RootTransform not found. UI will not appear correctly!", "UI");
                }

                var instance = Object.Instantiate(prefab, parent);
                var view = instance.GetComponent<T>();

                if (view == null)
                {
                    _logger.LogError($"[UIFactory] Prefab '{prefab.name}' is missing component of type {typeof(T).Name}", "UI");
                    Object.Destroy(instance);
                    return null;
                }

                return view;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[UIFactory] Exception creating view: {ex.Message}", "UI", ex);
                return null;
            }
        }


        public void ReleaseView<T>(T view) where T : UnityEngine.Component, IView
        {
            if (view != null)
            {
                UnityEngine.Object.Destroy(view.gameObject);
            }
        }
    }
}
