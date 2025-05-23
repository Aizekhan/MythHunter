// Шлях: Assets/_MythHunter/Code/Resources/Providers/DefaultResourceProvider.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.Resources.Core;
using UnityEngine;
using System;

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
            if (string.IsNullOrEmpty(key))
            {
                LogWarning($"Порожній ключ для завантаження {typeof(T).Name}");
                return null;
            }

            try
            {
                // ✅ Спробуйте різні варіанти шляхів
                string[] possiblePaths = {
            key,
            $"Resources/{key}",
            key.Replace("UI/", "")
        };

                foreach (string path in possiblePaths)
                {
                    var resource = UnityEngine.Resources.Load<T>(path);
                    if (resource != null)
                    {
                        LogInfo($"Завантажено {typeof(T).Name} з шляху: {path}");
                        return resource;
                    }
                }

                LogWarning($"Не вдалося завантажити {typeof(T).Name} за ключем: {key}");
                return null;
            }
            catch (Exception ex)
            {
                LogError($"Помилка завантаження {key}: {ex.Message}");
                return null;
            }
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
