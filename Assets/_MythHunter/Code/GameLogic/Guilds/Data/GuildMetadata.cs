using System;
using UnityEngine;

namespace MythHunter.GameLogic.Guilds
{
    /// <summary>
    /// MythHunter-specific guild metadata.
    /// Contains game-specific visual and social information for guilds.
    /// This data is separate from Framework GuildData to maintain separation of concerns.
    /// </summary>
    [Serializable]
    public struct GuildMetadata
    {
        /// <summary>
        /// Path to the guild emblem/logo sprite in the Resources folder.
        /// </summary>
        [Tooltip("Path to guild emblem sprite in Resources")]
        public string EmblemPath;

        /// <summary>
        /// Primary color for guild banners and UI elements.
        /// </summary>
        [Tooltip("Primary guild color for banners and UI")]
        public Color BannerColor;

        /// <summary>
        /// Secondary color for guild decorations.
        /// </summary>
        [Tooltip("Secondary guild color for accents")]
        public Color SecondaryColor;

        /// <summary>
        /// Guild website or external link.
        /// </summary>
        [Tooltip("Guild website URL (optional)")]
        public string Website;

        /// <summary>
        /// Discord server invite link.
        /// </summary>
        [Tooltip("Discord server invite link (optional)")]
        public string DiscordInvite;

        /// <summary>
        /// Guild motto or slogan.
        /// </summary>
        [Tooltip("Guild motto or slogan")]
        public string Motto;

        /// <summary>
        /// Date when the guild was founded (in-game lore date).
        /// </summary>
        [Tooltip("In-game founding date for lore purposes")]
        public string FoundedDate;

        /// <summary>
        /// Guild headquarters location name.
        /// </summary>
        [Tooltip("Name of the guild's headquarters location")]
        public string HeadquartersLocation;

        /// <summary>
        /// Recruitment status message.
        /// </summary>
        [Tooltip("Current recruitment status (e.g., 'Open', 'Closed', 'Selective')")]
        public string RecruitmentStatus;

        /// <summary>
        /// Minimum level requirement for joining.
        /// </summary>
        [Tooltip("Minimum character level to join")]
        public int MinimumLevel;

        /// <summary>
        /// Whether this guild focuses on PvE content.
        /// </summary>
        [Tooltip("Guild focuses on PvE content")]
        public bool IsPvEFocused;

        /// <summary>
        /// Whether this guild focuses on PvP content.
        /// </summary>
        [Tooltip("Guild focuses on PvP content")]
        public bool IsPvPFocused;

        /// <summary>
        /// Whether this guild focuses on crafting/trading.
        /// </summary>
        [Tooltip("Guild focuses on crafting and trading")]
        public bool IsTradeFocused;

        /// <summary>
        /// Whether this guild focuses on roleplaying.
        /// </summary>
        [Tooltip("Guild focuses on roleplay activities")]
        public bool IsRoleplayFocused;

        /// <summary>
        /// Create default guild metadata.
        /// </summary>
        public static GuildMetadata CreateDefault(string guildName)
        {
            return new GuildMetadata
            {
                EmblemPath = "Guilds/Emblems/Default",
                BannerColor = new Color(0.2f, 0.4f, 0.8f),
                SecondaryColor = Color.white,
                Website = string.Empty,
                DiscordInvite = string.Empty,
                Motto = $"Honor and glory to {guildName}!",
                FoundedDate = "Year 1",
                HeadquartersLocation = "Unknown",
                RecruitmentStatus = "Open",
                MinimumLevel = 1,
                IsPvEFocused = true,
                IsPvPFocused = false,
                IsTradeFocused = false,
                IsRoleplayFocused = false
            };
        }

        /// <summary>
        /// Create guild metadata with specific focus areas.
        /// </summary>
        public static GuildMetadata Create(
            string emblemPath,
            Color bannerColor,
            Color secondaryColor,
            string motto,
            string foundedDate,
            string headquarters,
            string recruitmentStatus,
            int minimumLevel,
            bool pve = false,
            bool pvp = false,
            bool trade = false,
            bool roleplay = false,
            string website = "",
            string discord = "")
        {
            return new GuildMetadata
            {
                EmblemPath = emblemPath,
                BannerColor = bannerColor,
                SecondaryColor = secondaryColor,
                Website = website,
                DiscordInvite = discord,
                Motto = motto,
                FoundedDate = foundedDate,
                HeadquartersLocation = headquarters,
                RecruitmentStatus = recruitmentStatus,
                MinimumLevel = minimumLevel,
                IsPvEFocused = pve,
                IsPvPFocused = pvp,
                IsTradeFocused = trade,
                IsRoleplayFocused = roleplay
            };
        }

        /// <summary>
        /// Get a formatted string of guild focus areas.
        /// </summary>
        public string GetFocusAreas()
        {
            var areas = new System.Collections.Generic.List<string>();
            if (IsPvEFocused) areas.Add("PvE");
            if (IsPvPFocused) areas.Add("PvP");
            if (IsTradeFocused) areas.Add("Trade");
            if (IsRoleplayFocused) areas.Add("Roleplay");

            return areas.Count > 0 ? string.Join(", ", areas) : "General";
        }
    }
}
