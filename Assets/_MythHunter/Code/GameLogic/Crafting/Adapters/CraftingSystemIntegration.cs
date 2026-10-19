using System;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.Crafting;
using RPGFramework.ECS;
using RPGFramework.Logging;

namespace MythHunter.GameLogic.Crafting
{
    /// <summary>
    /// Integration adapter that connects the RPG Framework Crafting System with MythHunter game logic.
    /// Provides high-level crafting operations for game systems.
    /// </summary>
    public class CraftingSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly ICraftingManager _craftingManager;
        private readonly ICraftingRecipeRegistry _recipeRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Create a new crafting system integration.
        /// </summary>
        public CraftingSystemIntegration(
            IEntityManager entityManager,
            ICraftingManager craftingManager,
            ICraftingRecipeRegistry recipeRegistry,
            ILogger logger)
        {
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _craftingManager = craftingManager ?? throw new ArgumentNullException(nameof(craftingManager));
            _recipeRegistry = recipeRegistry ?? throw new ArgumentNullException(nameof(recipeRegistry));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SubscribeToEvents();
        }

        /// <summary>
        /// Start crafting a recipe at a station.
        /// </summary>
        public bool StartCrafting(int entityId, string recipeId, ICraftingStation station)
        {
            if (string.IsNullOrEmpty(recipeId))
            {
                _logger.Log(LogSeverity.Error, $"Cannot start crafting: Recipe ID is empty");
                return false;
            }

            if (station == null)
            {
                _logger.Log(LogSeverity.Error, $"Cannot start crafting: Station is null");
                return false;
            }

            var recipe = _recipeRegistry.GetRecipe(recipeId);
            if (recipe == null)
            {
                _logger.Log(LogSeverity.Error, $"Cannot start crafting: Recipe '{recipeId}' not found in registry");
                return false;
            }

            // Check if entity has learned this recipe
            var craftingComponent = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);
            if (!craftingComponent.HasLearned(recipeId))
            {
                _logger.Log(LogSeverity.Error, $"Entity {entityId} has not learned recipe '{recipeId}'");
                return false;
            }

            // Try to start crafting via the framework manager
            var operationId = _craftingManager.StartCrafting(entityId, recipeId, station);
            if (operationId < 0)
            {
                _logger.Log(LogSeverity.Error, $"Failed to start crafting operation for recipe '{recipeId}'");
                return false;
            }

            _logger.Log(LogSeverity.Information, $"Started crafting operation {operationId} for recipe '{recipeId}' by entity {entityId}");
            return true;
        }

        /// <summary>
        /// Cancel an active crafting operation.
        /// </summary>
        public bool CancelCrafting(int entityId, int operationId)
        {
            var result = _craftingManager.CancelCrafting(entityId, operationId);
            if (result)
            {
                _logger.Log(LogSeverity.Information, $"Cancelled crafting operation {operationId} for entity {entityId}");
            }
            else
            {
                _logger.Log(LogSeverity.Warning, $"Failed to cancel crafting operation {operationId} for entity {entityId}");
            }
            return result;
        }

        /// <summary>
        /// Get all active crafting operations for an entity.
        /// </summary>
        public IReadOnlyList<ActiveCraftingOperation> GetActiveCrafts(int entityId)
        {
            return _craftingManager.GetActiveCrafts(entityId);
        }

        /// <summary>
        /// Check if an entity is currently crafting.
        /// </summary>
        public bool IsCrafting(int entityId)
        {
            return _craftingManager.IsCrafting(entityId);
        }

        /// <summary>
        /// Get the progress of a crafting operation as a percentage.
        /// </summary>
        public float GetCraftingProgress(int entityId, int operationId)
        {
            return _craftingManager.GetCraftingProgress(entityId, operationId);
        }

        /// <summary>
        /// Get all recipes available at a specific station that an entity has learned.
        /// </summary>
        public IReadOnlyList<ICraftingRecipe> GetAvailableRecipes(int entityId, ICraftingStation station)
        {
            if (station == null)
                return new List<ICraftingRecipe>();

            var component = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);
            var recipes = new List<ICraftingRecipe>();

            foreach (var recipeId in component.GetLearnedRecipes())
            {
                var recipe = _recipeRegistry.GetRecipe(recipeId);
                if (recipe != null && station.CanCraft(recipe))
                {
                    recipes.Add(recipe);
                }
            }

            return recipes;
        }

        /// <summary>
        /// Teach an entity a new recipe.
        /// </summary>
        public bool LearnRecipe(int entityId, string recipeId)
        {
            var recipe = _recipeRegistry.GetRecipe(recipeId);
            if (recipe == null)
            {
                _logger.Log(LogSeverity.Error, $"Cannot learn recipe: Recipe '{recipeId}' not found");
                return false;
            }

            var component = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);
            if (component.HasLearned(recipeId))
            {
                _logger.Log(LogSeverity.Warning, $"Entity {entityId} has already learned recipe '{recipeId}'");
                return false;
            }

            if (!component.LearnRecipe(recipeId))
            {
                _logger.Log(LogSeverity.Error, $"Failed to learn recipe '{recipeId}' for entity {entityId}");
                return false;
            }

            _entityManager.RemoveComponent<MythHunterCraftingComponent>(entityId);
            _entityManager.AddComponent(entityId, component);
            _logger.Log(LogSeverity.Information, $"Entity {entityId} learned recipe '{recipeId}' ({recipe.Name})");
            return true;
        }

        /// <summary>
        /// Get all learned recipes for an entity.
        /// </summary>
        public IReadOnlyList<string> GetLearnedRecipes(int entityId)
        {
            var component = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);
            return component.GetLearnedRecipes();
        }

        /// <summary>
        /// Get recipes learned for a specific station type.
        /// </summary>
        public IReadOnlyList<string> GetLearnedRecipesByStation(int entityId, CraftingStationType stationType)
        {
            var component = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);
            return component.GetLearnedRecipesByStation(stationType, _recipeRegistry);
        }

        /// <summary>
        /// Get the crafting skill level for an entity.
        /// </summary>
        public int GetCraftingSkillLevel(int entityId)
        {
            var component = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);
            return component.CraftingSkillLevel;
        }

        /// <summary>
        /// Add crafting experience to an entity.
        /// </summary>
        public void AddCraftingExperience(int entityId, int amount)
        {
            if (amount <= 0)
                return;

            var component = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);
            component.AddExperience(amount);
            _entityManager.RemoveComponent<MythHunterCraftingComponent>(entityId);
            _entityManager.AddComponent(entityId, component);

            _logger.Log(LogSeverity.Information, $"Entity {entityId} gained {amount} crafting experience (total: {component.CraftingExperience})");
        }

        /// <summary>
        /// Get the total crafting experience for an entity.
        /// </summary>
        public int GetCraftingExperience(int entityId)
        {
            var component = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);
            return component.CraftingExperience;
        }

        /// <summary>
        /// Register metadata for a recipe.
        /// </summary>
        public void RegisterRecipeMetadata(int entityId, string recipeId, CraftingRecipeMetadata metadata)
        {
            var component = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);
            component.RegisterMetadata(recipeId, metadata);
            _entityManager.RemoveComponent<MythHunterCraftingComponent>(entityId);
            _entityManager.AddComponent(entityId, component);
        }

        /// <summary>
        /// Get metadata for a specific recipe.
        /// </summary>
        public CraftingRecipeMetadata? GetRecipeMetadata(int entityId, string recipeId)
        {
            var component = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);
            return component.GetMetadata(recipeId);
        }

        /// <summary>
        /// Get all recipes of a specific type from the registry.
        /// </summary>
        public IReadOnlyList<ICraftingRecipe> GetRecipesByStationType(CraftingStationType stationType)
        {
            return _recipeRegistry.GetRecipesByStationType(stationType);
        }

        /// <summary>
        /// Get all registered recipes.
        /// </summary>
        public IReadOnlyList<ICraftingRecipe> GetAllRecipes()
        {
            return _recipeRegistry.GetAllRecipes();
        }

        /// <summary>
        /// Get total crafting statistics for an entity.
        /// </summary>
        public CraftingStatistics GetCraftingStatistics(int entityId)
        {
            var activeCrafts = _entityManager.GetComponent<ActiveCraftingData>(entityId);
            var component = _entityManager.GetComponent<MythHunterCraftingComponent>(entityId);

            return new CraftingStatistics
            {
                EntityId = entityId,
                SkillLevel = component.CraftingSkillLevel,
                TotalExperience = component.CraftingExperience,
                RecipesLearned = component.GetLearnedRecipes().Count,
                TotalItemsCrafted = activeCrafts.TotalItemsCrafted,
                TotalCraftingTimeSeconds = activeCrafts.TotalCraftingTimeSeconds,
                ActiveCraftingOperations = GetActiveCrafts(entityId).Count
            };
        }

        /// <summary>
        /// Subscribe to crafting manager events.
        /// </summary>
        private void SubscribeToEvents()
        {
            _craftingManager.CraftingStarted += OnCraftingStarted;
            _craftingManager.CraftingCompleted += OnCraftingCompleted;
            _craftingManager.CraftingFailed += OnCraftingFailed;
            _craftingManager.CraftingCancelled += OnCraftingCancelled;
        }

        private void OnCraftingStarted(object sender, CraftingStartedEvent e)
        {
            _logger.Log(LogSeverity.Information, $"[CRAFTING] Crafting started: Recipe '{e.RecipeId}' by entity {e.EntityId} " +
                $"at station '{e.Station.StationId}' (Duration: {e.CraftingTimeSeconds}s)");
        }

        private void OnCraftingCompleted(object sender, CraftingCompletedEvent e)
        {
            int totalQuantity = e.Results.Sum(r => r.Quantity);
            _logger.Log(LogSeverity.Information, $"[CRAFTING] Crafting completed: Recipe '{e.RecipeId}' by entity {e.EntityId} " +
                $"produced {totalQuantity} items in {e.TotalTimeSeconds}s");

            // Add experience on successful craft
            AddCraftingExperience(e.EntityId, (int)(e.TotalTimeSeconds * 10)); // 10 XP per second
        }

        private void OnCraftingFailed(object sender, CraftingFailedEvent e)
        {
            _logger.Log(LogSeverity.Warning, $"[CRAFTING] Crafting failed: Recipe '{e.RecipeId}' by entity {e.EntityId} " +
                $"- Reason: {e.FailureReason} (Time elapsed: {e.TimeBeforeFailureSeconds}s)");
        }

        private void OnCraftingCancelled(object sender, CraftingCancelledEvent e)
        {
            _logger.Log(LogSeverity.Information, $"[CRAFTING] Crafting cancelled: Recipe '{e.RecipeId}' by entity {e.EntityId} " +
                $"(Time spent: {e.TimeSpentSeconds}s)");
        }
    }

    /// <summary>
    /// Crafting statistics for an entity.
    /// </summary>
    public struct CraftingStatistics
    {
        public int EntityId;
        public int SkillLevel;
        public int TotalExperience;
        public int RecipesLearned;
        public int TotalItemsCrafted;
        public float TotalCraftingTimeSeconds;
        public int ActiveCraftingOperations;
    }
}
