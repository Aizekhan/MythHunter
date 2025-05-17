// Шлях: Assets/_MythHunter/Code/UI/Core/UIViewFactory.cs

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

        [Inject]
        public UIViewFactory(
            IResourceProvider resourceProvider,
            IMythLogger logger,
            IPoolManager poolManager,
            IDIContainer container)
        {
            _resourceProvider = resourceProvider;
            _logger = logger;
            _poolManager = poolManager;
            _container = container;
        }

        public async UniTask<T> CreateViewAsync<T>(string prefabPath) where T : Component, IView
        {
            _logger.LogInfo($"[UIFactory] Creating view: {typeof(T).Name} from path {prefabPath}", "UI");
            try
            {
                var prefab = await _resourceProvider.LoadAsync<GameObject>(prefabPath);
                if (prefab == null)
                {
                    _logger.LogError($"[UIFactory] Failed to load UI prefab at path: {prefabPath}", "UI");
                    return null;
                }

                // Визначаємо parent — GlobalCanvas (UIRoot)
                Transform parent = UIRoot.RootTransform;
                if (parent == null)
                {
                    _logger.LogError("[UIFactory] UIRoot.RootTransform not found. UI will not appear correctly!", "UI");
                }

                var instance = Object.Instantiate(prefab, parent);
                var view = instance.GetComponent<T>();

                if (view == null)
                {
                    _logger.LogError($"[UIFactory] Prefab '{prefab.name}' is missing component of type {typeof(T).Name}", "UI");
                    Object.Destroy(instance);
                    return null;
                }

                // Виконуємо ін'єкцію залежностей
                _container.InjectDependencies(view);

                return view;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[UIFactory] Exception creating view: {ex.Message}", "UI", ex);
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

        public void ReleaseView<T>(T view) where T : Component, IView
        {
            if (view != null)
            {
                // Спочатку спробуємо повернути в пул, якщо не вийде - знищимо
                try
                {
                    ReturnViewToPool(view);
                }
                catch (System.Exception)
                {
                    // Якщо не вдалося повернути в пул, знищуємо
                    Object.Destroy(view.gameObject);
                }
            }
        }
    }
}
