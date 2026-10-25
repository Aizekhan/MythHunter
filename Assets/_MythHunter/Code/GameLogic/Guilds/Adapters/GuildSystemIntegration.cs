// Fixed: Guild constructor takes only GuildData parameter
// Note: DepositGold/WithdrawGold methods removed - treasury management is separate in Framework
using System;
using System.Collections.Generic;
using RPGFramework.ECS;
using RPGFramework.Factions;
using RPGFramework.Guilds.Runtime;
using UnityEngine;

namespace MythHunter.GameLogic.Guilds
{
    /// <summary>
    /// Integration adapter between Framework Guild System and MythHunter game layer.
    /// Manages lifecycle of guild components and provides game-specific utilities.
    /// </summary>
    public class GuildSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly IFactionRegistry _factionRegistry;
        private readonly IGuildRegistry _guildRegistry;
        private readonly IGuildManager _guildManager;
        private readonly Dictionary<string, GuildMetadata> _guildMetadataRegistry;

        public IGuildManager GuildManager => _guildManager;
        public IGuildRegistry GuildRegistry => _guildRegistry;
        public IFactionRegistry FactionRegistry => _factionRegistry;

        public GuildSystemIntegration(
            IEntityManager entityManager,
            IFactionRegistry factionRegistry,
            IGuildRegistry guildRegistry)
        {
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _factionRegistry = factionRegistry ?? throw new ArgumentNullException(nameof(factionRegistry));
            _guildRegistry = guildRegistry ?? throw new ArgumentNullException(nameof(guildRegistry));
            _guildManager = new GuildManager(entityManager, factionRegistry, guildRegistry);
            _guildMetadataRegistry = new Dictionary<string, GuildMetadata>();
        }

        /// <summary>
        /// Create and initialize a MythHunter guild component for a GameObject.
        /// </summary>
        public MythHunterGuildComponent CreateGuildComponent(
            GameObject gameObject,
            string guildId,
            GuildMetadata metadata)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            if (string.IsNullOrEmpty(guildId))
                throw new ArgumentException("Guild ID cannot be null or empty", nameof(guildId));

            var component = gameObject.GetComponent<MythHunterGuildComponent>();
            if (component == null)
            {
                component = gameObject.AddComponent<MythHunterGuildComponent>();
            }

            component.Initialize(guildId, _guildManager, metadata);

            // Register metadata
            if (!_guildMetadataRegistry.ContainsKey(guildId))
            {
                _guildMetadataRegistry.Add(guildId, metadata);
            }

            return component;
        }

        /// <summary>
        /// Register a guild in the registry with MythHunter metadata (game layer convenience method).
        /// </summary>
        public IGuild RegisterGuild(
            string guildId,
            string name,
            string tag,
            int leaderId,
            string description,
            GuildMetadata metadata,
            int maxMembers = 100)
        {
            // First register as faction
            var faction = new Faction(guildId, name, description, FactionType.Guild, defaultReputation: 0);
            _factionRegistry.RegisterFaction(faction);

            // Create and register guild
            var guildData = GuildData.Create(guildId, name, tag, leaderId, description, maxMembers);
            var guild = new Guild(guildData);
            _guildRegistry.RegisterGuild(guild);

            // Store metadata
            if (!_guildMetadataRegistry.ContainsKey(guildId))
            {
                _guildMetadataRegistry.Add(guildId, metadata);
            }
            else
            {
                _guildMetadataRegistry[guildId] = metadata;
            }

            UnityEngine.Debug.Log($"[GuildSystemIntegration] Registered guild '{name}' [{tag}] with ID '{guildId}'");

            return guild;
        }

        /// <summary>
        /// Get metadata for a guild.
        /// </summary>
        public GuildMetadata? GetGuildMetadata(string guildId)
        {
            if (_guildMetadataRegistry.TryGetValue(guildId, out var metadata))
            {
                return metadata;
            }

            return null;
        }

        /// <summary>
        /// Update metadata for a guild.
        /// </summary>
        public void UpdateGuildMetadata(string guildId, GuildMetadata metadata)
        {
            if (_guildMetadataRegistry.ContainsKey(guildId))
            {
                _guildMetadataRegistry[guildId] = metadata;
            }
            else
            {
                _guildMetadataRegistry.Add(guildId, metadata);
            }
        }

        /// <summary>
        /// Add a member to a guild with validation (game layer convenience method).
        /// </summary>
        public bool AddMemberToGuild(string guildId, int entityId, string reason = "")
        {
            var guild = _guildRegistry.GetGuild(guildId);
            if (guild == null)
            {
                UnityEngine.Debug.LogWarning($"[GuildSystemIntegration] Guild '{guildId}' not found");
                return false;
            }

            if (guild.IsFull())
            {
                UnityEngine.Debug.LogWarning($"[GuildSystemIntegration] Guild '{guild.Name}' is full");
                return false;
            }

            var success = _guildManager.AddMember(guildId, entityId, GuildRankType.Initiate);

            if (success)
            {
                var reasonText = string.IsNullOrEmpty(reason) ? "" : $" - {reason}";
                UnityEngine.Debug.Log($"[GuildSystemIntegration] Entity {entityId} joined guild '{guild.Name}'{reasonText}");
            }

            return success;
        }

        /// <summary>
        /// Remove a member from a guild (game layer convenience method).
        /// </summary>
        public bool RemoveMemberFromGuild(string guildId, int entityId, string reason = "")
        {
            var guild = _guildRegistry.GetGuild(guildId);
            if (guild == null)
            {
                UnityEngine.Debug.LogWarning($"[GuildSystemIntegration] Guild '{guildId}' not found");
                return false;
            }

            var success = _guildManager.RemoveMember(guildId, entityId);

            if (success)
            {
                var reasonText = string.IsNullOrEmpty(reason) ? "" : $" - {reason}";
                UnityEngine.Debug.Log($"[GuildSystemIntegration] Entity {entityId} left guild '{guild.Name}'{reasonText}");
            }

            return success;
        }

        /// <summary>
        /// Promote a member to a higher rank (game layer convenience method).
        /// </summary>
        public bool PromoteMember(string guildId, int entityId, GuildRankType newRank, string reason = "")
        {
            var guild = _guildRegistry.GetGuild(guildId);
            if (guild == null)
            {
                UnityEngine.Debug.LogWarning($"[GuildSystemIntegration] Guild '{guildId}' not found");
                return false;
            }

            var success = _guildManager.PromoteMember(guildId, entityId, newRank);

            if (success)
            {
                var reasonText = string.IsNullOrEmpty(reason) ? "" : $" - {reason}";
                UnityEngine.Debug.Log($"[GuildSystemIntegration] Entity {entityId} promoted to {newRank} in guild '{guild.Name}'{reasonText}");
            }

            return success;
        }

        /// <summary>
        /// Get a user-friendly string representation of a guild rank.
        /// </summary>
        public string GetRankDisplayName(GuildRankType rank)
        {
            return rank switch
            {
                GuildRankType.Initiate => "Initiate",
                GuildRankType.Member => "Member",
                GuildRankType.Officer => "Officer",
                GuildRankType.Elder => "Elder",
                GuildRankType.Leader => "Leader",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Get a color representing a guild rank (for UI purposes).
        /// </summary>
        public Color GetRankColor(GuildRankType rank)
        {
            return rank switch
            {
                GuildRankType.Initiate => new Color(0.6f, 0.6f, 0.6f),  // Gray
                GuildRankType.Member => new Color(0.8f, 0.8f, 0.8f),    // Light Gray
                GuildRankType.Officer => new Color(0.3f, 0.8f, 0.3f),   // Green
                GuildRankType.Elder => new Color(0.3f, 0.5f, 1f),       // Blue
                GuildRankType.Leader => new Color(1f, 0.8f, 0f),        // Gold
                _ => Color.white
            };
        }

        /// <summary>
        /// Check if an entity has permission to perform an action in a guild.
        /// </summary>
        public bool HasPermission(string guildId, int entityId, GuildPermission permission)
        {
            return _guildManager.HasPermission(entityId, permission);
        }

        // TODO: Treasury operations are managed separately in Framework via IGuildTreasury
        // These methods are commented out until proper treasury integration is implemented
        // Framework provides IGuildTreasury.DepositGold/WithdrawGold directly on treasury objects

        /*
        /// <summary>
        /// Deposit gold into guild treasury (game layer convenience method).
        /// </summary>
        public bool DepositGold(string guildId, int entityId, int amount, string reason = "")
        {
            // TODO: Implement using IGuildTreasury instead of IGuildManager
            // var guild = _guildRegistry.GetGuild(guildId);
            // var treasury = guild.Treasury; // This property doesn't exist yet
            // return treasury.DepositGold(amount, entityId);
            return false;
        }

        /// <summary>
        /// Withdraw gold from guild treasury (game layer convenience method).
        /// </summary>
        public bool WithdrawGold(string guildId, int entityId, int amount, string reason = "")
        {
            // Check permission first
            if (!HasPermission(guildId, entityId, GuildPermission.WithdrawGold))
            {
                UnityEngine.Debug.LogWarning($"[GuildSystemIntegration] Entity {entityId} lacks permission to withdraw gold");
                return false;
            }

            // TODO: Implement using IGuildTreasury instead of IGuildManager
            // var guild = _guildRegistry.GetGuild(guildId);
            // var treasury = guild.Treasury; // This property doesn't exist yet
            // return treasury.WithdrawGold(amount, entityId);
            return false;
        }
        */

        /// <summary>
        /// Get formatted guild information for UI display.
        /// </summary>
        public string GetGuildInfoString(string guildId)
        {
            var guild = _guildRegistry.GetGuild(guildId);
            if (guild == null)
            {
                return $"Guild '{guildId}' not found";
            }

            var metadata = GetGuildMetadata(guildId);
            var metadataInfo = metadata.HasValue
                ? $"\nMotto: {metadata.Value.Motto}\nFocus: {metadata.Value.GetFocusAreas()}\nHeadquarters: {metadata.Value.HeadquartersLocation}"
                : "";

            return $"{guild.Name} [{guild.Tag}]\n" +
                   $"Level {guild.Level} | Members: {guild.MemberCount}/{guild.MaxMembers}\n" +
                   $"Experience: {guild.Experience}\n" +
                   $"Created: {guild.CreatedAt:yyyy-MM-dd}{metadataInfo}";
        }

        /// <summary>
        /// Get all guilds sorted by level (descending).
        /// </summary>
        public List<IGuild> GetGuildsByLevel()
        {
            var guilds = new List<IGuild>(_guildRegistry.GetAllGuilds());
            guilds.Sort((a, b) => b.Level.CompareTo(a.Level));
            return guilds;
        }

        /// <summary>
        /// Get all guilds that match specific focus criteria from metadata.
        /// </summary>
        public List<string> GetGuildsByFocus(bool pve = false, bool pvp = false, bool trade = false, bool roleplay = false)
        {
            var matchingGuilds = new List<string>();

            foreach (var kvp in _guildMetadataRegistry)
            {
                var metadata = kvp.Value;
                if ((pve && metadata.IsPvEFocused) ||
                    (pvp && metadata.IsPvPFocused) ||
                    (trade && metadata.IsTradeFocused) ||
                    (roleplay && metadata.IsRoleplayFocused))
                {
                    matchingGuilds.Add(kvp.Key);
                }
            }

            return matchingGuilds;
        }
    }
}
