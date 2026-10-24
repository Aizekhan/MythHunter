// Fixed: Updated to use ILootGenerator (Framework Pattern)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.Loot;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Loot UI for displaying loot rolls, item pickups, and loot windows.
    /// Keyboard shortcut: O (for open/loot)
    /// </summary>
    public class LootUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Loot Window")]
        [SerializeField] private GameObject lootWindowRoot;
        [SerializeField] private Button closeLootButton;
        [SerializeField] private Transform lootItemsContainer;
        [SerializeField] private GameObject lootItemEntryPrefab;
        [SerializeField] private Button takeAllButton;

        [Header("Loot Notifications")]
        [SerializeField] private GameObject notificationRoot;
        [SerializeField] private Transform notificationsContainer;
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private float notificationDuration = 3f;

        [Header("Loot Roll")]
        [SerializeField] private GameObject lootRollRoot;
        [SerializeField] private TextMeshProUGUI rollItemNameText;
        [SerializeField] private Image rollItemIcon;
        [SerializeField] private Transform rollPlayersContainer;
        [SerializeField] private GameObject rollPlayerEntryPrefab;
        [SerializeField] private Button rollNeedButton;
        [SerializeField] private Button rollGreedButton;
        [SerializeField] private Button rollPassButton;
        [SerializeField] private Slider rollTimerBar;

        [Header("Colors - Item Rarity")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = new Color(0.6f, 0f, 1f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.5f, 0f);
        #endregion

        #region Private Fields
        private ILootGenerator lootGenerator;
        private List<GameObject> lootItemEntries = new List<GameObject>();
        private List<GameObject> activeNotifications = new List<GameObject>();
        private LootResult? currentLootResult;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeLootButton != null)
                closeLootButton.onClick.AddListener(CloseLootWindow);

            if (takeAllButton != null)
                takeAllButton.onClick.AddListener(TakeAllLoot);

            // TODO: Implement loot roll buttons when loot roll system is complete
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseLootWindow();

            // TODO: Process notifications and loot rolls
        }
        #endregion

        #region Public Methods
        public void Initialize(ILootGenerator generator)
        {
            lootGenerator = generator;
        }

        public void ShowLootWindow(LootResult lootResult)
        {
            currentLootResult = lootResult;

            if (lootWindowRoot != null)
                lootWindowRoot.SetActive(true);

            RefreshLootItems();
        }

        public void CloseLootWindow()
        {
            if (lootWindowRoot != null)
                lootWindowRoot.SetActive(false);

            currentLootResult = null;
            ClearLootItems();
        }

        public void ShowLootNotification(string itemName, int quantity, Color rarityColor)
        {
            // TODO: Implement loot notifications when notification system is complete
        }

        public void HideLootRoll()
        {
            if (lootRollRoot != null)
                lootRollRoot.SetActive(false);
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            // TODO: Subscribe to loot events via EventBus
            // e.g., EventBus.Subscribe<LootGeneratedEvent>(OnLootGenerated);
        }

        private void UnsubscribeFromEvents()
        {
            // TODO: Unsubscribe from events
        }
        #endregion

        #region Private Methods - Loot Window
        private void RefreshLootItems()
        {
            ClearLootItems();

            if (!currentLootResult.HasValue) return;

            var items = currentLootResult.Value.Drops;

            if (items != null)
            {
                foreach (var drop in items)
                {
                    CreateLootItemEntry(drop);
                }
            }

            // Update take all button
            if (takeAllButton != null)
                takeAllButton.interactable = items != null && items.Count > 0;
        }

        private void CreateLootItemEntry(LootDrop drop)
        {
            if (lootItemEntryPrefab == null || lootItemsContainer == null) return;

            var entry = Instantiate(lootItemEntryPrefab, lootItemsContainer);
            lootItemEntries.Add(entry);

            // Set item name
            var nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                string quantityText = drop.Quantity > 1 ? $" x{drop.Quantity}" : "";
                nameText.text = $"{drop.ItemId}{quantityText}";
                nameText.color = commonColor; // TODO: Get rarity from item database
            }

            // Set click handler to take item
            var button = entry.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => TakeLootItem(drop));
        }

        private void TakeLootItem(LootDrop drop)
        {
            // TODO: Add item to player inventory via inventory system
            RefreshLootItems();
        }

        private void TakeAllLoot()
        {
            // TODO: Take all items from loot source
            // lootSystem.TakeAllLoot(currentLootSource);

            CloseLootWindow();
        }

        private void ClearLootItems()
        {
            foreach (var entry in lootItemEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            lootItemEntries.Clear();
        }
        #endregion

        #region Private Methods - Notifications (TODO: Implement)
        private System.Collections.IEnumerator AnimateNotification(GameObject notifObj, float duration)
        {
            var canvasGroup = notifObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = notifObj.AddComponent<CanvasGroup>();

            // Fade in
            float fadeInTime = 0.3f;
            float elapsed = 0f;
            while (elapsed < fadeInTime)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = elapsed / fadeInTime;
                yield return null;
            }

            // Wait
            yield return new WaitForSeconds(duration - fadeInTime - 0.5f);

            // Fade out
            elapsed = 0f;
            float fadeOutTime = 0.5f;
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / fadeOutTime);
                yield return null;
            }

            activeNotifications.Remove(notifObj);
            Destroy(notifObj);
        }
        #endregion

        #region Private Methods - Loot Roll (TODO: Implement)
        private void RefreshLootRollPlayers()
        {
            // TODO: Display all players participating in the roll and their choices
        }

        private void SubmitLootRoll(object rollType)
        {
            // TODO: Submit loot roll to loot system when implemented
        }
        #endregion
    }
}
