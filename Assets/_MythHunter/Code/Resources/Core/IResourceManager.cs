// Шлях: Assets/_MythHunter/Code/Resources/Core/IResourceManager.cs

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MythHunter.Resources.Core
{
    /// <summary>
    /// Інтерфейс єдиної точки доступу до ресурсів
    /// </summary>
    public interface IResourceManager
    {
        void RegisterProvider(IResourceProvider provider, int priority = 0);
        UniTask<T> LoadAsync<T>(string key) where T : UnityEngine.Object;
        UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern) where T : UnityEngine.Object;
        void Unload(string key);
        void UnloadAll();

        T GetFromPool<T>(string key) where T : UnityEngine.Object;
        void ReturnToPool<T>(string key, T instance) where T : UnityEngine.Object;

        UniTask<GameObject> InstantiateAsync(string key, Transform parent = null);
        UniTask InitializePoolAsync<T>(string key, int poolSize = 10) where T : UnityEngine.Object;
    }
}
