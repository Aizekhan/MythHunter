using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RPGFramework.Factions;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Faction reputation UI displaying faction standings and relationships.
    /// Keyboard shortcut: F
    /// </summary>
    public class FactionReputationUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("Faction List")]
        [SerializeField] private Transform factionListContainer;
        [SerializeField] private GameObject factionEntryPrefab;

        [Header("Faction Details")]
        [SerializeField] private TextMeshProUGUI factionNameText;
        [SerializeField] private TextMeshProUGUI factionDescriptionText;
        [SerializeField] private Image factionBanner;
        [SerializeField] private TextMeshProUGUI standingText;
        [SerializeField] private Slider reputationBar;
        [SerializeField] private TextMeshProUGUI reputationValueText;

        [Header("Relationships")]
        [SerializeField] private Transform alliesContainer;
        [SerializeField] private Transform enemiesContainer;
        [SerializeField] private GameObject relationshipEntryPrefab;

        [Header("Benefits")]
        [SerializeField] private Transform benefitsContainer;
        [SerializeField] private GameObject benefitEntryPrefab;

        [Header("Colors - Standing")]
        [SerializeField] private Color hatedColor = new Color(0.5f, 0f, 0f);
        [SerializeField] private Color hostileColor = Color.red;
        [SerializeField] private Color unfriendlyColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color neutralColor = Color.yellow;
        [SerializeField] private Color friendlyColor = Color.green;
        [SerializeField] private Color honoredColor = new Color(0f, 0.8f, 0f);
        [SerializeField] private Color exaltedColor = Color.cyan;
        #endregion

        #region Private Fields
        private IFactionManager factionManager;
        private List<GameObject> factionEntries = new List<GameObject>();
        private IFaction selectedFaction;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshFactionList();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Hide();
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            RefreshFactionList();
            Time.timeScale = 0f;
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            Time.timeScale = 1f;
        }

        public void Toggle()
        {
            if (panelRoot != null && panelRoot.activeSelf)
                Hide();
            else
                Show();
        }

        public void Initialize(IFactionManager manager)
        {
            factionManager = manager;
            RefreshFactionList();
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            if (factionManager == null) return;
            // factionManager.OnReputationChanged += OnReputationChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (factionManager == null) return;
            // factionManager.OnReputationChanged -= OnReputationChanged;
        }

        private void OnReputationChanged(IFaction faction, float oldValue, float newValue)
        {
            if (selectedFaction == faction)
                RefreshFactionDetails();
        }
        #endregion

        #region Private Methods - Faction List
        private void RefreshFactionList()
        {
            ClearFactionEntries();

            if (factionManager == null) return;

            var factions = GetKnownFactions();

            foreach (var faction in factions)
            {
                CreateFactionEntry(faction);
            }
        }

        private void CreateFactionEntry(FactionData faction)
        {
            if (factionEntryPrefab == null || factionListContainer == null) return;

            var entry = Instantiate(factionEntryPrefab, factionListContainer);
            factionEntries.Add(entry);

            var nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = faction.Name;
                nameText.color = GetStandingColor(faction.Standing);
            }

            var button = entry.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => SelectFaction(faction));
        }

        private void ClearFactionEntries()
        {
            foreach (var entry in factionEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            factionEntries.Clear();
        }

        private void SelectFaction(FactionData faction)
        {
            selectedFaction = null; // TODO: Set selected faction
            RefreshFactionDetails();
        }
        #endregion

        #region Private Methods - Faction Details
        private void RefreshFactionDetails()
        {
            if (selectedFaction == null) return;

            // Update faction info
            if (factionNameText != null)
                factionNameText.text = selectedFaction.Name;

            if (factionDescriptionText != null)
                factionDescriptionText.text = selectedFaction.Description;

            // Update reputation
            UpdateReputationDisplay();

            // Update relationships
            RefreshRelationships();

            // Update benefits
            RefreshBenefits();
        }

        private void UpdateReputationDisplay()
        {
            // TODO: Get actual reputation values
            float reputation = 0f;
            float minRep = -100f;
            float maxRep = 100f;
            string standing = "Neutral";

            if (standingText != null)
            {
                standingText.text = standing;
                standingText.color = neutralColor; // TODO: Use actual standing color
            }

            if (reputationBar != null)
            {
                reputationBar.minValue = minRep;
                reputationBar.maxValue = maxRep;
                reputationBar.value = reputation;
            }

            if (reputationValueText != null)
                reputationValueText.text = $"{reputation:F0} / {maxRep:F0}";
        }

        private void RefreshRelationships()
        {
            // TODO: Display allied and enemy factions
        }

        private void RefreshBenefits()
        {
            // TODO: Display benefits at current standing
        }
        #endregion

        #region Private Methods - Data Access
        private List<FactionData> GetKnownFactions()
        {
            // TODO: Get factions from faction manager
            return new List<FactionData>();
        }

        private Color GetStandingColor(string standing)
        {
            return standing switch
            {
                "Hated" => hatedColor,
                "Hostile" => hostileColor,
                "Unfriendly" => unfriendlyColor,
                "Neutral" => neutralColor,
                "Friendly" => friendlyColor,
                "Honored" => honoredColor,
                "Exalted" => exaltedColor,
                _ => neutralColor
            };
        }
        #endregion

        #region Helper Structs
        private struct FactionData
        {
            public string Name;
            public string Description;
            public string Standing;
            public float Reputation;
        }
        #endregion
    }
}
