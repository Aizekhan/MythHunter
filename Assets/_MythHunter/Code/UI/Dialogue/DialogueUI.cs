using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using RPGFramework.Dialogue.Runtime.Data;
using RPGFramework.Dialogue.Runtime.Events;
using MythHunter.GameLogic.Dialogue;

namespace MythHunter.UI.Dialogue
{
    /// <summary>
    /// Main UI controller for the dialogue system.
    /// Displays dialogue lines, speaker information, and player choices.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject choicePanel;

        [Header("Dialogue Display")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button continueButton;

        [Header("Choice Buttons")]
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Animation")]
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private bool skipTextAnimation;

        private DialogueSystemIntegration _dialogueIntegration;
        private int _currentEntityId;
        private string _currentDialogueId;
        private DialogueNodeData _currentNode;
        private List<GameObject> _activeChoiceButtons = new List<GameObject>();
        private Coroutine _textAnimationCoroutine;

        private void Awake()
        {
            // Hide UI by default
            dialoguePanel.SetActive(false);
            choicePanel.SetActive(false);

            // Setup continue button
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        /// <summary>
        /// Initialize the dialogue UI with the integration system
        /// </summary>
        public void Initialize(DialogueSystemIntegration dialogueIntegration)
        {
            _dialogueIntegration = dialogueIntegration;

            // Subscribe to dialogue events
            // In a real implementation, we'd subscribe to DialogueStartedEvent, LineSpokenEvent, etc.
        }

        /// <summary>
        /// Start a dialogue conversation
        /// </summary>
        public void StartDialogue(int entityId, string dialogueId)
        {
            _currentEntityId = entityId;
            _currentDialogueId = dialogueId;

            // Show dialogue panel
            dialoguePanel.SetActive(true);

            // Start the dialogue
            _dialogueIntegration.StartDialogue(entityId, dialogueId);

            // Get first node
            var activeDialogue = _dialogueIntegration.GetActiveDialogue(entityId, dialogueId);
            if (activeDialogue != null && activeDialogue.CurrentNodeId != null)
            {
                var node = _dialogueIntegration.GetCurrentNode(entityId, dialogueId);
                DisplayNode(node);
            }
        }

        /// <summary>
        /// Display a dialogue node (line, choice, or branch)
        /// </summary>
        private void DisplayNode(DialogueNodeData node)
        {
            if (node == null) return;

            _currentNode = node;

            // Get metadata for this node (speaker info, portrait, etc.)
            var metadata = _dialogueIntegration.GetDialogueMetadata(_currentDialogueId);

            // Display speaker name
            if (speakerNameText != null && metadata != null)
            {
                speakerNameText.text = metadata.ContainsKey("SpeakerName")
                    ? metadata["SpeakerName"]
                    : "Unknown";
            }

            // Display speaker portrait
            if (speakerPortrait != null && metadata != null && metadata.ContainsKey("PortraitPath"))
            {
                LoadPortrait(metadata["PortraitPath"]);
            }

            // Display dialogue text based on node type
            switch (node.NodeType)
            {
                case DialogueNodeType.Line:
                    DisplayLine(node.Text);
                    ShowContinueButton(true);
                    ShowChoices(false);
                    break;

                case DialogueNodeType.Choice:
                    DisplayLine(node.Text);
                    ShowContinueButton(false);
                    ShowChoices(true, node.Choices);
                    break;

                case DialogueNodeType.Branch:
                    // Branch nodes evaluate conditions and auto-advance
                    AdvanceToNextNode();
                    break;

                case DialogueNodeType.End:
                    EndDialogue();
                    break;
            }
        }

        /// <summary>
        /// Display dialogue line text with optional typewriter animation
        /// </summary>
        private void DisplayLine(string text)
        {
            if (dialogueText == null) return;

            // Stop any existing animation
            if (_textAnimationCoroutine != null)
            {
                StopCoroutine(_textAnimationCoroutine);
            }

            if (skipTextAnimation)
            {
                dialogueText.text = text;
            }
            else
            {
                _textAnimationCoroutine = StartCoroutine(AnimateText(text));
            }
        }

        /// <summary>
        /// Typewriter text animation
        /// </summary>
        private System.Collections.IEnumerator AnimateText(string text)
        {
            dialogueText.text = "";

            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }

            _textAnimationCoroutine = null;
        }

        /// <summary>
        /// Display player choice buttons
        /// </summary>
        private void ShowChoices(bool show, List<DialogueChoiceData> choices = null)
        {
            // Clear existing choice buttons
            foreach (var button in _activeChoiceButtons)
            {
                Destroy(button);
            }
            _activeChoiceButtons.Clear();

            if (!show || choices == null || choices.Count == 0)
            {
                choicePanel.SetActive(false);
                return;
            }

            choicePanel.SetActive(true);

            // Create choice buttons
            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                var choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
                _activeChoiceButtons.Add(choiceButton);

                // Set choice text
                var buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = choice.ChoiceText;
                }

                // Setup button click
                var button = choiceButton.GetComponent<Button>();
                if (button != null)
                {
                    int choiceIndex = i;
                    button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));

                    // Disable button if choice conditions not met
                    if (!_dialogueIntegration.IsChoiceAvailable(_currentEntityId, _currentDialogueId, choiceIndex))
                    {
                        button.interactable = false;
                        buttonText.color = Color.gray;
                    }
                }
            }
        }

        /// <summary>
        /// Show/hide the continue button
        /// </summary>
        private void ShowContinueButton(bool show)
        {
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(show);
            }
        }

        /// <summary>
        /// Handle continue button click (for Line nodes)
        /// </summary>
        private void OnContinueClicked()
        {
            // Skip text animation if still playing
            if (_textAnimationCoroutine != null)
            {
                StopCoroutine(_textAnimationCoroutine);
                dialogueText.text = _currentNode.Text;
                _textAnimationCoroutine = null;
                return;
            }

            // Advance to next node
            AdvanceToNextNode();
        }

        /// <summary>
        /// Handle player choice selection
        /// </summary>
        private void OnChoiceSelected(int choiceIndex)
        {
            // Select choice in dialogue system
            _dialogueIntegration.SelectChoice(_currentEntityId, _currentDialogueId, choiceIndex);

            // Get the next node
            AdvanceToNextNode();
        }

        /// <summary>
        /// Advance to the next dialogue node
        /// </summary>
        private void AdvanceToNextNode()
        {
            var nextNode = _dialogueIntegration.GetCurrentNode(_currentEntityId, _currentDialogueId);

            if (nextNode != null)
            {
                DisplayNode(nextNode);
            }
            else
            {
                // No next node means dialogue ended
                EndDialogue();
            }
        }

        /// <summary>
        /// End the dialogue and close UI
        /// </summary>
        private void EndDialogue()
        {
            _dialogueIntegration.EndDialogue(_currentEntityId, _currentDialogueId);

            dialoguePanel.SetActive(false);
            choicePanel.SetActive(false);

            _currentEntityId = 0;
            _currentDialogueId = null;
            _currentNode = null;
        }

        /// <summary>
        /// Load speaker portrait from Resources
        /// </summary>
        private void LoadPortrait(string portraitPath)
        {
            if (string.IsNullOrEmpty(portraitPath)) return;

            var sprite = Resources.Load<Sprite>(portraitPath);
            if (sprite != null && speakerPortrait != null)
            {
                speakerPortrait.sprite = sprite;
                speakerPortrait.enabled = true;
            }
        }

        /// <summary>
        /// Public method to check if dialogue is currently active
        /// </summary>
        public bool IsDialogueActive()
        {
            return dialoguePanel.activeSelf;
        }

        private void OnDestroy()
        {
            // Cleanup button listeners
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
            }

            foreach (var button in _activeChoiceButtons)
            {
                if (button != null)
                {
                    var btn = button.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }
}
