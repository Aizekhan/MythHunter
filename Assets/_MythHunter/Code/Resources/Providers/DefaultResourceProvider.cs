using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MythHunter.Core.DI;
namespace MythHunter.Resources.Providers
{
    public class DefaultResourceProvider : IResourceProvider
    {
        private readonly IMythLogger _logger;
        private readonly Dictionary<string, Object> _loadedResources = new Dictionary<string, Object>();

        [Inject]
        public DefaultResourceProvider(IMythLogger logger)
        {
            _logger = logger;
        }
       

        public async UniTask<T> LoadAsync<T>(string key) where T : Object
        {
            _logger.LogDebug($"Loading resource: {key}", "Resource");

            // Спроба завантажити з кешу
            if (_loadedResources.TryGetValue(key, out var cached) && cached is T cachedTyped)
            {
                return cachedTyped;
            }

            // Завантаження через наш статичний клас
            var resource = MythResourceUtils.Load<T>(key);

            if (resource != null)
            {
                _loadedResources[key] = resource;
                return resource;
            }

            _logger.LogWarning($"Failed to load resource: {key}", "Resource");
            return null;
        }

        public async UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern) where T : Object
        {
            // Використовуємо наш статичний клас
            T[] resources = MythResourceUtils.LoadAll<T>(pattern);
            return resources;
        }

        public void Unload(string key)
        {
            if (_loadedResources.TryGetValue(key, out var resource))
            {
                if (resource != null)
                {
                    MythResourceUtils.UnloadAsset(resource);
                }
                _loadedResources.Remove(key);
            }
        }

        public void UnloadAll()
        {
            foreach (var resource in _loadedResources.Values)
            {
                if (resource != null)
                {
                    MythResourceUtils.UnloadAsset(resource);
                }
            }
            _loadedResources.Clear();
            MythResourceUtils.UnloadUnusedAssets();
        }

        public T GetFromPool<T>(string key) where T : Object
        {
            return null;
        }

        public void ReturnToPool<T>(string key, T instance) where T : Object
        {
            // Заглушка
        }
    }
}
