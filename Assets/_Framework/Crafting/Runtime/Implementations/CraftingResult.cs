namespace RPGFramework.Crafting
{
    /// <summary>
    /// Default implementation of ICraftingResult.
    /// </summary>
    public class CraftingResult : ICraftingResult
    {
        /// <inheritdoc/>
        public string ItemId { get; }

        /// <inheritdoc/>
        public int Quantity { get; }

        /// <inheritdoc/>
        public float SuccessChance { get; }

        /// <summary>
        /// Create a new crafting result.
        /// </summary>
        public CraftingResult(string itemId, int quantity, float successChance = 100f)
        {
            if (string.IsNullOrEmpty(itemId))
                throw new System.ArgumentException("Item ID cannot be null or empty", nameof(itemId));
            if (quantity <= 0)
                throw new System.ArgumentException("Quantity must be greater than zero", nameof(quantity));
            if (successChance < 0 || successChance > 100)
                throw new System.ArgumentException("Success chance must be between 0 and 100", nameof(successChance));

            ItemId = itemId;
            Quantity = quantity;
            SuccessChance = successChance;
        }

        /// <summary>
        /// Create from serializable data.
        /// </summary>
        public static CraftingResult FromData(CraftingResult data)
        {
            return new CraftingResult(data.ItemId, data.Quantity, data.SuccessChance);
        }
    }
}
