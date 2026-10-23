using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using RPGFramework.Inventory;
using RPGFramework.Items;

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
        private IInventory inventory;
        private List<ItemSlotUI> inventorySlots = new List<ItemSlotUI>();
        private ItemSlotUI draggedSlot;
        private IItem hoveredItem;
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

        public void Initialize(IInventory playerInventory)
        {
            inventory = playerInventory;
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
            if (inventory == null) return;

            // Subscribe to inventory events for real-time updates
            // inventory.OnItemAdded += OnItemAdded;
            // inventory.OnItemRemoved += OnItemRemoved;
            // inventory.OnItemMoved += OnItemMoved;
        }

        private void UnsubscribeFromEvents()
        {
            if (inventory == null) return;

            // Unsubscribe from inventory events
            // inventory.OnItemAdded -= OnItemAdded;
            // inventory.OnItemRemoved -= OnItemRemoved;
            // inventory.OnItemMoved -= OnItemMoved;
        }

        private void OnItemAdded(IItem item)
        {
            RefreshInventory();
        }

        private void OnItemRemoved(IItem item)
        {
            RefreshInventory();
        }

        private void OnItemMoved(IItem item, int fromSlot, int toSlot)
        {
            RefreshInventory();
        }
        #endregion

        #region Private Methods - Slot Callbacks
        private void OnSlotClicked(ItemSlotUI slot)
        {
            if (slot.Item == null) return;

            // Right-click to use/equip item
            if (Input.GetMouseButton(1))
            {
                UseOrEquipItem(slot.Item);
            }
        }

        private void OnSlotBeginDrag(ItemSlotUI slot)
        {
            if (slot.Item == null) return;

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
            if (slot.Item != null)
            {
                hoveredItem = slot.Item;
                ShowTooltip(slot.Item);
            }
        }

        private void OnSlotPointerExit(ItemSlotUI slot)
        {
            hoveredItem = null;
            HideTooltip();
        }
        #endregion

        #region Private Methods - UI Updates
        private void RefreshInventory()
        {
            if (inventory == null) return;

            // Clear all slots
            foreach (var slot in inventorySlots)
            {
                slot.Clear();
            }

            // Get items from inventory
            var items = GetFilteredItems();

            // Populate slots with items
            for (int i = 0; i < items.Count && i < inventorySlots.Count; i++)
            {
                inventorySlots[i].SetItem(items[i]);
            }

            // Update equipment slots
            RefreshEquipmentSlots();

            // Update weight and gold
            UpdateWeightAndGold();
        }

        private void RefreshEquipmentSlots()
        {
            // TODO: Get equipped items and update equipment slots
            // headSlot?.SetItem(GetEquippedItem(EquipmentSlotType.Head));
            // chestSlot?.SetItem(GetEquippedItem(EquipmentSlotType.Chest));
            // etc...
        }

        private void UpdateWeightAndGold()
        {
            if (inventory == null) return;

            // TODO: Get actual weight and gold values
            float currentWeight = 0f; // inventory.TotalWeight;
            float maxWeight = 100f; // inventory.MaxWeight;
            int gold = 0; // inventory.Gold;

            if (weightText != null)
                weightText.text = $"Weight: {currentWeight:F1} / {maxWeight:F1}";

            if (weightBar != null)
            {
                weightBar.maxValue = maxWeight;
                weightBar.value = currentWeight;
            }

            if (goldText != null)
                goldText.text = $"Gold: {gold}";
        }

        private List<IItem> GetFilteredItems()
        {
            // TODO: Get items from inventory and apply filters
            return new List<IItem>();
        }

        private void OnSortChanged(int sortIndex)
        {
            // Sort by: Name, Type, Rarity, Weight, Value
            RefreshInventory();
        }
        #endregion

        #region Private Methods - Tooltip
        private void ShowTooltip(IItem item)
        {
            if (tooltipPanel == null || item == null) return;

            tooltipPanel.SetActive(true);

            // Set tooltip content
            if (tooltipNameText != null)
            {
                tooltipNameText.text = item.Name;
                tooltipNameText.color = GetRarityColor(item);
            }

            if (tooltipDescriptionText != null)
                tooltipDescriptionText.text = item.Description;

            if (tooltipStatsText != null)
                tooltipStatsText.text = GetItemStats(item);

            if (tooltipTypeText != null)
                tooltipTypeText.text = GetItemType(item);

            // TODO: Set tooltip icon
            // if (tooltipIconImage != null)
            //     tooltipIconImage.sprite = item.Icon;

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

        private string GetItemStats(IItem item)
        {
            // TODO: Get item stats as formatted string
            return "Damage: 10-15\nStrength: +5\nDexterity: +3";
        }

        private string GetItemType(IItem item)
        {
            // TODO: Get item type/category
            return "Weapon - Sword";
        }

        private Color GetRarityColor(IItem item)
        {
            // TODO: Get item rarity and return appropriate color
            return commonColor;
        }
        #endregion

        #region Private Methods - Item Actions
        private void UseOrEquipItem(IItem item)
        {
            // TODO: Use consumable or equip equipment
            RefreshInventory();
        }

        private void SwapItems(ItemSlotUI fromSlot, ItemSlotUI toSlot)
        {
            if (inventory == null) return;

            // TODO: Swap items in inventory
            // inventory.MoveItem(fromSlot.SlotIndex, toSlot.SlotIndex);

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

        private IItem item;
        private int slotIndex;

        private System.Action<ItemSlotUI> onClicked;
        private System.Action<ItemSlotUI> onBeginDrag;
        private System.Action<ItemSlotUI> onDrag;
        private System.Action<ItemSlotUI, PointerEventData> onEndDrag;
        private System.Action<ItemSlotUI> onPointerEnter;
        private System.Action<ItemSlotUI> onPointerExit;

        public IItem Item => item;
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

        public void SetItem(IItem newItem)
        {
            item = newItem;

            if (item != null)
            {
                // TODO: Set item icon
                // if (iconImage != null)
                //     iconImage.sprite = item.Icon;

                if (iconImage != null)
                    iconImage.enabled = true;

                // Show quantity for stackable items
                if (quantityText != null)
                {
                    int quantity = 1; // TODO: Get actual quantity from item
                    quantityText.text = quantity > 1 ? quantity.ToString() : "";
                }
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            item = null;

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
