using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MythHunter.Resources.SceneManagement
{
    public class SceneLoader : ISceneLoader
    {
        private readonly IMythLogger _logger;

        [MythHunter.Core.DI.Inject]
        public SceneLoader(IMythLogger logger)
        {
            _logger = logger;
        }

        public async UniTask LoadSceneAsync(string sceneName, bool showLoadingScreen = true)
        {
            _logger.LogInfo($"Loading scene: {sceneName}", "Scene");

            if (showLoadingScreen)
            {
                // TODO: Показати екран завантаження
            }

            await SceneManager.LoadSceneAsync(sceneName);

            _logger.LogInfo($"Scene loaded: {sceneName}", "Scene");
        }

        public async UniTask LoadSceneAdditiveAsync(string sceneName)
        {
            _logger.LogInfo($"Loading scene additively: {sceneName}", "Scene");

            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            await operation;

            _logger.LogInfo($"Scene loaded additively: {sceneName}", "Scene");
        }

        public async UniTask UnloadSceneAsync(string sceneName)
        {
            _logger.LogInfo($"Unloading scene: {sceneName}", "Scene");

            await SceneManager.UnloadSceneAsync(sceneName);

            _logger.LogInfo($"Scene unloaded: {sceneName}", "Scene");
        }

        public string GetActiveScene()
        {
            return SceneManager.GetActiveScene().name;
        }

        public bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
