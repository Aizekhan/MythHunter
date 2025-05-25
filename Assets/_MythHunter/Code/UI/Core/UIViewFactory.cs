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

            if (!_poolManager.HasPool(poolKey))
            {
                _logger.LogError($"❌ Пул '{poolKey}' для ViewId {viewId} не знайдено! Перевірте preload конфігурацію.", "UIViewFactory");
                return null;
            }

            try
            {
                var gameObject = _poolManager.GetFromPool<GameObject>(poolKey);
                if (gameObject == null)
                {
                    _logger.LogError($"❌ Не вдалося отримати GameObject з пулу '{poolKey}'", "UIViewFactory");
                    return null;
                }

                // Налаштування об'єкта
                gameObject.transform.SetParent(UIRoot.RootTransform, false);

                var view = gameObject.GetComponent<IView>();
                if (view == null)
                {
                    _logger.LogError($"❌ GameObject з пулу '{poolKey}' не містить компонента IView", "UIViewFactory");
                    _poolManager.ReturnToPool(poolKey, gameObject);
                    return null;
                }

                // Ін'єкція залежностей
                _container.InjectDependencies(gameObject);

                // Активуємо об'єкт
                gameObject.SetActive(true);

                _logger.LogDebug($"✅ Створено View {viewId} з пулу {poolKey}", "UIViewFactory");
                return view;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка створення View {viewId}: {ex.Message}", "UIViewFactory", ex);
                return null;
            }

            await UniTask.CompletedTask; // Для сумісності з async
        }

        /// <summary>
        /// Застарілий метод - використовуйте CreateViewAsync
        /// </summary>
        [Obsolete("Використовуйте CreateViewAsync - всі View тепер створюються з пулів")]
        public async UniTask<IView> CreateViewFromPoolAsync(ViewId viewId)
        {
            return await CreateViewAsync(viewId);
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
