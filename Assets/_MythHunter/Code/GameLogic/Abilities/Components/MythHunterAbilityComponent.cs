// Assets/_MythHunter/Code/GameLogic/Abilities/Components/MythHunterAbilityComponent.cs

using System.Collections.Generic;
using System.Linq;
using RPGFramework.ECS;
using RPGFramework.Abilities;

namespace MythHunter.GameLogic.Abilities.Components
{
    /// <summary>
    /// Per-ability instance data within MythHunterAbilityComponent.
    /// Tracks state for a single ability (cooldown, charges, casting).
    /// </summary>
    public struct AbilityInstanceData
    {
        public string AbilityId;
        public int Level;
        public CooldownState CooldownState;
        public int RemainingCharges; // -1 = unlimited
        public bool IsCasting;
        public float CastStartTime;
        public bool IsToggled;
        public float CooldownModifier; // 1.0 = normal, 0.5 = half cooldown, etc.
    }

    /// <summary>
    /// MythHunter ability component for entities that can use abilities.
    /// Attached to players, NPCs, enemies that have active or passive abilities.
    /// Container component that holds multiple ability instances.
    /// </summary>
    /// <remarks>
    /// Greenfield Integration - No legacy ability system to migrate from.
    /// Pattern: Follows EPIC 13 Phase 1 component design (MythHunterInventoryComponent).
    /// One component per entity, holds Dictionary of ability instances.
    /// </remarks>
    public struct MythHunterAbilityComponent : IComponent
    {
        /// <summary>
        /// Ability instances keyed by ability ID.
        /// Each entry represents one ability the entity knows.
        /// </summary>
        public Dictionary<string, AbilityInstanceData> Abilities;

        /// <summary>
        /// Currently casting ability ID (null if not casting).
        /// Only one ability can be cast at a time.
        /// </summary>
        public string CurrentlyCasting;

        #region Factory Methods

        /// <summary>
        /// Creates empty ability component (no abilities).
        /// Use AddAbility to add abilities to this component.
        /// </summary>
        /// <returns>Empty ability component</returns>
        public static MythHunterAbilityComponent CreateEmpty()
        {
            return new MythHunterAbilityComponent
            {
                Abilities = new Dictionary<string, AbilityInstanceData>(),
                CurrentlyCasting = null
            };
        }

        /// <summary>
        /// Creates ability component with initial abilities.
        /// </summary>
        /// <param name="abilities">Initial abilities to add</param>
        /// <returns>Ability component with pre-configured abilities</returns>
        public static MythHunterAbilityComponent Create(params AbilityInstanceData[] abilities)
        {
            var component = CreateEmpty();
            foreach (var ability in abilities)
            {
                component.Abilities[ability.AbilityId] = ability;
            }
            return component;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if entity has specific ability.
        /// </summary>
        public bool HasAbility(string abilityId) =>
            Abilities != null && Abilities.ContainsKey(abilityId);

        /// <summary>
        /// Gets ability instance data if exists.
        /// </summary>
        public bool TryGetAbility(string abilityId, out AbilityInstanceData data)
        {
            data = default;
            return Abilities != null && Abilities.TryGetValue(abilityId, out data);
        }

        /// <summary>
        /// Adds ability to this component.
        /// </summary>
        public void AddAbility(string abilityId, int level = 1, int maxCharges = -1, float cooldownModifier = 1.0f)
        {
            if (Abilities == null)
                Abilities = new Dictionary<string, AbilityInstanceData>();

            Abilities[abilityId] = new AbilityInstanceData
            {
                AbilityId = abilityId,
                Level = level,
                CooldownState = new CooldownState(abilityId, 0f, 0f),
                RemainingCharges = maxCharges,
                IsCasting = false,
                CastStartTime = 0f,
                IsToggled = false,
                CooldownModifier = cooldownModifier
            };
        }

        /// <summary>
        /// Removes ability from this component.
        /// </summary>
        public bool RemoveAbility(string abilityId)
        {
            return Abilities != null && Abilities.Remove(abilityId);
        }

        /// <summary>
        /// Gets all ability IDs in this component.
        /// </summary>
        public IEnumerable<string> GetAbilityIds()
        {
            return Abilities?.Keys ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Checks if ability is currently on cooldown.
        /// </summary>
        public bool IsOnCooldown(string abilityId, float currentTime)
        {
            return TryGetAbility(abilityId, out var data) && data.CooldownState.IsOnCooldown(currentTime);
        }

        /// <summary>
        /// Gets remaining cooldown time for ability.
        /// </summary>
        public float GetRemainingCooldown(string abilityId, float currentTime)
        {
            return TryGetAbility(abilityId, out var data)
                ? data.CooldownState.GetRemainingTime(currentTime)
                : 0f;
        }

        /// <summary>
        /// Checks if ability has charges available.
        /// </summary>
        public bool HasCharges(string abilityId)
        {
            return TryGetAbility(abilityId, out var data) &&
                   (data.RemainingCharges == -1 || data.RemainingCharges > 0);
        }

        /// <summary>
        /// Checks if ability is currently being cast.
        /// </summary>
        public bool IsCasting(string abilityId)
        {
            return CurrentlyCasting == abilityId;
        }

        /// <summary>
        /// Checks if entity is casting any ability.
        /// </summary>
        public bool IsCastingAny() => !string.IsNullOrEmpty(CurrentlyCasting);

        /// <summary>
        /// Updates ability instance data.
        /// </summary>
        public void UpdateAbility(string abilityId, AbilityInstanceData newData)
        {
            if (Abilities != null)
                Abilities[abilityId] = newData;
        }

        #endregion
    }
}
