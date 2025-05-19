// Assets/_MythHunter/Code/UI/Views/LobbyView.cs

using MythHunter.Core.DI;
using MythHunter.UI.Core;
using MythHunter.UI.Presenters;
using MythHunter.Utils.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MythHunter.UI.Views
{
    public class LobbyView : UIViewBase, ILobbyView
    {
        [SerializeField] private Transform _heroCardsContainer;
        [SerializeField] private Transform _selectedHeroesContainer;
        [SerializeField] private TextMeshProUGUI _manaText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private TextMeshProUGUI _gameStartingText;
        [SerializeField] private TextMeshProUGUI[] _playerStatusTexts;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _startGameButton;

        private ILobbyPresenter _presenter;
        private IMythLogger _logger;

        // Властивості для контейнерів
        public Transform HeroCardsContainer => _heroCardsContainer;
        public Transform SelectedHeroesContainer => _selectedHeroesContainer;

        [Inject]
        public void Construct(ILobbyPresenter presenter, IMythLogger logger)
        {
            _presenter = presenter;
            _logger = logger;

            // Ініціалізація View через Presenter
            _presenter.Initialize(this);

            _logger.LogInfo("LobbyView сконструйовано через DI", "LobbyView");
        }

        protected override void Awake()
        {
            base.Awake();

            // Налаштування кнопок
            if (_confirmButton)
                _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            if (_startGameButton)
                _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Відписка від подій
            if (_confirmButton)
                _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            if (_startGameButton)
                _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);

            _logger.LogInfo("LobbyView знищено", "LobbyView");
        }

        // Прості методи оновлення UI
        public void UpdateMana(int remainingMana, int totalMana)
        {
            if (_manaText)
                _manaText.text = $"Мана: {remainingMana}/{totalMana}";
        }

        public void UpdateTimer(float remainingTime, float totalTime)
        {
            if (_timerText)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60);
                int seconds = Mathf.FloorToInt(remainingTime % 60);
                _timerText.text = $"Час: {minutes:00}:{seconds:00}";
            }
        }

        public void ShowError(string message)
        {
            if (_errorText)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
                Invoke(nameof(HideError), 3f);
            }
        }

        public void ShowPlayerStatus(int index, bool isReady)
        {
            if (_playerStatusTexts == null || _playerStatusTexts.Length <= index || _playerStatusTexts[index] == null)
                return;

            _playerStatusTexts[index].text = isReady
                ? $"Гравець {index + 1}: Готовий"
                : $"Гравець {index + 1}: Вибирає";
            _playerStatusTexts[index].color = isReady ? Color.green : Color.white;
        }

        public void ShowGameStartingMessage()
        {
            if (_gameStartingText)
                _gameStartingText.gameObject.SetActive(true);
            if (_heroCardsContainer)
                _heroCardsContainer.gameObject.SetActive(false);
            if (_selectedHeroesContainer)
                _selectedHeroesContainer.gameObject.SetActive(false);
            if (_confirmButton)
                _confirmButton.gameObject.SetActive(false);
            if (_startGameButton)
                _startGameButton.gameObject.SetActive(false);
        }

        // Обробники подій кнопок - делегують роботу Presenter
        private void OnConfirmButtonClicked()
        {
            _presenter.OnSelectionConfirmed();
        }

        private async void OnStartGameButtonClicked()
        {
            await _presenter.StartGameAsync();
        }

        private void HideError()
        {
            if (_errorText)
                _errorText.gameObject.SetActive(false);
        }
    }
}
