// Шлях: Assets/_MythHunter/Code/Resources/Providers/AssetBundleProvider.cs
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
    /// Провайдер ресурсів на основі Asset Bundles
    /// </summary>
    public class AssetBundleProvider : ResourceProviderBase, IAssetBundleProvider
    {
        private readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
        private readonly Dictionary<string, Dictionary<string, UnityEngine.Object>> _cachedAssets = new Dictionary<string, Dictionary<string, UnityEngine.Object>>();

        [Inject]
        public AssetBundleProvider(IMythLogger logger) : base(logger)
        {
            SetPriority(5); // Проміжний пріоритет між AddressableProvider (10) і DefaultResourceProvider (0)
        }

        /// <summary>
        /// Завантажує Asset Bundle
        /// </summary>
        public async UniTask<AssetBundle> LoadBundleAsync(string bundlePath)
        {
            if (_loadedBundles.TryGetValue(bundlePath, out var bundle))
                return bundle;

            LogInfo($"Loading asset bundle: {bundlePath}");

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
            await request;

            if (request.assetBundle == null)
            {
                LogError($"Failed to load asset bundle: {bundlePath}");
                return null;
            }

            _loadedBundles[bundlePath] = request.assetBundle;
            _cachedAssets[bundlePath] = new Dictionary<string, UnityEngine.Object>();

            return request.assetBundle;
        }

        public override async UniTask<T> LoadAsync<T>(string key)
        {
            // Формат ключа: "bundlePath:assetName"
            if (!ParseKey(key, out string bundlePath, out string assetName))
            {
                LogWarning($"Invalid asset bundle key format: {key}. Expected format: 'bundlePath:assetName'");
                return null;
            }

            // Перевіряємо кеш
            if (_cachedAssets.TryGetValue(bundlePath, out var assetsCache) &&
                assetsCache.TryGetValue(assetName, out var cachedAsset) &&
                cachedAsset is T cachedTyped)
            {
                return cachedTyped;
            }

            // Завантажуємо бандл, якщо потрібно
            AssetBundle bundle = await LoadBundleAsync(bundlePath);
            if (bundle == null)
                return null;

            // Завантажуємо ассет
            AssetBundleRequest assetRequest = bundle.LoadAssetAsync<T>(assetName);
            await assetRequest;

            if (assetRequest.asset == null)
            {
                LogError($"Failed to load asset {assetName} from bundle {bundlePath}");
                return null;
            }

            T asset = assetRequest.asset as T;

            // Кешуємо ассет
            if (!_cachedAssets.TryGetValue(bundlePath, out assetsCache))
            {
                assetsCache = new Dictionary<string, UnityEngine.Object>();
                _cachedAssets[bundlePath] = assetsCache;
            }

            assetsCache[assetName] = asset;

            return asset;
        }

        public override async UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern)
        {
            // Формат pattern: "bundlePath:*" або "bundlePath:assetPattern*"
            if (!ParsePattern(pattern, out string bundlePath, out string assetPattern))
            {
                LogWarning($"Invalid asset bundle pattern format: {pattern}. Expected format: 'bundlePath:*' or 'bundlePath:assetPattern*'");
                return new List<T>();
            }

            AssetBundle bundle = await LoadBundleAsync(bundlePath);
            if (bundle == null)
                return new List<T>();

            string[] assetNames;
            if (assetPattern == "*")
            {
                assetNames = bundle.GetAllAssetNames();
            }
            else
            {
                assetNames = Array.FindAll(bundle.GetAllAssetNames(), name =>
                    name.Contains(assetPattern.TrimEnd('*')));
            }

            List<T> results = new List<T>();
            foreach (string assetName in assetNames)
            {
                T asset = await LoadAsync<T>($"{bundlePath}:{assetName}");
                if (asset != null)
                    results.Add(asset);
            }

            return results;
        }

        public override void Unload(string key)
        {
            if (!ParseKey(key, out string bundlePath, out string assetName))
                return;

            if (_cachedAssets.TryGetValue(bundlePath, out var assetsCache))
            {
                assetsCache.Remove(assetName);
                LogInfo($"Unloaded asset {assetName} from bundle {bundlePath}");
            }
        }

        public override void UnloadAll()
        {
            foreach (var bundle in _loadedBundles.Values)
            {
                bundle.Unload(false); // false = зберігати завантажені ассети
            }

            _loadedBundles.Clear();
            _cachedAssets.Clear();

            LogInfo("Unloaded all asset bundles");
        }

        /// <summary>
        /// Асинхронно вивантажує бандл з повним прибиранням ресурсів
        /// </summary>
        public async UniTask UnloadBundleAsync(string bundlePath, bool unloadAllAssets = false)
        {
            if (!_loadedBundles.TryGetValue(bundlePath, out var bundle))
                return;

            await UniTask.RunOnThreadPool(() => {
                bundle.Unload(unloadAllAssets);
            });

            _loadedBundles.Remove(bundlePath);
            _cachedAssets.Remove(bundlePath);

            LogInfo($"Unloaded bundle {bundlePath}");
        }

        /// <summary>
        /// Очищує невикористовувані ассети з пам'яті
        /// </summary>
        public async UniTask CleanupUnusedAssetsAsync()
        {
            LogInfo("Cleaning up unused assets from asset bundles");

            AsyncOperation operation = UnityEngine.Resources.UnloadUnusedAssets();
            await operation;

            System.GC.Collect();

            LogInfo("Cleanup complete");
        }

        // Парсить ключ формату "bundlePath:assetName"
        private bool ParseKey(string key, out string bundlePath, out string assetName)
        {
            bundlePath = null;
            assetName = null;

            if (string.IsNullOrEmpty(key))
                return false;

            int separatorIndex = key.IndexOf(':');
            if (separatorIndex <= 0 || separatorIndex >= key.Length - 1)
                return false;

            bundlePath = key.Substring(0, separatorIndex);
            assetName = key.Substring(separatorIndex + 1);

            return true;
        }

        // Парсить патерн формату "bundlePath:pattern*"
        private bool ParsePattern(string pattern, out string bundlePath, out string assetPattern)
        {
            bundlePath = null;
            assetPattern = null;

            if (string.IsNullOrEmpty(pattern))
                return false;

            int separatorIndex = pattern.IndexOf(':');
            if (separatorIndex <= 0)
                return false;

            bundlePath = pattern.Substring(0, separatorIndex);
            assetPattern = separatorIndex < pattern.Length - 1 ? pattern.Substring(separatorIndex + 1) : "*";

            return true;
        }
    }
}
