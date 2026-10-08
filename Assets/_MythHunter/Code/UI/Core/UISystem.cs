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
        private readonly Dictionary<Type, ViewId> _viewTypeToIdMap = new Dictionary<Type, ViewId>();
        private readonly IResourceManager _resourceManager;
        private readonly IUIViewFactory _viewFactory;
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;
        private readonly IViewConfigRegistry _viewConfigRegistry;

        [Inject]
        public UISystem(
            IResourceManager resourceManager,
            IUIViewFactory viewFactory,
            IMythLogger logger,
            IDIContainer container,
            IViewConfigRegistry viewConfigRegistry)
        {
            _resourceManager = resourceManager;
            _viewFactory = viewFactory;
            _logger = logger;
            _container = container;
            _viewConfigRegistry = viewConfigRegistry;
        }

        public INavigationService GetNavigationService()
        {
            return _container.Resolve<INavigationService>();
        }

        public UniTask<IView> ShowViewAsync(ViewId viewId)
        {
            if (_registeredViews.TryGetValue(viewId, out var view))
            {
                view.Show();
                _logger.LogInfo($"Showing view: {viewId}", "UI");
                return UniTask.FromResult<IView>(view);
            }

            _logger.LogWarning($"View {viewId} not registered", "UI");
            return UniTask.FromResult<IView>(null);
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

            // Додаємо запис у мапі типів для методів зворотньої сумісності
            if (view is Component component)
            {
                Type viewType = component.GetType();
                _viewTypeToIdMap[viewType] = viewId;
            }
        }

        public void UnregisterView(ViewId viewId)
        {
            if (_registeredViews.TryGetValue(viewId, out var view))
            {
                // Спочатку видаляємо з мапи типів
                if (view is Component component)
                {
                    Type viewType = component.GetType();
                    _viewTypeToIdMap.Remove(viewType);
                }

                // Видаляємо з мапи за ID
                _registeredViews.Remove(viewId);
                _logger.LogInfo($"Unregistered view: {viewId}", "UI");
            }
        }

        // Методи для зворотної сумісності
        public void RegisterView(IView view)
        {
            if (view is Component component)
            {
                Type viewType = component.GetType();

                // Перевіряємо, чи вже маємо цей вид в _viewTypeToIdMap
                if (_viewTypeToIdMap.TryGetValue(viewType, out ViewId existingViewId))
                {
                    RegisterView(existingViewId, view);
                    return;
                }

                // Не намагаємось шукати за типом, просто використовуємо ViewId.None як запасний варіант
                _logger.LogWarning($"Using ViewId.None for legacy view registration of type {viewType.Name}. Please use RegisterView(ViewId, IView) instead.", "UI");
                RegisterView(ViewId.None, view);
            }
            else
            {
                _logger.LogWarning($"Cannot register non-Component view. Use RegisterView(ViewId, IView) instead.", "UI");
            }
        }


        public void UnregisterView(IView view)
        {
            if (view is Component component)
            {
                Type viewType = component.GetType();

                // Спробуємо знайти ViewId за типом в нашій мапі
                if (_viewTypeToIdMap.TryGetValue(viewType, out ViewId viewId))
                {
                    UnregisterView(viewId);
                }
                else
                {
                    _logger.LogWarning($"Cannot find ViewId for type {viewType.Name} to unregister. Use UnregisterView(ViewId) instead.", "UI");
                }
            }
            else
            {
                _logger.LogWarning($"Cannot unregister non-Component view. Use UnregisterView(ViewId) instead.", "UI");
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
