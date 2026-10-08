// Шлях: Assets/_MythHunter/Code/UI/Views/HeroCardSelectorView.cs
using MythHunter.UI.Navigation;
using MythHunter.UI.Presenters;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;

namespace MythHunter.UI.Views
{
    public class HeroCardSelectorView : NavigableViewBase
    {
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _backButton;

        private IHeroCardSelectorPresenter _presenter;

        [Inject]
        public void Construct(IHeroCardSelectorPresenter presenter)
        {
            _presenter = presenter;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnConfirmClicked()
        {
            _presenter.ConfirmHeroSelection();
        }

        private void OnBackClicked()
        {
            _presenter.GoBackToLobby();
        }

        public override async UniTask OnViewCreatedAsync(NavigationParameters parameters)
        {
            var heroId = parameters.GetValue<string>("ArchetypeId", null);
            if (!string.IsNullOrEmpty(heroId))
            {
                await _presenter.InitializeWithHero(heroId);
            }
        }

        public override async UniTask OnViewDestroyedAsync()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            _backButton.onClick.RemoveListener(OnBackClicked);
            await UniTask.CompletedTask;
        }
    }
}
