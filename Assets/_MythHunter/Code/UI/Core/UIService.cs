// Assets/_MythHunter/Code/UI/Core/UIService.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;
using System;
using System.Linq;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Реалізація високорівневого сервісу для управління UI екранами
    /// </summary>
    public class UIService : IUIService
    {
        private readonly IUISystem _uiSystem;
        private readonly IViewConfigRegistry _viewConfigRegistry;
        private readonly IMythLogger _logger;

        [Inject]
        public UIService(IUISystem uiSystem, IViewConfigRegistry viewConfigRegistry, IMythLogger logger)
        {
            _uiSystem = uiSystem;
            _viewConfigRegistry = viewConfigRegistry;
            _logger = logger;

            _logger.LogInfo("Сервіс ініціалізовано", "UIService");
        }

        public async UniTask<TView> ShowScreenAsync<TView>(string screenId) where TView : Component, IView
        {
            _logger.LogInfo($"ShowScreenAsync<{typeof(TView).Name}>({screenId}) викликано", "UIService");

            try
            {
                var configs = _viewConfigRegistry.GetAll();
                _logger.LogInfo($"Знайдено {configs.Count} конфігурацій: {string.Join(", ", configs.Select(c => c.ViewId))}", "UIService");

                var config = _viewConfigRegistry.Get(screenId);
                if (config == null)
                {
                    _logger.LogError($"Не знайдено конфігурації з ID: {screenId}", "UIService");

                    // Варіант запасу: використовувати шлях безпосередньо
                    string hardcodedPath = $"UI/{screenId}/{screenId}View";
                    _logger.LogWarning($"Спроба використати хардкод шлях: {hardcodedPath}", "UIService");

                    var view = await _uiSystem.ShowViewAsync<TView>(hardcodedPath);
                    _logger.LogInfo($"Екран {screenId} показано (через хардкод шлях)", "UIService");
                    return view;
                }

                _logger.LogInfo($"Знайдено конфігурацію для {screenId}, шлях: {config.PrefabPath}", "UIService");
                var viewComponent = await _uiSystem.ShowViewAsync<TView>(config.PrefabPath);

                if (viewComponent == null)
                {
                    _logger.LogError($"Не вдалося створити екран {screenId}", "UIService");
                    return null;
                }

                _logger.LogInfo($"Екран {screenId} показано успішно", "UIService");
                return viewComponent;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при показі екрана {screenId}: {ex.Message}", "UIService");
                return null;
            }
        }

        public void HideScreen<TView>() where TView : Component, IView
        {
            _logger.LogInfo($"HideScreen<{typeof(TView).Name}> викликано", "UIService");
            _uiSystem.HideView<TView>();
            _logger.LogInfo($"Екран {typeof(TView).Name} приховано", "UIService");
        }

        public bool IsScreenActive<TView>() where TView : Component, IView
        {
            return _uiSystem.IsViewActive<TView>();
        }
    }
}
