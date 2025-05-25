// Шлях: Assets/_MythHunter/Code/UI/Core/UIViewFactory.cs

using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Спрощена фабрика представлень UI - працює тільки з пулами
    /// </summary>
    public class UIViewFactory : IUIViewFactory
    {
        private readonly IPoolManager _poolManager;
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;

        [Inject]
        public UIViewFactory(
            IPoolManager poolManager,
            IMythLogger logger,
            IDIContainer container)
        {
            _poolManager = poolManager;
            _logger = logger;
            _container = container;
        }

        /// <summary>
        /// Створює представлення з пулу (основний метод)
        /// </summary>
        public async UniTask<IView> CreateViewAsync(ViewId viewId)
        {
            string poolKey = GetPoolKeyFromViewId(viewId);

            // ✅ СПОЧАТКУ ПЕРЕВІРЯЄМО ЧИ Є ПУЛ
            if (_poolManager.HasPool(poolKey))
            {
                _logger.LogDebug($"🏊 Використовуємо пул для {viewId}", "UIViewFactory");
                return await CreateFromPoolAsync(viewId, poolKey);
            }
            else
            {
                _logger.LogDebug($"📦 Створюємо напряму для {viewId} (пул відсутній)", "UIViewFactory");
                return await CreateDirectlyAsync(viewId);
            }
        }
        // ✅ ДОДАЙ НОВИЙ МЕТОД
        private string GetResourcePathFromViewId(ViewId viewId)
        {
            return viewId switch
            {
                ViewId.MinimalLoading => "UI/Loading/MinimalLoadingView",
                ViewId.Lobby => "UI/Lobby/LobbyView",
                ViewId.HeroCard => "UI/Common/HeroCardUI",
                ViewId.GameplayUI => "UI/Gameplay/GameplayUIView",
                _ => $"UI/{viewId}"
            };
        }

        // ✅ ВИНЕСИ СПІЛЬНУ ЛОГІКУ
        private IView SetupViewFromGameObject(GameObject gameObject, ViewId viewId)
        {
            gameObject.transform.SetParent(UIRoot.RootTransform, false);

            var view = gameObject.GetComponent<IView>();
            if (view == null)
            {
                _logger.LogError($"❌ GameObject не містить IView компонента: {viewId}", "UIViewFactory");
                UnityEngine.Object.Destroy(gameObject);
                return null;
            }

            _container.InjectDependencies(gameObject);
            gameObject.SetActive(true);

            _logger.LogDebug($"✅ Створено View {viewId}", "UIViewFactory");
            return view;
        }

        /// <summary>
        /// Застарілий метод - використовуйте CreateViewAsync
        /// </summary>
        [Obsolete("Використовуйте CreateViewAsync - всі View тепер створюються з пулів")]
        private async UniTask<IView> CreateFromPoolAsync(ViewId viewId, string poolKey)
        {
            var gameObject = _poolManager.GetFromPool<GameObject>(poolKey);
            if (gameObject == null)
            {
                _logger.LogError($"❌ Не вдалося отримати GameObject з пулу '{poolKey}'", "UIViewFactory");
                return null;
            }

            return SetupView(gameObject, viewId);
        }
        private async UniTask<IView> CreateDirectlyAsync(ViewId viewId)
        {
            string resourcePath = GetResourcePathFromViewId(viewId);
            var prefab = UnityEngine.Resources.Load<GameObject>(resourcePath);

            if (prefab == null)
            {
                _logger.LogError($"❌ Префаб не знайдено: {resourcePath}", "UIViewFactory");
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, UIRoot.RootTransform, false);
            return SetupView(instance, viewId);
        }

        private IView SetupView(GameObject gameObject, ViewId viewId)
        {
            gameObject.transform.SetParent(UIRoot.RootTransform, false);

            var view = gameObject.GetComponent<IView>();
            if (view == null)
            {
                _logger.LogError($"❌ GameObject не містить IView: {viewId}", "UIViewFactory");
                UnityEngine.Object.Destroy(gameObject);
                return null;
            }

            _container.InjectDependencies(gameObject);
            gameObject.SetActive(true);

            return view;
        }
        /// <summary>
        /// Повертає представлення в пул
        /// </summary>
        public void ReturnViewToPool(ViewId viewId, IView view)
        {
            if (view == null)
                return;

            try
            {
                string poolKey = GetPoolKeyFromViewId(viewId);
                var gameObject = (view as Component)?.gameObject;

                if (gameObject == null)
                {
                    _logger.LogWarning($"⚠️ View {viewId} не є Component", "UIViewFactory");
                    return;
                }

                // Деактивуємо та повертаємо в пул
                gameObject.SetActive(false);
                _poolManager.ReturnToPool(poolKey, gameObject);

                _logger.LogDebug($"✅ View {viewId} повернено в пул {poolKey}", "UIViewFactory");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка повернення View {viewId} в пул: {ex.Message}", "UIViewFactory", ex);

                // Запасний варіант - знищення
                if (view is Component component)
                    UnityEngine.Object.Destroy(component.gameObject);
            }
        }

        /// <summary>
        /// Знищує представлення (рідко використовується)
        /// </summary>
        public void ReleaseView(ViewId viewId, IView view)
        {
            if (view is Component component && component != null)
            {
                UnityEngine.Object.Destroy(component.gameObject);
                _logger.LogDebug($"🗑️ View {viewId} знищено", "UIViewFactory");
            }
        }

        /// <summary>
        /// Конвертує ViewId в ключ пулу
        /// </summary>
        private string GetPoolKeyFromViewId(ViewId viewId)
        {
            return viewId switch
            {
                ViewId.Lobby => "LobbyView",
                ViewId.GameplayUI => "GameplayUIView",
                ViewId.MainMenu => "MainMenuView",
                ViewId.MinimalLoading => "MinimalLoadingView",
                ViewId.Settings => "SettingsView",
                ViewId.HeroCardSelector => "HeroCardSelectorView",
                ViewId.ConfirmDialog => "ConfirmationDialog",
                ViewId.HeroCard => "HeroCardUI",
                ViewId.Inventory => "InventoryView",
                _ => viewId.ToString() // Запасний варіант
            };
        }
    }
}
