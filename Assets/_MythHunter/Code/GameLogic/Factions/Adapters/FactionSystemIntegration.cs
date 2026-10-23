using System;
using RPGFramework.ECS;
using RPGFramework.Factions;
using UnityEngine;

namespace MythHunter.GameLogic.Factions
{
    /// <summary>
    /// Integration adapter between Framework Faction System and MythHunter game layer.
    /// Manages lifecycle of faction components and provides game-specific utilities.
    /// </summary>
    public class FactionSystemIntegration
    {
        private readonly IEntityManager _entityManager;
        private readonly IFactionRegistry _factionRegistry;
        private readonly IFactionManager _factionManager;

        public IFactionManager FactionManager => _factionManager;
        public IFactionRegistry FactionRegistry => _factionRegistry;

        public FactionSystemIntegration(IEntityManager entityManager, IFactionRegistry factionRegistry)
        {
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _factionRegistry = factionRegistry ?? throw new ArgumentNullException(nameof(factionRegistry));
            _factionManager = new FactionManager(entityManager, factionRegistry);
        }

        /// <summary>
        /// Create and initialize a MythHunter faction component for a GameObject.
        /// </summary>
        public MythHunterFactionComponent CreateFactionComponent(GameObject gameObject, int entityId)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            var component = gameObject.GetComponent<MythHunterFactionComponent>();
            if (component == null)
            {
                component = gameObject.AddComponent<MythHunterFactionComponent>();
            }

            component.Initialize(entityId, _factionManager);
            return component;
        }

        /// <summary>
        /// Register a faction in the registry (game layer convenience method).
        /// </summary>
        public void RegisterFaction(string factionId, string name, FactionType factionType,
            string description = "", int defaultReputation = 0)
        {
            var faction = new Faction(factionId, name, description, factionType, defaultReputation);
            _factionRegistry.RegisterFaction(faction);
        }

        /// <summary>
        /// Register a relationship between two factions (game layer convenience method).
        /// </summary>
        public void RegisterRelationship(string factionId1, string factionId2, FactionRelationType relationType)
        {
            var relationship = FactionRelationship.Create(factionId1, factionId2, relationType);
            _factionRegistry.RegisterRelationship(relationship);
        }

        /// <summary>
        /// Get a user-friendly string representation of a faction standing.
        /// </summary>
        public string GetStandingDisplayName(FactionStanding standing)
        {
            return standing switch
            {
                FactionStanding.Hated => "Hated",
                FactionStanding.Hostile => "Hostile",
                FactionStanding.Unfriendly => "Unfriendly",
                FactionStanding.Neutral => "Neutral",
                FactionStanding.Friendly => "Friendly",
                FactionStanding.Honored => "Honored",
                FactionStanding.Revered => "Revered",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Get a color representing a faction standing (for UI purposes).
        /// </summary>
        public Color GetStandingColor(FactionStanding standing)
        {
            return standing switch
            {
                FactionStanding.Hated => new Color(0.6f, 0f, 0f),      // Dark Red
                FactionStanding.Hostile => new Color(1f, 0.2f, 0f),    // Red-Orange
                FactionStanding.Unfriendly => new Color(1f, 0.5f, 0f), // Orange
                FactionStanding.Neutral => new Color(0.8f, 0.8f, 0.8f), // Gray
                FactionStanding.Friendly => new Color(0.5f, 1f, 0.5f), // Light Green
                FactionStanding.Honored => new Color(0f, 0.8f, 0f),    // Green
                FactionStanding.Revered => new Color(0f, 0.6f, 1f),    // Cyan
                _ => Color.white
            };
        }

        /// <summary>
        /// Check if an entity can interact with a faction-restricted feature.
        /// </summary>
        public bool CanInteractWithFaction(int entityId, string factionId, FactionStanding minimumStanding)
        {
            return _factionManager.HasMinimumStanding(entityId, factionId, minimumStanding);
        }

        /// <summary>
        /// Award reputation to an entity from a specific source (with logging).
        /// </summary>
        public void AwardReputation(int entityId, string factionId, int amount, string reason)
        {
            var oldReputation = _factionManager.GetReputation(entityId, factionId);
            var oldStanding = _factionManager.GetStanding(entityId, factionId);

            _factionManager.ModifyReputation(entityId, factionId, amount);

            var newReputation = _factionManager.GetReputation(entityId, factionId);
            var newStanding = _factionManager.GetStanding(entityId, factionId);

            UnityEngine.Debug.Log($"[FactionSystem] Entity {entityId} reputation with '{factionId}': " +
                      $"{oldReputation} → {newReputation} (Δ{amount:+#;-#;0}) - {reason}");

            if (oldStanding != newStanding)
            {
                UnityEngine.Debug.Log($"[FactionSystem] Entity {entityId} standing with '{factionId}' changed: " +
                          $"{GetStandingDisplayName(oldStanding)} → {GetStandingDisplayName(newStanding)}");
            }
        }
    }
}
