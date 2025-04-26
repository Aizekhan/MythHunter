using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MythHunter.Resources.Providers
{
    /// <summary>
    /// Інтерфейс провайдера Addressables
    /// </summary>
    public interface IAddressablesProvider
    {
        Task<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object;
        Task<IList<T>> LoadAssetsAsync<T>(IEnumerable<string> keys) where T : UnityEngine.Object;
        Task<IList<T>> LoadAssetsAsync<T>(string label) where T : UnityEngine.Object;
        void ReleaseAsset<T>(T asset) where T : UnityEngine.Object;
        void ReleaseAssets<T>(IList<T> assets) where T : UnityEngine.Object;
        Task<GameObject> InstantiateAsync(string key, Transform parent = null);
        void ReleaseInstance(GameObject instance);
    }
}