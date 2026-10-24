// TODO: This UI needs refactoring to work with actual Framework Trading interfaces
#if FALSE
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.Trading;
using RPGFramework.Items;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Trading UI for merchant interactions with buy/sell functionality and reputation discounts.
    /// Keyboard shortcut: T
    /// </summary>
    public class TradingUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("Merchant Info")]
        [SerializeField] private TextMeshProUGUI merchantNameText;
        [SerializeField] private TextMeshProUGUI merchantDescriptionText;
        [SerializeField] private Image merchantPortrait;
        [SerializeField] private TextMeshProUGUI reputationText;
        [SerializeField] private Slider reputationBar;

        [Header("Merchant Inventory")]
        [SerializeField] private Transform merchantInventoryContainer;
        [SerializeField] private GameObject merchantItemEntryPrefab;
        [SerializeField] private TMP_InputField merchantSearchField;
        [SerializeField] private TMP_Dropdown merchantCategoryFilter;

        [Header("Player Inventory")]
        [SerializeField] private Transform playerInventoryContainer;
        [SerializeField] private GameObject playerItemEntryPrefab;
        [SerializeField] private TMP_InputField playerSearchField;
        [SerializeField] private TMP_Dropdown playerCategoryFilter;

        [Header("Transaction")]
        [SerializeField] private TextMeshProUGUI selectedItemNameText;
        [SerializeField] private TextMeshProUGUI selectedItemDescriptionText;
        [SerializeField] private Image selectedItemIcon;
        [SerializeField] private TextMeshProUGUI basePriceText;
        [SerializeField] private TextMeshProUGUI discountText;
        [SerializeField] private TextMeshProUGUI finalPriceText;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Slider quantitySlider;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button sellButton;

        [Header("Player Resources")]
        [SerializeField] private TextMeshProUGUI playerGoldText;
        [SerializeField] private TextMeshProUGUI merchantGoldText;

        [Header("Tabs")]
        [SerializeField] private Button buyTab;
        [SerializeField] private Button sellTab;
        [SerializeField] private GameObject buyPanel;
        [SerializeField] private GameObject sellPanel;

        [Header("Colors")]
        [SerializeField] private Color canAffordColor = Color.green;
        [SerializeField] private Color cannotAffordColor = Color.red;
        [SerializeField] private Color discountColor = Color.yellow;
        #endregion

        #region Private Fields
        private ITradingSystem tradingSystem;
        private ITrader currentMerchant;
        private List<GameObject> merchantItemEntries = new List<GameObject>();
        private List<GameObject> playerItemEntries = new List<GameObject>();
        private IItem selectedItem;
        private bool isBuyMode = true;
        private int transactionQuantity = 1;
        private string merchantSearchFilter = "";
        private string playerSearchFilter = "";
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (buyTab != null)
                buyTab.onClick.AddListener(() => SetMode(true));

            if (sellTab != null)
                sellTab.onClick.AddListener(() => SetMode(false));

            if (buyButton != null)
                buyButton.onClick.AddListener(BuySelectedItem);

            if (sellButton != null)
                sellButton.onClick.AddListener(SellSelectedItem);

            if (merchantSearchField != null)
                merchantSearchField.onValueChanged.AddListener(OnMerchantSearchChanged);

            if (playerSearchField != null)
                playerSearchField.onValueChanged.AddListener(OnPlayerSearchChanged);

            if (merchantCategoryFilter != null)
                merchantCategoryFilter.onValueChanged.AddListener(_ => RefreshMerchantInventory());

            if (playerCategoryFilter != null)
                playerCategoryFilter.onValueChanged.AddListener(_ => RefreshPlayerInventory());

            if (quantityInput != null)
                quantityInput.onValueChanged.AddListener(OnQuantityInputChanged);

            if (quantitySlider != null)
                quantitySlider.onValueChanged.AddListener(OnQuantitySliderChanged);
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
            // Handle keyboard shortcuts
            if (Input.GetKeyDown(KeyCode.Escape))
                Hide();

            // Switch between buy/sell tabs
            if (Input.GetKeyDown(KeyCode.Tab))
                SetMode(!isBuyMode);
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            RefreshAll();
            Time.timeScale = 0f; // Pause game
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            Time.timeScale = 1f; // Resume game
        }

        public void Toggle()
        {
            if (panelRoot != null && panelRoot.activeSelf)
                Hide();
            else
                Show();
        }

        public void Initialize(ITradingSystem system, ITrader merchant)
        {
            tradingSystem = system;
            currentMerchant = merchant;
            RefreshAll();
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            if (tradingSystem == null) return;

            // Subscribe to trading events for real-time updates
            // tradingSystem.OnTradeCompleted += OnTradeCompleted;
            // tradingSystem.OnTradeFailed += OnTradeFailed;
        }

        private void UnsubscribeFromEvents()
        {
            if (tradingSystem == null) return;

            // Unsubscribe from trading events
            // tradingSystem.OnTradeCompleted -= OnTradeCompleted;
            // tradingSystem.OnTradeFailed -= OnTradeFailed;
        }

        private void OnTradeCompleted(IItem item, int quantity, int price)
        {
            RefreshAll();
        }

        private void OnTradeFailed(string reason)
        {
            Debug.LogWarning($"Trade failed: {reason}");
        }
        #endregion

        #region Private Methods - Mode & Display
        private void SetMode(bool buyMode)
        {
            isBuyMode = buyMode;

            if (buyPanel != null)
                buyPanel.SetActive(buyMode);

            if (sellPanel != null)
                sellPanel.SetActive(!buyMode);

            if (buyButton != null)
                buyButton.gameObject.SetActive(buyMode);

            if (sellButton != null)
                sellButton.gameObject.SetActive(!buyMode);

            RefreshAll();
        }

        private void RefreshAll()
        {
            UpdateMerchantInfo();
            RefreshMerchantInventory();
            RefreshPlayerInventory();
            UpdatePlayerResources();
            ClearSelection();
        }

        private void UpdateMerchantInfo()
        {
            if (currentMerchant == null) return;

            if (merchantNameText != null)
                merchantNameText.text = GetMerchantName();

            if (merchantDescriptionText != null)
                merchantDescriptionText.text = GetMerchantDescription();

            // TODO: Set merchant portrait
            // if (merchantPortrait != null)
            //     merchantPortrait.sprite = currentMerchant.Portrait;

            UpdateReputationDisplay();
        }

        private void UpdateReputationDisplay()
        {
            // TODO: Get reputation with merchant's faction
            float reputation = 0f;
            float maxReputation = 100f;
            string reputationLevel = "Neutral";

            if (reputationText != null)
                reputationText.text = $"Reputation: {reputationLevel} ({reputation:F0}/{maxReputation:F0})";

            if (reputationBar != null)
            {
                reputationBar.maxValue = maxReputation;
                reputationBar.value = reputation;
            }

            if (discountText != null)
            {
                float discount = CalculateReputationDiscount(reputation);
                if (discount > 0)
                {
                    discountText.text = $"{discount:P0} discount";
                    discountText.color = discountColor;
                }
                else
                {
                    discountText.text = "";
                }
            }
        }
        #endregion

        #region Private Methods - Merchant Inventory
        private void RefreshMerchantInventory()
        {
            ClearMerchantItemEntries();

            if (currentMerchant == null) return;

            var items = GetFilteredMerchantItems();

            foreach (var item in items)
            {
                CreateMerchantItemEntry(item);
            }
        }

        private void CreateMerchantItemEntry(MerchantItemData item)
        {
            if (merchantItemEntryPrefab == null || merchantInventoryContainer == null) return;

            var entry = Instantiate(merchantItemEntryPrefab, merchantInventoryContainer);
            merchantItemEntries.Add(entry);

            // Set item name
            var nameText = entry.GetComponentsInChildren<TextMeshProUGUI>()
                .FirstOrDefault(t => t.gameObject.name.Contains("Name"));
            if (nameText != null)
                nameText.text = item.Item.Name;

            // Set item price
            var priceText = entry.GetComponentsInChildren<TextMeshProUGUI>()
                .FirstOrDefault(t => t.gameObject.name.Contains("Price"));
            if (priceText != null)
            {
                int buyPrice = CalculateBuyPrice(item.BasePrice);
                priceText.text = $"{buyPrice} Gold";
                priceText.color = CanAffordItem(buyPrice) ? canAffordColor : cannotAffordColor;
            }

            // Set item stock
            var stockText = entry.GetComponentsInChildren<TextMeshProUGUI>()
                .FirstOrDefault(t => t.gameObject.name.Contains("Stock"));
            if (stockText != null && item.Stock >= 0)
                stockText.text = $"Stock: {item.Stock}";

            // Set click handler
            var button = entry.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => SelectMerchantItem(item));
        }

        private void ClearMerchantItemEntries()
        {
            foreach (var entry in merchantItemEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            merchantItemEntries.Clear();
        }

        private List<MerchantItemData> GetFilteredMerchantItems()
        {
            // TODO: Get items from merchant and apply filters
            var items = new List<MerchantItemData>();

            // Apply search filter
            if (!string.IsNullOrEmpty(merchantSearchFilter))
            {
                items = items.Where(i =>
                    i.Item.Name.ToLower().Contains(merchantSearchFilter.ToLower())
                ).ToList();
            }

            return items;
        }

        private void OnMerchantSearchChanged(string searchText)
        {
            merchantSearchFilter = searchText;
            RefreshMerchantInventory();
        }
        #endregion

        #region Private Methods - Player Inventory
        private void RefreshPlayerInventory()
        {
            ClearPlayerItemEntries();

            var items = GetFilteredPlayerItems();

            foreach (var item in items)
            {
                CreatePlayerItemEntry(item);
            }
        }

        private void CreatePlayerItemEntry(PlayerItemData item)
        {
            if (playerItemEntryPrefab == null || playerInventoryContainer == null) return;

            var entry = Instantiate(playerItemEntryPrefab, playerInventoryContainer);
            playerItemEntries.Add(entry);

            // Set item name
            var nameText = entry.GetComponentsInChildren<TextMeshProUGUI>()
                .FirstOrDefault(t => t.gameObject.name.Contains("Name"));
            if (nameText != null)
                nameText.text = item.Item.Name;

            // Set sell price
            var priceText = entry.GetComponentsInChildren<TextMeshProUGUI>()
                .FirstOrDefault(t => t.gameObject.name.Contains("Price"));
            if (priceText != null)
            {
                int sellPrice = CalculateSellPrice(item.Value);
                priceText.text = $"{sellPrice} Gold";
            }

            // Set quantity
            var quantityText = entry.GetComponentsInChildren<TextMeshProUGUI>()
                .FirstOrDefault(t => t.gameObject.name.Contains("Quantity"));
            if (quantityText != null && item.Quantity > 1)
                quantityText.text = $"x{item.Quantity}";

            // Set click handler
            var button = entry.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => SelectPlayerItem(item));
        }

        private void ClearPlayerItemEntries()
        {
            foreach (var entry in playerItemEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            playerItemEntries.Clear();
        }

        private List<PlayerItemData> GetFilteredPlayerItems()
        {
            // TODO: Get player items and apply filters
            var items = new List<PlayerItemData>();

            // Apply search filter
            if (!string.IsNullOrEmpty(playerSearchFilter))
            {
                items = items.Where(i =>
                    i.Item.Name.ToLower().Contains(playerSearchFilter.ToLower())
                ).ToList();
            }

            return items;
        }

        private void OnPlayerSearchChanged(string searchText)
        {
            playerSearchFilter = searchText;
            RefreshPlayerInventory();
        }
        #endregion

        #region Private Methods - Item Selection
        private void SelectMerchantItem(MerchantItemData item)
        {
            selectedItem = item.Item;
            isBuyMode = true;
            UpdateTransactionPanel(item.BasePrice, item.Stock);
        }

        private void SelectPlayerItem(PlayerItemData item)
        {
            selectedItem = item.Item;
            isBuyMode = false;
            UpdateTransactionPanel(item.Value, item.Quantity);
        }

        private void ClearSelection()
        {
            selectedItem = null;
            transactionQuantity = 1;

            if (selectedItemNameText != null)
                selectedItemNameText.text = "Select an item";

            if (selectedItemDescriptionText != null)
                selectedItemDescriptionText.text = "";

            if (basePriceText != null)
                basePriceText.text = "";

            if (finalPriceText != null)
                finalPriceText.text = "";

            if (buyButton != null)
                buyButton.interactable = false;

            if (sellButton != null)
                sellButton.interactable = false;
        }

        private void UpdateTransactionPanel(int basePrice, int maxQuantity)
        {
            if (selectedItem == null) return;

            // Update item info
            if (selectedItemNameText != null)
                selectedItemNameText.text = selectedItem.Name;

            if (selectedItemDescriptionText != null)
                selectedItemDescriptionText.text = selectedItem.Description;

            // TODO: Set item icon
            // if (selectedItemIcon != null)
            //     selectedItemIcon.sprite = selectedItem.Icon;

            // Update prices
            UpdatePriceDisplay(basePrice);

            // Update quantity controls
            if (quantitySlider != null)
            {
                quantitySlider.maxValue = maxQuantity;
                quantitySlider.value = transactionQuantity;
            }

            UpdateTransactionButton();
        }

        private void UpdatePriceDisplay(int basePrice)
        {
            int finalPrice = isBuyMode
                ? CalculateBuyPrice(basePrice) * transactionQuantity
                : CalculateSellPrice(basePrice) * transactionQuantity;

            if (basePriceText != null)
                basePriceText.text = $"Base: {basePrice * transactionQuantity} Gold";

            if (finalPriceText != null)
            {
                finalPriceText.text = $"Total: {finalPrice} Gold";
                finalPriceText.color = isBuyMode && !CanAffordItem(finalPrice)
                    ? cannotAffordColor
                    : canAffordColor;
            }
        }

        private void UpdateTransactionButton()
        {
            if (isBuyMode && buyButton != null)
            {
                int price = CalculateBuyPrice(GetItemBasePrice()) * transactionQuantity;
                buyButton.interactable = CanAffordItem(price);
            }
            else if (!isBuyMode && sellButton != null)
            {
                sellButton.interactable = true;
            }
        }
        #endregion

        #region Private Methods - Trading
        private void BuySelectedItem()
        {
            if (tradingSystem == null || currentMerchant == null || selectedItem == null) return;

            int totalPrice = CalculateBuyPrice(GetItemBasePrice()) * transactionQuantity;

            if (!CanAffordItem(totalPrice))
            {
                Debug.LogWarning("Cannot afford item");
                return;
            }

            // TODO: Execute trade through trading system
            // tradingSystem.Buy(currentMerchant, selectedItem, transactionQuantity);

            RefreshAll();
        }

        private void SellSelectedItem()
        {
            if (tradingSystem == null || currentMerchant == null || selectedItem == null) return;

            // TODO: Execute trade through trading system
            // tradingSystem.Sell(currentMerchant, selectedItem, transactionQuantity);

            RefreshAll();
        }

        private int CalculateBuyPrice(int basePrice)
        {
            float discount = CalculateReputationDiscount(0f); // TODO: Get actual reputation
            return Mathf.RoundToInt(basePrice * (1f - discount));
        }

        private int CalculateSellPrice(int baseValue)
        {
            float sellRatio = 0.5f; // Merchants buy at 50% of item value
            float bonus = CalculateReputationDiscount(0f) * 0.5f; // Reputation can increase sell price slightly
            return Mathf.RoundToInt(baseValue * (sellRatio + bonus));
        }

        private float CalculateReputationDiscount(float reputation)
        {
            // TODO: Calculate discount based on reputation
            // Example: 0-10% discount based on reputation level
            return 0f;
        }

        private bool CanAffordItem(int price)
        {
            // TODO: Check player gold
            return true;
        }

        private int GetItemBasePrice()
        {
            // TODO: Get item base price from selected item
            return 100;
        }
        #endregion

        #region Private Methods - Resources
        private void UpdatePlayerResources()
        {
            // TODO: Get player and merchant gold
            int playerGold = 0;
            int merchantGold = 0;

            if (playerGoldText != null)
                playerGoldText.text = $"Your Gold: {playerGold}";

            if (merchantGoldText != null && merchantGold >= 0)
                merchantGoldText.text = $"Merchant Gold: {merchantGold}";
        }
        #endregion

        #region Private Methods - Quantity
        private void OnQuantityInputChanged(string value)
        {
            if (int.TryParse(value, out int quantity))
            {
                transactionQuantity = Mathf.Max(1, quantity);
                UpdateQuantityUI();
                UpdatePriceDisplay(GetItemBasePrice());
                UpdateTransactionButton();
            }
        }

        private void OnQuantitySliderChanged(float value)
        {
            transactionQuantity = Mathf.RoundToInt(value);
            UpdateQuantityUI();
            UpdatePriceDisplay(GetItemBasePrice());
            UpdateTransactionButton();
        }

        private void UpdateQuantityUI()
        {
            if (quantityInput != null)
                quantityInput.text = transactionQuantity.ToString();

            if (quantitySlider != null)
                quantitySlider.value = transactionQuantity;
        }
        #endregion

        #region Private Methods - Data Access
        private string GetMerchantName()
        {
            // TODO: Get merchant name
            return "General Merchant";
        }

        private string GetMerchantDescription()
        {
            // TODO: Get merchant description
            return "Buy and sell general goods.";
        }
        #endregion

        #region Helper Structs
        private struct MerchantItemData
        {
            public IItem Item;
            public int BasePrice;
            public int Stock; // -1 for unlimited
        }

        private struct PlayerItemData
        {
            public IItem Item;
            public int Value;
            public int Quantity;
        }
        #endregion
    }
}
#endif
