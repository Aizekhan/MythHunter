// Fixed: Updated to use ICraftingManager (Framework Pattern A - Manager-based)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.Crafting;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Crafting UI with recipe list, material requirements, and crafting queue.
    /// Keyboard shortcut: C
    /// </summary>
    public class CraftingUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("Recipe List")]
        [SerializeField] private Transform recipeListContainer;
        [SerializeField] private GameObject recipeEntryPrefab;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown categoryFilter;

        [Header("Recipe Details")]
        [SerializeField] private TextMeshProUGUI recipeNameText;
        [SerializeField] private TextMeshProUGUI recipeDescriptionText;
        [SerializeField] private Image recipeIconImage;
        [SerializeField] private Transform materialsContainer;
        [SerializeField] private GameObject materialEntryPrefab;
        [SerializeField] private Button craftButton;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Slider quantitySlider;

        [Header("Crafting Queue")]
        [SerializeField] private Transform queueContainer;
        [SerializeField] private GameObject queueEntryPrefab;
        [SerializeField] private TextMeshProUGUI queueStatusText;

        [Header("Station Info")]
        [SerializeField] private TextMeshProUGUI stationNameText;
        [SerializeField] private TextMeshProUGUI stationLevelText;

        [Header("Skills")]
        [SerializeField] private Transform skillsContainer;
        [SerializeField] private GameObject skillEntryPrefab;

        [Header("Colors")]
        [SerializeField] private Color canCraftColor = Color.green;
        [SerializeField] private Color cannotCraftColor = Color.red;
        [SerializeField] private Color insufficientMaterialsColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color lockedRecipeColor = Color.gray;
        #endregion

        #region Private Fields
        private ICraftingManager craftingSystem;
        private List<GameObject> recipeEntries = new List<GameObject>();
        private List<GameObject> materialEntries = new List<GameObject>();
        private List<GameObject> queueEntries = new List<GameObject>();
        private ICraftingRecipe selectedRecipe;
        private int craftQuantity = 1;
        private string currentSearchFilter = "";
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (craftButton != null)
                craftButton.onClick.AddListener(CraftSelectedRecipe);

            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);

            if (categoryFilter != null)
                categoryFilter.onValueChanged.AddListener(OnCategoryChanged);

            if (quantityInput != null)
                quantityInput.onValueChanged.AddListener(OnQuantityInputChanged);

            if (quantitySlider != null)
                quantitySlider.onValueChanged.AddListener(OnQuantitySliderChanged);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshRecipeList();
            RefreshCraftingQueue();
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

            // Update crafting queue progress
            UpdateQueueProgress();
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            RefreshRecipeList();
            RefreshCraftingQueue();
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

        public void Initialize(ICraftingManager system)
        {
            craftingSystem = system;
            RefreshRecipeList();
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            if (craftingSystem == null) return;

            // Subscribe to crafting events for real-time updates
            // craftingSystem.OnCraftingStarted += OnCraftingStarted;
            // craftingSystem.OnCraftingCompleted += OnCraftingCompleted;
            // craftingSystem.OnCraftingFailed += OnCraftingFailed;
            // craftingSystem.OnRecipeUnlocked += OnRecipeUnlocked;
        }

        private void UnsubscribeFromEvents()
        {
            if (craftingSystem == null) return;

            // Unsubscribe from crafting events
            // craftingSystem.OnCraftingStarted -= OnCraftingStarted;
            // craftingSystem.OnCraftingCompleted -= OnCraftingCompleted;
            // craftingSystem.OnCraftingFailed -= OnCraftingFailed;
            // craftingSystem.OnRecipeUnlocked -= OnRecipeUnlocked;
        }

        private void OnCraftingStarted(ICraftingRecipe recipe)
        {
            RefreshCraftingQueue();
        }

        private void OnCraftingCompleted(ICraftingRecipe recipe)
        {
            RefreshCraftingQueue();
            RefreshRecipeList();
        }

        private void OnCraftingFailed(ICraftingRecipe recipe)
        {
            RefreshCraftingQueue();
        }

        private void OnRecipeUnlocked(ICraftingRecipe recipe)
        {
            RefreshRecipeList();
        }
        #endregion

        #region Private Methods - Recipe List
        private void RefreshRecipeList()
        {
            ClearRecipeEntries();

            if (craftingSystem == null) return;

            var recipes = GetFilteredRecipes();

            foreach (var recipe in recipes)
            {
                CreateRecipeEntry(recipe);
            }

            // Update station info
            UpdateStationInfo();

            // Update skills
            UpdateSkillsDisplay();
        }

        private void CreateRecipeEntry(ICraftingRecipe recipe)
        {
            if (recipeEntryPrefab == null || recipeListContainer == null) return;

            var entry = Instantiate(recipeEntryPrefab, recipeListContainer);
            recipeEntries.Add(entry);

            // Set recipe name
            var nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = recipe.Name;
                nameText.color = GetRecipeColor(recipe);
            }

            // Set recipe icon
            var icon = entry.GetComponentsInChildren<Image>().FirstOrDefault();
            if (icon != null)
            {
                // TODO: Set recipe icon
                // icon.sprite = recipe.Icon;
            }

            // Set click handler
            var button = entry.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => SelectRecipe(recipe));
                button.interactable = IsRecipeUnlocked(recipe);
            }
        }

        private void SelectRecipe(ICraftingRecipe recipe)
        {
            selectedRecipe = recipe;
            RefreshRecipeDetails();
        }

        private void ClearRecipeEntries()
        {
            foreach (var entry in recipeEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            recipeEntries.Clear();
        }

        private List<ICraftingRecipe> GetFilteredRecipes()
        {
            // TODO: Get recipes from crafting system and apply filters
            var recipes = new List<ICraftingRecipe>();

            // Apply search filter
            if (!string.IsNullOrEmpty(currentSearchFilter))
            {
                recipes = recipes.Where(r =>
                    r.Name.ToLower().Contains(currentSearchFilter.ToLower())
                ).ToList();
            }

            // Apply category filter
            // Apply station filter
            // Apply skill level filter

            return recipes;
        }

        private void OnSearchChanged(string searchText)
        {
            currentSearchFilter = searchText;
            RefreshRecipeList();
        }

        private void OnCategoryChanged(int categoryIndex)
        {
            RefreshRecipeList();
        }
        #endregion

        #region Private Methods - Recipe Details
        private void RefreshRecipeDetails()
        {
            if (selectedRecipe == null) return;

            // Update recipe name
            if (recipeNameText != null)
            {
                recipeNameText.text = selectedRecipe.Name;
                recipeNameText.color = GetRecipeColor(selectedRecipe);
            }

            // Update description
            if (recipeDescriptionText != null)
                recipeDescriptionText.text = selectedRecipe.Description;

            // Update icon
            if (recipeIconImage != null)
            {
                // TODO: Set recipe icon
                // recipeIconImage.sprite = selectedRecipe.Icon;
            }

            // Update materials
            RefreshMaterials();

            // Update craft button
            UpdateCraftButton();

            // Reset quantity
            craftQuantity = 1;
            UpdateQuantityUI();
        }

        private void RefreshMaterials()
        {
            ClearMaterialEntries();

            if (selectedRecipe == null || materialsContainer == null) return;

            var materials = GetRecipeMaterials(selectedRecipe);

            foreach (var material in materials)
            {
                CreateMaterialEntry(material);
            }
        }

        private void CreateMaterialEntry(MaterialRequirement material)
        {
            if (materialEntryPrefab == null || materialsContainer == null) return;

            var entry = Instantiate(materialEntryPrefab, materialsContainer);
            materialEntries.Add(entry);

            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                bool hasSufficient = material.CurrentAmount >= material.RequiredAmount;
                text.text = $"{material.Name}: {material.CurrentAmount}/{material.RequiredAmount}";
                text.color = hasSufficient ? canCraftColor : insufficientMaterialsColor;
            }

            // Add icon if available
            var icon = entry.GetComponentsInChildren<Image>().FirstOrDefault();
            if (icon != null)
            {
                // TODO: Set material icon
                // icon.sprite = material.Icon;
            }
        }

        private void ClearMaterialEntries()
        {
            foreach (var entry in materialEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            materialEntries.Clear();
        }

        private void UpdateCraftButton()
        {
            if (craftButton == null || selectedRecipe == null) return;

            bool canCraft = CanCraftRecipe(selectedRecipe, craftQuantity);

            craftButton.interactable = canCraft;

            var buttonText = craftButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = canCraft ? $"Craft x{craftQuantity}" : "Cannot Craft";
            }
        }

        private List<MaterialRequirement> GetRecipeMaterials(ICraftingRecipe recipe)
        {
            // TODO: Get recipe materials with current inventory amounts
            return new List<MaterialRequirement>();
        }
        #endregion

        #region Private Methods - Crafting
        private void CraftSelectedRecipe()
        {
            if (craftingSystem == null || selectedRecipe == null) return;

            if (!CanCraftRecipe(selectedRecipe, craftQuantity))
            {
                UnityEngine.Debug.LogWarning("Cannot craft recipe: insufficient materials or requirements not met");
                return;
            }

            // Start crafting
            for (int i = 0; i < craftQuantity; i++)
            {
                // TODO: Start crafting in the crafting system
                // craftingSystem.StartCrafting(selectedRecipe);
            }

            RefreshCraftingQueue();
            RefreshRecipeDetails();
        }

        private bool CanCraftRecipe(ICraftingRecipe recipe, int quantity)
        {
            // TODO: Check if player has sufficient materials and meets requirements
            return IsRecipeUnlocked(recipe);
        }

        private bool IsRecipeUnlocked(ICraftingRecipe recipe)
        {
            // TODO: Check if recipe is unlocked
            return true;
        }
        #endregion

        #region Private Methods - Crafting Queue
        private void RefreshCraftingQueue()
        {
            ClearQueueEntries();

            if (craftingSystem == null) return;

            var queuedRecipes = GetCraftingQueue();

            foreach (var queuedItem in queuedRecipes)
            {
                CreateQueueEntry(queuedItem);
            }

            UpdateQueueStatus();
        }

        private void CreateQueueEntry(CraftingQueueItem queueItem)
        {
            if (queueEntryPrefab == null || queueContainer == null) return;

            var entry = Instantiate(queueEntryPrefab, queueContainer);
            queueEntries.Add(entry);

            var nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = queueItem.RecipeName;

            // Add progress bar
            var progressBar = entry.GetComponentInChildren<Slider>();
            if (progressBar != null)
            {
                progressBar.maxValue = queueItem.TotalTime;
                progressBar.value = queueItem.ElapsedTime;
            }

            // Add cancel button
            var cancelButton = entry.GetComponentInChildren<Button>();
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(() => CancelCrafting(queueItem));
            }
        }

        private void UpdateQueueProgress()
        {
            // Update progress bars for all queue entries
            // This should be called in Update()
        }

        private void UpdateQueueStatus()
        {
            if (queueStatusText == null) return;

            var queue = GetCraftingQueue();
            queueStatusText.text = queue.Count > 0
                ? $"Crafting {queue.Count} item(s)"
                : "Queue empty";
        }

        private void CancelCrafting(CraftingQueueItem queueItem)
        {
            // TODO: Cancel crafting and refund materials
            RefreshCraftingQueue();
        }

        private void ClearQueueEntries()
        {
            foreach (var entry in queueEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            queueEntries.Clear();
        }

        private List<CraftingQueueItem> GetCraftingQueue()
        {
            // TODO: Get crafting queue from crafting system
            return new List<CraftingQueueItem>();
        }
        #endregion

        #region Private Methods - Quantity
        private void OnQuantityInputChanged(string value)
        {
            if (int.TryParse(value, out int quantity))
            {
                craftQuantity = Mathf.Max(1, quantity);
                UpdateQuantityUI();
                UpdateCraftButton();
            }
        }

        private void OnQuantitySliderChanged(float value)
        {
            craftQuantity = Mathf.RoundToInt(value);
            UpdateQuantityUI();
            UpdateCraftButton();
        }

        private void UpdateQuantityUI()
        {
            if (quantityInput != null)
                quantityInput.text = craftQuantity.ToString();

            if (quantitySlider != null)
                quantitySlider.value = craftQuantity;
        }
        #endregion

        #region Private Methods - Station & Skills
        private void UpdateStationInfo()
        {
            // TODO: Get current crafting station info
            if (stationNameText != null)
                stationNameText.text = "Blacksmith Forge";

            if (stationLevelText != null)
                stationLevelText.text = "Level 3";
        }

        private void UpdateSkillsDisplay()
        {
            // TODO: Display relevant crafting skills (Blacksmithing, Alchemy, etc.)
        }

        private Color GetRecipeColor(ICraftingRecipe recipe)
        {
            if (!IsRecipeUnlocked(recipe))
                return lockedRecipeColor;

            return CanCraftRecipe(recipe, 1) ? canCraftColor : cannotCraftColor;
        }
        #endregion

        #region Helper Structs
        private struct MaterialRequirement
        {
            public string Name;
            public int RequiredAmount;
            public int CurrentAmount;
        }

        private struct CraftingQueueItem
        {
            public string RecipeName;
            public float TotalTime;
            public float ElapsedTime;
        }
        #endregion
    }
}
