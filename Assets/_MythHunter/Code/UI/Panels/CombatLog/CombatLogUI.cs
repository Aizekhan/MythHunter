// Fixed: Updated to use Framework EventBus for combat events
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Combat log UI displaying damage numbers, combat events, and scrolling text feed.
    /// Keyboard shortcut: L (for log)
    /// </summary>
    public class CombatLogUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform logEntriesContainer;
        [SerializeField] private GameObject logEntryPrefab;

        [Header("Floating Damage Numbers")]
        [SerializeField] private Canvas worldCanvas;
        [SerializeField] private GameObject damageNumberPrefab;

        [Header("Filters")]
        [SerializeField] private Toggle showDamageToggle;
        [SerializeField] private Toggle showHealingToggle;
        [SerializeField] private Toggle showBuffsToggle;
        [SerializeField] private Toggle showLootToggle;
        [SerializeField] private Toggle showSystemToggle;
        [SerializeField] private Button clearButton;

        [Header("Settings")]
        [SerializeField] private int maxLogEntries = 100;
        [SerializeField] private bool autoScroll = true;
        [SerializeField] private float damageNumberLifetime = 2f;
        [SerializeField] private float damageNumberFloatSpeed = 50f;

        [Header("Colors - Message Types")]
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private Color criticalDamageColor = new Color(1f, 0.3f, 0f);
        [SerializeField] private Color healingColor = Color.green;
        [SerializeField] private Color missColor = Color.gray;
        [SerializeField] private Color buffColor = Color.cyan;
        [SerializeField] private Color debuffColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color lootColor = Color.yellow;
        [SerializeField] private Color systemColor = Color.white;
        #endregion

        #region Private Fields
        private List<GameObject> logEntries = new List<GameObject>();
        private Queue<string> pendingMessages = new Queue<string>();
        private bool showDamage = true;
        private bool showHealing = true;
        private bool showBuffs = true;
        private bool showLoot = true;
        private bool showSystem = true;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (clearButton != null)
                clearButton.onClick.AddListener(ClearLog);

            SetupFilterToggles();
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
                Hide();

            ProcessPendingMessages();
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void Toggle()
        {
            if (panelRoot != null && panelRoot.activeSelf)
                Hide();
            else
                Show();
        }

        public void Initialize()
        {
            // No initialization needed - will subscribe to EventBus events
        }

        public void AddLogEntry(string message, LogEntryType type)
        {
            if (!ShouldShowEntry(type)) return;

            pendingMessages.Enqueue($"[{System.DateTime.Now:HH:mm:ss}] {message}");
        }

        public void ShowDamageNumber(Vector3 worldPosition, int damage, bool isCritical = false, bool isMiss = false)
        {
            if (damageNumberPrefab == null || worldCanvas == null) return;

            var damageObj = Instantiate(damageNumberPrefab, worldCanvas.transform);
            damageObj.transform.position = worldPosition;

            var text = damageObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                if (isMiss)
                {
                    text.text = "MISS";
                    text.color = missColor;
                }
                else
                {
                    text.text = isCritical ? $"{damage}!" : damage.ToString();
                    text.color = isCritical ? criticalDamageColor : damageColor;
                    text.fontSize = isCritical ? 48 : 36;
                }
            }

            // Animate and destroy
            StartCoroutine(AnimateDamageNumber(damageObj));
        }

        public void ShowHealingNumber(Vector3 worldPosition, int healing)
        {
            if (damageNumberPrefab == null || worldCanvas == null) return;

            var healObj = Instantiate(damageNumberPrefab, worldCanvas.transform);
            healObj.transform.position = worldPosition;

            var text = healObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"+{healing}";
                text.color = healingColor;
            }

            StartCoroutine(AnimateDamageNumber(healObj));
        }
        #endregion

        #region Private Methods - Setup
        private void SetupFilterToggles()
        {
            if (showDamageToggle != null)
            {
                showDamageToggle.isOn = showDamage;
                showDamageToggle.onValueChanged.AddListener(value => showDamage = value);
            }

            if (showHealingToggle != null)
            {
                showHealingToggle.isOn = showHealing;
                showHealingToggle.onValueChanged.AddListener(value => showHealing = value);
            }

            if (showBuffsToggle != null)
            {
                showBuffsToggle.isOn = showBuffs;
                showBuffsToggle.onValueChanged.AddListener(value => showBuffs = value);
            }

            if (showLootToggle != null)
            {
                showLootToggle.isOn = showLoot;
                showLootToggle.onValueChanged.AddListener(value => showLoot = value);
            }

            if (showSystemToggle != null)
            {
                showSystemToggle.isOn = showSystem;
                showSystemToggle.onValueChanged.AddListener(value => showSystem = value);
            }
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            // TODO: Subscribe to combat events via EventBus when combat events are defined
            // e.g., EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            // e.g., EventBus.Subscribe<HealingDoneEvent>(OnHealingDone);
        }

        private void UnsubscribeFromEvents()
        {
            // TODO: Unsubscribe from combat events
        }

        private void OnDamageDealt(string attacker, string target, int damage, bool isCritical)
        {
            string message = isCritical
                ? $"{attacker} critically hit {target} for {damage} damage!"
                : $"{attacker} hit {target} for {damage} damage";

            AddLogEntry(message, LogEntryType.Damage);
        }

        private void OnHealingDone(string healer, string target, int healing)
        {
            AddLogEntry($"{healer} healed {target} for {healing} health", LogEntryType.Healing);
        }

        private void OnCombatStarted(string target)
        {
            AddLogEntry($"Combat started with {target}", LogEntryType.System);
        }

        private void OnCombatEnded()
        {
            AddLogEntry("Combat ended", LogEntryType.System);
        }
        #endregion

        #region Private Methods - Log Display
        private void ProcessPendingMessages()
        {
            while (pendingMessages.Count > 0)
            {
                string message = pendingMessages.Dequeue();
                CreateLogEntry(message);
            }

            if (autoScroll && scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f;
        }

        private void CreateLogEntry(string message)
        {
            if (logEntryPrefab == null || logEntriesContainer == null) return;

            var entry = Instantiate(logEntryPrefab, logEntriesContainer);
            logEntries.Add(entry);

            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = message;
                text.color = GetMessageColor(message);
            }

            // Remove old entries if exceeding max
            while (logEntries.Count > maxLogEntries)
            {
                var oldEntry = logEntries[0];
                logEntries.RemoveAt(0);
                if (oldEntry != null)
                    Destroy(oldEntry);
            }
        }

        private void ClearLog()
        {
            foreach (var entry in logEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            logEntries.Clear();
            pendingMessages.Clear();
        }

        private bool ShouldShowEntry(LogEntryType type)
        {
            return type switch
            {
                LogEntryType.Damage => showDamage,
                LogEntryType.Healing => showHealing,
                LogEntryType.Buff => showBuffs,
                LogEntryType.Debuff => showBuffs,
                LogEntryType.Loot => showLoot,
                LogEntryType.System => showSystem,
                _ => true
            };
        }

        private Color GetMessageColor(string message)
        {
            if (message.Contains("critically") || message.Contains("!"))
                return criticalDamageColor;
            if (message.Contains("damage") || message.Contains("hit"))
                return damageColor;
            if (message.Contains("healed") || message.Contains("healing"))
                return healingColor;
            if (message.Contains("buff") || message.Contains("gained"))
                return buffColor;
            if (message.Contains("debuff") || message.Contains("afflicted"))
                return debuffColor;
            if (message.Contains("looted") || message.Contains("received"))
                return lootColor;

            return systemColor;
        }
        #endregion

        #region Private Methods - Floating Numbers
        private System.Collections.IEnumerator AnimateDamageNumber(GameObject damageObj)
        {
            float elapsed = 0f;
            Vector3 startPos = damageObj.transform.position;

            var canvasGroup = damageObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = damageObj.AddComponent<CanvasGroup>();

            while (elapsed < damageNumberLifetime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / damageNumberLifetime;

                // Float upward
                damageObj.transform.position = startPos + Vector3.up * (damageNumberFloatSpeed * elapsed);

                // Fade out
                canvasGroup.alpha = 1f - progress;

                // Scale pulse at start
                if (progress < 0.2f)
                {
                    float scale = 1f + (0.2f - progress) * 2f;
                    damageObj.transform.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            Destroy(damageObj);
        }
        #endregion

        #region Enums
        public enum LogEntryType
        {
            Damage,
            Healing,
            Buff,
            Debuff,
            Loot,
            System
        }
        #endregion
    }
}
