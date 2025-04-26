using System.Threading.Tasks;

namespace MythHunter.Resources.SceneManagement
{
    /// <summary>
    /// Інтерфейс завантажувача сцен
    /// </summary>
    public interface ISceneLoader
    {
        Task LoadSceneAsync(string sceneName, bool showLoadingScreen = true);
        Task LoadSceneAdditiveAsync(string sceneName);
        void UnloadScene(string sceneName);
        string GetActiveScene();
        bool IsSceneLoaded(string sceneName);
    }
}