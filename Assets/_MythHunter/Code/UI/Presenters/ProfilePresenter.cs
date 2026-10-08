// Assets/_MythHunter/Code/UI/Presenters/ProfilePresenter.cs
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain.Profile;
using MythHunter.UI.Core;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;
using MythHunter.Core.Game;

namespace MythHunter.UI.Presenters
{
    public class ProfilePresenter : BasePresenter, IProfilePresenter
    {
        private readonly IGameFlowManager _gameFlowManager;
        private IProfileView _profileView => _view as IProfileView;

        [Inject]
        public ProfilePresenter(
            IEventBus eventBus,
            IMythLogger logger,
            IGameFlowManager gameFlowManager)
            : base(eventBus, logger)
        {
            _gameFlowManager = gameFlowManager;
            _viewId = ViewId.Profile;
        }
        public void Initialize(IProfileView view)
        {
            // Викликаємо базовий метод з правильним casting
            base.Initialize(view as IView, _viewId);

            // Запускаємо асинхронну ініціалізацію
            InitializeAsync().Forget();

            _logger.LogInfo("ProfilePresenter ініціалізовано з IProfileView", "Profile");
        }
        public override async UniTask InitializeAsync()
        {
            await base.InitializeAsync();
            _logger.LogInfo("ProfilePresenter ініціалізовано", "Profile");

            // Завантажуємо дані гравця
            await LoadPlayerDataAsync();
        }

        public void OnBackClicked()
        {
            _logger.LogInfo("👈 Повернення з профілю", "Profile");

            // Публікуємо подію закриття
            _eventBus.Publish(new ProfileClosedEvent
            {
                PlayerId = "LocalPlayer",
                Timestamp = DateTime.UtcNow
            });

            // Повертаємось до головного меню
            _gameFlowManager.ReturnToMainMenuAsync().Forget();
        }

        public void OnHeroesTabClicked()
        {
            _logger.LogInfo("🦸 Вкладка Героїв", "Profile");
            _profileView?.ShowHeroesTab();
            LoadHeroesAsync().Forget();
        }

        public void OnStatsTabClicked()
        {
            _logger.LogInfo("📊 Вкладка Статистики", "Profile");
            _profileView?.ShowStatsTab();
        }

        public void OnSettingsTabClicked()
        {
            _logger.LogInfo("⚙️ Вкладка Налаштувань", "Profile");
            _profileView?.ShowSettingsTab();
        }

        public void OnAchievementsTabClicked()
        {
            _logger.LogInfo("🏆 Вкладка Досягнень", "Profile");
            _profileView?.ShowAchievementsTab();
        }

        public async UniTask LoadPlayerDataAsync()
        {
            _profileView?.ShowLoadingState(true);

            try
            {
                // Поки що мок дані
                await UniTask.Delay(500); // Імітація завантаження

                _profileView?.SetPlayerName("Гравець");
                _profileView?.SetPlayerLevel(5);
                _profileView?.SetPlayerExp(750, 1000);

                _logger.LogInfo("✅ Дані гравця завантажено", "Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка завантаження даних: {ex.Message}", "Profile", ex);
                _profileView?.ShowError("Не вдалося завантажити дані профілю");
            }
            finally
            {
                _profileView?.ShowLoadingState(false);
            }
        }

        public async UniTask LoadHeroesAsync()
        {
            try
            {
                await UniTask.Delay(300);
                // TODO: Завантаження колекції героїв
                _logger.LogInfo("✅ Герої завантажено", "Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка завантаження героїв: {ex.Message}", "Profile", ex);
            }
        }

        public async UniTask SaveSettingsAsync()
        {
            try
            {
                await UniTask.Delay(200);
                // TODO: Збереження налаштувань
                _logger.LogInfo("✅ Налаштування збережено", "Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка збереження: {ex.Message}", "Profile", ex);
            }
        }
    }
}
