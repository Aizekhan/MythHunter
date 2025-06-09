// Assets/_MythHunter/Code/UI/Views/LobbyView.cs
using UnityEngine;
using TMPro;
using MythHunter.UI.Core;
using MythHunter.UI.Models;
using System.Collections.Generic;
using MythHunter.Services.GameSettings;

namespace MythHunter.UI.Views
{
    public class LobbyView : MonoBehaviour, ILobbyView
    {
        [Header("Containers")]
        [SerializeField] private Transform _heroCardsContainer;
        [SerializeField] private Transform _p1SlotsContainer;
        [SerializeField] private Transform _p2SlotsContainer;

        [Header("Mana & Timer")]
        [SerializeField] private TextMeshProUGUI _manaText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Messages & Status")]
        [SerializeField] private GameObject _gameStartPanel;
        [SerializeField] private GameObject _waitingPanel;
        [SerializeField] private TextMeshProUGUI _waitingText;
        [SerializeField] private TextMeshProUGUI _aiStatusText;
        [SerializeField] private TextMeshProUGUI _currentPlayerLabel;

       

        [Header("Player Status")]
        [SerializeField] private GameObject[] _readyIndicators; // індекс за гравцем

        public Transform HeroCardsContainer => _heroCardsContainer;

        public Transform P1SlotsContainer => _p1SlotsContainer;
        public Transform P2SlotsContainer => _p2SlotsContainer;

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void UpdateMana(int remainingMana, int totalMana)
        {
            if (_manaText != null)
                _manaText.text = $"Мана: {remainingMana}/{totalMana}";
        }

        public void UpdateTimer(float remainingTime, float totalTime)
        {
            if (_timerText != null)
                _timerText.text = $"Час: {remainingTime:F1} / {totalTime:F1}";
        }

        public void ShowError(string message)
        {
            UnityEngine.Debug.LogWarning($"[LobbyView] ПОМИЛКА: {message}");
            // TODO: Можна додати pop-up UI
        }

        public void ShowPlayerStatus(int playerIndex, bool isReady)
        {
            if (_readyIndicators != null && playerIndex < _readyIndicators.Length)
                _readyIndicators[playerIndex].SetActive(isReady);
        }

        public void ShowGameStartingMessage()
        {
            if (_gameStartPanel != null)
                _gameStartPanel.SetActive(true);
        }

        public void ConfigureForGameMode(GameMode mode)
        {
            // TODO: додати логіку в залежності від GameMode
        }

        public void ShowAIStatus(string status, bool isThinking = false)
        {
            if (_aiStatusText != null)
                _aiStatusText.text = isThinking ? $"🤖 Думає... {status}" : status;
        }

        public void ShowCurrentPlayer(int playerIndex)
        {
            if (_currentPlayerLabel != null)
                _currentPlayerLabel.text = $"Хід гравця {playerIndex + 1}";
        }

        public void ShowWaitingForPlayer(bool show, string message = "Очікування іншого гравця...")
        {
            if (_waitingPanel != null)
                _waitingPanel.SetActive(show);

            if (_waitingText != null)
                _waitingText.text = message;
        }

        public void UpdatePageInfo(int currentPage, int totalPages)
        {
            UnityEngine.Debug.Log($"Сторінка: {currentPage}/{totalPages}");
            // TODO: Додати UI якщо потрібно
        }

        public void SetHeroInSlot(int playerIndex, int slotIndex, HeroCardModel hero)
        {
            Transform targetContainer = playerIndex == 0 ? _p1SlotsContainer : _p2SlotsContainer;
            if (targetContainer.childCount <= slotIndex)
                return;

            var slot = targetContainer.GetChild(slotIndex).GetComponent<HeroSlotUI>();
            if (slot != null)
                slot.SetHero(hero);
        }

        public void ClearSlot(int playerIndex, int slotIndex)
        {
            Transform targetContainer = playerIndex == 0 ? _p1SlotsContainer : _p2SlotsContainer;
            if (targetContainer.childCount <= slotIndex)
                return;

            var slot = targetContainer.GetChild(slotIndex).GetComponent<HeroSlotUI>();
            if (slot != null)
                slot.ClearHero();
        }


    }
}
