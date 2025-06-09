using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using MythHunter.Resources.SceneManagement;

namespace MythHunter.Core.SceneManagement
{
    /// <summary>
    /// Централізований диспетчер сцен з підтримкою передачі даних та інфраструктурних операцій
    /// </summary>
    public class SceneDispatcher : ISceneDispatcher
    {
        private readonly SceneLoader _sceneLoader;
        private readonly IMythLogger _logger;

        // Тимчасове сховище для передачі даних між сценами
        private static readonly Dictionary<string, object> _sceneData = new();

        [Inject]
        public SceneDispatcher(SceneLoader sceneLoader, IMythLogger logger)
        {
            _sceneLoader = sceneLoader;
            _logger = logger;
        }
        public async UniTask LoadGameSceneAsync(string[] selectedHeroArchetypes)
        {
            // Зберігаємо дані, які потрібні після переходу
            SetSceneData("SelectedHeroArchetypes", selectedHeroArchetypes);

            _logger.LogInfo($"[SceneDispatcher] Loading GameScene with {selectedHeroArchetypes?.Length ?? 0} selected heroes", "Scene");

            await _sceneLoader.LoadSceneAsync("GameScene"); // або назву іншої сцени
        }
        public async UniTask LoadSceneAsync(string sceneName)
        {
            _logger.LogInfo($"[SceneDispatcher] Loading scene: {sceneName}", "Scene");
            await _sceneLoader.LoadSceneAsync(sceneName);
        }

        public async UniTask LoadSceneAdditiveAsync(string sceneName)
        {
            _logger.LogInfo($"[SceneDispatcher] Loading additive scene: {sceneName}", "Scene");
            await _sceneLoader.LoadSceneAdditiveAsync(sceneName);
        }

        public async UniTask UnloadSceneAsync(string sceneName)
        {
            _logger.LogInfo($"[SceneDispatcher] Unloading scene: {sceneName}", "Scene");
            await _sceneLoader.UnloadSceneAsync(sceneName);
        }

        public string GetActiveScene() => _sceneLoader.GetActiveScene();

        public bool IsSceneLoaded(string sceneName) => _sceneLoader.IsSceneLoaded(sceneName);

        /// <summary>
        /// Передача даних у наступну сцену
        /// </summary>
        public void SetSceneData<T>(string key, T data)
        {
            _sceneData[key] = data;
        }

        /// <summary>
        /// Отримання переданих даних у новій сцені
        /// </summary>
        public T GetSceneData<T>(string key, T defaultValue = default)
        {
            if (_sceneData.TryGetValue(key, out var value) && value is T typed)
                return typed;
            return defaultValue;
        }
    }
}
