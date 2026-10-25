// Шлях: Assets/_MythHunter/Code/Resources/Providers/IAddressablesProvider.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MythHunter.Resources.Core;

namespace MythHunter.Resources.Providers
{
    /// <summary>
    /// Розширений інтерфейс для провайдера Addressables
    /// </summary>
    public interface IAddressablesProvider : IResourceProvider
    {
        UniTask<GameObject> InstantiateAsync(string key, Transform parent = null);
        void ReleaseAsset<T>(T asset) where T : UnityEngine.Object;
        void ReleaseAssets<T>(IList<T> assets) where T : UnityEngine.Object;
        void ReleaseInstance(GameObject instance);
    }
}
