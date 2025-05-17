// Шлях: Assets/_MythHunter/Code/UI/Core/UIComponentFactory.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Core;
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.UI.Core
{
    public class UIComponentFactory : IUIComponentFactory
    {
        private readonly IDIContainer _container;
        private readonly IResourceProvider _resourceProvider;
        private readonly IPoolManager _poolManager;
        private readonly IMythLogger _logger;

        [Inject]
        public UIComponentFactory(
            IDIContainer container,
            IResourceProvider resourceProvider,
            IPoolManager poolManager,
            IMythLogger logger)
        {
            _container = container;
            _resourceProvider = resourceProvider;
            _poolManager = poolManager;
            _logger = logger;
        }

        public T CreateComponent<T>(GameObject prefab, Transform parent = null) where T : MonoBehaviour
        {
            if (prefab == null)
            {
                _logger.LogError($"Cannot create component: prefab is null", "UIComponentFactory");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            T component = instance.GetComponent<T>();

            if (component == null)
            {
                _logger.LogError($"Created instance doesn't have component of type {typeof(T).Name}", "UIComponentFactory");
                Object.Destroy(instance);
                return null;
            }

            // Ін'єкція залежностей
            _container.InjectDependencies(component);

            return component;
        }

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
            catch (System.Exception ex)
            {
                _logger.LogError($"Error creating component: {ex.Message}", "UIComponentFactory", ex);
                return null;
            }
        }

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
                if (parent != null)
                {
                    instance.transform.SetParent(parent, false);
                }
                else
                {
                    // Якщо батьківський об'єкт не вказано, використовуємо UIRoot
                    instance.transform.SetParent(UIRoot.RootTransform, false);
                }

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
            catch (System.Exception ex)
            {
                _logger.LogError($"Error creating component from pool: {ex.Message}", "UIComponentFactory", ex);
                return null;
            }
        }

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
            catch (System.Exception ex)
            {
                _logger.LogError($"Error returning component to pool: {ex.Message}", "UIComponentFactory", ex);
                // Запасний варіант - знищення об'єкта
                Object.Destroy(component.gameObject);
            }
        }

        // Допоміжний метод для перевірки наявності пулу
        private bool HasPool(string poolKey)
        {
            // Оскільки в IPoolManager немає прямого методу для перевірки наявності пулу,
            // можемо спробувати отримати об'єкт з пулу і якщо виникне помилка - пул не існує
            try
            {
                var obj = _poolManager.GetFromPool<GameObject>(poolKey);
                if (obj != null)
                {
                    // Повертаємо об'єкт назад в пул
                    _poolManager.ReturnToPool(poolKey, obj);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
