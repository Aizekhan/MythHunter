// Шлях: Assets/_MythHunter/Code/UI/Core/UIViewFactory.cs

using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Pool;
using MythHunter.UI.ViewConfigs;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Фабрика представлень UI з інтеграцією ViewConfig системи
    /// </summary>
    public class UIViewFactory : IUIViewFactory
    {
        private readonly IPoolManager _poolManager;
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;
        private readonly IViewConfigRegistry _viewConfigRegistry;

        [Inject]
        public UIViewFactory(
            IPoolManager poolManager,
            IMythLogger logger,
            IDIContainer container,
            IViewConfigRegistry viewConfigRegistry)
        {
            _poolManager = poolManager;
            _logger = logger;
            _container = container;
            _viewConfigRegistry = viewConfigRegistry;
        }

        /// <summary>
        /// Створює представлення на основі ViewConfig
        /// </summary>
        public async UniTask<IView> CreateViewAsync(ViewId viewId)
        {
            var config = _viewConfigRegistry.Get(viewId);
            if (config == null)
            {
                _logger.LogError($"❌ ViewConfig не знайдено для {viewId}", "UIViewFactory");
                return null;
            }

            _logger.LogDebug($"🏗️ Створення View {viewId} (cached: {config.isCached}, category: {config.category})", "UIViewFactory");

            // 🎯 ЛОГІКА РОЗДІЛЕННЯ: пул чи пряме створення
            if (ShouldUsePool(config))
            {
                return await CreateFromPoolAsync(viewId, config);
            }
            else
            {
                return await CreateDirectlyAsync(viewId, config);
            }
        }

        /// <summary>
        /// Визначає чи потрібно використовувати пул для цього View
        /// </summary>
        private bool ShouldUsePool(ViewConfig config)
        {
            return config.isCached &&
                   config.category == UICategory.Common &&  // ✅ лише Common!
                   !config.isPopup;
        }


        /// <summary>
        /// Створює View з пулу
        /// </summary>
        private async UniTask<IView> CreateFromPoolAsync(ViewId viewId, ViewConfig config)
        {
            string poolKey = config.prefabPath;

            if (!_poolManager.HasPool(poolKey))
            {
                _logger.LogError($"❌ Пул '{poolKey}' для {viewId} не знайдено! Перевірте preload конфігурацію.", "UIViewFactory");
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

                var view = SetupViewFromGameObject(gameObject, viewId);
                _logger.LogDebug($"✅ View {viewId} створено з пулу {poolKey}", "UIViewFactory");
                return view;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка створення View {viewId} з пулу: {ex.Message}", "UIViewFactory", ex);
                return null;
            }

            await UniTask.CompletedTask; // Для сумісності з async
        }

        /// <summary>
        /// Створює View напряму з Resources
        /// </summary>
        private async UniTask<IView> CreateDirectlyAsync(ViewId viewId, ViewConfig config)
        {
            try
            {
                var prefab = UnityEngine.Resources.Load<GameObject>(config.prefabPath);
                if (prefab == null)
                {
                    _logger.LogError($"❌ Префаб не знайдено: {config.prefabPath}", "UIViewFactory");
                    return null;
                }

                var gameObject = UnityEngine.Object.Instantiate(prefab);
                var view = SetupViewFromGameObject(gameObject, viewId);

                _logger.LogDebug($"✅ View {viewId} створено напряму з {config.prefabPath}", "UIViewFactory");
                return view;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка створення View {viewId} напряму: {ex.Message}", "UIViewFactory", ex);
                return null;
            }

            await UniTask.CompletedTask; // Для сумісності з async
        }

        /// <summary>
        /// Спільна логіка налаштування View з GameObject
        /// </summary>
        private IView SetupViewFromGameObject(GameObject gameObject, ViewId viewId)
        {
            if (gameObject == null)
            {
                _logger.LogError($"❌ GameObject є null для {viewId}", "UIViewFactory");
                return null;
            }

            // Встановлюємо батьківський об'єкт
            gameObject.transform.SetParent(UIRoot.RootTransform, false);

            // Отримуємо компонент IView
            var view = gameObject.GetComponent<IView>();
            if (view == null)
            {
                _logger.LogError($"❌ GameObject не містить компонента IView: {viewId}", "UIViewFactory");
                UnityEngine.Object.Destroy(gameObject);
                return null;
            }

            // Ін'єкція залежностей
            try
            {
                _container.InjectDependencies(gameObject);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ Помилка ін'єкції залежностей для {viewId}: {ex.Message}", "UIViewFactory");
            }

            // Активуємо об'єкт
            gameObject.SetActive(true);

            return view;
        }

        /// <summary>
        /// Повертає представлення в пул або знищує його
        /// </summary>
        public void ReturnViewToPool(ViewId viewId, IView view)
        {
            if (view == null)
                return;

            var config = _viewConfigRegistry.Get(viewId);
            if (config == null)
            {
                _logger.LogWarning($"⚠️ ViewConfig не знайдено для {viewId} при поверненні", "UIViewFactory");
                ReleaseView(viewId, view);
                return;
            }

            var gameObject = (view as Component)?.gameObject;
            if (gameObject == null)
            {
                _logger.LogWarning($"⚠️ View {viewId} не є Component", "UIViewFactory");
                return;
            }

            try
            {
                if (ShouldUsePool(config))
                {
                    // Повертаємо в пул
                    gameObject.SetActive(false);
                    _poolManager.ReturnToPool(config.prefabPath, gameObject);
                    _logger.LogDebug($"✅ View {viewId} повернено в пул {config.prefabPath}", "UIViewFactory");
                }
                else
                {
                    // Знищуємо напряму
                    UnityEngine.Object.Destroy(gameObject);
                    _logger.LogDebug($"🗑️ View {viewId} знищено (не pooled)", "UIViewFactory");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка повернення View {viewId}: {ex.Message}", "UIViewFactory", ex);

                // Запасний варіант - знищення
                if (gameObject != null)
                    UnityEngine.Object.Destroy(gameObject);
            }
        }

        /// <summary>
        /// Знищує представлення (для випадків коли пул не використовується)
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
        /// Застарілий метод - використовуйте CreateViewAsync
        /// </summary>
        [System.Obsolete("Використовуйте CreateViewAsync - всі View тепер створюються через ViewConfig")]
        public async UniTask<IView> CreateViewFromPoolAsync(ViewId viewId)
        {
            return await CreateViewAsync(viewId);
        }
    }
}
