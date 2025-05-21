// Шлях: Assets/_MythHunter/Code/UI/Core/UIViewFactory.cs

using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Core;
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Фабрика представлень UI
    /// </summary>
    public class UIViewFactory : IUIViewFactory
    {
        private readonly IResourceProvider _resourceProvider;
        private readonly IMythLogger _logger;
        private readonly IPoolManager _poolManager;
        private readonly IDIContainer _container;
        private readonly IViewConfigRegistry _viewConfigRegistry;
        [Inject]
        public UIViewFactory(
            IResourceProvider resourceProvider,
            IMythLogger logger,
            IPoolManager poolManager,
            IDIContainer container,
            IViewConfigRegistry viewConfigRegistry)
        {
            _resourceProvider = resourceProvider;
            _logger = logger;
            _poolManager = poolManager;
            _container = container;
            _viewConfigRegistry = viewConfigRegistry;
        }

        public async UniTask<IView> CreateViewAsync(ViewId viewId)
        {
            // Отримуємо конфігурацію за ViewId
            var config = _viewConfigRegistry.Get(viewId);
            if (config == null)
            {
                _logger.LogError($"Не знайдено конфігурацію для viewId: {viewId}", "UIViewFactory");
                return null;
            }

            try
            {
                // Використовуємо prefabPath з конфігурації
                var prefab = await _resourceProvider.LoadAsync<GameObject>(config.prefabPath);
                if (prefab == null)
                {
                    _logger.LogError($"Не вдалося завантажити префаб за шляхом {config.prefabPath}", "UIViewFactory");
                    return null;
                }

                // Створення екземпляра
                var instance = UnityEngine.Object.Instantiate(prefab, UIRoot.RootTransform);
                var view = instance.GetComponent<IView>();

                if (view == null)
                {
                    _logger.LogError($"Префаб не містить компонента, що реалізує IView", "UIViewFactory");
                    UnityEngine.Object.Destroy(instance);
                    return null;
                }

                // Ін'єкція залежностей
                _container.InjectDependencies(instance);

                return view;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при створенні представлення {viewId}: {ex.Message}", "UIViewFactory");
                return null;
            }
        }

        /// <summary>
        /// Створює представлення з об'єктного пулу за його ідентифікатором
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <returns>Створене представлення з пулу</returns>
        public async UniTask<IView> CreateViewFromPoolAsync(ViewId viewId)
        {
            // Отримуємо конфігурацію за ViewId
            var config = _viewConfigRegistry.Get(viewId);
            if (config == null)
            {
                _logger.LogError($"Не знайдено конфігурацію для viewId: {viewId}", "UIViewFactory");
                return null;
            }

            string prefabPath = config.prefabPath;
            _logger.LogInfo($"[UIFactory] Creating pooled view: {viewId} from path {prefabPath}", "UI");

            try
            {
                // Спробуємо отримати об'єкт з пулу
                GameObject instance = null;

                try
                {
                    instance = _poolManager.GetFromPool<GameObject>(prefabPath);
                }
                catch (System.Exception)
                {
                    // Якщо пул не існує, створюємо його
                    var prefab = await _resourceProvider.LoadAsync<GameObject>(prefabPath);
                    if (prefab == null)
                    {
                        _logger.LogError($"[UIFactory] Не вдалося завантажити префаб за шляхом: {prefabPath}", "UI");
                        return null;
                    }

                    _poolManager.CreatePool<GameObject>(prefabPath, prefab, 5);
                    instance = _poolManager.GetFromPool<GameObject>(prefabPath);
                }

                if (instance == null)
                {
                    _logger.LogError($"[UIFactory] Не вдалося отримати екземпляр з пулу для {prefabPath}", "UI");
                    return null;
                }

                // Встановлюємо батьківський трансформ і активуємо об'єкт
                instance.transform.SetParent(UIRoot.RootTransform, false);
                instance.SetActive(true);

                // Отримуємо компонент view
                var view = instance.GetComponent<IView>();
                if (view == null)
                {
                    _logger.LogError($"[UIFactory] Екземпляр з пулу не містить компонента, що реалізує IView", "UI");
                    _poolManager.ReturnToPool(prefabPath, instance);
                    return null;
                }

                // Виконуємо ін'єкцію залежностей
                _container.InjectDependencies(instance);

                return view;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[UIFactory] Помилка при створенні представлення з пулу для {viewId}: {ex.Message}", "UI", ex);
                return null;
            }
        }

        /// <summary>
        /// Повертає представлення в пул
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <param name="view">Представлення для повернення в пул</param>
        public void ReturnViewToPool(ViewId viewId, IView view)
        {
            if (view == null)
                return;

            try
            {
                // Отримуємо конфігурацію за ViewId для шляху до префабу
                var config = _viewConfigRegistry.Get(viewId);
                if (config == null)
                {
                    _logger.LogWarning($"Не знайдено конфігурацію для viewId: {viewId}, об'єкт буде знищено", "UIViewFactory");
                    if (view is Component component)
                        UnityEngine.Object.Destroy(component.gameObject);
                    return;
                }

                string prefabPath = config.prefabPath;
                GameObject gameObject = (view as Component)?.gameObject;
                if (gameObject == null)
                {
                    _logger.LogWarning($"View не є Component, неможливо повернути в пул", "UIViewFactory");
                    return;
                }

                // Перевіряємо, чи є у GameObject компонент PooledObject
                var pooledObject = gameObject.GetComponent<PooledObject>();
                if (pooledObject != null)
                {
                    // Використовуємо компонент PooledObject для повернення в пул
                    pooledObject.ReturnToPool();
                }
                else
                {
                    // Вимикаємо GameObject перед поверненням у пул
                    gameObject.SetActive(false);

                    // Повертаємо у пул за шляхом з конфігурації
                    _poolManager.ReturnToPool(prefabPath, gameObject);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[UIFactory] Помилка при поверненні представлення в пул: {ex.Message}", "UI", ex);
                // Знищуємо об'єкт, якщо не вдалося повернути його в пул
                if (view is Component component)
                    UnityEngine.Object.Destroy(component.gameObject);
            }
        }

        public void ReleaseView(ViewId viewId, IView view)
        {
            if (view is Component component)
            {
                UnityEngine.Object.Destroy(component.gameObject);
            }
        }
    }
}
