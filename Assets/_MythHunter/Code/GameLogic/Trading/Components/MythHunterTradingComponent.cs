using System.Collections.Generic;
using RPGFramework.Trading;

namespace MythHunter.GameLogic.Trading
{
    /// <summary>
    /// MythHunter-specific trading component.
    /// Adds game-specific functionality to the framework trading system.
    /// </summary>
    [System.Serializable]
    public struct MythHunterTradingComponent
    {
        /// <summary>Metadata for all known traders</summary>
        public Dictionary<string, TraderMetadata> TraderMetadata;

        /// <summary>Cache of traders by type for quick lookups</summary>
        private Dictionary<TraderType, List<string>> _tradersByType;

        /// <summary>List of trader IDs that favor this player</summary>
        public List<string> FavoredTraders;

        /// <summary>
        /// Factory method to create a new trading component.
        /// </summary>
        public static MythHunterTradingComponent Create()
        {
            return new MythHunterTradingComponent
            {
                TraderMetadata = new Dictionary<string, TraderMetadata>(),
                _tradersByType = new Dictionary<TraderType, List<string>>(),
                FavoredTraders = new List<string>()
            };
        }

        /// <summary>
        /// Register trader metadata.
        /// </summary>
        public void RegisterTrader(string traderId, TraderMetadata metadata, TraderType traderType)
        {
            TraderMetadata[traderId] = metadata;

            if (!_tradersByType.ContainsKey(traderType))
                _tradersByType[traderType] = new List<string>();

            if (!_tradersByType[traderType].Contains(traderId))
                _tradersByType[traderType].Add(traderId);
        }

        /// <summary>
        /// Get metadata for a trader.
        /// </summary>
        public TraderMetadata? GetMetadata(string traderId)
        {
            return TraderMetadata.TryGetValue(traderId, out var meta) ? meta : null;
        }

        /// <summary>
        /// Get all traders of a specific type.
        /// </summary>
        public List<string> GetTradersByType(TraderType traderType)
        {
            return _tradersByType.TryGetValue(traderType, out var traders)
                ? new List<string>(traders)
                : new List<string>();
        }

        /// <summary>
        /// Add a trader to the favored list.
        /// </summary>
        public void AddFavoredTrader(string traderId)
        {
            if (!FavoredTraders.Contains(traderId))
                FavoredTraders.Add(traderId);
        }

        /// <summary>
        /// Check if trader is favored.
        /// </summary>
        public bool IsFavoredTrader(string traderId)
        {
            return FavoredTraders.Contains(traderId);
        }

        /// <summary>
        /// Get price modifier for a trader (with any player bonuses).
        /// </summary>
        public float GetEffectivePrice(string traderId, float baseModifier, float playerDiscount = 1.0f)
        {
            float modifier = baseModifier * playerDiscount;

            // Favored traders give additional discount
            if (IsFavoredTrader(traderId))
                modifier *= 0.95f;

            return modifier;
        }
    }
}
