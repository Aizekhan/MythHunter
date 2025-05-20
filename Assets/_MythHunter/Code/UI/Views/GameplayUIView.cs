// Assets/_MythHunter/Code/UI/Views/GameplayUIView.cs
using MythHunter.UI.Core;
using MythHunter.UI.Navigation;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MythHunter.UI.Views
{
    /// <summary>
    /// Представлення ігрового інтерфейсу
    /// </summary>
    public class GameplayUIView : NavigableViewBase, IGameplayUIView
    {
        [Header("UI елементи")]
        [SerializeField] private TextMeshProUGUI _phaseInfoText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private GameObject _runeValueContainer;
        [SerializeField] private TextMeshProUGUI _runeValueText;

        [Header("Панелі")]
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _heroStatsPanel;
        [SerializeField] private GameObject _objectivesPanel;

        // Імплементація IGameplayUIView
        public void UpdatePhaseInfo(int phase, float timeRemaining)
        {
            if (_phaseInfoText != null)
            {
                string phaseName = GetPhaseNameById(phase);
                _phaseInfoText.text = $"Фаза: {phaseName}";
            }

            if (_timerText != null)
            {
                _timerText.text = $"Час: {Mathf.Ceil(timeRemaining)}с";
            }
        }

        public void ShowRuneValue(int value)
        {
            if (_runeValueContainer != null)
                _runeValueContainer.SetActive(true);

            if (_runeValueText != null)
                _runeValueText.text = $"Руна: {value}";
        }

        public void HideRuneValue()
        {
            if (_runeValueContainer != null)
                _runeValueContainer.SetActive(false);
        }

        // Допоміжні методи
        private string GetPhaseNameById(int phaseId)
        {
            return phaseId switch
            {
                0 => "Немає",
                1 => "Руна",
                2 => "Планування",
                3 => "Активна",
                4 => "Завмирання",
                _ => $"Невідома ({phaseId})",
            };
        }

        // Імплементація INavigableView
        public override async UniTask OnViewCreatedAsync(NavigationParameters parameters)
        {
            // Ініціалізація при створенні
            await base.OnViewCreatedAsync(parameters);
        }

        public override async UniTask OnViewNavigatedToAsync(NavigationParameters parameters)
        {
            // Логіка при навігації до цього представлення
            await base.OnViewNavigatedToAsync(parameters);

            // Можливе отримання даних з параметрів
            if (parameters.Contains("SelectedHeroArchetypes"))
            {
                var selectedHeroes = parameters.GetValue<string[]>("SelectedHeroArchetypes");
                // Обробка обраних героїв...
            }
        }
    }
}
