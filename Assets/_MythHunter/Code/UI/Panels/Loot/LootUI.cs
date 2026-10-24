// TODO: This UI needs refactoring to work with actual Framework Loot interfaces
#if FALSE
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RPGFramework.Loot;
using RPGFramework.Items;

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
        private ILootSystem lootSystem;
        private List<GameObject> lootItemEntries = new List<GameObject>();
        private List<GameObject> activeNotifications = new List<GameObject>();
        private Queue<LootNotification> notificationQueue = new Queue<LootNotification>();
        private ILootSource currentLootSource;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeLootButton != null)
                closeLootButton.onClick.AddListener(CloseLootWindow);

            if (takeAllButton != null)
                takeAllButton.onClick.AddListener(TakeAllLoot);

            if (rollNeedButton != null)
                rollNeedButton.onClick.AddListener(() => SubmitLootRoll(LootRollType.Need));

            if (rollGreedButton != null)
                rollGreedButton.onClick.AddListener(() => SubmitLootRoll(LootRollType.Greed));

            if (rollPassButton != null)
                rollPassButton.onClick.AddListener(() => SubmitLootRoll(LootRollType.Pass));
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

            ProcessNotificationQueue();
            UpdateLootRollTimer();
        }
        #endregion

        #region Public Methods
        public void Initialize(ILootSystem system)
        {
            lootSystem = system;
        }

        public void ShowLootWindow(ILootSource lootSource)
        {
            currentLootSource = lootSource;

            if (lootWindowRoot != null)
                lootWindowRoot.SetActive(true);

            RefreshLootItems();
        }

        public void CloseLootWindow()
        {
            if (lootWindowRoot != null)
                lootWindowRoot.SetActive(false);

            currentLootSource = null;
            ClearLootItems();
        }

        public void ShowLootNotification(string itemName, int quantity, Color rarityColor)
        {
            notificationQueue.Enqueue(new LootNotification
            {
                ItemName = itemName,
                Quantity = quantity,
                RarityColor = rarityColor,
                TimeToShow = notificationDuration
            });
        }

        public void ShowLootRoll(IItem item, float rollDuration)
        {
            if (lootRollRoot != null)
                lootRollRoot.SetActive(true);

            if (rollItemNameText != null)
            {
                rollItemNameText.text = item.Name;
                rollItemNameText.color = GetRarityColor(item);
            }

            // TODO: Set item icon
            // if (rollItemIcon != null)
            //     rollItemIcon.sprite = item.Icon;

            if (rollTimerBar != null)
            {
                rollTimerBar.maxValue = rollDuration;
                rollTimerBar.value = rollDuration;
            }

            RefreshLootRollPlayers();
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
            if (lootSystem == null) return;

            // Subscribe to loot events
            // lootSystem.OnLootGenerated += OnLootGenerated;
            // lootSystem.OnItemLooted += OnItemLooted;
            // lootSystem.OnLootRollStarted += OnLootRollStarted;
            // lootSystem.OnLootRollCompleted += OnLootRollCompleted;
        }

        private void UnsubscribeFromEvents()
        {
            if (lootSystem == null) return;

            // Unsubscribe from loot events
            // lootSystem.OnLootGenerated -= OnLootGenerated;
            // lootSystem.OnItemLooted -= OnItemLooted;
            // lootSystem.OnLootRollStarted -= OnLootRollStarted;
            // lootSystem.OnLootRollCompleted -= OnLootRollCompleted;
        }

        private void OnItemLooted(IItem item, int quantity)
        {
            ShowLootNotification(item.Name, quantity, GetRarityColor(item));
        }
        #endregion

        #region Private Methods - Loot Window
        private void RefreshLootItems()
        {
            ClearLootItems();

            if (currentLootSource == null) return;

            var items = GetLootItems(currentLootSource);

            foreach (var item in items)
            {
                CreateLootItemEntry(item);
            }

            // Update take all button
            if (takeAllButton != null)
                takeAllButton.interactable = items.Count > 0;
        }

        private void CreateLootItemEntry(LootItemData item)
        {
            if (lootItemEntryPrefab == null || lootItemsContainer == null) return;

            var entry = Instantiate(lootItemEntryPrefab, lootItemsContainer);
            lootItemEntries.Add(entry);

            // Set item name
            var nameText = entry.GetComponentsInChildren<TextMeshProUGUI>()
                .FirstOrDefault(t => t.gameObject.name.Contains("Name"));
            if (nameText != null)
            {
                nameText.text = item.Quantity > 1 ? $"{item.Name} x{item.Quantity}" : item.Name;
                nameText.color = item.RarityColor;
            }

            // Set item icon
            var icon = entry.GetComponentsInChildren<Image>()
                .FirstOrDefault(img => img.gameObject.name.Contains("Icon"));
            if (icon != null)
            {
                // TODO: Set item icon
                // icon.sprite = item.Icon;
            }

            // Set click handler to take item
            var button = entry.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => TakeLootItem(item));
        }

        private void TakeLootItem(LootItemData item)
        {
            // TODO: Take item from loot source
            // lootSystem.TakeLoot(currentLootSource, item);

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

        private List<LootItemData> GetLootItems(ILootSource lootSource)
        {
            // TODO: Get loot items from loot source
            return new List<LootItemData>
            {
                new LootItemData
                {
                    Name = "Health Potion",
                    Quantity = 3,
                    RarityColor = commonColor
                },
                new LootItemData
                {
                    Name = "Rare Sword",
                    Quantity = 1,
                    RarityColor = rareColor
                }
            };
        }
        #endregion

        #region Private Methods - Notifications
        private void ProcessNotificationQueue()
        {
            // Clean up expired notifications
            for (int i = activeNotifications.Count - 1; i >= 0; i--)
            {
                if (activeNotifications[i] == null)
                {
                    activeNotifications.RemoveAt(i);
                }
            }

            // Show new notifications
            while (notificationQueue.Count > 0 && activeNotifications.Count < 5)
            {
                var notification = notificationQueue.Dequeue();
                CreateNotification(notification);
            }
        }

        private void CreateNotification(LootNotification notification)
        {
            if (notificationPrefab == null || notificationsContainer == null) return;

            var notifObj = Instantiate(notificationPrefab, notificationsContainer);
            activeNotifications.Add(notifObj);

            var text = notifObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                string quantityText = notification.Quantity > 1 ? $" x{notification.Quantity}" : "";
                text.text = $"Looted: {notification.ItemName}{quantityText}";
                text.color = notification.RarityColor;
            }

            // Animate and destroy after duration
            StartCoroutine(AnimateNotification(notifObj, notification.TimeToShow));
        }

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

        #region Private Methods - Loot Roll
        private void RefreshLootRollPlayers()
        {
            // TODO: Display all players participating in the roll and their choices
        }

        private void SubmitLootRoll(LootRollType rollType)
        {
            // TODO: Submit loot roll to loot system
            // lootSystem.SubmitLootRoll(rollType);

            HideLootRoll();
        }

        private void UpdateLootRollTimer()
        {
            if (lootRollRoot == null || !lootRollRoot.activeSelf) return;

            // TODO: Update roll timer bar
            if (rollTimerBar != null)
            {
                rollTimerBar.value -= Time.deltaTime;

                if (rollTimerBar.value <= 0)
                {
                    // Auto-pass if time expires
                    SubmitLootRoll(LootRollType.Pass);
                }
            }
        }
        #endregion

        #region Private Methods - Helpers
        private Color GetRarityColor(IItem item)
        {
            // TODO: Get actual item rarity
            return commonColor;
        }
        #endregion

        #region Helper Structs & Enums
        private struct LootItemData
        {
            public string Name;
            public int Quantity;
            public Color RarityColor;
        }

        private struct LootNotification
        {
            public string ItemName;
            public int Quantity;
            public Color RarityColor;
            public float TimeToShow;
        }

        public enum LootRollType
        {
            Need,
            Greed,
            Pass
        }
        #endregion
    }
}
#endif
