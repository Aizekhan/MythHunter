// Fixed: Updated to use IStatusEffectManager (Framework Pattern A - Manager-based)

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RPGFramework.StatusEffects;

namespace MythHunter.UI.Panels
{
    /// <summary>
    /// Status effects UI displaying active buffs and debuffs with tooltips.
    /// Always visible during gameplay (HUD element).
    /// </summary>
    public class StatusEffectsUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform buffsContainer;
        [SerializeField] private Transform debuffsContainer;
        [SerializeField] private GameObject effectIconPrefab;

        [Header("Tooltip")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipNameText;
        [SerializeField] private TextMeshProUGUI tooltipDescriptionText;
        [SerializeField] private TextMeshProUGUI tooltipDurationText;
        [SerializeField] private TextMeshProUGUI tooltipStacksText;

        [Header("Layout")]
        [SerializeField] private int maxVisibleEffects = 10;
        [SerializeField] private bool showBuffsOnly = false;
        [SerializeField] private bool showDebuffsOnly = false;

        [Header("Colors")]
        [SerializeField] private Color buffColor = Color.green;
        [SerializeField] private Color debuffColor = Color.red;
        [SerializeField] private Color expiringColor = new Color(1f, 0.5f, 0f);
        #endregion

        #region Private Fields
        private IStatusEffectManager statusEffectSystem;
        private List<GameObject> effectIcons = new List<GameObject>();
        private IStatusEffect hoveredEffect;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            RefreshEffects();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            HideTooltip();
        }

        private void Update()
        {
            UpdateEffectTimers();
            UpdateTooltipPosition();
        }
        #endregion

        #region Public Methods
        public void Show()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            RefreshEffects();
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            HideTooltip();
        }

        public void Initialize(IStatusEffectManager system)
        {
            statusEffectSystem = system;
            RefreshEffects();
        }
        #endregion

        #region Private Methods - Event Handling
        private void SubscribeToEvents()
        {
            if (statusEffectSystem == null) return;
            // statusEffectSystem.OnEffectApplied += OnEffectApplied;
            // statusEffectSystem.OnEffectRemoved += OnEffectRemoved;
            // statusEffectSystem.OnEffectRefreshed += OnEffectRefreshed;
        }

        private void UnsubscribeFromEvents()
        {
            if (statusEffectSystem == null) return;
            // statusEffectSystem.OnEffectApplied -= OnEffectApplied;
            // statusEffectSystem.OnEffectRemoved -= OnEffectRemoved;
            // statusEffectSystem.OnEffectRefreshed -= OnEffectRefreshed;
        }

        private void OnEffectApplied(IStatusEffect effect)
        {
            RefreshEffects();
        }

        private void OnEffectRemoved(IStatusEffect effect)
        {
            RefreshEffects();
        }

        private void OnEffectRefreshed(IStatusEffect effect)
        {
            RefreshEffects();
        }
        #endregion

        #region Private Methods - Display
        private void RefreshEffects()
        {
            ClearEffectIcons();

            if (statusEffectSystem == null) return;

            var effects = GetActiveEffects();

            foreach (var effect in effects)
            {
                CreateEffectIcon(effect);
            }
        }

        private void CreateEffectIcon(StatusEffectData effect)
        {
            if (effectIconPrefab == null) return;

            // Determine which container to use
            Transform container = effect.IsBuff ? buffsContainer : debuffsContainer;
            if (container == null) return;

            // Skip if filtering
            if (showBuffsOnly && !effect.IsBuff) return;
            if (showDebuffsOnly && effect.IsBuff) return;

            // Check max visible
            if (effectIcons.Count >= maxVisibleEffects) return;

            var icon = Instantiate(effectIconPrefab, container);
            effectIcons.Add(icon);

            // Set icon image
            var image = icon.GetComponent<Image>();
            if (image != null)
            {
                // TODO: Set effect icon sprite
                // image.sprite = effect.Icon;
                image.color = effect.IsBuff ? buffColor : debuffColor;
            }

            // Set duration text
            var durationText = icon.GetComponentInChildren<TextMeshProUGUI>();
            if (durationText != null && effect.Duration > 0)
            {
                durationText.text = FormatDuration(effect.RemainingTime);
            }

            // Set stack count
            if (effect.Stacks > 1)
            {
                var stackText = icon.transform.Find("Stacks")?.GetComponent<TextMeshProUGUI>();
                if (stackText != null)
                    stackText.text = effect.Stacks.ToString();
            }

            // Add tooltip handlers
            var button = icon.GetComponent<Button>();
            if (button == null)
                button = icon.AddComponent<Button>();

            button.onClick.AddListener(() => ShowTooltip(effect));

            var eventTrigger = icon.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = icon.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            AddEventTrigger(eventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerEnter,
                () => ShowTooltip(effect));
            AddEventTrigger(eventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerExit,
                () => HideTooltip());
        }

        private void UpdateEffectTimers()
        {
            // Update duration text for all effect icons
            for (int i = 0; i < effectIcons.Count; i++)
            {
                if (effectIcons[i] == null) continue;

                var durationText = effectIcons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (durationText != null)
                {
                    // TODO: Get actual remaining time
                    float remainingTime = 10f;
                    durationText.text = FormatDuration(remainingTime);

                    // Change color if expiring soon
                    if (remainingTime < 5f)
                        durationText.color = expiringColor;
                }
            }
        }

        private void ClearEffectIcons()
        {
            foreach (var icon in effectIcons)
            {
                if (icon != null)
                    Destroy(icon);
            }
            effectIcons.Clear();
        }

        private string FormatDuration(float seconds)
        {
            if (seconds < 0) return "∞";
            if (seconds < 60) return $"{seconds:F0}s";
            return $"{seconds / 60:F0}m";
        }
        #endregion

        #region Private Methods - Tooltip
        private void ShowTooltip(StatusEffectData effect)
        {
            hoveredEffect = null; // TODO: Set hovered effect

            if (tooltipPanel == null) return;

            tooltipPanel.SetActive(true);

            if (tooltipNameText != null)
            {
                tooltipNameText.text = effect.Name;
                tooltipNameText.color = effect.IsBuff ? buffColor : debuffColor;
            }

            if (tooltipDescriptionText != null)
                tooltipDescriptionText.text = effect.Description;

            if (tooltipDurationText != null)
            {
                if (effect.Duration > 0)
                    tooltipDurationText.text = $"Duration: {FormatDuration(effect.RemainingTime)} / {FormatDuration(effect.Duration)}";
                else
                    tooltipDurationText.text = "Duration: Permanent";
            }

            if (tooltipStacksText != null && effect.Stacks > 1)
                tooltipStacksText.text = $"Stacks: {effect.Stacks}";

            UpdateTooltipPosition();
        }

        private void HideTooltip()
        {
            hoveredEffect = null;

            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }

        private void UpdateTooltipPosition()
        {
            if (tooltipPanel == null || !tooltipPanel.activeSelf) return;

            Vector2 mousePosition = Input.mousePosition;
            tooltipPanel.transform.position = mousePosition + new Vector2(15, -15);
        }
        #endregion

        #region Private Methods - Data Access
        private List<StatusEffectData> GetActiveEffects()
        {
            // TODO: Get active effects from status effect system
            return new List<StatusEffectData>
            {
                new StatusEffectData
                {
                    Name = "Strength Buff",
                    Description = "+10 Strength",
                    IsBuff = true,
                    Duration = 30f,
                    RemainingTime = 25f,
                    Stacks = 1
                },
                new StatusEffectData
                {
                    Name = "Poison",
                    Description = "5 damage per second",
                    IsBuff = false,
                    Duration = 20f,
                    RemainingTime = 8f,
                    Stacks = 3
                }
            };
        }

        private void AddEventTrigger(UnityEngine.EventSystems.EventTrigger trigger,
            UnityEngine.EventSystems.EventTriggerType eventType, System.Action callback)
        {
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(_ => callback());
            trigger.triggers.Add(entry);
        }
        #endregion

        #region Helper Structs
        private struct StatusEffectData
        {
            public string Name;
            public string Description;
            public bool IsBuff;
            public float Duration;
            public float RemainingTime;
            public int Stacks;
        }
        #endregion
    }
}

