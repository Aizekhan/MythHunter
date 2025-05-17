// Шлях: Assets/_MythHunter/Code/UI/Core/UIService.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Реалізація високорівневого сервісу для управління UI екранами
    /// </summary>
    public class UIService : IUIService
    {
        private readonly IUISystem _uiSystem;
        private readonly IViewConfigRegistry _viewConfigRegistry;
        private readonly IMythLogger _logger;

        [Inject]
        public UIService(IUISystem uiSystem, IViewConfigRegistry viewConfigRegistry, IMythLogger logger)
        {
            _uiSystem = uiSystem;
            _viewConfigRegistry = viewConfigRegistry;
            _logger = logger;
        }

        public async UniTask<TView> ShowScreenAsync<TView>(string screenId) where TView : Component, IView
        {
            var config = _viewConfigRegistry.Get(screenId);
            if (config == null)
            {
                _logger.LogError($"[UIService] Failed to find view config with ID: {screenId}", "UI");
                return null;
            }

            var view = await _uiSystem.ShowViewAsync<TView>(config.PrefabPath);
            _logger.LogInfo($"[UIService] Screen {screenId} shown", "UI");
            return view;
        }

        public void HideScreen<TView>() where TView : Component, IView
        {
            _uiSystem.HideView<TView>();
            _logger.LogInfo($"[UIService] Screen {typeof(TView).Name} hidden", "UI");
        }

        public bool IsScreenActive<TView>() where TView : Component, IView
        {
            return _uiSystem.IsViewActive<TView>();
        }
    }
}
