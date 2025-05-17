// Шлях: Assets/_MythHunter/Code/UI/Views/LobbyView.cs
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.UI.Models;
using MythHunter.UI.Presenters;
using MythHunter.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MythHunter.Services.GameSettings;
using MythHunter.Events;
using MythHunter.Core.Game;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// Перенесена реалізація LobbyView в архітектурну систему з підтримкою IView
    /// </summary>
    public class LobbyView : UIViewBase, ILobbyView
    {
        [SerializeField] private Transform _heroCardsContainer;
        [SerializeField] private Transform _selectedHeroesContainer;
        [SerializeField] private GameObject _heroCardPrefab;
        [SerializeField] private TextMeshProUGUI _manaText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private TextMeshProUGUI _gameStartingText;
        [SerializeField] private TextMeshProUGUI[] _playerStatusTexts;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _startGameButton;

        private ILobbyPresenter _presenter;
        private IGameSettingsService _settings;
      

     
        [Inject]
        public void Construct(ILobbyPresenter presenter, IGameSettingsService settings, IUISystem uiSystem)
        {
            _presenter = presenter;
            _settings = settings;

            // Реєструємо View в UISystem
            uiSystem.RegisterView(this);

            // Ініціалізуємо презентер з представленням
            _presenter.Initialize(this);
            InitializeView();
        }

        private void InitializeView()
        {
            _presenter.StartLobby(_settings.PlayerCount);

            _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
            _errorText.gameObject.SetActive(false);
            _gameStartingText.gameObject.SetActive(false);

            for (int i = 0; i < _playerStatusTexts.Length; i++)
            {
                _playerStatusTexts[i].text = $"Гравець {i + 1}: Очікує вибору";
            }
        }


        // У методі OnDestroy додаємо відписку
        private void OnDestroy()
        {
            // Перевіряємо чи _presenter - це IEventSubscriber і відписуємось
            if (_presenter != null && _presenter is IEventSubscriber eventSubscriber)
            {
                eventSubscriber.UnsubscribeFromEvents();
            }

            _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
            ClearContainer(_heroCardsContainer);
            ClearContainer(_selectedHeroesContainer);

            // Відписка від UISystem
            if (FindFirstObjectByType<GameBootstrapper>() != null)
            {
                var container = FindFirstObjectByType<GameBootstrapper>().GetContainer();
                if (container != null && container.IsRegistered<IUISystem>())
                {
                    var uiSystem = container.Resolve<IUISystem>();
                    uiSystem.UnregisterView(this);
                }
            }
        }

        public void PopulateHeroCards(List<HeroCardModel> heroes)
        {
            ClearContainer(_heroCardsContainer);
            foreach (var hero in heroes)
            {
                var card = Instantiate(_heroCardPrefab, _heroCardsContainer);
                
                var ui = card.GetComponent<HeroCardUI>();
                if (ui != null)
                {
                    ui.Setup(hero);
                    ui.OnHeroSelected += OnHeroCardSelected;
                }
            }
        }

        public void UpdateSelectedHeroes(List<HeroCardModel> selectedHeroes)
        {
            ClearContainer(_selectedHeroesContainer);
            foreach (var hero in selectedHeroes)
            {
                var card = Instantiate(_heroCardPrefab, _selectedHeroesContainer);
                var ui = card.GetComponent<HeroCardUI>();
                if (ui != null)
                {
                    ui.Setup(hero);
                    ui.SetInteractable(false);
                }
            }
        }

        public void UpdateMana(int remainingMana, int totalMana)
        {
            _manaText.text = $"Мана: {remainingMana}/{totalMana}";
        }

        public void UpdateTimer(float remainingTime, float totalTime)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            _timerText.text = $"Час: {minutes:00}:{seconds:00}";
        }

        public void ShowError(string message)
        {
            _errorText.text = message;
            _errorText.gameObject.SetActive(true);
            Invoke(nameof(HideError), 3f);
        }

        public void ShowPlayerStatus(int index, bool isReady)
        {
            if (index < _playerStatusTexts.Length)
            {
                _playerStatusTexts[index].text = isReady ? $"Гравець {index + 1}: Готовий" : $"Гравець {index + 1}: Вибирає";
                _playerStatusTexts[index].color = isReady ? Color.green : Color.white;
            }
        }

        public void ShowGameStartingMessage()
        {
            _gameStartingText.gameObject.SetActive(true);
            _heroCardsContainer.gameObject.SetActive(false);
            _selectedHeroesContainer.gameObject.SetActive(false);
            _confirmButton.gameObject.SetActive(false);
            _startGameButton.gameObject.SetActive(false);
        }

        public override void Show() => gameObject.SetActive(true);
        public override void Hide() => gameObject.SetActive(false);

        private void OnConfirmButtonClicked() => _presenter.OnSelectionConfirmed();
        private async void OnStartGameButtonClicked() => await _presenter.StartGameAsync();
        private void OnHeroCardSelected(string archetypeId) => _presenter.OnHeroSelected(archetypeId);
        private void HideError() => _errorText.gameObject.SetActive(false);
        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
