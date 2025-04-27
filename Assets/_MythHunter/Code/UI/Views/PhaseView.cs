// Файл: Assets/_MythHunter/Code/UI/Views/PhaseView.cs

using MythHunter.Events.Domain;
using MythHunter.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// Представлення фаз гри
    /// </summary>
    public class PhaseView : MonoBehaviour, IView
    {
        [Header("Загальні елементи")]
        [SerializeField] private GameObject _phasePanel;
        [SerializeField] private TextMeshProUGUI _phaseNameText;
        [SerializeField] private TextMeshProUGUI _phaseTimerText;
        [SerializeField] private Image _phaseTimerFill;
        [SerializeField] private Button _nextPhaseButton;

        [Header("Індикатори фаз")]
        [SerializeField] private GameObject _runePhaseIndicator;
        [SerializeField] private GameObject _planningPhaseIndicator;
        [SerializeField] private GameObject _movementPhaseIndicator;
        [SerializeField] private GameObject _combatPhaseIndicator;
        [SerializeField] private GameObject _freezePhaseIndicator;

        private GamePhase _currentPhase;
        private float _totalPhaseTime;

        public event System.Action OnNextPhaseRequested;

        private void Awake()
        {
            if (_nextPhaseButton != null)
            {
                _nextPhaseButton.onClick.AddListener(HandleNextPhaseClicked);
            }
        }

        private void OnDestroy()
        {
            if (_nextPhaseButton != null)
            {
                _nextPhaseButton.onClick.RemoveListener(HandleNextPhaseClicked);
            }
        }

        public void Show()
        {
            _phasePanel.SetActive(true);
        }

        public void Hide()
        {
            _phasePanel.SetActive(false);
        }

        private void HandleNextPhaseClicked()
        {
            OnNextPhaseRequested?.Invoke();
        }

        public void SetPhase(GamePhase phase, float duration)
        {
            _currentPhase = phase;
            _totalPhaseTime = duration;

            // Оновлення назви фази
            _phaseNameText.text = GetPhaseDisplayName(phase);

            // Оновлення індикаторів
            UpdatePhaseIndicators(phase);

            // Скидання таймера
            _phaseTimerFill.fillAmount = 1f;
            UpdateTimerText(duration);
        }

        public void UpdateTimer(float remainingTime)
        {
            if (_totalPhaseTime <= 0f)
                return;

            float progress = Mathf.Clamp01(remainingTime / _totalPhaseTime);
            _phaseTimerFill.fillAmount = progress;
            UpdateTimerText(remainingTime);
        }

        private void UpdateTimerText(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            _phaseTimerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void UpdatePhaseIndicators(GamePhase phase)
        {
            // Вимкнення всіх індикаторів
            if (_runePhaseIndicator)
                _runePhaseIndicator.SetActive(phase == GamePhase.Rune);
            if (_planningPhaseIndicator)
                _planningPhaseIndicator.SetActive(phase == GamePhase.Planning);
            if (_movementPhaseIndicator)
                _movementPhaseIndicator.SetActive(phase == GamePhase.Movement);
            if (_combatPhaseIndicator)
                _combatPhaseIndicator.SetActive(phase == GamePhase.Combat);
            if (_freezePhaseIndicator)
                _freezePhaseIndicator.SetActive(phase == GamePhase.Freeze);
        }

        private string GetPhaseDisplayName(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.Rune => "Фаза Рун",
                GamePhase.Planning => "Фаза Планування",
                GamePhase.Movement => "Фаза Руху",
                GamePhase.Combat => "Фаза Бою",
                GamePhase.Freeze => "Фаза Очікування",
                _ => "Невідома Фаза"
            };
        }
    }
}
