using RPGFramework.Factions;

namespace MythHunter.GameLogic.Factions
{
    /// <summary>
    /// Sample faction definitions for MythHunter.
    /// Demonstrates faction registration with various types, default reputations, and relationships.
    /// </summary>
    public static class SampleFactions
    {
        /// <summary>
        /// Register all sample factions and their relationships.
        /// Call this during game initialization.
        /// </summary>
        public static void RegisterAll(FactionSystemIntegration integration)
        {
            RegisterFactions(integration);
            RegisterRelationships(integration);
        }

        private static void RegisterFactions(FactionSystemIntegration integration)
        {
            // Mages Guild - A guild of magic users
            integration.RegisterFaction(
                factionId: "guild_mages",
                name: "Mages Guild",
                factionType: FactionType.Guild,
                description: "An ancient organization dedicated to the study and practice of magical arts. " +
                            "They value knowledge, discipline, and responsible use of arcane power.",
                defaultReputation: 0
            );

            // Kingdom of Valoria - The main ruling kingdom
            integration.RegisterFaction(
                factionId: "kingdom_valoria",
                name: "Kingdom of Valoria",
                factionType: FactionType.Kingdom,
                description: "The dominant human kingdom in the realm. Known for its strong military, " +
                            "fair laws, and prosperous trade. Led by King Aldric the Just.",
                defaultReputation: 0
            );

            // Thieves Guild - Criminal organization
            integration.RegisterFaction(
                factionId: "guild_thieves",
                name: "Shadow Syndicate",
                factionType: FactionType.Criminal,
                description: "A secretive network of thieves, smugglers, and information brokers. " +
                            "They operate in the shadows, always one step ahead of the law.",
                defaultReputation: -25
            );

            // Trade Consortium - Merchant organization
            integration.RegisterFaction(
                factionId: "consortium_trade",
                name: "Golden Trade Consortium",
                factionType: FactionType.Merchant,
                description: "A powerful alliance of merchants, traders, and craftsmen. " +
                            "They control most of the legitimate trade routes and market prices.",
                defaultReputation: 10
            );

            // Dragon Clan - Mysterious dragon-worshipping clan
            integration.RegisterFaction(
                factionId: "clan_dragon",
                name: "Dragon Clan",
                factionType: FactionType.Clan,
                description: "An isolated clan that worships ancient dragons and follows their teachings. " +
                            "They are distrustful of outsiders but possess ancient dragon lore.",
                defaultReputation: -10
            );

            // Order of the Sacred Flame - Religious faction
            integration.RegisterFaction(
                factionId: "religion_sacred_flame",
                name: "Order of the Sacred Flame",
                factionType: FactionType.Religion,
                description: "A religious order dedicated to the worship of the eternal flame. " +
                            "They provide healing, guidance, and protection to the faithful.",
                defaultReputation: 5
            );

            // Iron Legion - Military faction
            integration.RegisterFaction(
                factionId: "military_iron_legion",
                name: "Iron Legion",
                factionType: FactionType.Military,
                description: "Elite military force of Kingdom Valoria. Known for their discipline, " +
                            "tactical prowess, and unwavering loyalty to the crown.",
                defaultReputation: 0
            );

            // Forest Guardians - Nature-focused clan
            integration.RegisterFaction(
                factionId: "clan_forest_guardians",
                name: "Forest Guardians",
                factionType: FactionType.Clan,
                description: "Protectors of the ancient forests and keepers of natural balance. " +
                            "They live in harmony with nature and distrust those who exploit it.",
                defaultReputation: 0
            );
        }

        private static void RegisterRelationships(FactionSystemIntegration integration)
        {
            // Mages Guild ↔ Kingdom of Valoria: Allied
            // The kingdom supports the guild's research and magical services
            integration.RegisterRelationship(
                "guild_mages",
                "kingdom_valoria",
                FactionRelationType.Allied
            );

            // Shadow Syndicate ↔ Kingdom of Valoria: Enemy
            // The kingdom actively hunts down the criminal organization
            integration.RegisterRelationship(
                "guild_thieves",
                "kingdom_valoria",
                FactionRelationType.Enemy
            );

            // Golden Trade Consortium ↔ Kingdom of Valoria: Trade Partner
            // Mutual economic benefits from trade agreements
            integration.RegisterRelationship(
                "consortium_trade",
                "kingdom_valoria",
                FactionRelationType.TradePartner
            );

            // Iron Legion ↔ Kingdom of Valoria: Allied
            // The legion is the military arm of the kingdom
            integration.RegisterRelationship(
                "military_iron_legion",
                "kingdom_valoria",
                FactionRelationType.Allied
            );

            // Shadow Syndicate ↔ Golden Trade Consortium: Rival
            // Criminals interfere with legitimate trade
            integration.RegisterRelationship(
                "guild_thieves",
                "consortium_trade",
                FactionRelationType.Rival
            );

            // Mages Guild ↔ Order of the Sacred Flame: Allied
            // Both value knowledge and helping people
            integration.RegisterRelationship(
                "guild_mages",
                "religion_sacred_flame",
                FactionRelationType.Allied
            );

            // Dragon Clan ↔ Forest Guardians: Allied
            // Both protect ancient secrets and natural order
            integration.RegisterRelationship(
                "clan_dragon",
                "clan_forest_guardians",
                FactionRelationType.Allied
            );

            // Forest Guardians ↔ Golden Trade Consortium: Rival
            // Merchants exploit natural resources
            integration.RegisterRelationship(
                "clan_forest_guardians",
                "consortium_trade",
                FactionRelationType.Rival
            );

            // Order of the Sacred Flame ↔ Kingdom of Valoria: Allied
            // The church supports the kingdom's rule
            integration.RegisterRelationship(
                "religion_sacred_flame",
                "kingdom_valoria",
                FactionRelationType.Allied
            );

            // Dragon Clan ↔ Iron Legion: Rival
            // Military expansion threatens dragon territories
            integration.RegisterRelationship(
                "clan_dragon",
                "military_iron_legion",
                FactionRelationType.Rival
            );
        }

        /// <summary>
        /// Get faction ID constants for easy reference in game code.
        /// </summary>
        public static class FactionIds
        {
            public const string MagesGuild = "guild_mages";
            public const string KingdomValoria = "kingdom_valoria";
            public const string ThievesGuild = "guild_thieves";
            public const string TradeConsortium = "consortium_trade";
            public const string DragonClan = "clan_dragon";
            public const string SacredFlame = "religion_sacred_flame";
            public const string IronLegion = "military_iron_legion";
            public const string ForestGuardians = "clan_forest_guardians";
        }
    }
}
