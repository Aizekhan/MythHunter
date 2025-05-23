// Assets/_MythHunter/Code/UI/Views/LobbyView.cs
using UnityEngine;
using MythHunter.UI.Core;
using TMPro;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.UI.Views
{
    public class LobbyView : UIViewBase, ILobbyView
    {
        [Inject] private IMythLogger _logger;

        [Header("Контейнери")]
        [SerializeField] private Transform _heroCardsContainer;
        [SerializeField] private Transform _selectedHeroesContainer;

        [Header("UI елементи")]
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _manaText;
        [SerializeField] private TMP_Text _errorText;

        [SerializeField] private GameObject[] _playerReadyIndicators;
        [SerializeField] private GameObject _gameStartingPanel;

        
        public Transform HeroCardsContainer => _heroCardsContainer;
        public Transform SelectedHeroesContainer => _selectedHeroesContainer;

        public void ShowError(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
            }
        }

        public void UpdateTimer(float current, float max)
        {
            if (_timerText != null)
            {
                _timerText.text = $"{Mathf.CeilToInt(current)} сек";
            }
        }

        public void UpdateMana(int current, int max)
        {
            if (_manaText != null)
            {
                _manaText.text = $"Мана: {current}/{max}";
            }
        }

        public void ShowPlayerStatus(int playerIndex, bool ready)
        {
            if (playerIndex < 0 || playerIndex >= _playerReadyIndicators.Length)
                return;

            _playerReadyIndicators[playerIndex].SetActive(ready);
        }

        public void ShowGameStartingMessage()
        {
            if (_gameStartingPanel != null)
            {
                _gameStartingPanel.SetActive(true);
            }
        }
        protected override void OnInitialized()
        {
            base.OnInitialized();

            // ✅ ДЕТАЛЬНЕ логування
            _logger?.LogInfo($"🎬 LobbyView ініціалізовано", "UI");
            _logger?.LogInfo($"HeroCardsContainer: {(_heroCardsContainer != null ? "✅ OK" : "❌ NULL")}", "UI");
            _logger?.LogInfo($"SelectedHeroesContainer: {(_selectedHeroesContainer != null ? "✅ OK" : "❌ NULL")}", "UI");

            if (_heroCardsContainer == null)
            {
                _logger?.LogError("⚠️ HeroCardsContainer не призначений в інспекторі!", "UI");
            }

            if (_selectedHeroesContainer == null)
            {
                _logger?.LogError("⚠️ SelectedHeroesContainer не призначений в інспекторі!", "UI");
            }
        }

        protected override void OnDestroy()
        {
            _logger?.LogInfo("🗑️ LobbyView.OnDestroy() викликано", "UI");

            // Виклик базового методу, якщо він є в UIViewBase
            base.OnDestroy();
        }

        // ✅ ДОДАЙ ТАКОЖ OnDisable для додаткової діагностики
        private void OnDisable()
        {
            _logger?.LogInfo("⏸️ LobbyView.OnDisable() викликано", "UI");
        }

        private void OnEnable()
        {
            _logger?.LogInfo("▶️ LobbyView.OnEnable() викликано", "UI");
        }

    }
}
