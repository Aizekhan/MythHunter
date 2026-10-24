// TODO: This UI needs refactoring to work with actual Framework interfaces (IAbilityExecutor, IAbilityRegistry)
#if FALSE
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
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
        private IAbilitySystem abilitySystem;
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

        public void Initialize(IAbilitySystem system)
        {
            abilitySystem = system;
            RefreshAll();
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            if (abilitySystem == null) return;
            // abilitySystem.OnAbilityUnlocked += OnAbilityUnlocked;
            // abilitySystem.OnAbilityUsed += OnAbilityUsed;
        }

        private void UnsubscribeFromEvents()
        {
            if (abilitySystem == null) return;
            // abilitySystem.OnAbilityUnlocked -= OnAbilityUnlocked;
            // abilitySystem.OnAbilityUsed -= OnAbilityUsed;
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

            if (abilitySystem == null) return;

            var abilities = GetAvailableAbilities();

            foreach (var ability in abilities)
            {
                CreateAvailableAbilityEntry(ability);
            }
        }

        private void CreateAvailableAbilityEntry(AbilityData ability)
        {
            if (availableAbilityPrefab == null || availableAbilitiesContainer == null) return;

            var entry = Instantiate(availableAbilityPrefab, availableAbilitiesContainer);
            availableEntries.Add(entry);

            var nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = ability.Name;
                nameText.color = ability.IsUnlocked ? availableColor : lockedColor;
            }

            var button = entry.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => SelectAbility(ability));
                button.interactable = ability.IsUnlocked;
            }

            // Add drag handler
            if (ability.IsUnlocked)
            {
                var dragHandler = entry.AddComponent<AbilityDragHandler>();
                dragHandler.Initialize(ability);
            }
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
        private void SelectAbility(AbilityData ability)
        {
            selectedAbility = null; // TODO: Set selected ability
            RefreshAbilityDetails();
        }

        private void RefreshAbilityDetails()
        {
            if (selectedAbility == null) return;

            if (abilityNameText != null)
                abilityNameText.text = selectedAbility.Name;

            if (abilityDescriptionText != null)
                abilityDescriptionText.text = selectedAbility.Description;

            // TODO: Display ability stats
            if (cooldownText != null)
                cooldownText.text = "Cooldown: 5s";

            if (manaCostText != null)
                manaCostText.text = "Cost: 25 Mana";

            if (rangeText != null)
                rangeText.text = "Range: 10m";

            if (damageText != null)
                damageText.text = "Damage: 50-75";
        }
        #endregion

        #region Private Methods - Ability Management
        public void AssignAbilityToSlot(AbilityData ability, int slotIndex)
        {
            // TODO: Assign ability to slot
            RefreshActiveAbilities();
        }

        private List<AbilityData> GetAvailableAbilities()
        {
            // TODO: Get abilities from ability system
            return new List<AbilityData>
            {
                new AbilityData { Name = "Fireball", IsUnlocked = true },
                new AbilityData { Name = "Ice Blast", IsUnlocked = true },
                new AbilityData { Name = "Chain Lightning", IsUnlocked = false }
            };
        }
        #endregion

        #region Helper Classes
        private class AbilitySlotDropHandler : MonoBehaviour, UnityEngine.EventSystems.IDropHandler
        {
            private int slotIndex;
            private AbilitiesUI ui;

            public void Initialize(int index, AbilitiesUI abilitiesUI)
            {
                slotIndex = index;
                ui = abilitiesUI;
            }

            public void OnDrop(UnityEngine.EventSystems.PointerEventData eventData)
            {
                var dragHandler = eventData.pointerDrag?.GetComponent<AbilityDragHandler>();
                if (dragHandler != null && ui != null)
                {
                    ui.AssignAbilityToSlot(dragHandler.Ability, slotIndex);
                }
            }
        }

        private class AbilityDragHandler : MonoBehaviour, UnityEngine.EventSystems.IBeginDragHandler,
            UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IEndDragHandler
        {
            public AbilityData Ability { get; private set; }
            private Transform originalParent;
            private CanvasGroup canvasGroup;

            public void Initialize(AbilityData ability)
            {
                Ability = ability;
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            public void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
            {
                originalParent = transform.parent;
                transform.SetParent(transform.root);
                canvasGroup.alpha = 0.6f;
                canvasGroup.blocksRaycasts = false;
            }

            public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
            {
                transform.position = eventData.position;
            }

            public void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
            {
                transform.SetParent(originalParent);
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
        }
        #endregion

        #region Helper Structs
        public struct AbilityData
        {
            public string Name;
            public string Description;
            public bool IsUnlocked;
        }
        #endregion
    }
}
#endif
