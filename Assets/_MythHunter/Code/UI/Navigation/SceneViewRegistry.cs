// Assets/_MythHunter/Code/UI/Navigation/SceneViewRegistry.cs
using System.Collections.Generic;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Реєстр зв'язків між сценами та початковими ViewId
    /// </summary>
    public interface ISceneViewRegistry
    {
        void RegisterSceneView(string sceneName, ViewId viewId);
        ViewId GetViewIdForScene(string sceneName);
        bool HasViewForScene(string sceneName);
        void UnregisterScene(string sceneName);
    }

    public class SceneViewRegistry : ISceneViewRegistry
    {
        private readonly Dictionary<string, ViewId> _sceneToViewMap = new();
        private readonly IMythLogger _logger;

        [Inject]
        public SceneViewRegistry(IMythLogger logger)
        {
            _logger = logger;
            RegisterDefaultMappings();
        }

        private void RegisterDefaultMappings()
        {
            // Базові зв'язки - можна перевизначити пізніше
            _sceneToViewMap["LobbyScene"] = ViewId.Lobby;
            _sceneToViewMap["GameScene"] = ViewId.GameplayUI;
            _sceneToViewMap["MainMenuScene"] = ViewId.MainMenu;
            _sceneToViewMap["LoadingScene"] = ViewId.LoadingScreen;

            _logger.LogInfo($"Зареєстровано {_sceneToViewMap.Count} стандартних зв'язків сцена-ViewId", "SceneViewRegistry");
        }

        public void RegisterSceneView(string sceneName, ViewId viewId)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                _logger.LogWarning("Спроба реєстрації порожньої назви сцени", "SceneViewRegistry");
                return;
            }

            _sceneToViewMap[sceneName] = viewId;
            _logger.LogInfo($"Зареєстровано зв'язок: {sceneName} -> {viewId}", "SceneViewRegistry");
        }

        public ViewId GetViewIdForScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return ViewId.None;

            if (_sceneToViewMap.TryGetValue(sceneName, out var viewId))
            {
                return viewId;
            }

            _logger.LogInfo($"Не знайдено ViewId для сцени '{sceneName}', повертаємо ViewId.None", "SceneViewRegistry");
            return ViewId.None;
        }

        public bool HasViewForScene(string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) && _sceneToViewMap.ContainsKey(sceneName);
        }

        public void UnregisterScene(string sceneName)
        {
            if (_sceneToViewMap.Remove(sceneName))
            {
                _logger.LogInfo($"Видалено зв'язок для сцени: {sceneName}", "SceneViewRegistry");
            }
        }
    }
}
