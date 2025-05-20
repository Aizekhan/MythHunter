// Шлях: Assets/_MythHunter/Code/UI/Core/UIService.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;
using System;
using MythHunter.UI.Core;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Реалізація високорівневого сервісу для управління UI екранами
    /// </summary>
    public class UIService : IUIService
    {
        private readonly IUISystem _uiSystem;
        private readonly IMythLogger _logger;

        [Inject]
        public UIService(IUISystem uiSystem, IMythLogger logger)
        {
            _uiSystem = uiSystem;
            _logger = logger;
        }

        public async UniTask<TView> ShowScreenAsync<TView>(string screenId)
            where TView : Component, IView
        {
            _logger.LogInfo($"Показ екрана {screenId} з типом {typeof(TView).Name}", "UIService");
            return await _uiSystem.ShowViewAsync<TView>(screenId);
        }

        public async UniTask<IView> ShowScreenAsync(Type viewType, string prefabPath)
        {
            var view = await _uiSystem.ShowScreenAsync(viewType, prefabPath);
            if (view == null)
                _logger.LogError($"Не вдалося показати View {viewType.Name}", "UIService");
            return view;
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
