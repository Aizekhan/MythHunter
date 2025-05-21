using System;
using MythHunter.UI.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MythHunter.Utils.Logging;

namespace MythHunter.UI.Runtime
{
    public class UIService : IUIService
    {
        private readonly IUISystem _uiSystem;
        private readonly IViewConfigRegistry _viewConfigRegistry;
        private readonly IMythLogger _logger;
        private readonly IUIViewFactory _viewFactory;

        public UIService(
            IUISystem uiSystem,
            IViewConfigRegistry viewConfigRegistry,
            IMythLogger logger,
            IUIViewFactory viewFactory)
        {
            _uiSystem = uiSystem;
            _viewConfigRegistry = viewConfigRegistry;
            _logger = logger;
            _viewFactory = viewFactory;
        }

        public async UniTask<TView> ShowScreenAsync<TView>() where TView : Component, IView
        {
            var config = _viewConfigRegistry.GetByType<TView>();
            if (config == null)
            {
                _logger.LogError($"ViewConfig не знайдено для типу {typeof(TView).Name}", "UIService");
                return null;
            }

            var view = await _viewFactory.CreateViewAsync<TView>(config.prefabPath);
            if (view == null)
            {
                _logger.LogError($"Не вдалося створити View: {typeof(TView).Name}", "UIService");
                return null;
            }

            _uiSystem.RegisterView(view);
            view.Show();
            return view;
        }

        public void HideScreen<TView>() where TView : Component, IView
        {
            _uiSystem.HideView<TView>();
        }

        public bool IsScreenActive<TView>() where TView : Component, IView
        {
            return _uiSystem.IsViewActive<TView>();
        }
    }
}
