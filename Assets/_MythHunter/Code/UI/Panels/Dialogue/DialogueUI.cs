// TODO: This UI needs refactoring to work with actual Framework Dialogue interfaces
#if FALSE
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RPGFramework.Dialogue;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Dialogue UI with conversation flow, choices, and speaker portraits.
    /// Keyboard shortcut: D (or Space to advance)
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("Dialogue Display")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private Image playerPortrait;

        [Header("Choices")]
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Controls")]
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject continueIndicator;
        [SerializeField] private Button skipButton;

        [Header("Text Animation")]
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private bool enableTypewriter = true;

        [Header("Colors")]
        [SerializeField] private Color playerChoiceColor = Color.cyan;
        [SerializeField] private Color npcChoiceColor = Color.white;
        [SerializeField] private Color lockedChoiceColor = Color.gray;
        #endregion

        #region Private Fields
        private IDialogueSystem dialogueSystem;
        private IDialogue currentDialogue;
        private IDialogueNode currentNode;
        private List<GameObject> choiceButtons = new List<GameObject>();
        private bool isTyping = false;
        private string fullText = "";
        private Coroutine typewriterCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(EndDialogue);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);

            if (skipButton != null)
                skipButton.onClick.AddListener(SkipTypewriter);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
        }

        private void Update()
        {
            // Handle keyboard shortcuts
            if (Input.GetKeyDown(KeyCode.Escape))
                EndDialogue();

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (isTyping)
                    SkipTypewriter();
                else
                    OnContinueClicked();
            }

            // Number keys for quick choice selection
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                    SelectChoice(i - 1);
            }
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

        public void Initialize(IDialogueSystem system)
        {
            dialogueSystem = system;
        }

        public void StartDialogue(IDialogue dialogue)
        {
            currentDialogue = dialogue;
            Show();
            DisplayNode(GetStartNode());
        }

        public void EndDialogue()
        {
            Hide();
            currentDialogue = null;
            currentNode = null;
            ClearChoices();
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            if (dialogueSystem == null) return;

            // Subscribe to dialogue events
            // dialogueSystem.OnDialogueStarted += OnDialogueStarted;
            // dialogueSystem.OnDialogueEnded += OnDialogueEnded;
            // dialogueSystem.OnNodeChanged += OnNodeChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (dialogueSystem == null) return;

            // Unsubscribe from dialogue events
            // dialogueSystem.OnDialogueStarted -= OnDialogueStarted;
            // dialogueSystem.OnDialogueEnded -= OnDialogueEnded;
            // dialogueSystem.OnNodeChanged -= OnNodeChanged;
        }
        #endregion

        #region Private Methods - Display
        private void DisplayNode(IDialogueNode node)
        {
            if (node == null)
            {
                EndDialogue();
                return;
            }

            currentNode = node;

            // Update speaker name
            if (speakerNameText != null)
                speakerNameText.text = GetSpeakerName(node);

            // Display dialogue text with typewriter effect
            fullText = node.Text;
            if (enableTypewriter && typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);

            if (enableTypewriter)
                typewriterCoroutine = StartCoroutine(TypewriterEffect());
            else
                DisplayFullText();

            // Update portraits
            UpdatePortraits(node);

            // Display choices
            DisplayChoices(node);

            // Update continue button visibility
            UpdateContinueButton(node);
        }

        private void DisplayFullText()
        {
            if (dialogueText != null)
                dialogueText.text = fullText;

            isTyping = false;

            if (continueIndicator != null)
                continueIndicator.SetActive(true);
        }

        private System.Collections.IEnumerator TypewriterEffect()
        {
            isTyping = true;

            if (continueIndicator != null)
                continueIndicator.SetActive(false);

            if (dialogueText != null)
                dialogueText.text = "";

            foreach (char letter in fullText.ToCharArray())
            {
                if (dialogueText != null)
                    dialogueText.text += letter;

                yield return new WaitForSeconds(textSpeed);
            }

            isTyping = false;

            if (continueIndicator != null)
                continueIndicator.SetActive(true);
        }

        private void SkipTypewriter()
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);

            DisplayFullText();
        }

        private void UpdatePortraits(IDialogueNode node)
        {
            // TODO: Update speaker portraits based on node
            // if (speakerPortrait != null)
            //     speakerPortrait.sprite = GetSpeakerPortrait(node);
        }

        private void UpdateContinueButton(IDialogueNode node)
        {
            bool hasChoices = HasChoices(node);
            bool hasNextNode = HasNextNode(node);

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(!hasChoices && hasNextNode);
            }

            if (continueIndicator != null && !hasChoices && !hasNextNode)
            {
                // End of dialogue
                continueIndicator.SetActive(false);
            }
        }
        #endregion

        #region Private Methods - Choices
        private void DisplayChoices(IDialogueNode node)
        {
            ClearChoices();

            if (!HasChoices(node)) return;

            var choices = GetNodeChoices(node);

            for (int i = 0; i < choices.Count; i++)
            {
                CreateChoiceButton(choices[i], i);
            }
        }

        private void CreateChoiceButton(DialogueChoice choice, int index)
        {
            if (choiceButtonPrefab == null || choicesContainer == null) return;

            var buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
            choiceButtons.Add(buttonObj);

            var button = buttonObj.GetComponent<Button>();
            var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
            {
                string prefix = index < 9 ? $"[{index + 1}] " : "";
                text.text = $"{prefix}{choice.Text}";
                text.color = choice.IsAvailable ? playerChoiceColor : lockedChoiceColor;
            }

            if (button != null)
            {
                button.onClick.AddListener(() => SelectChoice(choice));
                button.interactable = choice.IsAvailable;
            }
        }

        private void SelectChoice(int index)
        {
            var choices = GetNodeChoices(currentNode);
            if (index >= 0 && index < choices.Count)
                SelectChoice(choices[index]);
        }

        private void SelectChoice(DialogueChoice choice)
        {
            if (!choice.IsAvailable) return;

            // Execute choice action if any
            // choice.OnSelected?.Invoke();

            // Move to next node
            var nextNode = GetNextNode(choice);
            DisplayNode(nextNode);
        }

        private void ClearChoices()
        {
            foreach (var button in choiceButtons)
            {
                if (button != null)
                    Destroy(button);
            }
            choiceButtons.Clear();
        }

        private void OnContinueClicked()
        {
            if (isTyping)
            {
                SkipTypewriter();
                return;
            }

            if (currentNode == null) return;

            if (HasChoices(currentNode))
                return; // Wait for player to select a choice

            // Move to next node
            var nextNode = GetNextNode(null);
            DisplayNode(nextNode);
        }
        #endregion

        #region Private Methods - Data Access
        private IDialogueNode GetStartNode()
        {
            // TODO: Get start node from dialogue
            return null;
        }

        private bool HasChoices(IDialogueNode node)
        {
            // TODO: Check if node has choices
            return false;
        }

        private bool HasNextNode(IDialogueNode node)
        {
            // TODO: Check if node has next node
            return false;
        }

        private List<DialogueChoice> GetNodeChoices(IDialogueNode node)
        {
            // TODO: Get choices from node
            return new List<DialogueChoice>();
        }

        private IDialogueNode GetNextNode(DialogueChoice choice)
        {
            // TODO: Get next node based on choice or auto-progression
            return null;
        }

        private string GetSpeakerName(IDialogueNode node)
        {
            // TODO: Get speaker name from node
            return "NPC";
        }
        #endregion

        #region Helper Structs
        private struct DialogueChoice
        {
            public string Text;
            public bool IsAvailable;
            public System.Action OnSelected;
        }
        #endregion
    }
}
#endif
