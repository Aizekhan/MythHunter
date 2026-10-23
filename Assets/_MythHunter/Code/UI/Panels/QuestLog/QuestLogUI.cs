using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.Quests;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Quest log UI that displays active and completed quests with objectives tracking.
    /// Keyboard shortcut: Q
    /// </summary>
    public class QuestLogUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("Quest List")]
        [SerializeField] private Transform activeQuestsContainer;
        [SerializeField] private Transform completedQuestsContainer;
        [SerializeField] private GameObject questEntryPrefab;

        [Header("Quest Details")]
        [SerializeField] private TextMeshProUGUI questTitleText;
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private Transform objectivesContainer;
        [SerializeField] private GameObject objectiveEntryPrefab;
        [SerializeField] private TextMeshProUGUI rewardsText;

        [Header("Tabs")]
        [SerializeField] private Button activeQuestsTab;
        [SerializeField] private Button completedQuestsTab;
        [SerializeField] private GameObject activeQuestsPanel;
        [SerializeField] private GameObject completedQuestsPanel;

        [Header("Search & Filter")]
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown filterDropdown;

        [Header("Colors")]
        [SerializeField] private Color activeQuestColor = Color.yellow;
        [SerializeField] private Color completedQuestColor = Color.green;
        [SerializeField] private Color failedQuestColor = Color.red;
        [SerializeField] private Color objectiveCompleteColor = new Color(0.5f, 1f, 0.5f);
        [SerializeField] private Color objectiveIncompleteColor = Color.white;
        #endregion

        #region Private Fields
        private IQuestManager questManager;
        private List<GameObject> questEntries = new List<GameObject>();
        private List<GameObject> objectiveEntries = new List<GameObject>();
        private IQuest selectedQuest;
        private bool isShowingActiveQuests = true;
        private string currentSearchFilter = "";
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (activeQuestsTab != null)
                activeQuestsTab.onClick.AddListener(() => ShowTab(true));

            if (completedQuestsTab != null)
                completedQuestsTab.onClick.AddListener(() => ShowTab(false));

            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);

            if (filterDropdown != null)
                filterDropdown.onValueChanged.AddListener(OnFilterChanged);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshQuestList();
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

            // Refresh objectives if quest details are visible
            if (selectedQuest != null)
                RefreshObjectives();
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            RefreshQuestList();
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

        public void Initialize(IQuestManager manager)
        {
            questManager = manager;
            RefreshQuestList();
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            if (questManager == null) return;

            // Subscribe to quest events for real-time updates
            // questManager.OnQuestStarted += OnQuestStarted;
            // questManager.OnQuestCompleted += OnQuestCompleted;
            // questManager.OnQuestObjectiveUpdated += OnObjectiveUpdated;
        }

        private void UnsubscribeFromEvents()
        {
            if (questManager == null) return;

            // Unsubscribe from quest events
            // questManager.OnQuestStarted -= OnQuestStarted;
            // questManager.OnQuestCompleted -= OnQuestCompleted;
            // questManager.OnQuestObjectiveUpdated -= OnObjectiveUpdated;
        }

        private void OnQuestStarted(IQuest quest)
        {
            RefreshQuestList();
        }

        private void OnQuestCompleted(IQuest quest)
        {
            RefreshQuestList();
        }

        private void OnObjectiveUpdated(IQuest quest)
        {
            if (selectedQuest == quest)
                RefreshObjectives();
        }
        #endregion

        #region Private Methods - UI Updates
        private void ShowTab(bool showActive)
        {
            isShowingActiveQuests = showActive;

            if (activeQuestsPanel != null)
                activeQuestsPanel.SetActive(showActive);

            if (completedQuestsPanel != null)
                completedQuestsPanel.SetActive(!showActive);

            RefreshQuestList();
        }

        private void RefreshQuestList()
        {
            ClearQuestEntries();

            if (questManager == null) return;

            var quests = isShowingActiveQuests
                ? GetActiveQuests()
                : GetCompletedQuests();

            // Apply search filter
            if (!string.IsNullOrEmpty(currentSearchFilter))
            {
                quests = quests.Where(q =>
                    q.Title.ToLower().Contains(currentSearchFilter.ToLower()) ||
                    q.Description.ToLower().Contains(currentSearchFilter.ToLower())
                ).ToList();
            }

            var container = isShowingActiveQuests ? activeQuestsContainer : completedQuestsContainer;

            foreach (var quest in quests)
            {
                CreateQuestEntry(quest, container);
            }
        }

        private void CreateQuestEntry(IQuest quest, Transform parent)
        {
            if (questEntryPrefab == null || parent == null) return;

            var entry = Instantiate(questEntryPrefab, parent);
            questEntries.Add(entry);

            // Set quest title
            var titleText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = quest.Title;
                titleText.color = GetQuestColor(quest);
            }

            // Set click handler
            var button = entry.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => SelectQuest(quest));
        }

        private void SelectQuest(IQuest quest)
        {
            selectedQuest = quest;
            RefreshQuestDetails();
        }

        private void RefreshQuestDetails()
        {
            if (selectedQuest == null) return;

            // Update quest title
            if (questTitleText != null)
            {
                questTitleText.text = selectedQuest.Title;
                questTitleText.color = GetQuestColor(selectedQuest);
            }

            // Update description
            if (questDescriptionText != null)
                questDescriptionText.text = selectedQuest.Description;

            // Update objectives
            RefreshObjectives();

            // Update rewards
            RefreshRewards();
        }

        private void RefreshObjectives()
        {
            ClearObjectiveEntries();

            if (selectedQuest == null || objectivesContainer == null) return;

            var objectives = GetQuestObjectives(selectedQuest);

            foreach (var objective in objectives)
            {
                CreateObjectiveEntry(objective);
            }
        }

        private void CreateObjectiveEntry(QuestObjectiveData objective)
        {
            if (objectiveEntryPrefab == null || objectivesContainer == null) return;

            var entry = Instantiate(objectiveEntryPrefab, objectivesContainer);
            objectiveEntries.Add(entry);

            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"{(objective.IsComplete ? "✓" : "○")} {objective.Description} ({objective.CurrentProgress}/{objective.RequiredProgress})";
                text.color = objective.IsComplete ? objectiveCompleteColor : objectiveIncompleteColor;
            }

            // Add progress bar if applicable
            var progressBar = entry.GetComponentInChildren<Slider>();
            if (progressBar != null && objective.RequiredProgress > 0)
            {
                progressBar.maxValue = objective.RequiredProgress;
                progressBar.value = objective.CurrentProgress;
            }
        }

        private void RefreshRewards()
        {
            if (selectedQuest == null || rewardsText == null) return;

            var rewards = GetQuestRewards(selectedQuest);
            rewardsText.text = $"Rewards:\n{rewards}";
        }

        private void ClearQuestEntries()
        {
            foreach (var entry in questEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            questEntries.Clear();
        }

        private void ClearObjectiveEntries()
        {
            foreach (var entry in objectiveEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            objectiveEntries.Clear();
        }

        private void OnSearchChanged(string searchText)
        {
            currentSearchFilter = searchText;
            RefreshQuestList();
        }

        private void OnFilterChanged(int filterIndex)
        {
            // Implement filtering by quest type, difficulty, etc.
            RefreshQuestList();
        }
        #endregion

        #region Private Methods - Data Access
        private List<IQuest> GetActiveQuests()
        {
            // TODO: Get active quests from quest manager
            return new List<IQuest>();
        }

        private List<IQuest> GetCompletedQuests()
        {
            // TODO: Get completed quests from quest manager
            return new List<IQuest>();
        }

        private List<QuestObjectiveData> GetQuestObjectives(IQuest quest)
        {
            // TODO: Get quest objectives
            return new List<QuestObjectiveData>();
        }

        private string GetQuestRewards(IQuest quest)
        {
            // TODO: Get quest rewards as formatted string
            return "- 1000 XP\n- 500 Gold\n- Rare Sword";
        }

        private Color GetQuestColor(IQuest quest)
        {
            // TODO: Determine quest state and return appropriate color
            return activeQuestColor;
        }
        #endregion

        #region Helper Structs
        private struct QuestObjectiveData
        {
            public string Description;
            public int CurrentProgress;
            public int RequiredProgress;
            public bool IsComplete;
        }
        #endregion
    }
}
