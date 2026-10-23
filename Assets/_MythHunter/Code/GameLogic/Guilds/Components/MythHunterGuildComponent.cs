using RPGFramework.Guilds.Runtime;
using UnityEngine;

namespace MythHunter.GameLogic.Guilds
{
    /// <summary>
    /// MythHunter-specific MonoBehaviour component for Guild System.
    /// Dual-component pattern: provides Unity integration while delegating to Framework ECS.
    /// This component represents a guild entity in the Unity scene with visual and metadata.
    /// </summary>
    public class MythHunterGuildComponent : MonoBehaviour
    {
        [Header("Guild Reference")]
        [SerializeField]
        [Tooltip("The unique guild identifier")]
        private string _guildId = string.Empty;

        [Header("Guild Metadata")]
        [SerializeField]
        [Tooltip("MythHunter-specific guild visual and social metadata")]
        private GuildMetadata _metadata;

        [Header("Debug Info")]
        [SerializeField]
        [Tooltip("Display current guild information in Inspector")]
        private bool _showDebugInfo = false;

        private IGuildManager _guildManager;
        private IGuild _guild;

        /// <summary>
        /// The unique guild identifier.
        /// </summary>
        public string GuildId
        {
            get => _guildId;
            set => _guildId = value;
        }

        /// <summary>
        /// MythHunter-specific guild metadata.
        /// </summary>
        public GuildMetadata Metadata
        {
            get => _metadata;
            set => _metadata = value;
        }

        /// <summary>
        /// Reference to the Framework guild instance.
        /// </summary>
        public IGuild Guild => _guild;

        /// <summary>
        /// Initialize with GuildManager from integration layer.
        /// Called by GuildSystemIntegration.
        /// </summary>
        public void Initialize(string guildId, IGuildManager guildManager, GuildMetadata metadata)
        {
            _guildId = guildId;
            _guildManager = guildManager;
            _metadata = metadata;

            // Get guild reference from manager
            if (_guildManager != null)
            {
                _guild = _guildManager.GetGuild(_guildId);
                if (_guild == null)
                {
                    UnityEngine.Debug.LogWarning($"[MythHunterGuildComponent] Guild '{_guildId}' not found in GuildManager");
                }
            }
        }

        /// <summary>
        /// Refresh guild reference (call after guild operations).
        /// </summary>
        public void RefreshGuild()
        {
            if (_guildManager != null)
            {
                _guild = _guildManager.GetGuild(_guildId);
            }
        }

        /// <summary>
        /// Get guild name.
        /// </summary>
        public string GetGuildName()
        {
            if (_guild == null)
            {
                UnityEngine.Debug.LogWarning($"[MythHunterGuildComponent] Guild not initialized");
                return "Unknown";
            }

            return _guild.Name;
        }

        /// <summary>
        /// Get guild tag.
        /// </summary>
        public string GetGuildTag()
        {
            if (_guild == null)
            {
                UnityEngine.Debug.LogWarning($"[MythHunterGuildComponent] Guild not initialized");
                return string.Empty;
            }

            return _guild.Tag;
        }

        /// <summary>
        /// Get current member count.
        /// </summary>
        public int GetMemberCount()
        {
            if (_guild == null)
            {
                UnityEngine.Debug.LogWarning($"[MythHunterGuildComponent] Guild not initialized");
                return 0;
            }

            return _guild.MemberCount;
        }

        /// <summary>
        /// Get guild level.
        /// </summary>
        public int GetLevel()
        {
            if (_guild == null)
            {
                UnityEngine.Debug.LogWarning($"[MythHunterGuildComponent] Guild not initialized");
                return 1;
            }

            return _guild.Level;
        }

        /// <summary>
        /// Get message of the day.
        /// </summary>
        public string GetMessageOfTheDay()
        {
            if (_guild == null)
            {
                UnityEngine.Debug.LogWarning($"[MythHunterGuildComponent] Guild not initialized");
                return string.Empty;
            }

            return _guild.MessageOfTheDay;
        }

        /// <summary>
        /// Check if guild is accepting new members.
        /// </summary>
        public bool IsRecruiting()
        {
            if (_guild == null)
            {
                return false;
            }

            // Guild must not be full and recruitment status must indicate openness
            return !_guild.IsFull() &&
                   (_metadata.RecruitmentStatus.ToLower() == "open" ||
                    _metadata.RecruitmentStatus.ToLower() == "selective");
        }

        /// <summary>
        /// Check if an entity can join this guild based on metadata requirements.
        /// </summary>
        public bool CanJoin(int entityLevel)
        {
            return entityLevel >= _metadata.MinimumLevel && IsRecruiting();
        }

        /// <summary>
        /// Get formatted guild information for UI display.
        /// </summary>
        public string GetGuildInfo()
        {
            if (_guild == null)
            {
                return "Guild not initialized";
            }

            return $"{_guild.Name} [{_guild.Tag}]\n" +
                   $"Level {_guild.Level} | Members: {_guild.MemberCount}/{_guild.MaxMembers}\n" +
                   $"Motto: {_metadata.Motto}\n" +
                   $"Focus: {_metadata.GetFocusAreas()}\n" +
                   $"Recruitment: {_metadata.RecruitmentStatus} (Min Level {_metadata.MinimumLevel})\n" +
                   $"Headquarters: {_metadata.HeadquartersLocation}";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_showDebugInfo && _guild != null)
            {
                // Debug info displayed in custom inspector
                UnityEngine.Debug.Log(GetGuildInfo());
            }
        }
#endif
    }
}
