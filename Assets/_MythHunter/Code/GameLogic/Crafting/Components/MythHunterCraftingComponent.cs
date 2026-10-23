using System;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.Crafting;
using RPGFramework.ECS;

namespace MythHunter.GameLogic.Crafting
{
    /// <summary>
    /// MythHunter-specific crafting component that extends the framework's crafting capabilities.
    /// Stores game-specific metadata for recipes and manages recipe availability.
    /// </summary>
    [Serializable]
    public struct MythHunterCraftingComponent : IComponent
    {
        /// <summary>
        /// Dictionary mapping recipe IDs to their game-specific metadata.
        /// </summary>
        public Dictionary<string, CraftingRecipeMetadata> RecipeMetadata;

        /// <summary>
        /// Set of recipe IDs that this entity has learned.
        /// </summary>
        public HashSet<string> LearnedRecipeIds;

        /// <summary>
        /// Current crafting skill level (0-100 scale).
        /// </summary>
        public int CraftingSkillLevel;

        /// <summary>
        /// Total experience points earned from crafting.
        /// </summary>
        public int CraftingExperience;

        /// <summary>
        /// Initialize the component with default values.
        /// </summary>
        public void Initialize()
        {
            RecipeMetadata = new Dictionary<string, CraftingRecipeMetadata>();
            LearnedRecipeIds = new HashSet<string>();
            CraftingSkillLevel = 0;
            CraftingExperience = 0;
        }

        /// <summary>
        /// Get metadata for a specific recipe.
        /// </summary>
        public CraftingRecipeMetadata? GetMetadata(string recipeId)
        {
            if (RecipeMetadata == null)
                return null;

            return RecipeMetadata.TryGetValue(recipeId, out var metadata) ? metadata : (CraftingRecipeMetadata?)null;
        }

        /// <summary>
        /// Get all recipes available for a specific station type that this entity has learned.
        /// </summary>
        public IReadOnlyList<string> GetLearnedRecipesByStation(CraftingStationType stationType, ICraftingRecipeRegistry registry)
        {
            if (LearnedRecipeIds == null || registry == null)
                return new List<string>();

            var learnedRecipes = LearnedRecipeIds;
            return registry
                .GetRecipesByStationType(stationType)
                .Where(recipe => learnedRecipes.Contains(recipe.RecipeId))
                .Select(recipe => recipe.RecipeId)
                .ToList();
        }

        /// <summary>
        /// Get all learned recipes for this entity.
        /// </summary>
        public IReadOnlyList<string> GetLearnedRecipes()
        {
            return LearnedRecipeIds?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Learn a new recipe.
        /// </summary>
        public bool LearnRecipe(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
                return false;

            if (LearnedRecipeIds == null)
                LearnedRecipeIds = new HashSet<string>();

            return LearnedRecipeIds.Add(recipeId);
        }

        /// <summary>
        /// Check if a recipe has been learned.
        /// </summary>
        public bool HasLearned(string recipeId)
        {
            return LearnedRecipeIds != null && LearnedRecipeIds.Contains(recipeId);
        }

        /// <summary>
        /// Register metadata for a recipe.
        /// </summary>
        public void RegisterMetadata(string recipeId, CraftingRecipeMetadata metadata)
        {
            if (string.IsNullOrEmpty(recipeId))
                return;

            if (RecipeMetadata == null)
                RecipeMetadata = new Dictionary<string, CraftingRecipeMetadata>();

            RecipeMetadata[recipeId] = metadata;
        }

        /// <summary>
        /// Add crafting experience.
        /// </summary>
        public void AddExperience(int amount)
        {
            if (amount < 0)
                return;

            CraftingExperience += amount;

            // Simple experience to level conversion: 100 XP per level
            int newLevel = CraftingExperience / 100;
            CraftingSkillLevel = Math.Min(100, newLevel);
        }
    }
}
