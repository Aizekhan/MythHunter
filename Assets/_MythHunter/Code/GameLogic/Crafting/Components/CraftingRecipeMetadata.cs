using System;
using RPGFramework.Crafting;

namespace MythHunter.GameLogic.Crafting
{
    /// <summary>
    /// Game-specific metadata for crafting recipes.
    /// Extends framework recipe data with MythHunter-specific information like icons, animations, and sounds.
    /// </summary>
    [Serializable]
    public struct CraftingRecipeMetadata
    {
        /// <summary>
        /// Asset path to the recipe icon sprite.
        /// </summary>
        public string IconPath;

        /// <summary>
        /// Asset path to the crafting sound effect.
        /// </summary>
        public string CraftingSoundPath;

        /// <summary>
        /// Asset path to the success animation.
        /// </summary>
        public string SuccessAnimationPath;

        /// <summary>
        /// Asset path to the failure animation.
        /// </summary>
        public string FailureAnimationPath;

        /// <summary>
        /// Asset path to the crafting effect prefab (VFX).
        /// </summary>
        public string CraftingEffectPath;

        /// <summary>
        /// Localization key for recipe name.
        /// </summary>
        public string NameLocalizationKey;

        /// <summary>
        /// Localization key for recipe description.
        /// </summary>
        public string DescriptionLocalizationKey;

        /// <summary>
        /// Create metadata for a Forge recipe.
        /// </summary>
        public static CraftingRecipeMetadata CreateForge(
            string iconPath,
            string craftingSoundPath,
            string successAnimationPath,
            string failureAnimationPath,
            string craftingEffectPath,
            string nameKey = "",
            string descKey = "")
        {
            return new CraftingRecipeMetadata
            {
                IconPath = iconPath,
                CraftingSoundPath = craftingSoundPath,
                SuccessAnimationPath = successAnimationPath,
                FailureAnimationPath = failureAnimationPath,
                CraftingEffectPath = craftingEffectPath,
                NameLocalizationKey = nameKey,
                DescriptionLocalizationKey = descKey
            };
        }

        /// <summary>
        /// Create metadata for an Alchemy recipe.
        /// </summary>
        public static CraftingRecipeMetadata CreateAlchemy(
            string iconPath,
            string craftingSoundPath,
            string successAnimationPath,
            string failureAnimationPath,
            string craftingEffectPath,
            string nameKey = "",
            string descKey = "")
        {
            return new CraftingRecipeMetadata
            {
                IconPath = iconPath,
                CraftingSoundPath = craftingSoundPath,
                SuccessAnimationPath = successAnimationPath,
                FailureAnimationPath = failureAnimationPath,
                CraftingEffectPath = craftingEffectPath,
                NameLocalizationKey = nameKey,
                DescriptionLocalizationKey = descKey
            };
        }

        /// <summary>
        /// Create metadata for a Cooking recipe.
        /// </summary>
        public static CraftingRecipeMetadata CreateCooking(
            string iconPath,
            string craftingSoundPath,
            string successAnimationPath,
            string failureAnimationPath,
            string craftingEffectPath,
            string nameKey = "",
            string descKey = "")
        {
            return new CraftingRecipeMetadata
            {
                IconPath = iconPath,
                CraftingSoundPath = craftingSoundPath,
                SuccessAnimationPath = successAnimationPath,
                FailureAnimationPath = failureAnimationPath,
                CraftingEffectPath = craftingEffectPath,
                NameLocalizationKey = nameKey,
                DescriptionLocalizationKey = descKey
            };
        }

        /// <summary>
        /// Create metadata for an Enchanting recipe.
        /// </summary>
        public static CraftingRecipeMetadata CreateEnchanting(
            string iconPath,
            string craftingSoundPath,
            string successAnimationPath,
            string failureAnimationPath,
            string craftingEffectPath,
            string nameKey = "",
            string descKey = "")
        {
            return new CraftingRecipeMetadata
            {
                IconPath = iconPath,
                CraftingSoundPath = craftingSoundPath,
                SuccessAnimationPath = successAnimationPath,
                FailureAnimationPath = failureAnimationPath,
                CraftingEffectPath = craftingEffectPath,
                NameLocalizationKey = nameKey,
                DescriptionLocalizationKey = descKey
            };
        }

        /// <summary>
        /// Create metadata for a Carpentry recipe.
        /// </summary>
        public static CraftingRecipeMetadata CreateCarpentry(
            string iconPath,
            string craftingSoundPath,
            string successAnimationPath,
            string failureAnimationPath,
            string craftingEffectPath,
            string nameKey = "",
            string descKey = "")
        {
            return new CraftingRecipeMetadata
            {
                IconPath = iconPath,
                CraftingSoundPath = craftingSoundPath,
                SuccessAnimationPath = successAnimationPath,
                FailureAnimationPath = failureAnimationPath,
                CraftingEffectPath = craftingEffectPath,
                NameLocalizationKey = nameKey,
                DescriptionLocalizationKey = descKey
            };
        }
    }
}
