// Fixed: Updated to use Framework InventoryComponent via IEntityManager (ECS Pattern)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.Inventory.Runtime;
using RPGFramework.ECS;
using RPGFramework.Items;
using MythHunter.GameLogic.Inventory;
using MythHunter.GameLogic.Inventory.Components;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Grid-based inventory UI with equipment slots, item tooltips, and drag-drop support.
    /// Keyboard shortcut: I
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("Inventory Grid")]
        [SerializeField] private GridLayoutGroup inventoryGrid;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 6;

        [Header("Equipment Slots")]
        [SerializeField] private ItemSlotUI headSlot;
        [SerializeField] private ItemSlotUI chestSlot;
        [SerializeField] private ItemSlotUI legsSlot;
        [SerializeField] private ItemSlotUI feetSlot;
        [SerializeField] private ItemSlotUI mainHandSlot;
        [SerializeField] private ItemSlotUI offHandSlot;
        [SerializeField] private ItemSlotUI neckSlot;
        [SerializeField] private ItemSlotUI ring1Slot;
        [SerializeField] private ItemSlotUI ring2Slot;

        [Header("Character Preview")]
        [SerializeField] private RawImage characterPreview;
        [SerializeField] private TextMeshProUGUI characterNameText;

        [Header("Item Tooltip")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipNameText;
        [SerializeField] private TextMeshProUGUI tooltipDescriptionText;
        [SerializeField] private TextMeshProUGUI tooltipStatsText;
        [SerializeField] private TextMeshProUGUI tooltipTypeText;
        [SerializeField] private Image tooltipIconImage;

        [Header("Item Info")]
        [SerializeField] private TextMeshProUGUI weightText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Slider weightBar;

        [Header("Filters & Sorting")]
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private Toggle showWeaponsToggle;
        [SerializeField] private Toggle showArmorToggle;
        [SerializeField] private Toggle showConsumablesToggle;
        [SerializeField] private Toggle showMiscToggle;

        [Header("Colors - Item Rarity")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = new Color(0.6f, 0f, 1f);
        [SerializeField] private Color legendaryColor = Color.yellow;
        #endregion

        #region Private Fields
        private IEntityManager entityManager;
        private int playerEntityId;
        private List<ItemSlotUI> inventorySlots = new List<ItemSlotUI>();
        private ItemSlotUI draggedSlot;
        private string hoveredItemId;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (sortDropdown != null)
                sortDropdown.onValueChanged.AddListener(OnSortChanged);

            SetupFilterToggles();
            CreateInventorySlots();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshInventory();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            HideTooltip();
        }

        private void Update()
        {
            // Handle keyboard shortcuts
            if (Input.GetKeyDown(KeyCode.Escape))
                Hide();

            // Update tooltip position to follow mouse
            if (tooltipPanel != null && tooltipPanel.activeSelf)
                UpdateTooltipPosition();
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            RefreshInventory();
            Time.timeScale = 0f; // Pause game
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            HideTooltip();
            Time.timeScale = 1f; // Resume game
        }

        public void Toggle()
        {
            if (panelRoot != null && panelRoot.activeSelf)
                Hide();
            else
                Show();
        }

        public void Initialize(IEntityManager entityMgr, int playerEntity)
        {
            entityManager = entityMgr;
            playerEntityId = playerEntity;
            RefreshInventory();
        }
        #endregion

        #region Private Methods - Initialization
        private void SetupFilterToggles()
        {
            if (showWeaponsToggle != null)
                showWeaponsToggle.onValueChanged.AddListener(_ => RefreshInventory());

            if (showArmorToggle != null)
                showArmorToggle.onValueChanged.AddListener(_ => RefreshInventory());

            if (showConsumablesToggle != null)
                showConsumablesToggle.onValueChanged.AddListener(_ => RefreshInventory());

            if (showMiscToggle != null)
                showMiscToggle.onValueChanged.AddListener(_ => RefreshInventory());
        }

        private void CreateInventorySlots()
        {
            if (inventoryGrid == null || itemSlotPrefab == null) return;

            int totalSlots = gridWidth * gridHeight;

            for (int i = 0; i < totalSlots; i++)
            {
                var slotObj = Instantiate(itemSlotPrefab, inventoryGrid.transform);
                var slot = slotObj.GetComponent<ItemSlotUI>();

                if (slot != null)
                {
                    slot.Initialize(i, OnSlotClicked, OnSlotBeginDrag, OnSlotDrag, OnSlotEndDrag, OnSlotPointerEnter, OnSlotPointerExit);
                    inventorySlots.Add(slot);
                }
            }
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            // TODO: Subscribe to inventory events via EventBus
            // e.g., EventBus.Subscribe<ItemAddedEvent>(OnItemAdded);
        }

        private void UnsubscribeFromEvents()
        {
            // TODO: Unsubscribe from events
        }
        #endregion

        #region Private Methods - Slot Callbacks
        private void OnSlotClicked(ItemSlotUI slot)
        {
            if (slot.ItemId == null) return;

            // Right-click to use/equip item
            if (Input.GetMouseButton(1))
            {
                UseOrEquipItem(slot.ItemId);
            }
        }

        private void OnSlotBeginDrag(ItemSlotUI slot)
        {
            if (slot.ItemId == null) return;

            draggedSlot = slot;
            HideTooltip();
        }

        private void OnSlotDrag(ItemSlotUI slot)
        {
            // Visual feedback during drag
        }

        private void OnSlotEndDrag(ItemSlotUI slot, PointerEventData eventData)
        {
            if (draggedSlot == null) return;

            // Find the slot we dropped onto
            var targetSlot = GetSlotUnderPointer(eventData);

            if (targetSlot != null && targetSlot != draggedSlot)
            {
                SwapItems(draggedSlot, targetSlot);
            }

            draggedSlot = null;
        }

        private void OnSlotPointerEnter(ItemSlotUI slot)
        {
            if (slot.ItemId != null)
            {
                hoveredItemId = slot.ItemId;
                ShowTooltip(slot.ItemId);
            }
        }

        private void OnSlotPointerExit(ItemSlotUI slot)
        {
            hoveredItemId = null;
            HideTooltip();
        }
        #endregion

        #region Private Methods - UI Updates
        private void RefreshInventory()
        {
            if (entityManager == null || !entityManager.HasComponent<MythHunterInventoryComponent>(playerEntityId))
                return;

            var inventoryComp = entityManager.GetComponent<MythHunterInventoryComponent>(playerEntityId);

            // Clear all slots
            foreach (var slot in inventorySlots)
            {
                slot.Clear();
            }

            // Get items from primary container
            var items = inventoryComp.Inventory.PrimaryContainer.GetAllItems().ToList();
            var filteredItems = ApplyFilters(items);

            // Populate slots with items
            for (int i = 0; i < filteredItems.Count && i < inventorySlots.Count; i++)
            {
                inventorySlots[i].SetItemId(filteredItems[i]);
            }

            // Update equipment slots
            RefreshEquipmentSlots();

            // Update weight and gold
            UpdateWeightAndGold();
        }

        private void RefreshEquipmentSlots()
        {
            if (entityManager == null || !entityManager.HasComponent<MythHunterInventoryComponent>(playerEntityId))
                return;

            var inventoryComp = entityManager.GetComponent<MythHunterInventoryComponent>(playerEntityId);

            // Update equipment slots
            headSlot?.SetItemId(GetEquippedItem(inventoryComp, MythHunterEquipmentSlot.Helmet));
            chestSlot?.SetItemId(GetEquippedItem(inventoryComp, MythHunterEquipmentSlot.Chest));
            legsSlot?.SetItemId(null); // MythHunter doesn't have legs slot
            feetSlot?.SetItemId(GetEquippedItem(inventoryComp, MythHunterEquipmentSlot.Boots));
            mainHandSlot?.SetItemId(GetEquippedItem(inventoryComp, MythHunterEquipmentSlot.MainWeapon));
            offHandSlot?.SetItemId(GetEquippedItem(inventoryComp, MythHunterEquipmentSlot.SecondaryWeapon));
            neckSlot?.SetItemId(GetEquippedItem(inventoryComp, MythHunterEquipmentSlot.Necklace));
            ring1Slot?.SetItemId(GetEquippedItem(inventoryComp, MythHunterEquipmentSlot.Ring1));
            ring2Slot?.SetItemId(GetEquippedItem(inventoryComp, MythHunterEquipmentSlot.Ring2));
        }

        private string GetEquippedItem(MythHunterInventoryComponent inventoryComp, MythHunterEquipmentSlot slotType)
        {
            if (inventoryComp.Inventory.EquipmentSlots.TryGetValue(slotType, out var slot))
            {
                return slot.IsOccupied ? slot.EquippedItem : null;
            }
            return null;
        }

        private void UpdateWeightAndGold()
        {
            if (entityManager == null || !entityManager.HasComponent<MythHunterInventoryComponent>(playerEntityId))
                return;

            var inventoryComp = entityManager.GetComponent<MythHunterInventoryComponent>(playerEntityId);

            // TODO: Calculate weight from items when item database is available
            float currentWeight = 0f;
            float maxWeight = 100f; // TODO: Get from character stats

            int itemCount = inventoryComp.Inventory.PrimaryContainer.Count;
            int maxItems = inventoryComp.Inventory.PrimaryContainer.Capacity;

            if (weightText != null)
                weightText.text = maxItems > 0
                    ? $"Items: {itemCount} / {maxItems}"
                    : $"Items: {itemCount}";

            if (weightBar != null && maxItems > 0)
            {
                weightBar.maxValue = maxItems;
                weightBar.value = itemCount;
            }

            // TODO: Get gold from currency component when available
            if (goldText != null)
                goldText.text = "Gold: 0";
        }

        private List<string> ApplyFilters(IReadOnlyList<string> items)
        {
            // TODO: Implement filtering by item type when item database is available
            // For now, return all items
            return items.ToList();
        }

        private void OnSortChanged(int sortIndex)
        {
            // Sort by: Name, Type, Rarity, Weight, Value
            RefreshInventory();
        }
        #endregion

        #region Private Methods - Tooltip
        private void ShowTooltip(string itemId)
        {
            if (tooltipPanel == null || string.IsNullOrEmpty(itemId)) return;

            tooltipPanel.SetActive(true);

            // TODO: Get item data from item database/registry when available
            // For now, display item ID
            if (tooltipNameText != null)
            {
                tooltipNameText.text = itemId;
                tooltipNameText.color = Color.white;
            }

            if (tooltipDescriptionText != null)
                tooltipDescriptionText.text = "Item description";

            if (tooltipStatsText != null)
                tooltipStatsText.text = GetItemStats(itemId);

            if (tooltipTypeText != null)
                tooltipTypeText.text = GetItemType(itemId);

            UpdateTooltipPosition();
        }

        private void HideTooltip()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }

        private void UpdateTooltipPosition()
        {
            if (tooltipPanel == null) return;

            Vector2 mousePosition = Input.mousePosition;
            tooltipPanel.transform.position = mousePosition + new Vector2(15, -15);
        }

        private string GetItemStats(string itemId)
        {
            // TODO: Get item stats from item database when available
            return "Stats: TBD";
        }

        private string GetItemType(string itemId)
        {
            // TODO: Get item type from item database when available
            return "Type: Unknown";
        }
        #endregion

        #region Private Methods - Item Actions
        private void UseOrEquipItem(string itemId)
        {
            // TODO: Use consumable or equip equipment via inventory system
            RefreshInventory();
        }

        private void SwapItems(ItemSlotUI fromSlot, ItemSlotUI toSlot)
        {
            // TODO: Implement item swapping via Framework inventory container
            // This requires modifying the container's internal list order
            RefreshInventory();
        }

        private ItemSlotUI GetSlotUnderPointer(PointerEventData eventData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                var slot = result.gameObject.GetComponent<ItemSlotUI>();
                if (slot != null)
                    return slot;
            }

            return null;
        }
        #endregion
    }

    /// <summary>
    /// Individual item slot UI component for drag-drop and interaction.
    /// </summary>
    public class ItemSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image backgroundImage;

        private string itemId;
        private int slotIndex;

        private System.Action<ItemSlotUI> onClicked;
        private System.Action<ItemSlotUI> onBeginDrag;
        private System.Action<ItemSlotUI> onDrag;
        private System.Action<ItemSlotUI, PointerEventData> onEndDrag;
        private System.Action<ItemSlotUI> onPointerEnter;
        private System.Action<ItemSlotUI> onPointerExit;

        public string ItemId => itemId;
        public int SlotIndex => slotIndex;

        public void Initialize(int index,
            System.Action<ItemSlotUI> clickCallback,
            System.Action<ItemSlotUI> beginDragCallback,
            System.Action<ItemSlotUI> dragCallback,
            System.Action<ItemSlotUI, PointerEventData> endDragCallback,
            System.Action<ItemSlotUI> pointerEnterCallback,
            System.Action<ItemSlotUI> pointerExitCallback)
        {
            slotIndex = index;
            onClicked = clickCallback;
            onBeginDrag = beginDragCallback;
            onDrag = dragCallback;
            onEndDrag = endDragCallback;
            onPointerEnter = pointerEnterCallback;
            onPointerExit = pointerExitCallback;
        }

        public void SetItemId(string newItemId)
        {
            itemId = newItemId;

            if (!string.IsNullOrEmpty(itemId))
            {
                // TODO: Set item icon from item database when available
                if (iconImage != null)
                    iconImage.enabled = true;

                // TODO: Show quantity for stackable items when item stacking is implemented
                if (quantityText != null)
                    quantityText.text = "";
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            itemId = null;

            if (iconImage != null)
                iconImage.enabled = false;

            if (quantityText != null)
                quantityText.text = "";
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClicked?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke(this, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke(this);
        }
    }
}
