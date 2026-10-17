// Assets/_MythHunter/Code/GameLogic/Progression/Data/SampleUnlocks.cs

using RPGFramework.Progression;
using System.Collections.Generic;

namespace MythHunter.GameLogic.Progression.Data
{
    /// <summary>
    /// Sample unlock definitions for testing and development.
    /// Provides pre-configured unlocks for abilities, features, and equipment.
    ///
    /// Pattern:
    /// - Static factory methods for unlock data
    /// - Categorized by type: Abilities, Features, Equipment
    /// - Uses Framework UnlockData
    ///
    /// Usage:
    /// - GetAbilityUnlocks(): Level-based ability unlocks
    /// - GetFeatureUnlocks(): UI/system feature unlocks
    /// - GetEquipmentUnlocks(): Equipment slot/tier unlocks
    /// - GetAllUnlocks(): All unlocks combined
    /// </summary>
    public static class SampleUnlocks
    {
        // ============================================================
        // ABILITY UNLOCKS
        // ============================================================

        /// <summary>
        /// Ability unlocks by level.
        /// Grants new abilities as player levels up.
        /// </summary>
        public static List<UnlockData> GetAbilityUnlocks()
        {
            return new List<UnlockData>
            {
                // Early game abilities (1-10)
                new UnlockData("ability_melee_strike", "Melee Strike", 1, "Ability")
                {
                    Description = "Basic melee attack"
                },
                new UnlockData("ability_heal", "Heal", 3, "Ability")
                {
                    Description = "Restore health to ally"
                },
                new UnlockData("ability_fireball", "Fireball", 5, "Ability")
                {
                    Description = "Ranged fire damage spell"
                },
                new UnlockData("ability_shield", "Shield", 7, "Ability")
                {
                    Description = "Apply damage absorption"
                },
                new UnlockData("ability_power_strike", "Power Strike", 10, "Ability")
                {
                    Description = "High damage melee attack"
                },

                // Mid game abilities (11-30)
                new UnlockData("ability_whirlwind", "Whirlwind", 15, "Ability")
                {
                    Description = "AoE melee attack"
                },
                new UnlockData("ability_blink", "Blink", 20, "Ability")
                {
                    Description = "Teleport to target location"
                },
                new UnlockData("ability_berserker_stance", "Berserker Stance", 25, "Ability")
                {
                    Description = "Toggle stance: +damage, -defense"
                },
                new UnlockData("ability_holy_aura", "Holy Aura", 30, "Ability")
                {
                    Description = "AoE healing over time"
                },

                // Late game abilities (31-50)
                new UnlockData("ability_meteor", "Meteor", 35, "Ability")
                {
                    Description = "Massive AoE spell damage"
                },
                new UnlockData("ability_resurrection", "Resurrection", 40, "Ability")
                {
                    Description = "Revive fallen ally"
                },
                new UnlockData("ability_divine_shield", "Divine Shield", 45, "Ability")
                {
                    Description = "Temporary invulnerability"
                },
                new UnlockData("ability_ultimate_strike", "Ultimate Strike", 50, "Ability")
                {
                    Description = "Ultimate melee finisher"
                }
            };
        }

        // ============================================================
        // FEATURE UNLOCKS
        // ============================================================

        /// <summary>
        /// Feature unlocks by level.
        /// Grants access to game systems and UI features.
        /// </summary>
        public static List<UnlockData> GetFeatureUnlocks()
        {
            return new List<UnlockData>
            {
                // UI/System features
                new UnlockData("feature_inventory", "Inventory System", 1, "Feature")
                {
                    Description = "Access inventory and equipment"
                },
                new UnlockData("feature_crafting", "Crafting System", 5, "Feature")
                {
                    Description = "Craft items from materials"
                },
                new UnlockData("feature_trading", "Trading System", 8, "Feature")
                {
                    Description = "Trade with other players"
                },
                new UnlockData("feature_guild", "Guild System", 10, "Feature")
                {
                    Description = "Join or create guilds"
                },
                new UnlockData("feature_mounts", "Mount System", 15, "Feature")
                {
                    Description = "Use mounts for faster travel"
                },
                new UnlockData("feature_pets", "Pet System", 20, "Feature")
                {
                    Description = "Summon and control pets"
                },
                new UnlockData("feature_pvp", "PvP Arena", 25, "Feature")
                {
                    Description = "Engage in player vs player combat"
                },
                new UnlockData("feature_raid", "Raid Finder", 30, "Feature")
                {
                    Description = "Join raid groups for endgame content"
                },
                new UnlockData("feature_transmog", "Transmogrification", 35, "Feature")
                {
                    Description = "Change equipment appearance"
                },
                new UnlockData("feature_talents", "Talent System", 40, "Feature")
                {
                    Description = "Customize character build with talents"
                }
            };
        }

        // ============================================================
        // EQUIPMENT UNLOCKS
        // ============================================================

        /// <summary>
        /// Equipment slot and tier unlocks by level.
        /// Grants access to new equipment slots and higher tier items.
        /// </summary>
        public static List<UnlockData> GetEquipmentUnlocks()
        {
            return new List<UnlockData>
            {
                // Equipment slots
                new UnlockData("equipment_offhand", "Off-Hand Slot", 5, "Equipment")
                {
                    Description = "Equip shields or dual weapons"
                },
                new UnlockData("equipment_ring_2", "Second Ring Slot", 10, "Equipment")
                {
                    Description = "Equip a second ring"
                },
                new UnlockData("equipment_trinket_1", "Trinket Slot 1", 15, "Equipment")
                {
                    Description = "Equip first trinket"
                },
                new UnlockData("equipment_trinket_2", "Trinket Slot 2", 20, "Equipment")
                {
                    Description = "Equip second trinket"
                },

                // Equipment tiers
                new UnlockData("equipment_tier_uncommon", "Uncommon Equipment", 8, "Equipment")
                {
                    Description = "Equip uncommon (green) items"
                },
                new UnlockData("equipment_tier_rare", "Rare Equipment", 15, "Equipment")
                {
                    Description = "Equip rare (blue) items"
                },
                new UnlockData("equipment_tier_epic", "Epic Equipment", 25, "Equipment")
                {
                    Description = "Equip epic (purple) items"
                },
                new UnlockData("equipment_tier_legendary", "Legendary Equipment", 40, "Equipment")
                {
                    Description = "Equip legendary (orange) items"
                },
                new UnlockData("equipment_tier_mythic", "Mythic Equipment", 50, "Equipment")
                {
                    Description = "Equip mythic (red) items"
                }
            };
        }

        // ============================================================
        // PASSIVE UNLOCKS
        // ============================================================

        /// <summary>
        /// Passive bonuses unlocked by level.
        /// Permanent stat increases and special bonuses.
        /// </summary>
        public static List<UnlockData> GetPassiveUnlocks()
        {
            return new List<UnlockData>
            {
                new UnlockData("passive_health_bonus_1", "Health Bonus I", 10, "Passive")
                {
                    Description = "+100 max health"
                },
                new UnlockData("passive_health_bonus_2", "Health Bonus II", 25, "Passive")
                {
                    Description = "+250 max health"
                },
                new UnlockData("passive_health_bonus_3", "Health Bonus III", 40, "Passive")
                {
                    Description = "+500 max health"
                },

                new UnlockData("passive_damage_bonus_1", "Damage Bonus I", 12, "Passive")
                {
                    Description = "+10% damage"
                },
                new UnlockData("passive_damage_bonus_2", "Damage Bonus II", 28, "Passive")
                {
                    Description = "+20% damage"
                },
                new UnlockData("passive_damage_bonus_3", "Damage Bonus III", 45, "Passive")
                {
                    Description = "+30% damage"
                },

                new UnlockData("passive_crit_bonus", "Critical Strike Mastery", 18, "Passive")
                {
                    Description = "+5% critical strike chance"
                },
                new UnlockData("passive_speed_bonus", "Movement Speed", 22, "Passive")
                {
                    Description = "+15% movement speed"
                },
                new UnlockData("passive_xp_bonus", "XP Boost", 30, "Passive")
                {
                    Description = "+10% experience gain"
                }
            };
        }

        // ============================================================
        // UTILITY METHODS
        // ============================================================

        /// <summary>
        /// Get all sample unlocks.
        /// Useful for populating unlock manager on initialization.
        /// </summary>
        public static List<UnlockData> GetAllUnlocks()
        {
            var allUnlocks = new List<UnlockData>();
            allUnlocks.AddRange(GetAbilityUnlocks());
            allUnlocks.AddRange(GetFeatureUnlocks());
            allUnlocks.AddRange(GetEquipmentUnlocks());
            allUnlocks.AddRange(GetPassiveUnlocks());
            return allUnlocks;
        }

        /// <summary>
        /// Get unlocks by type.
        /// </summary>
        public static List<UnlockData> GetUnlocksByType(string unlockType)
        {
            switch (unlockType)
            {
                case "Ability":
                    return GetAbilityUnlocks();
                case "Feature":
                    return GetFeatureUnlocks();
                case "Equipment":
                    return GetEquipmentUnlocks();
                case "Passive":
                    return GetPassiveUnlocks();
                default:
                    return new List<UnlockData>();
            }
        }

        /// <summary>
        /// Get unlocks available at specific level.
        /// </summary>
        public static List<UnlockData> GetUnlocksAtLevel(int level)
        {
            var allUnlocks = GetAllUnlocks();
            var result = new List<UnlockData>();

            foreach (var unlock in allUnlocks)
            {
                if (unlock.RequiredLevel == level)
                    result.Add(unlock);
            }

            return result;
        }

        /// <summary>
        /// Get count of unlocks by type.
        /// </summary>
        public static Dictionary<string, int> GetUnlockCountsByType()
        {
            return new Dictionary<string, int>
            {
                { "Ability", GetAbilityUnlocks().Count },
                { "Feature", GetFeatureUnlocks().Count },
                { "Equipment", GetEquipmentUnlocks().Count },
                { "Passive", GetPassiveUnlocks().Count },
                { "Total", GetAllUnlocks().Count }
            };
        }
    }
}
