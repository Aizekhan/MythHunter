// Шлях: Assets/_MythHunter/Code/UI/Core/UISystem.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.UI.Navigation;
using MythHunter.UI.Core;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Реалізація UI системи для керування представленнями
    /// </summary>
    public class UISystem : IUISystem
    {
        private readonly Dictionary<ViewId, IView> _registeredViews = new Dictionary<ViewId, IView>();
        private readonly IResourceManager _resourceManager;
        private readonly IUIViewFactory _viewFactory;
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;

        [Inject]
        public UISystem(IResourceManager resourceManager, IUIViewFactory viewFactory, IMythLogger logger, IDIContainer container)
        {
            _resourceManager = resourceManager;
            _viewFactory = viewFactory;
            _logger = logger;
            _container = container;
        }

        public INavigationService GetNavigationService()
        {
            return _container.Resolve<INavigationService>();
        }

        public async UniTask<IView> ShowViewAsync(ViewId viewId)
        {
            if (_registeredViews.TryGetValue(viewId, out var view))
            {
                view.Show();
                _logger.LogInfo($"Showing view: {viewId}", "UI");
                return view;
            }

            _logger.LogWarning($"View {viewId} not registered", "UI");
            return null;
        }

        public void ShowView(ViewId viewId)
        {
            if (_registeredViews.TryGetValue(viewId, out var view))
            {
                view.Show();
                _logger.LogInfo($"Showing view: {viewId}", "UI");
            }
            else
            {
                _logger.LogWarning($"View {viewId} not registered", "UI");
            }
        }

        public void HideView(ViewId viewId)
        {
            if (_registeredViews.TryGetValue(viewId, out var view))
            {
                view.Hide();
                _logger.LogInfo($"Hiding view: {viewId}", "UI");
            }
            else
            {
                _logger.LogWarning($"View {viewId} not registered", "UI");
            }
        }

        public void RegisterView(ViewId viewId, IView view)
        {
            if (_registeredViews.ContainsKey(viewId))
            {
                _registeredViews[viewId] = view;
                _logger.LogInfo($"Updated registered view: {viewId}", "UI");
            }
            else
            {
                _registeredViews.Add(viewId, view);
                _logger.LogInfo($"Registered view: {viewId}", "UI");
            }
        }

        public void UnregisterView(ViewId viewId)
        {
            if (_registeredViews.Remove(viewId))
            {
                _logger.LogInfo($"Unregistered view: {viewId}", "UI");
            }
        }

        public IView GetView(ViewId viewId)
        {
            if (_registeredViews.TryGetValue(viewId, out var view))
            {
                return view;
            }

            _logger.LogWarning($"View {viewId} not found", "UI");
            return null;
        }

        public bool IsViewActive(ViewId viewId)
        {
            if (_registeredViews.TryGetValue(viewId, out var view))
            {
                return view is Component component && component.gameObject.activeInHierarchy;
            }

            return false;
        }
    }
}
