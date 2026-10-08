// Шлях: Assets/_MythHunter/Code/Resources/IPreloadManager.cs
using MythHunter.Resources.Config;
using System;
namespace MythHunter.Resources
{
    /// <summary>
    /// Інтерфейс для менеджера прееміптивного завантаження ресурсів
    /// </summary>
    public interface IPreloadManager
    {
        void RegisterPhasePreload<T>(string phaseId, string resourceKey, int priority = 0, bool createPool = false, int poolSize = 10) where T : UnityEngine.Object;
        void RegisterPhasePreload(string phaseId, string resourceKey, Type resourceType, int priority = 0, bool createPool = false, int poolSize = 10, PreloadSceneConfig.LoadingMode loadingMode = PreloadSceneConfig.LoadingMode.SingleResource);
        void RegisterScenePreload<T>(string sceneName, string resourceKey, int priority = 0, bool createPool = false, int poolSize = 10, PreloadSceneConfig.LoadingMode loadingMode = PreloadSceneConfig.LoadingMode.SingleResource) where T : UnityEngine.Object;
        void RegisterScenePreload(string sceneName, string resourceKey, Type resourceType, int priority = 0, bool createPool = false, int poolSize = 10, PreloadSceneConfig.LoadingMode loadingMode = PreloadSceneConfig.LoadingMode.SingleResource);
    }
}
