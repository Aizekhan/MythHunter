// Шлях: Assets/_MythHunter/Code/UI/Core/IUIComponentFactory.cs
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MythHunter.UI.Core
{
    public interface IUIComponentFactory
    {
        T CreateComponent<T>(GameObject prefab, Transform parent = null) where T : MonoBehaviour;
        UniTask<T> CreateComponentAsync<T>(string prefabPath, Transform parent = null) where T : MonoBehaviour;
        UniTask<T> CreateFromPoolAsync<T>(string prefabPath, Transform parent = null) where T : MonoBehaviour;
        void ReturnToPool<T>(T component) where T : MonoBehaviour;
    }
}
