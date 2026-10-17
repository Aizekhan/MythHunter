// Assets/_MythHunter/Code/GameLogic/Abilities/Initialization/AbilityRegistryInitializer.cs

using RPGFramework.Abilities;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.GameLogic.Abilities.Data;
using System;

namespace MythHunter.GameLogic.Abilities.Initialization
{
    /// <summary>
    /// Initializes and populates the ability registry on game start.
    /// Registers all sample abilities and provides hooks for custom ability registration.
    ///
    /// Usage:
    /// - Inject into game initialization flow (e.g., GameBootstrap, StartupSequence)
    /// - Call Initialize() during game startup
    /// - Registry will be populated with sample abilities from SampleAbilities
    ///
    /// Pattern:
    /// - Dependency injection for IAbilityRegistry
    /// - Logging for tracking initialization
    /// - Extensible for custom ability registration
    /// </summary>
    public class AbilityRegistryInitializer
    {
        private readonly IAbilityRegistry _abilityRegistry;
        private readonly IMythLogger _logger;

        private const string LogCategory = "AbilityRegistry";

        private bool _isInitialized = false;

        [Inject]
        public AbilityRegistryInitializer(
            IAbilityRegistry abilityRegistry,
            IMythLogger logger)
        {
            _abilityRegistry = abilityRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Initializes the ability registry with sample abilities.
        /// Should be called once during game startup.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                _logger.LogWarning("AbilityRegistryInitializer already initialized, skipping", LogCategory);
                return;
            }

            _logger.LogInfo("Initializing ability registry...", LogCategory);

            try
            {
                // Register all sample abilities
                RegisterSampleAbilities();

                _isInitialized = true;

                _logger.LogInfo(
                    $"Ability registry initialized successfully. " +
                    $"Registered {_abilityRegistry.GetAllAbilities().Count} abilities.",
                    LogCategory
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize ability registry: {ex.Message}", LogCategory);
                throw;
            }
        }

        /// <summary>
        /// Registers all sample abilities from SampleAbilities.
        /// </summary>
        private void RegisterSampleAbilities()
        {
            var abilities = SampleAbilities.GetAllAbilities();

            foreach (var ability in abilities)
            {
                try
                {
                    _abilityRegistry.RegisterAbility(ability);
                    _logger.LogDebug($"Registered ability: '{ability.AbilityId}' - {ability.Name} ({ability.Type})", LogCategory);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to register ability '{ability.AbilityId}': {ex.Message}", LogCategory);
                }
            }

            _logger.LogInfo($"Registered {abilities.Count} sample abilities", LogCategory);
        }

        /// <summary>
        /// Registers a custom ability at runtime.
        /// Useful for mod support, dynamic content, or skill unlocks.
        /// </summary>
        /// <param name="ability">Ability to register</param>
        public void RegisterCustomAbility(IAbility ability)
        {
            if (ability == null)
            {
                _logger.LogWarning("Cannot register null ability", LogCategory);
                return;
            }

            try
            {
                _abilityRegistry.RegisterAbility(ability);
                _logger.LogInfo($"Registered custom ability: '{ability.AbilityId}' - {ability.Name}", LogCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to register custom ability '{ability.AbilityId}': {ex.Message}", LogCategory);
            }
        }

        /// <summary>
        /// Unregisters an ability by ID.
        /// Useful for dynamic content removal or ability revokes.
        /// </summary>
        /// <param name="abilityId">Ability ID to unregister</param>
        /// <returns>True if unregistered, false if not found</returns>
        public bool UnregisterAbility(string abilityId)
        {
            bool success = _abilityRegistry.UnregisterAbility(abilityId);

            if (success)
            {
                _logger.LogInfo($"Unregistered ability: '{abilityId}'", LogCategory);
            }
            else
            {
                _logger.LogWarning($"Failed to unregister ability '{abilityId}' - not found in registry", LogCategory);
            }

            return success;
        }

        /// <summary>
        /// Clears all registered abilities.
        /// Use with caution - typically only needed for testing or full reloads.
        /// </summary>
        public void ClearAllAbilities()
        {
            _abilityRegistry.Clear();
            _isInitialized = false;
            _logger.LogWarning("Cleared all abilities from registry", LogCategory);
        }

        /// <summary>
        /// Checks if an ability is registered.
        /// </summary>
        /// <param name="abilityId">Ability ID to check</param>
        /// <returns>True if registered</returns>
        public bool HasAbility(string abilityId)
        {
            return _abilityRegistry.HasAbility(abilityId);
        }

        /// <summary>
        /// Gets the number of registered abilities.
        /// </summary>
        public int GetAbilityCount()
        {
            return _abilityRegistry.GetAllAbilities().Count;
        }

        /// <summary>
        /// Gets the number of abilities by type.
        /// </summary>
        public int GetAbilityCountByType(AbilityType type)
        {
            return _abilityRegistry.GetAbilitiesByType(type).Count;
        }

        /// <summary>
        /// Logs all registered abilities for debugging.
        /// </summary>
        public void LogRegisteredAbilities()
        {
            var abilities = _abilityRegistry.GetAllAbilities();

            _logger.LogInfo($"=== Registered Abilities ({abilities.Count}) ===", LogCategory);

            // Group by type for cleaner output
            var types = new[] { AbilityType.Active, AbilityType.Passive, AbilityType.Toggle, AbilityType.Triggered, AbilityType.Channeled };

            foreach (var type in types)
            {
                var abilitiesOfType = _abilityRegistry.GetAbilitiesByType(type);
                if (abilitiesOfType.Count > 0)
                {
                    _logger.LogInfo($"  {type} Abilities ({abilitiesOfType.Count}):", LogCategory);

                    foreach (var ability in abilitiesOfType)
                    {
                        string costInfo = ability.ResourceCosts != null && ability.ResourceCosts.Count > 0
                            ? $", Cost: {ability.ResourceCosts[0].Amount} {ability.ResourceCosts[0].ResourceType}"
                            : "";

                        _logger.LogInfo(
                            $"    [{ability.AbilityId}] {ability.Name} - " +
                            $"CD: {ability.Cooldown}s, Range: {ability.Range}" +
                            $"{costInfo}",
                            LogCategory
                        );
                    }
                }
            }
        }
    }
}
