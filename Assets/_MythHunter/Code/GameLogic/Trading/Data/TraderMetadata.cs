namespace MythHunter.GameLogic.Trading
{
    /// <summary>
    /// Game-specific metadata for traders in MythHunter.
    /// Contains UI/presentation information and game-specific settings.
    /// </summary>
    [System.Serializable]
    public struct TraderMetadata
    {
        /// <summary>Display name of the trader (shown in UI)</summary>
        public string TraderName;

        /// <summary>Path to trader portrait sprite</summary>
        public string PortraitPath;

        /// <summary>Path to shop ambient music</summary>
        public string ShopMusicPath;

        /// <summary>Dialogue ID for trader greetings/responses</summary>
        public string DialogueId;

        /// <summary>Price modifier for items (1.0 = standard, 0.8 = 20% discount)</summary>
        public float PriceModifier;

        /// <summary>Reputation gain per successful trade</summary>
        public int ReputationGain;

        /// <summary>Whether trader is a fence (buys stolen goods)</summary>
        public bool IsFence;

        /// <summary>Whether trader requires faction alignment</summary>
        public bool RequiresFactionAlignment;

        /// <summary>Associated faction ID if faction alignment required</summary>
        public string FactionId;

        public TraderMetadata(string traderName, string portraitPath, string shopMusicPath,
            string dialogueId, float priceModifier = 1.0f, int reputationGain = 10)
        {
            TraderName = traderName;
            PortraitPath = portraitPath;
            ShopMusicPath = shopMusicPath;
            DialogueId = dialogueId;
            PriceModifier = priceModifier;
            ReputationGain = reputationGain;
            IsFence = false;
            RequiresFactionAlignment = false;
            FactionId = "";
        }

        /// <summary>
        /// Create metadata for a general merchant.
        /// </summary>
        public static TraderMetadata CreateMerchant(string traderName, string portraitPath,
            string dialogueId, float priceModifier = 1.0f)
        {
            return new TraderMetadata(
                traderName,
                portraitPath,
                "Audio/Ambient/ShopMerchant",
                dialogueId,
                priceModifier,
                reputationGain: 5
            );
        }

        /// <summary>
        /// Create metadata for a blacksmith.
        /// </summary>
        public static TraderMetadata CreateBlacksmith(string traderName, string portraitPath,
            string dialogueId)
        {
            return new TraderMetadata(
                traderName,
                portraitPath,
                "Audio/Ambient/ShopBlacksmith",
                dialogueId,
                priceModifier: 1.2f,
                reputationGain: 15
            );
        }

        /// <summary>
        /// Create metadata for a fence dealer.
        /// </summary>
        public static TraderMetadata CreateFenceDealer(string traderName, string portraitPath,
            string dialogueId)
        {
            var metadata = new TraderMetadata(
                traderName,
                portraitPath,
                "Audio/Ambient/ShopFence",
                dialogueId,
                priceModifier: 0.7f,
                reputationGain: 8
            );
            metadata.IsFence = true;
            return metadata;
        }

        /// <summary>
        /// Create metadata for an alchemist.
        /// </summary>
        public static TraderMetadata CreateAlchemist(string traderName, string portraitPath,
            string dialogueId)
        {
            return new TraderMetadata(
                traderName,
                portraitPath,
                "Audio/Ambient/ShopAlchemy",
                dialogueId,
                priceModifier: 1.1f,
                reputationGain: 10
            );
        }
    }
}
