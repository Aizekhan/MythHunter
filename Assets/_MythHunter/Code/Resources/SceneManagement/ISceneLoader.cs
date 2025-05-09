using Cysharp.Threading.Tasks;

namespace MythHunter.Resources.SceneManagement
{
    /// <summary>
    /// Інтерфейс завантажувача сцен
    /// </summary>
    public interface ISceneLoader
    {
        UniTask LoadSceneAsync(string sceneName, bool showLoadingScreen = true);
        UniTask LoadSceneAdditiveAsync(string sceneName);
        UniTask UnloadSceneAsync(string sceneName);
        string GetActiveScene();
        bool IsSceneLoaded(string sceneName);
    }
}