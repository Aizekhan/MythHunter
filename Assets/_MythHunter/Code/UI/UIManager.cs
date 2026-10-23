using UnityEngine;
using MythHunter.UI.Dialogue;
using MythHunter.UI.Factions;
using MythHunter.GameLogic.Dialogue;
using MythHunter.GameLogic.Factions;
using RPGFramework.ECS.Contracts;
using RPGFramework.Logging.Contracts;

namespace MythHunter.UI
{
    /// <summary>
    /// Central UI manager coordinating all UI systems.
    /// Manages UI panel visibility, input handling, and integration with game systems.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private FactionReputationUI factionUI;
        // Future: QuestUI, InventoryUI, CraftingUI, TradingUI, etc.

        [Header("Settings")]
        [SerializeField] private bool pauseGameDuringDialogue = true;
        [SerializeField] private KeyCode openFactionPanelKey = KeyCode.R; // R for Reputation

        private DialogueSystemIntegration _dialogueIntegration;
        private FactionSystemIntegration _factionIntegration;
        private int _playerEntityId;

        private bool _isInitialized;

        /// <summary>
        /// Initialize UI Manager with game systems
        /// </summary>
        public void Initialize(
            IEntityManager entityManager,
            IMythLogger logger,
            int playerEntityId)
        {
            _playerEntityId = playerEntityId;

            // Create integration systems
            _dialogueIntegration = new DialogueSystemIntegration(entityManager, logger);
            _factionIntegration = new FactionSystemIntegration(entityManager, logger);

            // Initialize UI components
            if (dialogueUI != null)
            {
                dialogueUI.Initialize(_dialogueIntegration);
            }

            if (factionUI != null)
            {
                factionUI.Initialize(_factionIntegration, playerEntityId);
            }

            _isInitialized = true;

            Debug.Log("UIManager initialized successfully");
        }

        private void Update()
        {
            if (!_isInitialized) return;

            HandleInput();
            UpdateTimeScale();
        }

        /// <summary>
        /// Handle UI input
        /// </summary>
        private void HandleInput()
        {
            // Open faction panel
            if (Input.GetKeyDown(openFactionPanelKey))
            {
                if (factionUI != null)
                {
                    if (factionUI.IsPanelOpen())
                    {
                        factionUI.CloseFactionPanel();
                    }
                    else
                    {
                        factionUI.OpenFactionPanel();
                    }
                }
            }

            // ESC to close any open panels
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (factionUI != null && factionUI.IsPanelOpen())
                {
                    factionUI.CloseFactionPanel();
                }
                // Future: Close other panels (Inventory, Crafting, etc.)
            }
        }

        /// <summary>
        /// Update game time scale based on UI state
        /// </summary>
        private void UpdateTimeScale()
        {
            if (pauseGameDuringDialogue && dialogueUI != null)
            {
                if (dialogueUI.IsDialogueActive())
                {
                    Time.timeScale = 0f; // Pause game during dialogue
                }
                else if (!IsAnyPanelOpen())
                {
                    Time.timeScale = 1f; // Resume game when no UI is open
                }
            }
        }

        /// <summary>
        /// Check if any UI panel is currently open
        /// </summary>
        private bool IsAnyPanelOpen()
        {
            bool anyOpen = false;

            if (dialogueUI != null && dialogueUI.IsDialogueActive())
            {
                anyOpen = true;
            }

            if (factionUI != null && factionUI.IsPanelOpen())
            {
                anyOpen = true;
            }

            // Future: Check other panels (Inventory, Crafting, etc.)

            return anyOpen;
        }

        #region Public API for Game Systems

        /// <summary>
        /// Start a dialogue conversation
        /// </summary>
        public void StartDialogue(string dialogueId)
        {
            if (dialogueUI != null)
            {
                dialogueUI.StartDialogue(_playerEntityId, dialogueId);
            }
        }

        /// <summary>
        /// Open the faction reputation panel
        /// </summary>
        public void OpenFactionPanel()
        {
            if (factionUI != null)
            {
                factionUI.OpenFactionPanel();
            }
        }

        /// <summary>
        /// Update faction reputation display
        /// </summary>
        public void UpdateFactionReputation(string factionId)
        {
            if (factionUI != null)
            {
                factionUI.UpdateReputation(factionId);
            }
        }

        #endregion

        #region Getters

        public DialogueUI DialogueUI => dialogueUI;
        public FactionReputationUI FactionUI => factionUI;

        #endregion

        private void OnApplicationQuit()
        {
            // Restore time scale
            Time.timeScale = 1f;
        }
    }
}
