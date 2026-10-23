using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RPGFramework.Stats;
using RPGFramework.Progression;

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
        private IStatsSystem statsSystem;
        private IProgressionSystem progressionSystem;
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

        public void Initialize(IStatsSystem stats, IProgressionSystem progression)
        {
            statsSystem = stats;
            progressionSystem = progression;
            RefreshAll();
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            if (statsSystem == null) return;
            // statsSystem.OnStatChanged += OnStatChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (statsSystem == null) return;
            // statsSystem.OnStatChanged -= OnStatChanged;
        }

        private void OnStatChanged(string statName, float oldValue, float newValue)
        {
            RefreshAll();
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
            // TODO: Get character info
            if (characterNameText != null)
                characterNameText.text = "Player";

            if (characterLevelText != null)
                characterLevelText.text = "Level 10";

            if (characterClassText != null)
                characterClassText.text = "Warrior";

            // Update experience bar
            if (experienceBar != null)
            {
                experienceBar.value = 750f; // TODO: Get actual XP
                experienceBar.maxValue = 1000f;
            }

            if (experienceText != null)
                experienceText.text = "750 / 1000 XP";

            // Available attribute points
            if (availablePointsText != null)
                availablePointsText.text = "Available Points: 5";
        }

        private void RefreshStats()
        {
            ClearStatEntries();

            if (statsSystem == null) return;

            // Create primary stat entries
            CreateStatEntry("Strength", 25, primaryStatsContainer, true);
            CreateStatEntry("Dexterity", 18, primaryStatsContainer, true);
            CreateStatEntry("Intelligence", 12, primaryStatsContainer, true);
            CreateStatEntry("Vitality", 20, primaryStatsContainer, true);

            // Create secondary stat entries
            CreateStatEntry("Health", 500, secondaryStatsContainer, false);
            CreateStatEntry("Mana", 200, secondaryStatsContainer, false);
            CreateStatEntry("Armor", 85, secondaryStatsContainer, false);
            CreateStatEntry("Attack Power", 125, secondaryStatsContainer, false);
        }

        private void CreateStatEntry(string statName, float value, Transform container, bool canIncrease)
        {
            if (statEntryPrefab == null || container == null) return;

            var entry = Instantiate(statEntryPrefab, container);
            statEntries.Add(entry);

            var nameText = entry.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = statName;

            var valueText = entry.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
            if (valueText != null)
                valueText.text = value.ToString("F0");

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

            // TODO: Get skills from progression system
            CreateSkillEntry("Swordsmanship", 45, 100);
            CreateSkillEntry("Archery", 32, 100);
            CreateSkillEntry("Blacksmithing", 67, 100);
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
            // TODO: Increase stat if points available
            RefreshAll();
        }

        private void ResetStats()
        {
            // TODO: Reset stats and refund points
            RefreshAll();
        }
        #endregion
    }
}
