// Шлях: Assets/_MythHunter/Code/Resources/IPreloadManager.cs
namespace MythHunter.Resources
{
    /// <summary>
    /// Інтерфейс для менеджера прееміптивного завантаження ресурсів
    /// </summary>
    public interface IPreloadManager
    {
        void RegisterPhasePreload<T>(Events.Domain.GamePhase phase, string resourceKey, int priority = 0, bool createPool = false, int poolSize = 10) where T : UnityEngine.Object;
        void RegisterPhasePreload(Events.Domain.GamePhase phase, string resourceKey, System.Type resourceType, int priority = 0, bool createPool = false, int poolSize = 10);
        void RegisterScenePreload<T>(string sceneName, string resourceKey, int priority = 0, bool createPool = false, int poolSize = 10) where T : UnityEngine.Object;
        void RegisterScenePreload(string sceneName, string resourceKey, System.Type resourceType, int priority = 0, bool createPool = false, int poolSize = 10);
        void SubscribeToEvents();
        void UnsubscribeFromEvents();
    }
}
