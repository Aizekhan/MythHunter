using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Providers
{
    /// <summary>
    /// Запасний провайдер замість Addressables, що використовує стандартні Resources
    /// </summary>
    public class FallbackResourceProvider : IAddressablesProvider
    {
        private readonly IMythLogger _logger;
        private readonly DefaultResourceProvider _resourceProvider;

        [MythHunter.Core.DI.Inject]
        public FallbackResourceProvider(IMythLogger logger, DefaultResourceProvider resourceProvider)
        {
            _logger = logger;
            _resourceProvider = resourceProvider;
        }

        public async UniTask<T> LoadAssetAsync<T>(string key) where T : Object
        {
            _logger.LogDebug($"Fallback loading asset: {key}", "Resource");
            return await _resourceProvider.LoadAsync<T>(key);
        }

        public async UniTask<IList<T>> LoadAssetsAsync<T>(IEnumerable<string> keys) where T : Object
        {
            var result = new List<T>();

            foreach (var key in keys)
            {
                var asset = await _resourceProvider.LoadAsync<T>(key);
                if (asset != null)
                {
                    result.Add(asset);
                }
            }

            return result;
        }

        public async UniTask<IList<T>> LoadAssetsAsync<T>(string label) where T : Object
        {
            _logger.LogDebug($"Fallback loading assets with label: {label}", "Resource");
            // Спрощена версія - шукаємо в підпапці з назвою мітки
            var assets = await _resourceProvider.LoadAllAsync<T>(label);
            return new List<T>(assets);
        }

        public void ReleaseAsset<T>(T asset) where T : Object
        {
            // No-op в режимі Resources
        }

        public void ReleaseAssets<T>(IList<T> assets) where T : Object
        {
            // No-op в режимі Resources
        }

        public async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            var prefab = await _resourceProvider.LoadAsync<GameObject>(key);
            if (prefab != null)
            {
                return Object.Instantiate(prefab, parent);
            }
            return null;
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instance != null)
            {
                Object.Destroy(instance);
            }
        }
    }
}
