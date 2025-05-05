// Шлях: Assets/_MythHunter/Code/Resources/Providers/DefaultResourceProvider.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.Resources.Core;
using UnityEngine;

namespace MythHunter.Resources.Providers
{
    /// <summary>
    /// Базовий провайдер ресурсів через Resources API
    /// </summary>
    public class DefaultResourceProvider : ResourceProviderBase
    {
        [Inject]
        public DefaultResourceProvider(IMythLogger logger) : base(logger)
        {
        }

        public override async UniTask<T> LoadAsync<T>(string key)
        {
            LogDebug($"Loading resource: {key}");

            // Спроба завантажити з кешу
            if (_loadedResources.TryGetValue(key, out var cached) && cached is T cachedTyped)
            {
                await UniTask.Yield();
                return cachedTyped;
            }

            // Завантаження через MythResourceUtils
            T resource = MythResourceUtils.Load<T>(key);

            if (resource != null)
            {
                _loadedResources[key] = resource;
                return resource;
            }

            LogWarning($"Failed to load resource: {key}");
            return null;
        }

        public override async UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern)
        {
            // Використовуємо статичний клас
            T[] resources = MythResourceUtils.LoadAll<T>(pattern);
            await UniTask.Yield();
            return resources;
        }

        public override void Unload(string key)
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

        public override void UnloadAll()
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
    }
}
