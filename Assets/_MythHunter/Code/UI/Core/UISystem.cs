using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Реалізація UI системи для керування представленнями
    /// </summary>
    public class UISystem : IUISystem
    {
        private readonly Dictionary<Type, Component> _registeredViews = new Dictionary<Type, Component>();
        private readonly IResourceProvider _resourceProvider;
        private readonly IUIViewFactory _viewFactory;
        private readonly IMythLogger _logger;

        [MythHunter.Core.DI.Inject]
        public UISystem(IResourceProvider resourceProvider, IUIViewFactory viewFactory, IMythLogger logger)
        {
            _resourceProvider = resourceProvider;
            _viewFactory = viewFactory;
            _logger = logger;
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

        public async UniTask<TView> ShowViewAsync<TView>(string prefabPath) where TView : Component, IView
        {
            try
            {
                // Спробуємо отримати вже зареєстроване представлення
                if (_registeredViews.TryGetValue(typeof(TView), out var existingView) && existingView is TView typedView)
                {
                    typedView.gameObject.SetActive(true);
                    ((IView)typedView).Show();
                    return typedView;
                }

                // Створюємо нове представлення
                var view = await _viewFactory.CreateViewAsync<TView>(prefabPath);
                if (view != null)
                {
                    RegisterView(view);
                    view.gameObject.SetActive(true);
                    ((IView)view).Show();
                    return view;
                }

                _logger.LogError($"Failed to load view from path: {prefabPath}", "UI");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error showing view: {ex.Message}", "UI", ex);
                return null;
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
