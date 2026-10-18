using RPGFramework.Trading;
using System.Collections.Generic;
using UnityEngine;

namespace MythHunter.GameLogic.Trading
{
    /// <summary>
    /// Integration adapter between framework trading system and MythHunter game logic.
    /// Handles game-specific trading operations and constraints.
    /// </summary>
    public class TradingSystemIntegration
    {
        private readonly ITradingManager _tradingManager;
        private readonly ITraderRegistry _traderRegistry;
        private readonly MythHunterTradingComponent _tradingComponent;

        public TradingSystemIntegration(ITradingManager tradingManager, ITraderRegistry traderRegistry)
        {
            _tradingManager = tradingManager;
            _traderRegistry = traderRegistry;
            _tradingComponent = new MythHunterTradingComponent();
        }

        /// <summary>
        /// Propose a trade with gold cost.
        /// </summary>
        public bool ProposeTradeWithGold(int playerEntityId, string offerId)
        {
            return _tradingManager.ProposeTradeOffer(playerEntityId, offerId);
        }

        /// <summary>
        /// Propose a barter-only trade (no gold involved).
        /// </summary>
        public bool ProposeTradeBarterOnly(int playerEntityId, string offerId)
        {
            var offer = GetOfferFromRegistry(offerId);
            if (offer == null || offer.GoldAmount != 0)
                return false;

            return _tradingManager.ProposeTradeOffer(playerEntityId, offerId);
        }

        /// <summary>
        /// Accept a proposed trade and handle all consequences.
        /// </summary>
        public TradeResultData AcceptTrade(int playerEntityId, string offerId)
        {
            var result = _tradingManager.AcceptTrade(playerEntityId, offerId);

            if (result.Success)
            {
                LogTrade(playerEntityId, offerId, result);
                RaiseTradeCompletedEvent(playerEntityId, offerId, result);
            }

            return result;
        }

        /// <summary>
        /// Get all available trades from a specific trader.
        /// </summary>
        public IReadOnlyList<ITradeOffer> GetTraderOffers(string traderId, int playerEntityId)
        {
            return _tradingManager.GetTraderOffers(traderId, playerEntityId);
        }

        /// <summary>
        /// Get trader by ID with metadata.
        /// </summary>
        public ITrader? GetTrader(string traderId)
        {
            return _traderRegistry.GetTrader(traderId);
        }

        /// <summary>
        /// Get trader metadata.
        /// </summary>
        public TraderMetadata? GetTraderMetadata(string traderId)
        {
            return _tradingComponent.GetMetadata(traderId);
        }

        /// <summary>
        /// Get all traders of a specific type.
        /// </summary>
        public IReadOnlyList<ITrader> GetTradersByType(TraderType traderType)
        {
            return _traderRegistry.GetTradersByType(traderType);
        }

        /// <summary>
        /// Get player's current gold.
        /// </summary>
        public long GetPlayerGold(int playerEntityId)
        {
            return _tradingManager.GetPlayerGold(playerEntityId);
        }

        /// <summary>
        /// Get player's reputation with a trader.
        /// </summary>
        public int GetPlayerReputation(int playerEntityId, string traderId)
        {
            return _tradingManager.GetPlayerReputation(playerEntityId, traderId);
        }

        /// <summary>
        /// Set player reputation (admin command).
        /// </summary>
        public void SetPlayerReputation(int playerEntityId, string traderId, int reputation)
        {
            _tradingManager.SetPlayerReputation(playerEntityId, traderId, reputation);
        }

        /// <summary>
        /// Check if player can afford a trade.
        /// </summary>
        public bool CanAffordTrade(int playerEntityId, string offerId)
        {
            var offer = GetOfferFromRegistry(offerId);
            if (offer == null)
                return false;

            long goldCost = offer.GoldAmount < 0 ? -offer.GoldAmount : 0;
            long playerGold = _tradingManager.GetPlayerGold(playerEntityId);

            return playerGold >= goldCost;
        }

        /// <summary>
        /// Check if player meets all conditions for a trade.
        /// </summary>
        public bool MeetsTradeConditions(int playerEntityId, string offerId)
        {
            var offer = GetOfferFromRegistry(offerId);
            if (offer == null)
                return false;

            // Check reputation conditions
            foreach (var condition in offer.Conditions)
            {
                if (condition.Type == ConditionType.ReputationLevel)
                {
                    int playerRep = _tradingManager.GetPlayerReputation(playerEntityId, condition.ParameterId);
                    if (playerRep < condition.Value)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Register trader with metadata.
        /// </summary>
        public void RegisterTrader(ITrader trader, TraderMetadata metadata)
        {
            _traderRegistry.RegisterTrader(trader);
            _tradingComponent.RegisterTrader(trader.TraderId, metadata, trader.TraderType);
        }

        /// <summary>
        /// Mark a trader as favored (player reputation bonus).
        /// </summary>
        public void MarkTraderFavored(string traderId)
        {
            _tradingComponent.AddFavoredTrader(traderId);
        }

        /// <summary>
        /// Get effective price modifier for a trade.
        /// </summary>
        public float GetEffectivePriceModifier(string traderId, float baseModifier)
        {
            return _tradingComponent.GetEffectivePrice(traderId, baseModifier);
        }

        /// <summary>
        /// Decline a proposed trade.
        /// </summary>
        public void DeclineTrade(int playerEntityId, string offerId)
        {
            _tradingManager.DeclineTrade(playerEntityId, offerId);
            RaiseTradeDeclinedEvent(playerEntityId, offerId);
        }

        /// <summary>
        /// Get trade history count for an offer.
        /// </summary>
        public int GetTradeUseCount(int playerEntityId, string offerId)
        {
            return _tradingManager.GetTradeUseCount(playerEntityId, offerId);
        }

        /// <summary>
        /// Initialize player with starting gold.
        /// </summary>
        public void InitializePlayerGold(int playerEntityId, long goldAmount)
        {
            if (_tradingManager is TradingManager tm)
                tm.InitializePlayerGold(playerEntityId, goldAmount);
        }

        // ==================== Private Helper Methods ====================

        private ITradeOffer? GetOfferFromRegistry(string offerId)
        {
            foreach (var trader in _traderRegistry.GetAllTraders())
            {
                var offer = trader.GetOffer(offerId);
                if (offer != null)
                    return offer;
            }
            return null;
        }

        private void LogTrade(int playerEntityId, string offerId, TradeResultData result)
        {
            Debug.Log($"[Trade] Player {playerEntityId} completed trade {offerId}. " +
                     $"Gold change: {result.GoldChange}, Items received: {result.ItemsReceived.Count}");
        }

        private void RaiseTradeCompletedEvent(int playerEntityId, string offerId, TradeResultData result)
        {
            // Framework does not have built-in event bus, so game layer handles this
            var completedEvent = new TradeCompletedEvent
            {
                PlayerEntityId = playerEntityId,
                TraderId = GetOfferFromRegistry(offerId)?.TraderId ?? "",
                TradeOfferId = offerId,
                TotalValue = result.GoldChange,
                Timestamp = result.Timestamp
            };
            // Game layer would publish this event
        }

        private void RaiseTradeDeclinedEvent(int playerEntityId, string offerId)
        {
            var declinedEvent = new TradeDeclinedEvent
            {
                PlayerEntityId = playerEntityId,
                TraderId = GetOfferFromRegistry(offerId)?.TraderId ?? "",
                TradeOfferId = offerId,
                Timestamp = Time.time
            };
            // Game layer would publish this event
        }
    }
}
