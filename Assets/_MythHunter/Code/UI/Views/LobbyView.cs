// Assets/_MythHunter/Code/UI/Views/LobbyView.cs
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.UI.Models;
using MythHunter.UI.Presenters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// Реалізація представлення лоббі на основі MonoBehaviour
    /// </summary>
    public class LobbyView : MonoBehaviour, ILobbyView
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

        [Inject]
        public void Construct(ILobbyPresenter presenter)
        {
            _presenter = presenter;
            _presenter.Initialize(this);
        }

        private void Start()
        {
            // Ініціалізуємо лоббі для 2 гравців
            _presenter.StartLobby(2);

            // Налаштовуємо слухачі подій
            _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);

            // Приховуємо повідомлення про помилку та початок гри
            _errorText.gameObject.SetActive(false);
            _gameStartingText.gameObject.SetActive(false);

            // Налаштовуємо тексти статусу гравців
            for (int i = 0; i < _playerStatusTexts.Length; i++)
            {
                _playerStatusTexts[i].text = $"Гравець {i + 1}: Очікує вибору";
            }
        }

        private void OnDestroy()
        {
            // Відписуємося від подій
            _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);

            // Очищуємо контейнери
            ClearContainer(_heroCardsContainer);
            ClearContainer(_selectedHeroesContainer);
        }

        public void PopulateHeroCards(List<HeroCardModel> heroes)
        {
            // Очищуємо контейнер
            ClearContainer(_heroCardsContainer);

            // Створюємо картки героїв
            foreach (var hero in heroes)
            {
                var heroCard = Instantiate(_heroCardPrefab, _heroCardsContainer);
                var heroCardUI = heroCard.GetComponent<HeroCardUI>();

                if (heroCardUI != null)
                {
                    heroCardUI.Setup(hero);
                    heroCardUI.OnHeroSelected += OnHeroCardSelected;
                }
            }
        }

        public void UpdateSelectedHeroes(List<HeroCardModel> selectedHeroes)
        {
            // Очищуємо контейнер
            ClearContainer(_selectedHeroesContainer);

            // Створюємо картки вибраних героїв
            foreach (var hero in selectedHeroes)
            {
                var heroCard = Instantiate(_heroCardPrefab, _selectedHeroesContainer);
                var heroCardUI = heroCard.GetComponent<HeroCardUI>();

                if (heroCardUI != null)
                {
                    heroCardUI.Setup(hero);
                    // Вибрані герої не можуть бути вибрані повторно
                    heroCardUI.SetInteractable(false);
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

            // Приховуємо повідомлення через 3 секунди
            Invoke(nameof(HideError), 3f);
        }

        public void ShowPlayerStatus(int playerIndex, bool isReady)
        {
            if (playerIndex >= 0 && playerIndex < _playerStatusTexts.Length)
            {
                _playerStatusTexts[playerIndex].text = isReady
                    ? $"Гравець {playerIndex + 1}: Готовий"
                    : $"Гравець {playerIndex + 1}: Вибирає";

                // Змінюємо колір тексту
                _playerStatusTexts[playerIndex].color = isReady ? Color.green : Color.white;
            }
        }

        public void ShowGameStartingMessage()
        {
            _gameStartingText.gameObject.SetActive(true);

            // Приховуємо інші елементи
            _heroCardsContainer.gameObject.SetActive(false);
            _selectedHeroesContainer.gameObject.SetActive(false);
            _confirmButton.gameObject.SetActive(false);
            _startGameButton.gameObject.SetActive(false);
        }

        private void OnConfirmButtonClicked()
        {
            _presenter.OnSelectionConfirmed();
        }

        private async void OnStartGameButtonClicked()
        {
            await _presenter.StartGameAsync();
        }

        private void OnHeroCardSelected(string archetypeId)
        {
            _presenter.OnHeroSelected(archetypeId);
        }

        private void HideError()
        {
            _errorText.gameObject.SetActive(false);
        }

        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
