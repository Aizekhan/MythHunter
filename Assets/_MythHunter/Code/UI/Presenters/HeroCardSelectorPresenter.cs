// Шлях: Assets/_MythHunter/Code/UI/Presenters/HeroCardSelectorPresenter.cs
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Systems.Lobby;
using MythHunter.UI.Navigation;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Presenters
{
    public class HeroCardSelectorPresenter : IHeroCardSelectorPresenter
    {
        private readonly INavigationService _navigationService;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly ILobbySystem _lobbySystem;

        private string _archetypeId;

        [Inject]
        public HeroCardSelectorPresenter(
            INavigationService navigationService,
            IEventBus eventBus,
            IMythLogger logger,
            ILobbySystem lobbySystem)
        {
            _navigationService = navigationService;
            _eventBus = eventBus;
            _logger = logger;
            _lobbySystem = lobbySystem;
        }

        public async UniTask InitializeWithHero(string archetypeId)
        {
            _archetypeId = archetypeId;
            _logger.LogInfo($"HeroCardSelectorPresenter initialized with {_archetypeId}", "HeroSelector");
            await UniTask.CompletedTask;
        }
        public async UniTask InitializeAsync()
        {
            await UniTask.CompletedTask;
        }
        public void ConfirmHeroSelection()
        {
            if (!_lobbySystem.SelectHero(_archetypeId))
            {
                _logger.LogWarning($"Не вдалося вибрати героя: {_archetypeId}", "HeroSelector");
                return;
            }
            _logger.LogInfo($"Confirmed hero {_archetypeId}", "HeroSelector");
            _navigationService.GoBackAsync();
        }

        public void GoBackToLobby()
        {
            _navigationService.GoBackAsync();
        }

        public void Dispose()
        {
        }
    }
}
