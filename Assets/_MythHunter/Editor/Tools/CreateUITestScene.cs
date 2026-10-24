using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using MythHunter.UI.Managers;
using MythHunter.UI.Panels;

namespace MythHunter.Editor.Tools
{
    /// <summary>
    /// Editor tool to create a test scene for UI panels with all necessary setup.
    /// Menu: MythHunter → Create UI Test Scene
    /// </summary>
    public static class CreateUITestScene
    {
        [MenuItem("MythHunter/Create UI Test Scene")]
        public static void CreateTestScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create UIManager root
            var uiManagerObj = new GameObject("UIManager");
            var uiManager = uiManagerObj.AddComponent<UIManager>();

            // Create Canvas for UI
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create all 11 UI panels
            var questLogUI = CreateUIPanel<QuestLogUI>(canvasObj.transform, "QuestLogUI");
            var inventoryUI = CreateUIPanel<InventoryUI>(canvasObj.transform, "InventoryUI");
            var craftingUI = CreateUIPanel<CraftingUI>(canvasObj.transform, "CraftingUI");
            var tradingUI = CreateUIPanel<TradingUI>(canvasObj.transform, "TradingUI");
            var dialogueUI = CreateUIPanel<DialogueUI>(canvasObj.transform, "DialogueUI");
            var factionReputationUI = CreateUIPanel<FactionReputationUI>(canvasObj.transform, "FactionReputationUI");
            var statsUI = CreateUIPanel<StatsUI>(canvasObj.transform, "StatsUI");
            var abilitiesUI = CreateUIPanel<AbilitiesUI>(canvasObj.transform, "AbilitiesUI");
            var statusEffectsUI = CreateUIPanel<StatusEffectsUI>(canvasObj.transform, "StatusEffectsUI");
            var combatLogUI = CreateUIPanel<CombatLogUI>(canvasObj.transform, "CombatLogUI");
            var lootUI = CreateUIPanel<LootUI>(canvasObj.transform, "LootUI");

            // Assign references to UIManager using SerializedObject
            var so = new SerializedObject(uiManager);
            so.FindProperty("questLogUI").objectReferenceValue = questLogUI;
            so.FindProperty("inventoryUI").objectReferenceValue = inventoryUI;
            so.FindProperty("craftingUI").objectReferenceValue = craftingUI;
            so.FindProperty("tradingUI").objectReferenceValue = tradingUI;
            so.FindProperty("dialogueUI").objectReferenceValue = dialogueUI;
            so.FindProperty("factionReputationUI").objectReferenceValue = factionReputationUI;
            so.FindProperty("statsUI").objectReferenceValue = statsUI;
            so.FindProperty("abilitiesUI").objectReferenceValue = abilitiesUI;
            so.FindProperty("statusEffectsUI").objectReferenceValue = statusEffectsUI;
            so.FindProperty("combatLogUI").objectReferenceValue = combatLogUI;
            so.FindProperty("lootUI").objectReferenceValue = lootUI;
            so.ApplyModifiedProperties();

            // Save scene
            EditorSceneManager.SaveScene(scene, "Assets/_MythHunter/Scenes/UITestScene.unity");

            UnityEngine.Debug.Log("[CreateUITestScene] ✅ UI Test Scene created successfully!");
            UnityEngine.Debug.Log("[CreateUITestScene] Scene saved to: Assets/_MythHunter/Scenes/UITestScene.unity");
            UnityEngine.Debug.Log("[CreateUITestScene] Press Play to test UI panels with keyboard shortcuts:");
            UnityEngine.Debug.Log("[CreateUITestScene] Q=Quest, I=Inventory, C=Crafting, T=Trading, D=Dialogue, F=Faction, K=Stats, P=Abilities, L=CombatLog, O=Loot");
        }

        private static T CreateUIPanel<T>(Transform parent, string name) where T : MonoBehaviour
        {
            // Create panel GameObject
            var panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);

            // Add RectTransform and stretch to fill screen
            var rectTransform = panelObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add UI component
            var uiComponent = panelObj.AddComponent<T>();

            // Create panel root (background)
            var panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(panelObj.transform, false);

            var rootRect = panelRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;
            rootRect.anchoredPosition = Vector2.zero;

            // Add semi-transparent background
            var image = panelRoot.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Dark semi-transparent

            // Add title text
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelRoot.transform, false);

            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(400, 50);

            var titleText = titleObj.AddComponent<UnityEngine.UI.Text>();
            titleText.text = name;
            titleText.fontSize = 24;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            titleText.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Add close instruction text
            var instructionObj = new GameObject("Instructions");
            instructionObj.transform.SetParent(panelRoot.transform, false);

            var instructionRect = instructionObj.AddComponent<RectTransform>();
            instructionRect.anchorMin = new Vector2(0.5f, 0f);
            instructionRect.anchorMax = new Vector2(0.5f, 0f);
            instructionRect.pivot = new Vector2(0.5f, 0f);
            instructionRect.anchoredPosition = new Vector2(0, 20);
            instructionRect.sizeDelta = new Vector2(600, 40);

            var instructionText = instructionObj.AddComponent<UnityEngine.UI.Text>();
            instructionText.text = "Press ESC to close | Press keyboard shortcuts to test other panels";
            instructionText.fontSize = 16;
            instructionText.alignment = TextAnchor.MiddleCenter;
            instructionText.color = Color.yellow;
            instructionText.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Set panelRoot to hidden by default
            panelRoot.SetActive(false);

            // Assign panelRoot reference using reflection
            var panelRootField = typeof(T).GetField("panelRoot",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (panelRootField != null)
            {
                panelRootField.SetValue(uiComponent, panelRoot);
            }

            return uiComponent;
        }
    }
}
