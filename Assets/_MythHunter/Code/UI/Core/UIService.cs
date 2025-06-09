// Assets/_MythHunter/Code/UI/Core/UIService.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.UI.Core;
using MythHunter.Utils.Logging;

namespace MythHunter.UI.Runtime
{
    /// <summary>
    /// Спрощений UI сервіс - прямо працює з UIViewFactory
    /// </summary>
    public class UIService : IUIService
    {
        private readonly IUIViewFactory _viewFactory;
        private readonly IMythLogger _logger;

        [Inject]
        public UIService(IUIViewFactory viewFactory, IMythLogger logger)
        {
            _viewFactory = viewFactory;
            _logger = logger;
        }

        public async UniTask<IView> ShowScreenAsync(ViewId viewId)
        {
            _logger.LogInfo($"📺 Показуємо екран: {viewId}", "UIService");

            var view = await _viewFactory.CreateViewAsync(viewId);
            if (view == null)
            {
                _logger.LogError($"❌ Не вдалося створити екран {viewId}", "UIService");
                return null;
            }

            view.Show();
            _logger.LogInfo($"✅ Екран {viewId} показано", "UIService");
            return view;
        }

        public void HideScreen(ViewId viewId)
        {
            _logger.LogInfo($"🙈 Приховуємо екран: {viewId}", "UIService");
            // Тут можна додати логіку пошуку активного екрану і його приховування
            // Поки що спрощена версія
        }

        public bool IsScreenActive(ViewId viewId)
        {
            // Спрощена версія - завжди повертає false
            // Можна розширити при потребі
            return false;
        }
    }
}
