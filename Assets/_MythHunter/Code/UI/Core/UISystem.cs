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
        private readonly Dictionary<Type, Component> _registeredViews = new Dictionary<Type, Component>();
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

        public async UniTask<TView> ShowViewAsync<TView>() where TView : Component, IView
        {
            if (_registeredViews.TryGetValue(typeof(TView), out var component) && component is TView view)
            {
                view.gameObject.SetActive(true);
                ((IView)view).Show();
                _logger.LogInfo($"Showing view: {typeof(TView).Name}", "UI");
                return view;
            }

            _logger.LogWarning($"View {typeof(TView).Name} not registered", "UI");
            return null;
        }

        public void ShowView<TView>() where TView : Component, IView
        {
            if (_registeredViews.TryGetValue(typeof(TView), out var component) && component is TView view)
            {
                view.gameObject.SetActive(true);
                ((IView)view).Show();
                _logger.LogInfo($"Showing view: {typeof(TView).Name}", "UI");
            }
            else
            {
                _logger.LogWarning($"View {typeof(TView).Name} not registered", "UI");
            }
        }

        public void HideView<TView>() where TView : Component, IView
        {
            if (_registeredViews.TryGetValue(typeof(TView), out var component) && component is TView view)
            {
                ((IView)view).Hide();
                _logger.LogInfo($"Hiding view: {typeof(TView).Name}", "UI");
            }
            else
            {
                _logger.LogWarning($"View {typeof(TView).Name} not registered", "UI");
            }
        }

        public void RegisterView<TView>(TView view) where TView : Component, IView
        {
            Type viewType = typeof(TView);
            if (_registeredViews.ContainsKey(viewType))
            {
                _registeredViews[viewType] = view;
                _logger.LogInfo($"Updated registered view: {viewType.Name}", "UI");
            }
            else
            {
                _registeredViews.Add(viewType, view);
                _logger.LogInfo($"Registered view: {viewType.Name}", "UI");
            }
        }

        public void UnregisterView<TView>(TView view) where TView : Component, IView
        {
            Type viewType = typeof(TView);
            if (_registeredViews.TryGetValue(viewType, out var registeredView) && registeredView == view)
            {
                _registeredViews.Remove(viewType);
                _logger.LogInfo($"Unregistered view: {viewType.Name}", "UI");
            }
        }

        public TView GetView<TView>() where TView : Component, IView
        {
            if (_registeredViews.TryGetValue(typeof(TView), out var component) && component is TView view)
            {
                return view;
            }

            _logger.LogWarning($"View {typeof(TView).Name} not found", "UI");
            return null;
        }

        public bool IsViewActive<TView>() where TView : Component, IView
        {
            if (_registeredViews.TryGetValue(typeof(TView), out var component) && component is TView view)
            {
                return view.gameObject.activeInHierarchy;
            }

            return false;
        }
    }
}
