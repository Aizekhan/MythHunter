using UnityEngine;
using System.Collections.Generic;
using MythHunter.UI.Panels;

namespace MythHunter.UI.Managers
{
    /// <summary>
    /// Central UI manager coordinating all UI panels with keyboard shortcuts.
    /// Handles panel visibility, input management, and UI state.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI Panels")]
        [SerializeField] private QuestLogUI questLogUI;
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private CraftingUI craftingUI;
        [SerializeField] private TradingUI tradingUI;
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private FactionReputationUI factionReputationUI;
        [SerializeField] private StatsUI statsUI;
        [SerializeField] private AbilitiesUI abilitiesUI;
        [SerializeField] private StatusEffectsUI statusEffectsUI;
        [SerializeField] private CombatLogUI combatLogUI;
        [SerializeField] private LootUI lootUI;

        [Header("Settings")]
        [SerializeField] private bool allowMultiplePanelsOpen = false;
        [SerializeField] private bool pauseGameWhenPanelOpen = true;
        [SerializeField] private KeyCode closeAllPanelsKey = KeyCode.Escape;

        [Header("Keyboard Shortcuts")]
        [SerializeField] private KeyCode questLogKey = KeyCode.Q;
        [SerializeField] private KeyCode inventoryKey = KeyCode.I;
        [SerializeField] private KeyCode craftingKey = KeyCode.C;
        [SerializeField] private KeyCode tradingKey = KeyCode.T;
        [SerializeField] private KeyCode dialogueKey = KeyCode.D;
        [SerializeField] private KeyCode factionReputationKey = KeyCode.F;
        [SerializeField] private KeyCode statsKey = KeyCode.K;
        [SerializeField] private KeyCode abilitiesKey = KeyCode.P;
        [SerializeField] private KeyCode combatLogKey = KeyCode.L;
        [SerializeField] private KeyCode lootKey = KeyCode.O;
        #endregion

        #region Private Fields
        private Dictionary<string, IUIPanel> panels = new Dictionary<string, IUIPanel>();
        private List<string> openPanels = new List<string>();
        private static UIManager instance;
        #endregion

        #region Singleton
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<UIManager>();

                    if (instance == null)
                    {
                        var obj = new GameObject("UIManager");
                        instance = obj.AddComponent<UIManager>();
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePanels();
        }

        private void Update()
        {
            HandleKeyboardInput();
        }
        #endregion

        #region Initialization
        private void InitializePanels()
        {
            // Register all panels
            RegisterPanel("QuestLog", questLogUI);
            RegisterPanel("Inventory", inventoryUI);
            RegisterPanel("Crafting", craftingUI);
            RegisterPanel("Trading", tradingUI);
            RegisterPanel("Dialogue", dialogueUI);
            RegisterPanel("FactionReputation", factionReputationUI);
            RegisterPanel("Stats", statsUI);
            RegisterPanel("Abilities", abilitiesUI);
            RegisterPanel("StatusEffects", statusEffectsUI);
            RegisterPanel("CombatLog", combatLogUI);
            RegisterPanel("Loot", lootUI);
        }

        private void RegisterPanel(string panelName, MonoBehaviour panelComponent)
        {
            if (panelComponent == null) return;

            var panel = panelComponent as IUIPanel;
            if (panel != null)
            {
                panels[panelName] = panel;
            }
            else
            {
                // Create wrapper for MonoBehaviour panels
                panels[panelName] = new UIMonoBehaviourWrapper(panelComponent);
            }
        }
        #endregion

        #region Input Handling
        private void HandleKeyboardInput()
        {
            // Close all panels on Escape
            if (Input.GetKeyDown(closeAllPanelsKey))
            {
                CloseAllPanels();
                return;
            }

            // Individual panel shortcuts
            if (Input.GetKeyDown(questLogKey))
                TogglePanel("QuestLog");

            if (Input.GetKeyDown(inventoryKey))
                TogglePanel("Inventory");

            if (Input.GetKeyDown(craftingKey))
                TogglePanel("Crafting");

            if (Input.GetKeyDown(tradingKey))
                TogglePanel("Trading");

            if (Input.GetKeyDown(dialogueKey))
                TogglePanel("Dialogue");

            if (Input.GetKeyDown(factionReputationKey))
                TogglePanel("FactionReputation");

            if (Input.GetKeyDown(statsKey))
                TogglePanel("Stats");

            if (Input.GetKeyDown(abilitiesKey))
                TogglePanel("Abilities");

            if (Input.GetKeyDown(combatLogKey))
                TogglePanel("CombatLog");

            if (Input.GetKeyDown(lootKey))
                TogglePanel("Loot");
        }
        #endregion

        #region Public Methods - Panel Control
        public void ShowPanel(string panelName)
        {
            if (!panels.ContainsKey(panelName))
            {
                Debug.LogWarning($"Panel '{panelName}' not found");
                return;
            }

            // Close other panels if not allowing multiple
            if (!allowMultiplePanelsOpen && openPanels.Count > 0)
            {
                CloseAllPanels();
            }

            // Show panel
            panels[panelName].Show();

            if (!openPanels.Contains(panelName))
                openPanels.Add(panelName);

            // Handle game pause
            if (pauseGameWhenPanelOpen)
                Time.timeScale = 0f;
        }

        public void HidePanel(string panelName)
        {
            if (!panels.ContainsKey(panelName))
            {
                Debug.LogWarning($"Panel '{panelName}' not found");
                return;
            }

            panels[panelName].Hide();
            openPanels.Remove(panelName);

            // Resume game if no panels open
            if (openPanels.Count == 0 && pauseGameWhenPanelOpen)
                Time.timeScale = 1f;
        }

        public void TogglePanel(string panelName)
        {
            if (IsPanelOpen(panelName))
                HidePanel(panelName);
            else
                ShowPanel(panelName);
        }

        public void CloseAllPanels()
        {
            // Create copy to avoid modification during iteration
            var panelsToClose = new List<string>(openPanels);

            foreach (var panelName in panelsToClose)
            {
                HidePanel(panelName);
            }

            // Resume game
            if (pauseGameWhenPanelOpen)
                Time.timeScale = 1f;
        }

        public bool IsPanelOpen(string panelName)
        {
            return openPanels.Contains(panelName);
        }

        public bool IsAnyPanelOpen()
        {
            return openPanels.Count > 0;
        }
        #endregion

        #region Public Methods - Specific Panel Access
        public QuestLogUI GetQuestLogUI() => questLogUI;
        public InventoryUI GetInventoryUI() => inventoryUI;
        public CraftingUI GetCraftingUI() => craftingUI;
        public TradingUI GetTradingUI() => tradingUI;
        public DialogueUI GetDialogueUI() => dialogueUI;
        public FactionReputationUI GetFactionReputationUI() => factionReputationUI;
        public StatsUI GetStatsUI() => statsUI;
        public AbilitiesUI GetAbilitiesUI() => abilitiesUI;
        public StatusEffectsUI GetStatusEffectsUI() => statusEffectsUI;
        public CombatLogUI GetCombatLogUI() => combatLogUI;
        public LootUI GetLootUI() => lootUI;
        #endregion

        #region Helper Interface & Wrapper
        private interface IUIPanel
        {
            void Show();
            void Hide();
            void Toggle();
        }

        private class UIMonoBehaviourWrapper : IUIPanel
        {
            private readonly MonoBehaviour component;
            private readonly System.Reflection.MethodInfo showMethod;
            private readonly System.Reflection.MethodInfo hideMethod;
            private readonly System.Reflection.MethodInfo toggleMethod;

            public UIMonoBehaviourWrapper(MonoBehaviour component)
            {
                this.component = component;

                var type = component.GetType();
                showMethod = type.GetMethod("Show");
                hideMethod = type.GetMethod("Hide");
                toggleMethod = type.GetMethod("Toggle");
            }

            public void Show()
            {
                showMethod?.Invoke(component, null);
            }

            public void Hide()
            {
                hideMethod?.Invoke(component, null);
            }

            public void Toggle()
            {
                toggleMethod?.Invoke(component, null);
            }
        }
        #endregion
    }
}
