// Шлях: Assets/_MythHunter/Code/Resources/Providers/IAssetBundleProvider.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Resources.Core;
using UnityEngine;

namespace MythHunter.Resources.Providers
{
    /// <summary>
    /// Інтерфейс для провайдера ресурсів на основі Asset Bundles
    /// </summary>
    public interface IAssetBundleProvider : IResourceProvider
    {
        UniTask<AssetBundle> LoadBundleAsync(string bundlePath);
        UniTask UnloadBundleAsync(string bundlePath, bool unloadAllAssets = false);
        UniTask CleanupUnusedAssetsAsync();
    }
}
