using RPGFramework.Factions;
using UnityEngine;

namespace MythHunter.GameLogic.Factions
{
    /// <summary>
    /// MythHunter-specific MonoBehaviour component for Faction System.
    /// Dual-component pattern: provides Unity integration while delegating to Framework ECS.
    /// </summary>
    public class MythHunterFactionComponent : MonoBehaviour
    {
        [Header("Entity Reference")]
        [SerializeField]
        [Tooltip("The ECS entity ID this faction component is attached to")]
        private int _entityId = -1;

        [Header("Initial Faction Configuration")]
        [SerializeField]
        [Tooltip("Initialize entity with default reputations for all factions on Start")]
        private bool _initializeOnStart = true;

        [Header("Debug Info")]
        [SerializeField]
        [Tooltip("Display current faction standings in Inspector")]
        private bool _showDebugInfo = false;

        private IFactionManager _factionManager;

        public int EntityId
        {
            get => _entityId;
            set => _entityId = value;
        }

        /// <summary>
        /// Initialize with FactionManager from integration layer.
        /// Called by FactionSystemIntegration.
        /// </summary>
        public void Initialize(int entityId, IFactionManager factionManager)
        {
            _entityId = entityId;
            _factionManager = factionManager;

            if (_initializeOnStart)
            {
                _factionManager.InitializeEntityReputations(_entityId);
            }
        }

        /// <summary>
        /// Get current reputation with a faction.
        /// </summary>
        public int GetReputation(string factionId)
        {
            if (_factionManager == null || _entityId < 0)
            {
                Debug.LogWarning($"[MythHunterFactionComponent] Cannot get reputation - not initialized");
                return 0;
            }

            return _factionManager.GetReputation(_entityId, factionId);
        }

        /// <summary>
        /// Get current standing with a faction.
        /// </summary>
        public FactionStanding GetStanding(string factionId)
        {
            if (_factionManager == null || _entityId < 0)
            {
                Debug.LogWarning($"[MythHunterFactionComponent] Cannot get standing - not initialized");
                return FactionStanding.Neutral;
            }

            return _factionManager.GetStanding(_entityId, factionId);
        }

        /// <summary>
        /// Modify reputation with a faction.
        /// </summary>
        public void ModifyReputation(string factionId, int delta)
        {
            if (_factionManager == null || _entityId < 0)
            {
                Debug.LogWarning($"[MythHunterFactionComponent] Cannot modify reputation - not initialized");
                return;
            }

            _factionManager.ModifyReputation(_entityId, factionId, delta);
        }

        /// <summary>
        /// Check if entity has minimum standing with a faction.
        /// </summary>
        public bool HasMinimumStanding(string factionId, FactionStanding minimumStanding)
        {
            if (_factionManager == null || _entityId < 0)
            {
                Debug.LogWarning($"[MythHunterFactionComponent] Cannot check standing - not initialized");
                return false;
            }

            return _factionManager.HasMinimumStanding(_entityId, factionId, minimumStanding);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_showDebugInfo && _factionManager != null && _entityId >= 0)
            {
                // Debug info displayed in custom inspector
            }
        }
#endif
    }
}
