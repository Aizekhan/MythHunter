// Шлях: Assets/_MythHunter/Code/Resources/Core/IResourceProvider.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MythHunter.Resources.Core
{
    /// <summary>
    /// Базовий інтерфейс для всіх провайдерів ресурсів
    /// </summary>
    public interface IResourceProvider
    {
        UniTask<T> LoadAsync<T>(string key) where T : UnityEngine.Object;
        UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern) where T : UnityEngine.Object;
        void Unload(string key);
        void UnloadAll();
    }
}
