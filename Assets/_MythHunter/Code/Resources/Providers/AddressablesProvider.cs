using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.Resources.Core;
using UnityEngine;
using System.Linq;

namespace MythHunter.Resources.Providers
{
    /// <summary>
    /// Провайдер ресурсів на основі Addressables
    /// </summary>
    public class AddressablesProvider : ResourceProviderBase, IAddressablesProvider
    {
        [Inject]
        public AddressablesProvider(IMythLogger logger) : base(logger)
        {
            SetPriority(10); // ✅ Безпечне встановлення пріоритету
        }

        public override async UniTask<T> LoadAsync<T>(string key)
        {
            LogDebug($"Loading addressable asset: {key}");

            try
            {
                var result = await LoadAddressableAssetAsync<T>(key);
                if (result != null)
                    _loadedResources[key] = result;

                return result;
            }
            catch (System.Exception ex)
            {
                LogError($"Error loading addressable asset {key}: {ex.Message}");
                return null;
            }
        }

        public override async UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern)
        {
            LogDebug($"Loading addressable assets with pattern: {pattern}");

            try
            {
                var result = await LoadAddressableAssetsAsync<T>(pattern);
                return result;
            }
            catch (System.Exception ex)
            {
                LogError($"Error loading addressable assets with pattern {pattern}: {ex.Message}");
                return new List<T>();
            }
        }

        public async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            LogDebug($"Instantiating addressable asset: {key}");

            try
            {
                var result = await InstantiateAddressableAsync(key, parent);
                return result;
            }
            catch (System.Exception ex)
            {
                LogError($"Error instantiating addressable asset {key}: {ex.Message}");
                return null;
            }
        }

        public void ReleaseAsset<T>(T asset) where T : Object
        {
            ReleaseAddressableAsset(asset);
        }

        public void ReleaseAssets<T>(IList<T> assets) where T : Object
        {
            foreach (var asset in assets)
            {
                ReleaseAddressableAsset(asset);
            }
        }

        public void ReleaseInstance(GameObject instance)
        {
            ReleaseAddressableInstance(instance);
        }

        public override void Unload(string key)
        {
            base.Unload(key);
            // Optional: add Addressables.Release(key) if needed
        }

        public override void UnloadAll()
        {
            base.UnloadAll();
            // Optional: clean up if needed
        }

        #region Addressables Implementation Stubs

        private async UniTask<T> LoadAddressableAssetAsync<T>(string key) where T : Object
        {
            await UniTask.Yield();

            // Real implementation:
            // return await Addressables.LoadAssetAsync<T>(key).ToUniTask();
            return null;
        }

        private async UniTask<List<T>> LoadAddressableAssetsAsync<T>(string pattern) where T : Object
        {
            await UniTask.Yield();

            // Real implementation:
            // var locations = await Addressables.LoadResourceLocationsAsync(pattern).ToUniTask();
            // var results = new List<T>();
            // foreach (var loc in locations)
            // {
            //     var asset = await Addressables.LoadAssetAsync<T>(loc).ToUniTask();
            //     results.Add(asset);
            // }
            return new List<T>();
        }

        private async UniTask<GameObject> InstantiateAddressableAsync(string key, Transform parent)
        {
            await UniTask.Yield();

            // Real implementation:
            // return await Addressables.InstantiateAsync(key, parent).ToUniTask();
            return null;
        }

        private void ReleaseAddressableAsset<T>(T asset) where T : Object
        {
            // Real: Addressables.Release(asset);
        }

        private void ReleaseAddressableInstance(GameObject instance)
        {
            // Real: Addressables.ReleaseInstance(instance);
        }

        #endregion
    }
}
