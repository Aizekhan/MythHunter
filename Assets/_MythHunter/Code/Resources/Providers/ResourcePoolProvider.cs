// Шлях: Assets/_MythHunter/Code/Resources/Providers/ResourcePoolProvider.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Pool;
using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Providers
{
    /// <summary>
    /// Провайдер ресурсів з підтримкою пулінгу
    /// </summary>
    public class ResourcePoolProvider : BaseResourceProvider, IResourceProvider
    {
        private readonly IResourceProvider _baseProvider;
        private readonly IPoolManager _poolManager;
        private readonly Dictionary<string, bool> _poolInitialized = new Dictionary<string, bool>();

        [Inject]
        public ResourcePoolProvider(IResourceProvider baseProvider, IPoolManager poolManager, IMythLogger logger)
            : base(logger)
        {
            _baseProvider = baseProvider;
            _poolManager = poolManager;
        }

        public async UniTask<T> LoadAsync<T>(string key) where T : UnityEngine.Object
        {
            return await _baseProvider.LoadAsync<T>(key);
        }

        public async UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern) where T : UnityEngine.Object
        {
            return await _baseProvider.LoadAllAsync<T>(pattern);
        }

        public void Unload(string key)
        {
            _baseProvider.Unload(key);
        }

        public void UnloadAll()
        {
            _baseProvider.UnloadAll();
        }

        public T GetFromPool<T>(string key) where T : UnityEngine.Object
        {
            // Перевіряємо, чи пул ініціалізовано
            if (!_poolInitialized.TryGetValue(key, out bool initialized) || !initialized)
            {
                Log($"Pool for key '{key}' is not initialized. Initializing it now.", LogLevel.Info);
                InitializePool<T>(key).Forget();
                return null; // Повертаємо null на перший раз, поки пул ініціалізується
            }

            // Отримуємо об'єкт з пулу
            T instance = _poolManager.GetFromPool<T>(key);
            if (instance == null)
            {
                Log($"Failed to get object from pool '{key}'", LogLevel.Warning);
            }

            return instance;
        }

        public void ReturnToPool<T>(string key, T instance) where T : UnityEngine.Object
        {
            if (instance == null)
            {
                Log("Trying to return null instance to pool", LogLevel.Warning);
                return;
            }

            // Перевіряємо, чи пул ініціалізовано
            if (!_poolInitialized.TryGetValue(key, out bool initialized) || !initialized)
            {
                Log($"Pool for key '{key}' is not initialized. Cannot return object.", LogLevel.Warning);
                return;
            }

            _poolManager.ReturnToPool(key, instance);
        }

        /// <summary>
        /// Ініціалізує пул для вказаного ключа асинхронно
        /// </summary>
        private async UniTaskVoid InitializePool<T>(string key) where T : UnityEngine.Object
        {
            // Перевіряємо, чи пул вже ініціалізовано
            if (_poolInitialized.TryGetValue(key, out bool initialized) && initialized)
            {
                return;
            }

            // Позначаємо пул як такий, що ініціалізується
            _poolInitialized[key] = false;

            try
            {
                // Завантажуємо префаб
                T prefab = await _baseProvider.LoadAsync<T>(key);
                if (prefab == null)
                {
                    Log($"Failed to load prefab for pool '{key}'", LogLevel.Error);
                    return;
                }

                // Створюємо пул
                _poolManager.CreatePool(key, prefab, 10);
                _poolInitialized[key] = true;
                Log($"Pool for key '{key}' initialized successfully", LogLevel.Info);
            }
            catch (System.Exception ex)
            {
                Log($"Error initializing pool for key '{key}': {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Асинхронно ініціалізує пул з вказаним розміром
        /// </summary>
        public async UniTask InitializePoolAsync<T>(string key, int poolSize = 10) where T : UnityEngine.Object
        {
            // Перевіряємо, чи пул вже ініціалізовано
            if (_poolInitialized.TryGetValue(key, out bool initialized) && initialized)
            {
                Log($"Pool for key '{key}' is already initialized", LogLevel.Info);
                return;
            }

            // Позначаємо пул як такий, що ініціалізується
            _poolInitialized[key] = false;

            try
            {
                // Завантажуємо префаб
                T prefab = await _baseProvider.LoadAsync<T>(key);
                if (prefab == null)
                {
                    Log($"Failed to load prefab for pool '{key}'", LogLevel.Error);
                    return;
                }

                // Створюємо пул
                _poolManager.CreatePool(key, prefab, poolSize);
                _poolInitialized[key] = true;
                Log($"Pool for key '{key}' initialized successfully with {poolSize} instances", LogLevel.Info);
            }
            catch (System.Exception ex)
            {
                Log($"Error initializing pool for key '{key}': {ex.Message}", LogLevel.Error);
            }
        }
    }
}
