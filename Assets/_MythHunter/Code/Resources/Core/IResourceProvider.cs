using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MythHunter.Resources.Core
{
    /// <summary>
    /// Інтерфейс провайдера ресурсів
    /// </summary>
    public interface IResourceProvider
    {
        Task<T> LoadAsync<T>(string key) where T : UnityEngine.Object;
        Task<IReadOnlyList<T>> LoadAllAsync<T>(string pattern) where T : UnityEngine.Object;
        void Unload(string key);
        void UnloadAll();
        T GetFromPool<T>(string key) where T : UnityEngine.Object;
        void ReturnToPool<T>(string key, T instance) where T : UnityEngine.Object;
    }
}