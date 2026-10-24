// Fixed: Updated to use IAbilityRegistry and IAbilityExecutor (Framework Pattern)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.Abilities;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Abilities UI with ability loadout, cooldowns, and skill tree visualization.
    /// Keyboard shortcut: P (for powers/abilities)
    /// </summary>
    public class AbilitiesUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("Active Abilities")]
        [SerializeField] private Transform activeAbilitiesContainer;
        [SerializeField] private GameObject abilitySlotPrefab;
        [SerializeField] private int maxActiveAbilities = 10;

        [Header("Available Abilities")]
        [SerializeField] private Transform availableAbilitiesContainer;
        [SerializeField] private GameObject availableAbilityPrefab;
        [SerializeField] private TMP_InputField searchField;

        [Header("Ability Details")]
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityDescriptionText;
        [SerializeField] private Image abilityIcon;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private TextMeshProUGUI manaCostText;
        [SerializeField] private TextMeshProUGUI rangeText;
        [SerializeField] private TextMeshProUGUI damageText;

        [Header("Skill Tree")]
        [SerializeField] private Transform skillTreeContainer;
        [SerializeField] private GameObject skillNodePrefab;

        [Header("Tabs")]
        [SerializeField] private Button loadoutTab;
        [SerializeField] private Button skillTreeTab;
        [SerializeField] private GameObject loadoutPanel;
        [SerializeField] private GameObject skillTreePanel;

        [Header("Colors")]
        [SerializeField] private Color availableColor = Color.green;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color cooldownColor = Color.red;
        #endregion

        #region Private Fields
        private IAbilityRegistry abilityRegistry;
        private IAbilityExecutor abilityExecutor;
        private int playerEntityId;
        private List<GameObject> activeSlots = new List<GameObject>();
        private List<GameObject> availableEntries = new List<GameObject>();
        private IAbility selectedAbility;
        private bool showingLoadout = true;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (loadoutTab != null)
                loadoutTab.onClick.AddListener(() => ShowTab(true));

            if (skillTreeTab != null)
                skillTreeTab.onClick.AddListener(() => ShowTab(false));

            if (searchField != null)
                searchField.onValueChanged.AddListener(_ => RefreshAvailableAbilities());
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshAll();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Hide();

            UpdateCooldowns();
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            RefreshAll();
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

        public void Initialize(IAbilityRegistry registry, IAbilityExecutor executor, int playerEntity)
        {
            abilityRegistry = registry;
            abilityExecutor = executor;
            playerEntityId = playerEntity;
            RefreshAll();
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            // TODO: Subscribe to ability events via EventBus
            // e.g., EventBus.Subscribe<AbilityExecutedEvent>(OnAbilityExecuted);
        }

        private void UnsubscribeFromEvents()
        {
            // TODO: Unsubscribe from events
        }
        #endregion

        #region Private Methods - Display
        private void ShowTab(bool showLoadout)
        {
            showingLoadout = showLoadout;

            if (loadoutPanel != null)
                loadoutPanel.SetActive(showLoadout);

            if (skillTreePanel != null)
                skillTreePanel.SetActive(!showLoadout);
        }

        private void RefreshAll()
        {
            RefreshActiveAbilities();
            RefreshAvailableAbilities();
            RefreshSkillTree();
        }

        private void RefreshActiveAbilities()
        {
            ClearActiveSlots();

            if (activeAbilitiesContainer == null || abilitySlotPrefab == null) return;

            for (int i = 0; i < maxActiveAbilities; i++)
            {
                CreateActiveSlot(i);
            }
        }

        private void CreateActiveSlot(int slotIndex)
        {
            var slot = Instantiate(abilitySlotPrefab, activeAbilitiesContainer);
            activeSlots.Add(slot);

            // Set slot number
            var numberText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (numberText != null)
                numberText.text = (slotIndex + 1).ToString();

            // TODO: Get ability in this slot and display it

            // Set drop handler for drag-drop
            var dropHandler = slot.AddComponent<AbilitySlotDropHandler>();
            dropHandler.Initialize(slotIndex, this);
        }

        private void RefreshAvailableAbilities()
        {
            ClearAvailableEntries();

            if (abilityRegistry == null) return;

            var abilities = abilityRegistry.GetAllAbilities();

            foreach (var ability in abilities)
            {
                CreateAvailableAbilityEntry(ability);
            }
        }

        private void CreateAvailableAbilityEntry(IAbility ability)
        {
            if (availableAbilityPrefab == null || availableAbilitiesContainer == null) return;

            var entry = Instantiate(availableAbilityPrefab, availableAbilitiesContainer);
            availableEntries.Add(entry);

            var nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = ability.Name;
                nameText.color = availableColor; // TODO: Check if unlocked when unlocking system is available
            }

            var button = entry.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => SelectAbility(ability));
                button.interactable = true; // TODO: Check if unlocked
            }

            // TODO: Add drag handler when drag-drop system is implemented
        }

        private void RefreshSkillTree()
        {
            // TODO: Build skill tree visualization
        }

        private void UpdateCooldowns()
        {
            // Update cooldown displays on active abilities
        }

        private void ClearActiveSlots()
        {
            foreach (var slot in activeSlots)
            {
                if (slot != null)
                    Destroy(slot);
            }
            activeSlots.Clear();
        }

        private void ClearAvailableEntries()
        {
            foreach (var entry in availableEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            availableEntries.Clear();
        }
        #endregion

        #region Private Methods - Ability Details
        private void SelectAbility(IAbility ability)
        {
            selectedAbility = ability;
            RefreshAbilityDetails();
        }

        private void RefreshAbilityDetails()
        {
            if (selectedAbility == null) return;

            if (abilityNameText != null)
                abilityNameText.text = selectedAbility.Name;

            if (abilityDescriptionText != null)
                abilityDescriptionText.text = selectedAbility.Metadata.Description;

            if (cooldownText != null)
                cooldownText.text = $"Cooldown: {selectedAbility.Cooldown:F1}s";

            if (manaCostText != null)
            {
                var manaCost = selectedAbility.ResourceCosts.FirstOrDefault(c => c.ResourceType == "Mana");
                manaCostText.text = manaCost != null ? $"Cost: {manaCost.Amount} Mana" : "Cost: None";
            }

            if (rangeText != null)
                rangeText.text = $"Range: {selectedAbility.Range:F1}m";

            if (damageText != null)
                damageText.text = "Damage: TBD"; // TODO: Extract damage from ability metadata
        }
        #endregion

        #region Private Methods - Ability Management
        public void AssignAbilityToSlot(IAbility ability, int slotIndex)
        {
            // TODO: Implement ability slot assignment when ability loadout system is available
            RefreshActiveAbilities();
        }
        #endregion
    }
}
