// Fixed: Updated to use IProgressionTracker and IEntityManager (Framework ECS Pattern)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RPGFramework.Progression;
using RPGFramework.ECS;
using MythHunter.Components.Character;
using MythHunter.Components.Combat;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Character stats UI with attributes, skills, and progression bars.
    /// Keyboard shortcut: K (for character)
    /// </summary>
    public class StatsUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("Character Info")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI characterLevelText;
        [SerializeField] private TextMeshProUGUI characterClassText;
        [SerializeField] private Slider experienceBar;
        [SerializeField] private TextMeshProUGUI experienceText;

        [Header("Primary Stats")]
        [SerializeField] private Transform primaryStatsContainer;
        [SerializeField] private GameObject statEntryPrefab;

        [Header("Secondary Stats")]
        [SerializeField] private Transform secondaryStatsContainer;

        [Header("Resistances")]
        [SerializeField] private Transform resistancesContainer;

        [Header("Skills")]
        [SerializeField] private Transform skillsContainer;
        [SerializeField] private GameObject skillEntryPrefab;

        [Header("Attribute Points")]
        [SerializeField] private TextMeshProUGUI availablePointsText;
        [SerializeField] private Button resetStatsButton;

        [Header("Tabs")]
        [SerializeField] private Button statsTab;
        [SerializeField] private Button skillsTab;
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private GameObject skillsPanel;
        #endregion

        #region Private Fields
        private IProgressionTracker progressionTracker;
        private IEntityManager entityManager;
        private int playerEntityId;
        private List<GameObject> statEntries = new List<GameObject>();
        private List<GameObject> skillEntries = new List<GameObject>();
        private bool showingStats = true;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (statsTab != null)
                statsTab.onClick.AddListener(() => ShowTab(true));

            if (skillsTab != null)
                skillsTab.onClick.AddListener(() => ShowTab(false));

            if (resetStatsButton != null)
                resetStatsButton.onClick.AddListener(ResetStats);
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

        public void Initialize(IProgressionTracker progression, IEntityManager entityMgr, int playerEntity)
        {
            progressionTracker = progression;
            entityManager = entityMgr;
            playerEntityId = playerEntity;
            RefreshAll();
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            // TODO: Subscribe to stat change events via EventBus if needed
            // e.g., EventBus.Subscribe<StatChangedEvent>(OnStatChanged);
        }

        private void UnsubscribeFromEvents()
        {
            // TODO: Unsubscribe from events
        }
        #endregion

        #region Private Methods - Display
        private void ShowTab(bool showStats)
        {
            showingStats = showStats;

            if (statsPanel != null)
                statsPanel.SetActive(showStats);

            if (skillsPanel != null)
                skillsPanel.SetActive(!showStats);
        }

        private void RefreshAll()
        {
            UpdateCharacterInfo();
            RefreshStats();
            RefreshSkills();
        }

        private void UpdateCharacterInfo()
        {
            if (progressionTracker == null || entityManager == null) return;

            // Get progression data
            var xpData = progressionTracker.GetExperienceData(playerEntityId);
            int currentLevel = progressionTracker.GetLevel(playerEntityId);
            long totalXP = progressionTracker.GetTotalXP(playerEntityId);
            long xpForNextLevel = progressionTracker.GetXPRequiredForLevel(currentLevel + 1);
            long xpForCurrentLevel = currentLevel > 1 ? progressionTracker.GetXPRequiredForLevel(currentLevel) : 0;
            long xpInCurrentLevel = totalXP - xpForCurrentLevel;
            long xpNeededForNextLevel = xpForNextLevel - xpForCurrentLevel;

            // Update character name (can be extended to get actual name from NameComponent)
            if (characterNameText != null)
                characterNameText.text = "Player";

            if (characterLevelText != null)
                characterLevelText.text = $"Level {currentLevel}";

            // TODO: Get class from component when available
            if (characterClassText != null)
                characterClassText.text = "Hero";

            // Update experience bar
            if (experienceBar != null && xpNeededForNextLevel > 0)
            {
                experienceBar.value = xpInCurrentLevel;
                experienceBar.maxValue = xpNeededForNextLevel;
            }

            if (experienceText != null)
                experienceText.text = $"{xpInCurrentLevel} / {xpNeededForNextLevel} XP";

            // TODO: Available attribute points from component when implemented
            if (availablePointsText != null)
                availablePointsText.text = "Available Points: 0";
        }

        private void RefreshStats()
        {
            ClearStatEntries();

            if (entityManager == null) return;

            // Try to get stats from MythHunterStatsComponent (combat stats)
            if (entityManager.HasComponent<MythHunterStatsComponent>(playerEntityId))
            {
                var combatStats = entityManager.GetComponent<MythHunterStatsComponent>(playerEntityId);

                // Display combat stats in primary container
                CreateStatEntry("Attack Power", combatStats.Stats.GetFinalStat(MythHunter.GameLogic.Combat.CombatStatType.AttackPower), primaryStatsContainer, false);
                CreateStatEntry("Defense", combatStats.Stats.GetFinalStat(MythHunter.GameLogic.Combat.CombatStatType.Defense), primaryStatsContainer, false);
                CreateStatEntry("Attack Speed", combatStats.Stats.GetFinalStat(MythHunter.GameLogic.Combat.CombatStatType.AttackSpeed), primaryStatsContainer, false);

                // Display secondary stats
                CreateStatEntry("Critical Chance", combatStats.Stats.GetFinalStat(MythHunter.GameLogic.Combat.CombatStatType.CriticalChance) * 100f, secondaryStatsContainer, false, "%");
                CreateStatEntry("Critical Multiplier", combatStats.Stats.GetFinalStat(MythHunter.GameLogic.Combat.CombatStatType.CriticalMultiplier), secondaryStatsContainer, false, "x");
                CreateStatEntry("Accuracy", combatStats.Stats.GetFinalStat(MythHunter.GameLogic.Combat.CombatStatType.Accuracy) * 100f, secondaryStatsContainer, false, "%");
                CreateStatEntry("Dodge Chance", combatStats.Stats.GetFinalStat(MythHunter.GameLogic.Combat.CombatStatType.DodgeChance) * 100f, secondaryStatsContainer, false, "%");
                CreateStatEntry("Block Chance", combatStats.Stats.GetFinalStat(MythHunter.GameLogic.Combat.CombatStatType.BlockChance) * 100f, secondaryStatsContainer, false, "%");
            }
            // Fallback to basic StatsComponent if available
            else if (entityManager.HasComponent<StatsComponent>(playerEntityId))
            {
                var stats = entityManager.GetComponent<StatsComponent>(playerEntityId);

                CreateStatEntry("Health", stats.HP, primaryStatsContainer, false);
                CreateStatEntry("HP Regen", stats.HpRegen, primaryStatsContainer, false);
                CreateStatEntry("Stamina", stats.Stamina, primaryStatsContainer, false);
                CreateStatEntry("Damage", stats.Damage, secondaryStatsContainer, false);
                CreateStatEntry("Armor", stats.ArmorResistance, secondaryStatsContainer, false);
                CreateStatEntry("Crit Chance", stats.CritChance * 100f, secondaryStatsContainer, false, "%");
                CreateStatEntry("Block Chance", stats.BlockChance * 100f, secondaryStatsContainer, false, "%");
            }
        }

        private void CreateStatEntry(string statName, float value, Transform container, bool canIncrease, string suffix = "")
        {
            if (statEntryPrefab == null || container == null) return;

            var entry = Instantiate(statEntryPrefab, container);
            statEntries.Add(entry);

            var nameText = entry.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = statName;

            var valueText = entry.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
            if (valueText != null)
                valueText.text = value.ToString("F1") + suffix;

            if (canIncrease)
            {
                var increaseButton = entry.GetComponentInChildren<Button>();
                if (increaseButton != null)
                    increaseButton.onClick.AddListener(() => IncreaseStat(statName));
            }
        }

        private void RefreshSkills()
        {
            ClearSkillEntries();

            // TODO: Implement skills display when Skills system component is available
            // For now, show placeholder message
            if (skillsContainer != null && skillEntryPrefab != null)
            {
                CreateSkillEntry("Skills System", 0, 100);
            }
        }

        private void CreateSkillEntry(string skillName, float current, float max)
        {
            if (skillEntryPrefab == null || skillsContainer == null) return;

            var entry = Instantiate(skillEntryPrefab, skillsContainer);
            skillEntries.Add(entry);

            var nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = $"{skillName}: {current:F0}";

            var progressBar = entry.GetComponentInChildren<Slider>();
            if (progressBar != null)
            {
                progressBar.maxValue = max;
                progressBar.value = current;
            }
        }

        private void ClearStatEntries()
        {
            foreach (var entry in statEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            statEntries.Clear();
        }

        private void ClearSkillEntries()
        {
            foreach (var entry in skillEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            skillEntries.Clear();
        }
        #endregion

        #region Private Methods - Actions
        private void IncreaseStat(string statName)
        {
            // TODO: Implement attribute point spending when system is available
            RefreshAll();
        }

        private void ResetStats()
        {
            // TODO: Implement stat reset when system is available
            RefreshAll();
        }
        #endregion
    }
}
