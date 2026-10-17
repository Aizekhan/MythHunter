// Assets/_MythHunter/Code/GameLogic/StatusEffects/Components/MythHunterStatusEffectsComponent.cs

using System.Collections.Generic;
using System.Linq;
using RPGFramework.ECS;
using RPGFramework.StatusEffects;

namespace MythHunter.GameLogic.StatusEffects.Components
{
    /// <summary>
    /// Per-effect metadata specific to MythHunter.
    /// Extends Framework StatusEffectInstanceData with game-specific properties.
    /// </summary>
    public struct EffectMetadata
    {
        public string EffectId;
        public string SourceEntityName;      // Name of entity that applied the effect
        public string VisualEffectPrefab;    // Particle effect to show
        public string SoundEffectId;         // Sound to play on application
        public bool ShowFloatingText;        // Whether to show damage/heal numbers
        public int DisplayPriority;          // Priority for UI display (higher = more important)
        public string IconPath;              // Icon for UI
        public string TooltipOverride;       // Custom tooltip text
    }

    /// <summary>
    /// MythHunter status effects component for entities that can have buffs/debuffs.
    /// Attached to players, NPCs, enemies that can receive status effects.
    /// Wraps Framework ActiveEffectsData with game-specific metadata.
    /// </summary>
    /// <remarks>
    /// Greenfield Integration - Following Dual-Component Pattern from EPICs 10-14.
    /// Pattern: Game-layer component wraps Framework data + adds game metadata.
    /// One component per entity, manages all active effects.
    /// </remarks>
    public struct MythHunterStatusEffectsComponent : IComponent
    {
        /// <summary>
        /// Game-specific metadata for each active effect.
        /// Keyed by effect ID, synchronized with Framework ActiveEffectsData.
        /// </summary>
        public Dictionary<string, EffectMetadata> EffectMetadata;

        /// <summary>
        /// Immunity list - effect IDs that this entity is immune to.
        /// Used for boss mechanics, player immunities, etc.
        /// </summary>
        public HashSet<string> Immunities;

        /// <summary>
        /// Maximum number of effects this entity can have at once.
        /// -1 = unlimited. Used for balance (prevent effect spam).
        /// </summary>
        public int MaxActiveEffects;

        /// <summary>
        /// Whether to show visual effects for status effects on this entity.
        /// Can be disabled for performance or aesthetic reasons.
        /// </summary>
        public bool ShowVisualEffects;

        /// <summary>
        /// Whether to play sound effects for status effects.
        /// </summary>
        public bool PlaySoundEffects;

        #region Factory Methods

        /// <summary>
        /// Creates empty status effects component.
        /// Use with StatusEffectManager to add effects.
        /// </summary>
        /// <returns>Empty component with default settings</returns>
        public static MythHunterStatusEffectsComponent CreateEmpty()
        {
            return new MythHunterStatusEffectsComponent
            {
                EffectMetadata = new Dictionary<string, EffectMetadata>(),
                Immunities = new HashSet<string>(),
                MaxActiveEffects = -1, // Unlimited by default
                ShowVisualEffects = true,
                PlaySoundEffects = true
            };
        }

        /// <summary>
        /// Creates component with custom settings.
        /// </summary>
        public static MythHunterStatusEffectsComponent Create(
            int maxActiveEffects = -1,
            bool showVisualEffects = true,
            bool playSoundEffects = true)
        {
            return new MythHunterStatusEffectsComponent
            {
                EffectMetadata = new Dictionary<string, EffectMetadata>(),
                Immunities = new HashSet<string>(),
                MaxActiveEffects = maxActiveEffects,
                ShowVisualEffects = showVisualEffects,
                PlaySoundEffects = playSoundEffects
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Adds effect metadata when Framework applies an effect.
        /// Called by adapter after StatusEffectManager.ApplyEffect.
        /// </summary>
        public void AddEffectMetadata(EffectMetadata metadata)
        {
            if (EffectMetadata == null)
                EffectMetadata = new Dictionary<string, EffectMetadata>();

            EffectMetadata[metadata.EffectId] = metadata;
        }

        /// <summary>
        /// Removes effect metadata when Framework removes an effect.
        /// Called by adapter after effect expires/removed.
        /// </summary>
        public void RemoveEffectMetadata(string effectId)
        {
            EffectMetadata?.Remove(effectId);
        }

        /// <summary>
        /// Gets metadata for specific effect.
        /// </summary>
        public bool TryGetMetadata(string effectId, out EffectMetadata metadata)
        {
            metadata = default;
            return EffectMetadata != null && EffectMetadata.TryGetValue(effectId, out metadata);
        }

        /// <summary>
        /// Checks if entity is immune to specific effect.
        /// </summary>
        public bool IsImmuneTo(string effectId)
        {
            return Immunities != null && Immunities.Contains(effectId);
        }

        /// <summary>
        /// Adds immunity to specific effect type.
        /// </summary>
        public void AddImmunity(string effectId)
        {
            if (Immunities == null)
                Immunities = new HashSet<string>();

            Immunities.Add(effectId);
        }

        /// <summary>
        /// Removes immunity to specific effect type.
        /// </summary>
        public bool RemoveImmunity(string effectId)
        {
            return Immunities != null && Immunities.Remove(effectId);
        }

        /// <summary>
        /// Checks if entity can accept more effects (based on MaxActiveEffects).
        /// </summary>
        public bool CanAcceptMoreEffects(int currentEffectCount)
        {
            return MaxActiveEffects == -1 || currentEffectCount < MaxActiveEffects;
        }

        /// <summary>
        /// Gets all effect metadata ordered by display priority.
        /// Useful for UI rendering.
        /// </summary>
        public IEnumerable<EffectMetadata> GetOrderedMetadata()
        {
            return EffectMetadata?.Values
                .OrderByDescending(m => m.DisplayPriority)
                ?? Enumerable.Empty<EffectMetadata>();
        }

        /// <summary>
        /// Gets metadata for effects that should show visual effects.
        /// </summary>
        public IEnumerable<EffectMetadata> GetVisualEffects()
        {
            if (!ShowVisualEffects || EffectMetadata == null)
                return Enumerable.Empty<EffectMetadata>();

            return EffectMetadata.Values.Where(m => !string.IsNullOrEmpty(m.VisualEffectPrefab));
        }

        /// <summary>
        /// Clears all metadata (usually when entity dies or resets).
        /// </summary>
        public void ClearAll()
        {
            EffectMetadata?.Clear();
        }

        #endregion
    }
}
