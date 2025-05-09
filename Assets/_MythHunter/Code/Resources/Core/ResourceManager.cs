// Шлях: Assets/_MythHunter/Code/Resources/Core/ResourceManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Pool;
using MythHunter.Resources.Providers;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Core
{
    /// <summary>
    /// Центральний менеджер ресурсів
    /// </summary>
    public class ResourceManager : IResourceManager
    {
        private readonly List<IResourceProvider> _providers = new List<IResourceProvider>();
        private readonly Dictionary<string, IResourceProvider> _resourceCache = new Dictionary<string, IResourceProvider>();
        private readonly IPoolManager _poolManager;
        private readonly IMythLogger _logger;
        private bool _useAddressables;

        [Inject]
        public ResourceManager(
            IResourceProvider defaultProvider,
            IPoolManager poolManager,
            IMythLogger logger)
        {
            _logger = logger;
            _poolManager = poolManager;

            // Завжди додаємо базовий провайдер
            _providers.Add(defaultProvider);

            // Перевіряємо доступність Addressables
            _useAddressables = IsAddressablesAvailable();
            if (_useAddressables)
            {
                _logger.LogInfo("Addressables are available and will be used for resource loading", "Resource");
            }
            else
            {
                _logger.LogInfo("Addressables are not available, using default resource loading", "Resource");
            }
        }

        public void RegisterProvider(IResourceProvider provider, int priority = 0)
        {
            if (provider == null)
                return;

            _providers.Add(provider);

            // Сортуємо провайдери за пріоритетом, якщо вони реалізують IPrioritizable
            _providers.Sort((a, b) => {
                var priorityA = a is IPrioritizable prioritizableA ? prioritizableA.Priority : 0;
                var priorityB = b is IPrioritizable prioritizableB ? prioritizableB.Priority : 0;
                return priorityB.CompareTo(priorityA);
            });

            _logger.LogInfo($"Registered resource provider: {provider.GetType().Name} with priority {priority}", "Resource");
        }

        public async UniTask<T> LoadAsync<T>(string key) where T : UnityEngine.Object
        {
            // Спочатку перевіряємо, чи ресурс вже кешовано
            if (_resourceCache.TryGetValue(key, out var cachedProvider))
            {
                var result = await cachedProvider.LoadAsync<T>(key);
                if (result != null)
                    return result;
            }

            // Послідовно пробуємо кожен провайдер
            foreach (var provider in _providers)
            {
                try
                {
                    var result = await provider.LoadAsync<T>(key);
                    if (result != null)
                    {
                        // Запам'ятовуємо, який провайдер завантажив ресурс
                        _resourceCache[key] = provider;
                        _logger.LogDebug($"Loaded resource '{key}' using {provider.GetType().Name}", "Resource");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"Provider {provider.GetType().Name} failed to load '{key}': {ex.Message}", "Resource");
                    // Продовжуємо з наступним провайдером
                }
            }

            _logger.LogWarning($"Failed to load resource: {key}", "Resource");
            return null;
        }

        public async UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern) where T : UnityEngine.Object
        {
            // Пробуємо всі провайдери та об'єднуємо результати
            List<T> results = new List<T>();

            foreach (var provider in _providers)
            {
                try
                {
                    var providerResults = await provider.LoadAllAsync<T>(pattern);
                    if (providerResults != null && providerResults.Count > 0)
                    {
                        results.AddRange(providerResults);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"Provider {provider.GetType().Name} failed to load pattern '{pattern}': {ex.Message}", "Resource");
                    // Продовжуємо з наступним провайдером
                }
            }

            return results;
        }

        public void Unload(string key)
        {
            if (_resourceCache.TryGetValue(key, out var provider))
            {
                provider.Unload(key);
                _resourceCache.Remove(key);
            }
            else
            {
                // Якщо невідомо, який провайдер завантажив ресурс, вивантажуємо з усіх
                foreach (var p in _providers)
                {
                    p.Unload(key);
                }
            }
        }

        public void UnloadAll()
        {
            foreach (var provider in _providers)
            {
                provider.UnloadAll();
            }
            _resourceCache.Clear();
        }

        public T GetFromPool<T>(string key) where T : UnityEngine.Object
        {
            return _poolManager.GetFromPool<T>(key);
        }

        public void ReturnToPool<T>(string key, T instance) where T : UnityEngine.Object
        {
            _poolManager.ReturnToPool(key, instance);
        }

        public async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            if (_useAddressables)
            {
                var addressableProvider = _providers.OfType<IAddressablesProvider>().FirstOrDefault();
                if (addressableProvider != null)
                {
                    return await addressableProvider.InstantiateAsync(key, parent);
                }
            }

            // Запасний варіант - звичайне створення через Resources
            var prefab = await LoadAsync<GameObject>(key);
            if (prefab != null)
            {
                return UnityEngine.Object.Instantiate(prefab, parent);
            }

            return null;
        }

        public async UniTask InitializePoolAsync<T>(string key, int poolSize = 10) where T : UnityEngine.Object
        {
            // Завантажуємо префаб
            var prefab = await LoadAsync<T>(key);
            if (prefab == null)
            {
                _logger.LogError($"Failed to load prefab for pool '{key}'", "Pool");
                return;
            }

            // Створюємо пул
            _poolManager.CreatePool(key, prefab, poolSize);
            _logger.LogInfo($"Pool for key '{key}' initialized with size {poolSize}", "Pool");
        }

        private bool IsAddressablesAvailable()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name == "Unity.Addressables")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
