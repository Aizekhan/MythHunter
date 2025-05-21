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

        public async UniTask<IView> ShowScreenAsync(ViewId viewId)
        {
            var config = _viewConfigRegistry.Get(viewId);
            if (config == null)
            {
                _logger.LogError($"ViewConfig не знайдено для ViewId {viewId}", "UIService");
                return null;
            }

            // Створюємо представлення через фабрику, яка тепер використовує тільки ViewId
            var view = await _viewFactory.CreateViewAsync(viewId);
            if (view == null)
            {
                _logger.LogError($"Не вдалося створити View для ViewId {viewId}", "UIService");
                return null;
            }

            _uiSystem.RegisterView(viewId, view);
            view.Show();
            return view;
        }


        public void HideScreen(ViewId viewId)
        {
            _uiSystem.HideView(viewId);
        }

        public bool IsScreenActive(ViewId viewId)
        {
            return _uiSystem.IsViewActive(viewId);
        }
    }
}
