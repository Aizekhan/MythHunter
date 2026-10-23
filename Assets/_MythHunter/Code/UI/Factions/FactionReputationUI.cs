using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.Factions.Runtime.Data;
using MythHunter.GameLogic.Factions;

namespace MythHunter.UI.Factions
{
    /// <summary>
    /// UI controller for displaying faction reputation and relationships.
    /// Shows player standing with each faction and reputation progress bars.
    /// </summary>
    public class FactionReputationUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject factionPanel;
        [SerializeField] private Transform factionListContainer;

        [Header("Prefabs")]
        [SerializeField] private GameObject factionEntryPrefab;

        [Header("Detail View")]
        [SerializeField] private GameObject factionDetailPanel;
        [SerializeField] private TextMeshProUGUI factionNameText;
        [SerializeField] private TextMeshProUGUI factionTypeText;
        [SerializeField] private TextMeshProUGUI reputationValueText;
        [SerializeField] private TextMeshProUGUI standingText;
        [SerializeField] private Slider reputationSlider;
        [SerializeField] private Transform relationshipContainer;
        [SerializeField] private GameObject relationshipEntryPrefab;

        private FactionSystemIntegration _factionIntegration;
        private int _currentEntityId;
        private List<GameObject> _activeFactionEntries = new List<GameObject>();
        private string _selectedFactionId;

        private void Awake()
        {
            // Hide panels by default
            if (factionPanel != null)
            {
                factionPanel.SetActive(false);
            }

            if (factionDetailPanel != null)
            {
                factionDetailPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Initialize the faction UI with the integration system
        /// </summary>
        public void Initialize(FactionSystemIntegration factionIntegration, int entityId)
        {
            _factionIntegration = factionIntegration;
            _currentEntityId = entityId;
        }

        /// <summary>
        /// Open the faction reputation panel
        /// </summary>
        public void OpenFactionPanel()
        {
            if (factionPanel != null)
            {
                factionPanel.SetActive(true);
                RefreshFactionList();
            }
        }

        /// <summary>
        /// Close the faction reputation panel
        /// </summary>
        public void CloseFactionPanel()
        {
            if (factionPanel != null)
            {
                factionPanel.SetActive(false);
            }

            if (factionDetailPanel != null)
            {
                factionDetailPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Refresh the faction list with current reputation data
        /// </summary>
        public void RefreshFactionList()
        {
            // Clear existing entries
            foreach (var entry in _activeFactionEntries)
            {
                Destroy(entry);
            }
            _activeFactionEntries.Clear();

            // Get all factions
            var allFactions = _factionIntegration.GetAllFactions();

            // Create faction entry for each faction
            foreach (var faction in allFactions)
            {
                CreateFactionEntry(faction);
            }
        }

        /// <summary>
        /// Create a faction list entry
        /// </summary>
        private void CreateFactionEntry(IFaction faction)
        {
            if (factionEntryPrefab == null || factionListContainer == null) return;

            var entry = Instantiate(factionEntryPrefab, factionListContainer);
            _activeFactionEntries.Add(entry);

            // Get reputation data
            var reputation = _factionIntegration.GetReputation(_currentEntityId, faction.FactionId);
            var standing = _factionIntegration.GetStanding(_currentEntityId, faction.FactionId);

            // Setup entry components
            var nameText = entry.transform.Find("FactionName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = faction.Name;
            }

            var typeText = entry.transform.Find("FactionType")?.GetComponent<TextMeshProUGUI>();
            if (typeText != null)
            {
                typeText.text = faction.FactionType.ToString();
            }

            var standingText = entry.transform.Find("Standing")?.GetComponent<TextMeshProUGUI>();
            if (standingText != null)
            {
                standingText.text = standing.ToString();
                standingText.color = GetStandingColor(standing);
            }

            var repBar = entry.transform.Find("ReputationBar")?.GetComponent<Slider>();
            if (repBar != null)
            {
                // Map reputation (-100 to +100) to slider (0 to 1)
                repBar.value = (reputation + 100f) / 200f;
                repBar.fillRect.GetComponent<Image>().color = GetStandingColor(standing);
            }

            // Setup click handler
            var button = entry.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnFactionSelected(faction.FactionId));
            }
        }

        /// <summary>
        /// Handle faction selection to show detail view
        /// </summary>
        private void OnFactionSelected(string factionId)
        {
            _selectedFactionId = factionId;
            ShowFactionDetails(factionId);
        }

        /// <summary>
        /// Show detailed faction information
        /// </summary>
        private void ShowFactionDetails(string factionId)
        {
            if (factionDetailPanel == null) return;

            var faction = _factionIntegration.GetFaction(factionId);
            if (faction == null) return;

            factionDetailPanel.SetActive(true);

            // Get reputation data
            var reputation = _factionIntegration.GetReputation(_currentEntityId, factionId);
            var standing = _factionIntegration.GetStanding(_currentEntityId, factionId);

            // Display faction info
            if (factionNameText != null)
            {
                factionNameText.text = faction.Name;
            }

            if (factionTypeText != null)
            {
                factionTypeText.text = $"Type: {faction.FactionType}";
            }

            if (reputationValueText != null)
            {
                reputationValueText.text = $"Reputation: {reputation:+0;-0}";
            }

            if (standingText != null)
            {
                standingText.text = standing.ToString();
                standingText.color = GetStandingColor(standing);
            }

            if (reputationSlider != null)
            {
                // Show progress within current standing tier
                var (min, max) = GetStandingRange(standing);
                var progress = (reputation - min) / (float)(max - min);
                reputationSlider.value = Mathf.Clamp01(progress);
                reputationSlider.fillRect.GetComponent<Image>().color = GetStandingColor(standing);
            }

            // Display relationships
            DisplayRelationships(factionId);
        }

        /// <summary>
        /// Display faction relationships
        /// </summary>
        private void DisplayRelationships(string factionId)
        {
            if (relationshipContainer == null || relationshipEntryPrefab == null) return;

            // Clear existing relationships
            foreach (Transform child in relationshipContainer)
            {
                Destroy(child.gameObject);
            }

            // Get all relationships for this faction
            var relationships = _factionIntegration.GetRelationships(factionId);

            foreach (var relationship in relationships)
            {
                var entry = Instantiate(relationshipEntryPrefab, relationshipContainer);

                // Get the other faction in the relationship
                var otherFactionId = relationship.FactionAId == factionId
                    ? relationship.FactionBId
                    : relationship.FactionAId;

                var otherFaction = _factionIntegration.GetFaction(otherFactionId);
                if (otherFaction == null) continue;

                // Display relationship info
                var factionNameText = entry.transform.Find("FactionName")?.GetComponent<TextMeshProUGUI>();
                if (factionNameText != null)
                {
                    factionNameText.text = otherFaction.Name;
                }

                var relationTypeText = entry.transform.Find("RelationType")?.GetComponent<TextMeshProUGUI>();
                if (relationTypeText != null)
                {
                    relationTypeText.text = relationship.RelationType.ToString();
                    relationTypeText.color = GetRelationTypeColor(relationship.RelationType);
                }

                var modifierText = entry.transform.Find("Modifier")?.GetComponent<TextMeshProUGUI>();
                if (modifierText != null)
                {
                    modifierText.text = $"Cascade: {relationship.CascadeModifier:+0.0;-0.0}x";
                }
            }
        }

        /// <summary>
        /// Get color for faction standing
        /// </summary>
        private Color GetStandingColor(FactionStanding standing)
        {
            return standing switch
            {
                FactionStanding.Hated => new Color(0.5f, 0f, 0f),      // Dark Red
                FactionStanding.Hostile => new Color(1f, 0.2f, 0f),    // Red
                FactionStanding.Unfriendly => new Color(1f, 0.5f, 0f), // Orange
                FactionStanding.Neutral => new Color(0.8f, 0.8f, 0.8f), // Gray
                FactionStanding.Friendly => new Color(0.5f, 1f, 0.5f), // Light Green
                FactionStanding.Honored => new Color(0f, 0.8f, 0f),    // Green
                FactionStanding.Revered => new Color(0f, 0.5f, 1f),    // Blue
                _ => Color.white
            };
        }

        /// <summary>
        /// Get color for relationship type
        /// </summary>
        private Color GetRelationTypeColor(FactionRelationType relationType)
        {
            return relationType switch
            {
                FactionRelationType.Enemy => new Color(1f, 0.2f, 0f),    // Red
                FactionRelationType.Rival => new Color(1f, 0.5f, 0f),    // Orange
                FactionRelationType.Neutral => new Color(0.8f, 0.8f, 0.8f), // Gray
                FactionRelationType.Friendly => new Color(0.5f, 1f, 0.5f), // Light Green
                FactionRelationType.Allied => new Color(0f, 0.8f, 0f),   // Green
                _ => Color.white
            };
        }

        /// <summary>
        /// Get reputation range for a standing tier
        /// </summary>
        private (int min, int max) GetStandingRange(FactionStanding standing)
        {
            return standing switch
            {
                FactionStanding.Hated => (-100, -75),
                FactionStanding.Hostile => (-74, -50),
                FactionStanding.Unfriendly => (-49, -25),
                FactionStanding.Neutral => (-24, 24),
                FactionStanding.Friendly => (25, 49),
                FactionStanding.Honored => (50, 74),
                FactionStanding.Revered => (75, 100),
                _ => (-100, 100)
            };
        }

        /// <summary>
        /// Update reputation display (call this when reputation changes)
        /// </summary>
        public void UpdateReputation(string factionId)
        {
            // Refresh the entire list to show updated values
            RefreshFactionList();

            // If this faction is currently selected, refresh detail view
            if (_selectedFactionId == factionId)
            {
                ShowFactionDetails(factionId);
            }
        }

        /// <summary>
        /// Public method to check if faction panel is open
        /// </summary>
        public bool IsPanelOpen()
        {
            return factionPanel != null && factionPanel.activeSelf;
        }

        private void OnDestroy()
        {
            // Cleanup button listeners
            foreach (var entry in _activeFactionEntries)
            {
                if (entry != null)
                {
                    var button = entry.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }
}
