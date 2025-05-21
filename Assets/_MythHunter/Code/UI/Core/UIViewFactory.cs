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

        public async UniTask<T> CreateViewFromPoolAsync<T>(string prefabPath) where T : Component, IView
        {
            _logger.LogInfo($"[UIFactory] Creating pooled view: {typeof(T).Name} from path {prefabPath}", "UI");

            try
            {
                // Завантажуємо префаб для створення пулу, якщо потрібно
                GameObject prefab = null;

                // Спробуємо отримати об'єкт з пулу
                GameObject instance = null;

                try
                {
                    instance = _poolManager.GetFromPool<GameObject>(prefabPath);
                }
                catch (System.Exception)
                {
                    // Якщо пул не існує, створюємо його
                    prefab = await _resourceProvider.LoadAsync<GameObject>(prefabPath);
                    if (prefab == null)
                    {
                        _logger.LogError($"[UIFactory] Failed to load prefab at path: {prefabPath}", "UI");
                        return null;
                    }

                    _poolManager.CreatePool<GameObject>(prefabPath, prefab, 5);
                    instance = _poolManager.GetFromPool<GameObject>(prefabPath);
                }

                if (instance == null)
                {
                    _logger.LogError($"[UIFactory] Failed to get instance from pool for {prefabPath}", "UI");
                    return null;
                }

                // Встановлюємо батьківський трансформ і активуємо об'єкт
                instance.transform.SetParent(UIRoot.RootTransform, false);
                instance.SetActive(true);

                // Отримуємо компонент view
                var view = instance.GetComponent<T>();
                if (view == null)
                {
                    _logger.LogError($"[UIFactory] Instance from pool doesn't have component of type {typeof(T).Name}", "UI");
                    _poolManager.ReturnToPool(prefabPath, instance);
                    return null;
                }

                // Виконуємо ін'єкцію залежностей
                _container.InjectDependencies(view);

                return view;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[UIFactory] Exception creating view from pool: {ex.Message}", "UI", ex);
                return null;
            }
        }

        public void ReturnViewToPool<T>(T view) where T : Component, IView
        {
            if (view == null)
                return;

            try
            {
                GameObject gameObject = view.gameObject;

                // Перевіряємо, чи є у GameObject компонент PooledObject
                var pooledObject = gameObject.GetComponent<PooledObject>();
                if (pooledObject != null)
                {
                    // Використовуємо компонент PooledObject для повернення в пул
                    pooledObject.ReturnToPool();
                }
                else
                {
                    // Використовуємо ім'я префабу як ключ пулу
                    string poolKey = gameObject.name.Replace("(Clone)", "").Trim();

                    // Вимикаємо GameObject перед поверненням у пул
                    gameObject.SetActive(false);

                    // Повертаємо у пул
                    _poolManager.ReturnToPool(poolKey, gameObject);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[UIFactory] Error returning view to pool: {ex.Message}", "UI", ex);
                // Знищуємо об'єкт, якщо не вдалося повернути його в пул
                Object.Destroy(view.gameObject);
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
