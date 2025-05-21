// Шлях: Assets/_MythHunter/Code/UI/Core/UIComponentFactory.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Core;
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;
using MythHunter.UI.ViewConfigs;
using System;
using UnityEngine;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Фабрика компонентів UI
    /// </summary>
    public class UIComponentFactory : IUIComponentFactory
    {
        private readonly IDIContainer _container;
        private readonly IResourceProvider _resourceProvider;
        private readonly IPoolManager _poolManager;
        private readonly IMythLogger _logger;
        private readonly IViewConfigRegistry _viewConfigRegistry;

        [Inject]
        public UIComponentFactory(
            IDIContainer container,
            IResourceProvider resourceProvider,
            IPoolManager poolManager,
            IMythLogger logger,
            IViewConfigRegistry viewConfigRegistry)
        {
            _container = container;
            _resourceProvider = resourceProvider;
            _poolManager = poolManager;
            _logger = logger;
            _viewConfigRegistry = viewConfigRegistry;
        }

        /// <summary>
        /// Створює компонент із ViewId
        /// </summary>
        public async UniTask<GameObject> CreateComponentAsync(ViewId viewId, Transform parent = null)
        {
            // Отримуємо конфігурацію за ViewId
            var config = _viewConfigRegistry.Get(viewId);
            if (config == null)
            {
                _logger.LogError($"Не знайдено конфігурацію для viewId: {viewId}", "UIComponentFactory");
                return null;
            }

            try
            {
                // Завантажуємо префаб
                var prefab = await _resourceProvider.LoadAsync<GameObject>(config.prefabPath);
                if (prefab == null)
                {
                    _logger.LogError($"Не вдалося завантажити префаб за шляхом {config.prefabPath}", "UIComponentFactory");
                    return null;
                }

                // Створюємо екземпляр
                GameObject instance = UnityEngine.Object.Instantiate(prefab, parent ?? UIRoot.RootTransform);
                if (instance == null)
                {
                    _logger.LogError($"Не вдалося створити екземпляр префабу {config.prefabPath}", "UIComponentFactory");
                    return null;
                }

                // Ін'єкція залежностей
                _container.InjectDependencies(instance);

                return instance;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при створенні компонента для {viewId}: {ex.Message}", "UIComponentFactory", ex);
                return null;
            }
        }

        /// <summary>
        /// Створює компонент із пулу за ViewId
        /// </summary>
        public async UniTask<GameObject> CreateFromPoolAsync(ViewId viewId, Transform parent = null)
        {
            // Отримуємо конфігурацію за ViewId
            var config = _viewConfigRegistry.Get(viewId);
            if (config == null)
            {
                _logger.LogError($"Не знайдено конфігурацію для viewId: {viewId}", "UIComponentFactory");
                return null;
            }

            string prefabPath = config.prefabPath;
            try
            {
                // Перевіряємо, чи існує пул
                if (!_poolManager.HasPool(prefabPath))
                {
                    // Завантажуємо префаб
                    var prefab = await _resourceProvider.LoadAsync<GameObject>(prefabPath);
                    if (prefab == null)
                    {
                        _logger.LogError($"Не вдалося завантажити префаб за шляхом {prefabPath}", "UIComponentFactory");
                        return null;
                    }

                    // Створюємо пул
                    _poolManager.CreatePool<GameObject>(prefabPath, prefab, 5);
                }

                // Отримуємо об'єкт з пулу
                var instance = _poolManager.GetFromPool<GameObject>(prefabPath);
                if (instance == null)
                {
                    _logger.LogError($"Не вдалося отримати екземпляр з пулу для {prefabPath}", "UIComponentFactory");
                    return null;
                }

                // Встановлюємо батьківський об'єкт
                instance.transform.SetParent(parent ?? UIRoot.RootTransform, false);
                instance.SetActive(true);

                // Ін'єкція залежностей
                _container.InjectDependencies(instance);

                return instance;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при створенні компонента з пулу для {viewId}: {ex.Message}", "UIComponentFactory", ex);
                return null;
            }
        }

        /// <summary>
        /// Повертає компонент у пул
        /// </summary>
        public void ReturnToPool(ViewId viewId, GameObject gameObject)
        {
            if (gameObject == null)
                return;

            try
            {
                // Отримуємо конфігурацію за ViewId
                var config = _viewConfigRegistry.Get(viewId);
                if (config == null)
                {
                    _logger.LogWarning($"Не знайдено конфігурацію для viewId: {viewId}, об'єкт буде знищено", "UIComponentFactory");
                    UnityEngine.Object.Destroy(gameObject);
                    return;
                }

                string prefabPath = config.prefabPath;

                // Перевіряємо, чи є компонент PooledObject
                var pooledObject = gameObject.GetComponent<PooledObject>();
                if (pooledObject != null)
                {
                    pooledObject.ReturnToPool();
                }
                else
                {
                    // Деактивуємо об'єкт
                    gameObject.SetActive(false);

                    // Повертаємо в пул
                    _poolManager.ReturnToPool(prefabPath, gameObject);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при поверненні компонента в пул: {ex.Message}", "UIComponentFactory", ex);
                UnityEngine.Object.Destroy(gameObject);
            }
        }

        #region Legacy Methods (For backward compatibility)

        [Obsolete("Використовуйте CreateComponentAsync(ViewId, Transform) для відповідності архітектурним принципам")]
        public T CreateComponent<T>(GameObject prefab, Transform parent = null) where T : MonoBehaviour
        {
            if (prefab == null)
            {
                _logger.LogError($"Cannot create component: prefab is null", "UIComponentFactory");
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, parent ?? UIRoot.RootTransform);
            T component = instance.GetComponent<T>();

            if (component == null)
            {
                _logger.LogError($"Created instance doesn't have component of type {typeof(T).Name}", "UIComponentFactory");
                UnityEngine.Object.Destroy(instance);
                return null;
            }

            // Ін'єкція залежностей
            _container.InjectDependencies(component);

            return component;
        }

        [Obsolete("Використовуйте CreateComponentAsync(ViewId, Transform) для відповідності архітектурним принципам")]
        public async UniTask<T> CreateComponentAsync<T>(string prefabPath, Transform parent = null) where T : MonoBehaviour
        {
            try
            {
                GameObject prefab = await _resourceProvider.LoadAsync<GameObject>(prefabPath);
                if (prefab == null)
                {
                    _logger.LogError($"Failed to load prefab from path: {prefabPath}", "UIComponentFactory");
                    return null;
                }

                return CreateComponent<T>(prefab, parent);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating component: {ex.Message}", "UIComponentFactory", ex);
                return null;
            }
        }

        [Obsolete("Використовуйте CreateFromPoolAsync(ViewId, Transform) для відповідності архітектурним принципам")]
        public async UniTask<T> CreateFromPoolAsync<T>(string prefabPath, Transform parent = null) where T : MonoBehaviour
        {
            try
            {
                // Завантажуємо префаб, якщо він ще не завантажений
                GameObject prefab = await _resourceProvider.LoadAsync<GameObject>(prefabPath);
                if (prefab == null)
                {
                    _logger.LogError($"Failed to load prefab from path: {prefabPath}", "UIComponentFactory");
                    return null;
                }

                // Перевіряємо, чи створений пул для цього префаба, якщо ні - створюємо
                // Використовуємо назву префаба як ключ пулу
                string poolKey = prefabPath;

                // Створюємо пул, якщо він не існує
                if (!_poolManager.HasPool(poolKey))
                {
                    _poolManager.CreatePool<GameObject>(poolKey, prefab, 5);
                }

                // Отримуємо GameObject з пулу
                GameObject instance = _poolManager.GetFromPool<GameObject>(poolKey);
                if (instance == null)
                {
                    _logger.LogError($"Failed to get instance from pool: {prefabPath}", "UIComponentFactory");
                    return null;
                }

                // Встановлюємо батьківський об'єкт
                instance.transform.SetParent(parent ?? UIRoot.RootTransform, false);
                instance.SetActive(true);

                // Отримуємо компонент
                T component = instance.GetComponent<T>();
                if (component == null)
                {
                    _logger.LogError($"Instance from pool doesn't have component of type {typeof(T).Name}", "UIComponentFactory");
                    _poolManager.ReturnToPool(poolKey, instance);
                    return null;
                }

                // Ін'єкція залежностей
                _container.InjectDependencies(component);

                return component;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating component from pool: {ex.Message}", "UIComponentFactory", ex);
                return null;
            }
        }

        [Obsolete("Використовуйте ReturnToPool(ViewId, GameObject) для відповідності архітектурним принципам")]
        public void ReturnToPool<T>(T component) where T : MonoBehaviour
        {
            if (component == null)
                return;

            try
            {
                GameObject gameObject = component.gameObject;

                // Спроба отримати ключ пулу - за замовчуванням використовуємо шлях до префабу, 
                // який має бути збережений в компоненті PooledObject
                var pooledObject = gameObject.GetComponent<PooledObject>();
                if (pooledObject != null)
                {
                    // Використовуємо метод компонента PooledObject для повернення в пул
                    pooledObject.ReturnToPool();
                }
                else
                {
                    // Якщо компонент PooledObject відсутній, можемо спробувати використати ім'я як ключ
                    string poolKey = gameObject.name.Replace("(Clone)", "").Trim();
                    _poolManager.ReturnToPool(poolKey, gameObject);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error returning component to pool: {ex.Message}", "UIComponentFactory", ex);
                // Запасний варіант - знищення об'єкта
                UnityEngine.Object.Destroy(component.gameObject);
            }
        }

        #endregion
    }
}
