// Assets/_MythHunter/Code/UI/Views/LobbyView.cs
using UnityEngine;
using MythHunter.UI.Core;
using TMPro;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.UI.Presenters;
namespace MythHunter.UI.Views
{
    public class LobbyView : UIViewBase, ILobbyView
    {
        [Inject] private IMythLogger _logger;
        [Inject] private IDIContainer _container;
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
            else
            {
                _logger?.LogWarning("⚠️ _timerText is null в UpdateTimer!", "UI");
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
            ConnectToPresenter();
        }
        private void ConnectToPresenter()
        {
            try
            {
                _logger?.LogInfo("🔗 Починаємо зв'язування з LobbyPresenter...", "UI");

                if (_container == null)
                {
                    _logger?.LogError("❌ DIContainer не ін'єктовано в LobbyView!", "UI");
                    return;
                }

                var presenter = _container.Resolve<ILobbyPresenter>();
                if (presenter != null)
                {
                    _logger?.LogInfo("✅ LobbyPresenter знайдено, викликаємо Initialize(this)", "UI");
                    presenter.Initialize(this);
                    _logger?.LogInfo("🎯 LobbyView успішно зв'язано з LobbyPresenter!", "UI");
                }
                else
                {
                    _logger?.LogError("❌ LobbyPresenter не знайдено в контейнері!", "UI");
                }
            }
            catch (System.Exception ex)
            {
                _logger?.LogError($"❌ Помилка зв'язування presenter: {ex.Message}", "UI", ex);
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
