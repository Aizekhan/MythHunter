// Assets/_MythHunter/Code/GameLogic/Abilities/Data/SampleAbilities.cs

using RPGFramework.Abilities;
using System.Collections.Generic;

namespace MythHunter.GameLogic.Abilities.Data
{
    /// <summary>
    /// Sample ability definitions for testing and development.
    /// Provides pre-configured abilities for common RPG actions (combat, healing, buffs).
    ///
    /// Pattern:
    /// - Static factory methods for easy access
    /// - Categorized by type (combat, utility, passive, toggle)
    /// - Ready for registry population
    /// - Uses Framework Ability classes directly
    /// </summary>
    public static class SampleAbilities
    {
        // ============================================================
        // COMBAT ABILITIES (Active attacks and spells)
        // ============================================================

        /// <summary>
        /// Basic melee attack - no cooldown, no cost.
        /// Standard auto-attack for warriors.
        /// </summary>
        public static IAbility MeleeStrike()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "melee_strike",
                    name: "Melee Strike",
                    type: AbilityType.Active,
                    cooldown: 0f,
                    range: 2f,
                    aoeRadius: 0f,
                    minLevel: 0
                ),
                resourceCosts: null,
                castInfo: CastInfo.Instant,
                targetType: TargetType.Enemy
            );
        }

        /// <summary>
        /// Fireball spell - ranged damage with cooldown and mana cost.
        /// Classic mage attack spell.
        /// </summary>
        public static IAbility Fireball()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "fireball",
                    name: "Fireball",
                    type: AbilityType.Active,
                    cooldown: 5f,
                    range: 30f,
                    aoeRadius: 3f,
                    minLevel: 1
                ),
                resourceCosts: new List<ResourceCost>
                {
                    new ResourceCost("MP", 20f, false)
                },
                castInfo: new CastInfo(castTime: 1.5f, isInterruptible: true),
                targetType: TargetType.Enemy
            );
        }

        /// <summary>
        /// Power strike - high damage melee with cooldown.
        /// Warrior burst damage ability.
        /// </summary>
        public static IAbility PowerStrike()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "power_strike",
                    name: "Power Strike",
                    type: AbilityType.Active,
                    cooldown: 8f,
                    range: 2f,
                    aoeRadius: 0f,
                    minLevel: 3
                ),
                resourceCosts: new List<ResourceCost>
                {
                    new ResourceCost("Stamina", 30f, false)
                },
                castInfo: CastInfo.Instant,
                targetType: TargetType.Enemy
            );
        }

        /// <summary>
        /// Whirlwind - AoE melee attack hitting all nearby enemies.
        /// </summary>
        public static IAbility Whirlwind()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "whirlwind",
                    name: "Whirlwind",
                    type: AbilityType.Active,
                    cooldown: 12f,
                    range: 0f,
                    aoeRadius: 5f,
                    minLevel: 5
                ),
                resourceCosts: new List<ResourceCost>
                {
                    new ResourceCost("Stamina", 40f, false)
                },
                castInfo: CastInfo.Instant,
                targetType: TargetType.Area
            );
        }

        // ============================================================
        // UTILITY ABILITIES (Healing, buffs, movement)
        // ============================================================

        /// <summary>
        /// Heal - restores health with moderate cooldown.
        /// Basic healing spell for support classes.
        /// </summary>
        public static IAbility Heal()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "heal",
                    name: "Heal",
                    type: AbilityType.Active,
                    cooldown: 10f,
                    range: 40f,
                    aoeRadius: 0f,
                    minLevel: 1
                ),
                resourceCosts: new List<ResourceCost>
                {
                    new ResourceCost("MP", 30f, false)
                },
                castInfo: new CastInfo(castTime: 2f, isInterruptible: true, canMoveWhileCasting: false),
                targetType: TargetType.Ally
            );
        }

        /// <summary>
        /// Blink - instant teleport with charges.
        /// Mobility spell for escaping or repositioning.
        /// </summary>
        public static IAbility Blink()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "blink",
                    name: "Blink",
                    type: AbilityType.Active,
                    cooldown: 0f, // Cooldown managed by charges
                    range: 15f,
                    aoeRadius: 0f,
                    minLevel: 3,
                    maxCharges: 3
                ),
                resourceCosts: null,
                castInfo: CastInfo.Instant,
                targetType: TargetType.Ground
            );
        }

        /// <summary>
        /// Shield - applies damage absorption buff.
        /// Protective spell for self or allies.
        /// </summary>
        public static IAbility Shield()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "shield",
                    name: "Shield",
                    type: AbilityType.Active,
                    cooldown: 15f,
                    range: 40f,
                    aoeRadius: 0f,
                    minLevel: 2
                ),
                resourceCosts: new List<ResourceCost>
                {
                    new ResourceCost("MP", 25f, false)
                },
                castInfo: CastInfo.Instant,
                targetType: TargetType.Ally
            );
        }

        // ============================================================
        // PASSIVE ABILITIES (Always active stat boosts)
        // ============================================================

        /// <summary>
        /// Critical strike mastery - increases crit chance.
        /// Passive damage boost for melee classes.
        /// </summary>
        public static IAbility CriticalStrikeMastery()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "crit_strike_mastery",
                    name: "Critical Strike Mastery",
                    type: AbilityType.Passive,
                    cooldown: 0f,
                    range: 0f,
                    aoeRadius: 0f,
                    minLevel: 2
                ),
                resourceCosts: null,
                castInfo: CastInfo.Instant,
                targetType: TargetType.None
            );
        }

        /// <summary>
        /// Armor proficiency - reduces physical damage taken.
        /// Passive defense boost for tank classes.
        /// </summary>
        public static IAbility ArmorProficiency()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "armor_proficiency",
                    name: "Armor Proficiency",
                    type: AbilityType.Passive,
                    cooldown: 0f,
                    range: 0f,
                    aoeRadius: 0f,
                    minLevel: 1
                ),
                resourceCosts: null,
                castInfo: CastInfo.Instant,
                targetType: TargetType.None
            );
        }

        /// <summary>
        /// Mana regeneration - increases mana recovery rate.
        /// Passive resource boost for casters.
        /// </summary>
        public static IAbility ManaRegeneration()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "mana_regen",
                    name: "Mana Regeneration",
                    type: AbilityType.Passive,
                    cooldown: 0f,
                    range: 0f,
                    aoeRadius: 0f,
                    minLevel: 1
                ),
                resourceCosts: null,
                castInfo: CastInfo.Instant,
                targetType: TargetType.None
            );
        }

        // ============================================================
        // TOGGLE ABILITIES (Can be turned on/off)
        // ============================================================

        /// <summary>
        /// Berserker stance - increases damage but reduces defense.
        /// Toggle stance for high-risk, high-reward playstyle.
        /// </summary>
        public static IAbility BerserkerStance()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "berserker_stance",
                    name: "Berserker Stance",
                    type: AbilityType.Toggle,
                    cooldown: 1f, // Small cooldown to prevent spam
                    range: 0f,
                    aoeRadius: 0f,
                    minLevel: 4
                ),
                resourceCosts: new List<ResourceCost>
                {
                    new ResourceCost("Stamina", 5f, true) // 5% stamina per second while active
                },
                castInfo: CastInfo.Instant,
                targetType: TargetType.Self
            );
        }

        /// <summary>
        /// Holy aura - provides healing over time to nearby allies.
        /// Toggle aura for support/healing classes.
        /// </summary>
        public static IAbility HolyAura()
        {
            return new Ability(
                metadata: new AbilityMetadata(
                    abilityId: "holy_aura",
                    name: "Holy Aura",
                    type: AbilityType.Toggle,
                    cooldown: 1f,
                    range: 0f,
                    aoeRadius: 10f,
                    minLevel: 5
                ),
                resourceCosts: new List<ResourceCost>
                {
                    new ResourceCost("MP", 10f, true) // 10% mana per second while active
                },
                castInfo: CastInfo.Instant,
                targetType: TargetType.Self
            );
        }

        // ============================================================
        // UTILITY METHODS
        // ============================================================

        /// <summary>
        /// Gets all sample abilities.
        /// Useful for populating ability registry during initialization.
        /// </summary>
        public static List<IAbility> GetAllAbilities()
        {
            return new List<IAbility>
            {
                // Combat
                MeleeStrike(),
                Fireball(),
                PowerStrike(),
                Whirlwind(),

                // Utility
                Heal(),
                Blink(),
                Shield(),

                // Passive
                CriticalStrikeMastery(),
                ArmorProficiency(),
                ManaRegeneration(),

                // Toggle
                BerserkerStance(),
                HolyAura()
            };
        }

        /// <summary>
        /// Gets abilities by type.
        /// </summary>
        public static List<IAbility> GetAbilitiesByType(AbilityType type)
        {
            var all = GetAllAbilities();
            var filtered = new List<IAbility>();
            foreach (var ability in all)
            {
                if (ability.Type == type)
                    filtered.Add(ability);
            }
            return filtered;
        }
    }
}
