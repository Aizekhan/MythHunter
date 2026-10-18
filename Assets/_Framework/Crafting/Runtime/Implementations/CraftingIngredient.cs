namespace RPGFramework.Crafting
{
    /// <summary>
    /// Default implementation of ICraftingIngredient.
    /// </summary>
    public class CraftingIngredient : ICraftingIngredient
    {
        /// <inheritdoc/>
        public string ItemId { get; }

        /// <inheritdoc/>
        public int Quantity { get; }

        /// <summary>
        /// Create a new crafting ingredient.
        /// </summary>
        public CraftingIngredient(string itemId, int quantity)
        {
            if (string.IsNullOrEmpty(itemId))
                throw new System.ArgumentException("Item ID cannot be null or empty", nameof(itemId));
            if (quantity <= 0)
                throw new System.ArgumentException("Quantity must be greater than zero", nameof(quantity));

            ItemId = itemId;
            Quantity = quantity;
        }

        /// <summary>
        /// Create from serializable data.
        /// </summary>
        public static CraftingIngredient FromData(CraftingIngredient data)
        {
            return new CraftingIngredient(data.ItemId, data.Quantity);
        }
    }
}
