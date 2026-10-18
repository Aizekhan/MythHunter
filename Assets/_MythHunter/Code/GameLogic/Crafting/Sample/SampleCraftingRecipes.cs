using System.Collections.Generic;
using RPGFramework.Crafting;

namespace MythHunter.GameLogic.Crafting.Sample
{
    /// <summary>
    /// Sample crafting recipes for MythHunter.
    /// Demonstrates various station types, ingredients, results, and requirements.
    /// </summary>
    public static class SampleCraftingRecipes
    {
        /// <summary>
        /// Create a sample Iron Sword recipe (Forge, 2 ingredients).
        /// </summary>
        public static ICraftingRecipe CreateIronSwordRecipe()
        {
            var ingredients = new List<ICraftingIngredient>
            {
                new CraftingIngredient("iron_ore", 5),
                new CraftingIngredient("leather_wrap", 2)
            };

            var results = new List<ICraftingResult>
            {
                new CraftingResult("iron_sword", 1, 90f)
            };

            var requirements = new List<ICraftingRequirement>
            {
                new CraftingRequirement(RequirementType.SkillLevel, "blacksmithing", 5)
            };

            return new CraftingRecipe(
                "recipe_iron_sword",
                "Iron Sword",
                "A sturdy iron sword suitable for novice adventurers.",
                CraftingStationType.Forge,
                1,
                45f,
                ingredients,
                results,
                requirements);
        }

        /// <summary>
        /// Create a sample Health Potion recipe (Alchemy, 3 ingredients).
        /// </summary>
        public static ICraftingRecipe CreateHealthPotionRecipe()
        {
            var ingredients = new List<ICraftingIngredient>
            {
                new CraftingIngredient("red_herb", 3),
                new CraftingIngredient("mineral_water", 2),
                new CraftingIngredient("crystal_dust", 1)
            };

            var results = new List<ICraftingResult>
            {
                new CraftingResult("health_potion", 3, 85f)
            };

            var requirements = new List<ICraftingRequirement>
            {
                new CraftingRequirement(RequirementType.SkillLevel, "alchemy", 3)
            };

            return new CraftingRecipe(
                "recipe_health_potion",
                "Health Potion",
                "Restores 50 health when consumed.",
                CraftingStationType.Alchemy,
                0,
                30f,
                ingredients,
                results,
                requirements);
        }

        /// <summary>
        /// Create a sample Bread Loaf recipe (Cooking, 2 ingredients).
        /// </summary>
        public static ICraftingRecipe CreateBreadLoafRecipe()
        {
            var ingredients = new List<ICraftingIngredient>
            {
                new CraftingIngredient("wheat", 3),
                new CraftingIngredient("salt", 1)
            };

            var results = new List<ICraftingResult>
            {
                new CraftingResult("bread_loaf", 2, 95f)
            };

            var requirements = new List<ICraftingRequirement>();

            return new CraftingRecipe(
                "recipe_bread_loaf",
                "Bread Loaf",
                "A fresh loaf of bread that restores 20 stamina.",
                CraftingStationType.Cooking,
                0,
                15f,
                ingredients,
                results,
                requirements);
        }

        /// <summary>
        /// Create a sample Enchanted Ring recipe (Enchanting, 4 ingredients, high requirements).
        /// </summary>
        public static ICraftingRecipe CreateEnchantedRingRecipe()
        {
            var ingredients = new List<ICraftingIngredient>
            {
                new CraftingIngredient("gold_bar", 3),
                new CraftingIngredient("sapphire", 2),
                new CraftingIngredient("mana_crystal", 1),
                new CraftingIngredient("enchanting_scroll", 1)
            };

            var results = new List<ICraftingResult>
            {
                new CraftingResult("enchanted_ring", 1, 60f)
            };

            var requirements = new List<ICraftingRequirement>
            {
                new CraftingRequirement(RequirementType.SkillLevel, "enchanting", 15),
                new CraftingRequirement(RequirementType.ToolRequired, "enchanting_rod", 1)
            };

            return new CraftingRecipe(
                "recipe_enchanted_ring",
                "Enchanted Ring",
                "A powerful magical ring that grants +3 Intelligence.",
                CraftingStationType.Enchanting,
                3,
                60f,
                ingredients,
                results,
                requirements);
        }

        /// <summary>
        /// Create a sample Wooden Shield recipe (Carpentry, 3 ingredients).
        /// </summary>
        public static ICraftingRecipe CreateWoodenShieldRecipe()
        {
            var ingredients = new List<ICraftingIngredient>
            {
                new CraftingIngredient("oak_wood", 8),
                new CraftingIngredient("iron_plate", 2),
                new CraftingIngredient("leather_strap", 1)
            };

            var results = new List<ICraftingResult>
            {
                new CraftingResult("wooden_shield", 1, 88f)
            };

            var requirements = new List<ICraftingRequirement>
            {
                new CraftingRequirement(RequirementType.SkillLevel, "carpentry", 4)
            };

            return new CraftingRecipe(
                "recipe_wooden_shield",
                "Wooden Shield",
                "A sturdy wooden shield that provides good protection.",
                CraftingStationType.Carpentry,
                1,
                50f,
                ingredients,
                results,
                requirements);
        }

        /// <summary>
        /// Get all sample recipes.
        /// </summary>
        public static IReadOnlyList<ICraftingRecipe> GetAllSampleRecipes()
        {
            return new List<ICraftingRecipe>
            {
                CreateIronSwordRecipe(),
                CreateHealthPotionRecipe(),
                CreateBreadLoafRecipe(),
                CreateEnchantedRingRecipe(),
                CreateWoodenShieldRecipe()
            };
        }

        /// <summary>
        /// Register all sample recipes to a registry.
        /// </summary>
        public static void RegisterAllSampleRecipes(ICraftingRecipeRegistry registry)
        {
            if (registry == null)
                return;

            foreach (var recipe in GetAllSampleRecipes())
            {
                registry.RegisterRecipe(recipe);
            }
        }
    }
}
