// Assets/_MythHunter/Code/Core/SceneManagement/SceneDispatcher.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MythHunter.Core.SceneManagement
{
    /// <summary>
    /// Диспетчер сцен
    /// </summary>
    public class SceneDispatcher : ISceneDispatcher
    {
        private readonly IMythLogger _logger;

        // Тимчасове сховище для передачі даних між сценами
        private static readonly Dictionary<string, object> _sceneData = new Dictionary<string, object>();

        [Inject]
        public SceneDispatcher(IMythLogger logger)
        {
            _logger = logger;
        }

        public async UniTask LoadSceneAsync(string sceneName)
        {
            _logger.LogInfo($"Loading scene: {sceneName}", "SceneDispatcher");

            try
            {
                await SceneManager.LoadSceneAsync(sceneName);
                _logger.LogInfo($"Scene {sceneName} loaded successfully", "SceneDispatcher");
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Failed to load scene {sceneName}: {ex.Message}", "SceneDispatcher", ex);
                throw;
            }
        }

        public async UniTask LoadGameSceneAsync(string[] selectedHeroArchetypes)
        {
            // Зберігаємо дані про вибраних героїв
            _sceneData["SelectedHeroes"] = selectedHeroArchetypes;

            // Завантажуємо ігрову сцену
            await LoadSceneAsync("GameScene");
        }

        /// <summary>
        /// Отримує дані, передані між сценами
        /// </summary>
        public static T GetSceneData<T>(string key, T defaultValue = default)
        {
            if (_sceneData.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return defaultValue;
        }
    }
}
