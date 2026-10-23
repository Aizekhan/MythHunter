using RPGFramework.Guilds.Runtime;
using UnityEngine;

namespace MythHunter.GameLogic.Guilds
{
    /// <summary>
    /// Sample guild definitions for MythHunter.
    /// Demonstrates guild registration with metadata, various focuses, and requirements.
    /// Includes 5 diverse guilds across different playstyles.
    /// </summary>
    public static class SampleGuilds
    {
        /// <summary>
        /// Register all sample guilds with their metadata.
        /// Call this during game initialization.
        /// </summary>
        public static void RegisterAll(GuildSystemIntegration integration, int defaultLeaderId)
        {
            RegisterWarriorsGuild(integration, defaultLeaderId);
            RegisterMagesConclave(integration, defaultLeaderId);
            RegisterThievesSyndicate(integration, defaultLeaderId);
            RegisterMerchantConsortium(integration, defaultLeaderId);
            RegisterHolyOrder(integration, defaultLeaderId);
        }

        /// <summary>
        /// Warriors Guild - PvE and PvP focused guild for combat enthusiasts.
        /// </summary>
        private static void RegisterWarriorsGuild(GuildSystemIntegration integration, int leaderId)
        {
            var metadata = GuildMetadata.Create(
                emblemPath: "Guilds/Emblems/Warriors",
                bannerColor: new Color(0.8f, 0.2f, 0.2f), // Deep Red
                secondaryColor: new Color(0.9f, 0.7f, 0.3f), // Gold
                motto: "Strength Through Battle, Honor Through Victory",
                foundedDate: "Year 850, Age of Heroes",
                headquarters: "Ironforge Keep, Northern Mountains",
                recruitmentStatus: "Open",
                minimumLevel: 10,
                pve: true,
                pvp: true,
                trade: false,
                roleplay: false,
                website: "https://warriors-guild.example.com",
                discord: "https://discord.gg/warriors"
            );

            integration.RegisterGuild(
                guildId: GuildIds.WarriorsGuild,
                name: "Warriors Guild",
                tag: "WAR",
                leaderId: leaderId,
                description: "Elite warriors united in the pursuit of martial excellence. " +
                           "We seek glory in battle, honor in combat, and camaraderie among brothers and sisters in arms. " +
                           "Whether facing monstrous hordes in dungeons or testing our mettle against rival guilds, " +
                           "we stand together as the finest fighters in the realm.",
                metadata: metadata,
                maxMembers: 100
            );
        }

        /// <summary>
        /// Mages Conclave - PvE focused guild for magic users and scholars.
        /// </summary>
        private static void RegisterMagesConclave(GuildSystemIntegration integration, int leaderId)
        {
            var metadata = GuildMetadata.Create(
                emblemPath: "Guilds/Emblems/Mages",
                bannerColor: new Color(0.3f, 0.3f, 0.9f), // Arcane Blue
                secondaryColor: new Color(0.7f, 0.3f, 0.9f), // Purple
                motto: "Knowledge is Power, Wisdom is Eternal",
                foundedDate: "Year 312, Age of Enlightenment",
                headquarters: "Arcane Spire, Crystal Lake",
                recruitmentStatus: "Selective",
                minimumLevel: 15,
                pve: true,
                pvp: false,
                trade: false,
                roleplay: true,
                website: "https://mages-conclave.example.com",
                discord: "https://discord.gg/mages"
            );

            integration.RegisterGuild(
                guildId: GuildIds.MagesConclave,
                name: "Mages Conclave",
                tag: "MAGE",
                leaderId: leaderId,
                description: "An ancient order of magic wielders dedicated to the pursuit of arcane knowledge. " +
                           "We delve into forgotten ruins, decipher ancient texts, and push the boundaries of magical theory. " +
                           "Our members value intelligence, discipline, and the responsible use of power. " +
                           "Together, we unravel the mysteries of the cosmos.",
                metadata: metadata,
                maxMembers: 75
            );
        }

        /// <summary>
        /// Thieves Syndicate - PvP and trade focused guild for rogues and opportunists.
        /// </summary>
        private static void RegisterThievesSyndicate(GuildSystemIntegration integration, int leaderId)
        {
            var metadata = GuildMetadata.Create(
                emblemPath: "Guilds/Emblems/Thieves",
                bannerColor: new Color(0.2f, 0.2f, 0.2f), // Shadow Black
                secondaryColor: new Color(0.6f, 0.5f, 0.3f), // Tarnished Silver
                motto: "Fortune Favors the Bold, Shadows Conceal the Swift",
                foundedDate: "Year 721, Age of Secrets",
                headquarters: "The Undercroft, Old City Sewers",
                recruitmentStatus: "Closed",
                minimumLevel: 20,
                pve: false,
                pvp: true,
                trade: true,
                roleplay: false,
                website: "",
                discord: ""
            );

            integration.RegisterGuild(
                guildId: GuildIds.ThievesSyndicate,
                name: "Shadow Syndicate",
                tag: "SHDW",
                leaderId: leaderId,
                description: "A clandestine network of master thieves, cunning smugglers, and information brokers. " +
                           "We operate in the shadows, always one step ahead of the law and our competitors. " +
                           "Whether it's acquiring rare goods through alternative means, gathering intelligence, " +
                           "or eliminating obstacles, we get the job done. Discretion and loyalty are paramount.",
                metadata: metadata,
                maxMembers: 50
            );
        }

        /// <summary>
        /// Merchant Consortium - Trade focused guild for crafters and traders.
        /// </summary>
        private static void RegisterMerchantConsortium(GuildSystemIntegration integration, int leaderId)
        {
            var metadata = GuildMetadata.Create(
                emblemPath: "Guilds/Emblems/Merchants",
                bannerColor: new Color(0.9f, 0.7f, 0.2f), // Golden Yellow
                secondaryColor: new Color(0.3f, 0.6f, 0.3f), // Emerald Green
                motto: "Gold Opens All Doors, Trade Builds All Kingdoms",
                foundedDate: "Year 503, Age of Commerce",
                headquarters: "Grand Bazaar, Trade District",
                recruitmentStatus: "Open",
                minimumLevel: 5,
                pve: false,
                pvp: false,
                trade: true,
                roleplay: true,
                website: "https://merchant-consortium.example.com",
                discord: "https://discord.gg/merchants"
            );

            integration.RegisterGuild(
                guildId: GuildIds.MerchantConsortium,
                name: "Golden Trade Consortium",
                tag: "GOLD",
                leaderId: leaderId,
                description: "A powerful alliance of merchants, master craftsmen, and shrewd traders. " +
                           "We control the flow of goods across the realm, set market prices, and broker deals between kingdoms. " +
                           "Our members benefit from shared resources, bulk discounts, exclusive trade routes, and insider knowledge. " +
                           "Together, we turn profit into power and gold into influence.",
                metadata: metadata,
                maxMembers: 150
            );
        }

        /// <summary>
        /// Holy Order - PvE and roleplay focused guild for healers and paladins.
        /// </summary>
        private static void RegisterHolyOrder(GuildSystemIntegration integration, int leaderId)
        {
            var metadata = GuildMetadata.Create(
                emblemPath: "Guilds/Emblems/Holy",
                bannerColor: new Color(0.95f, 0.95f, 0.85f), // Holy White
                secondaryColor: new Color(0.9f, 0.7f, 0.2f), // Sacred Gold
                motto: "Faith Guides Our Path, Light Conquers Darkness",
                foundedDate: "Year 1, Age of Divine Revelation",
                headquarters: "Cathedral of Eternal Light, Holy City",
                recruitmentStatus: "Open",
                minimumLevel: 8,
                pve: true,
                pvp: false,
                trade: false,
                roleplay: true,
                website: "https://holy-order.example.com",
                discord: "https://discord.gg/holyorder"
            );

            integration.RegisterGuild(
                guildId: GuildIds.HolyOrder,
                name: "Order of the Sacred Flame",
                tag: "HOLY",
                leaderId: leaderId,
                description: "A righteous order of paladins, priests, and faithful defenders dedicated to the service of the divine. " +
                           "We stand as a beacon of hope against the forces of darkness, providing healing to the wounded, " +
                           "protection to the innocent, and justice to the wicked. Our members walk the path of virtue, " +
                           "bound by sacred oaths to uphold truth, compassion, and courage.",
                metadata: metadata,
                maxMembers: 80
            );
        }

        /// <summary>
        /// Get guild ID constants for easy reference in game code.
        /// </summary>
        public static class GuildIds
        {
            public const string WarriorsGuild = "guild_warriors";
            public const string MagesConclave = "guild_mages_conclave";
            public const string ThievesSyndicate = "guild_thieves_syndicate";
            public const string MerchantConsortium = "guild_merchant_consortium";
            public const string HolyOrder = "guild_holy_order";
        }
    }
}
