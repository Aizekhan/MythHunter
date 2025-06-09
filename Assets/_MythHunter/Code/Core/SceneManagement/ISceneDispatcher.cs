using Cysharp.Threading.Tasks;

namespace MythHunter.Core.SceneManagement
{
    /// <summary>
    /// Інтерфейс для диспетчера сцен
    /// </summary>
    public interface ISceneDispatcher
    {
        UniTask LoadSceneAsync(string sceneName);
        UniTask LoadSceneAdditiveAsync(string sceneName);
        UniTask UnloadSceneAsync(string sceneName);
        UniTask LoadGameSceneAsync(string[] selectedHeroArchetypes);
        string GetActiveScene();
        bool IsSceneLoaded(string sceneName);

        // Метод для передачі даних між сценами
        void SetSceneData<T>(string key, T data);
        T GetSceneData<T>(string key, T defaultValue = default);

    }
}
