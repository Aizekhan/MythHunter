// UIViewFactory.cs
namespace MythHunter.UI.Core
{
    /// <summary>
    /// Фабрика представлень UI
    /// </summary>
    public class UIViewFactory : IUIViewFactory
    {
        private readonly MythHunter.Resources.Core.IResourceProvider _resourceProvider;
        private readonly MythHunter.Utils.Logging.IMythLogger _logger;

        [MythHunter.Core.DI.Inject]
        public UIViewFactory(MythHunter.Resources.Core.IResourceProvider resourceProvider, MythHunter.Utils.Logging.IMythLogger logger)
        {
            _resourceProvider = resourceProvider;
            _logger = logger;
        }

        public async Cysharp.Threading.Tasks.UniTask<T> CreateViewAsync<T>(string prefabPath) where T : UnityEngine.Component, IView
        {
            try
            {
                var prefab = await _resourceProvider.LoadAsync<UnityEngine.GameObject>(prefabPath);
                if (prefab == null)
                {
                    _logger.LogError($"Failed to load UI prefab from path: {prefabPath}", "UI");
                    return null;
                }

                var instance = UnityEngine.Object.Instantiate(prefab);
                var view = instance.GetComponent<T>();

                if (view == null)
                {
                    _logger.LogError($"Prefab doesn't have component of type {typeof(T).Name}", "UI");
                    UnityEngine.Object.Destroy(instance);
                    return null;
                }

                return view;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error creating view: {ex.Message}", "UI", ex);
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
