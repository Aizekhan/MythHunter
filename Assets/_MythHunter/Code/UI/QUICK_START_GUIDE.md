# MythHunter UI Systems - Quick Start Guide

## What Was Created

### File Count
- **11 UI Panel Scripts** (QuestLog, Inventory, Crafting, Trading, Dialogue, FactionReputation, Stats, Abilities, StatusEffects, CombatLog, Loot)
- **1 UIManager Script** (Central coordinator)
- **2 Documentation Files** (This guide + comprehensive summary)
- **Total Lines of Code:** ~5,000+ lines
- **Total Size:** ~190 KB

### Directory Structure
```
Assets/_MythHunter/Code/UI/
├── Managers/
│   └── UIManager.cs
├── Panels/
│   ├── Abilities/AbilitiesUI.cs
│   ├── CombatLog/CombatLogUI.cs
│   ├── Crafting/CraftingUI.cs
│   ├── Dialogue/DialogueUI.cs
│   ├── FactionReputation/FactionReputationUI.cs
│   ├── Inventory/InventoryUI.cs
│   ├── Loot/LootUI.cs
│   ├── QuestLog/QuestLogUI.cs
│   ├── Stats/StatsUI.cs
│   ├── StatusEffects/StatusEffectsUI.cs
│   └── Trading/TradingUI.cs
├── UI_SYSTEMS_SUMMARY.md
└── QUICK_START_GUIDE.md
```

---

## Keyboard Shortcuts (Complete List)

| Key | Panel | Description |
|-----|-------|-------------|
| Q | Quest Log | Active/completed quests, objectives |
| I | Inventory | Item management, equipment |
| C | Crafting | Recipes, crafting queue |
| T | Trading | Merchant interactions |
| D | Dialogue | Conversation (when active) |
| F | Faction Reputation | Faction standings |
| K | Stats/Character | Attributes, skills |
| P | Abilities/Powers | Ability loadout, skill tree |
| L | Combat Log | Combat history |
| O | Loot | Loot windows |
| ESC | Close All | Close all panels |

---

## Quick Setup (5 Steps)

### Step 1: Unity Scene Setup
```
1. Create Canvas (Screen Space - Overlay)
2. Create Empty GameObject "UIManager"
3. Attach UIManager.cs component
4. Create panel GameObjects for each UI system
5. Assign panels in UIManager inspector
```

### Step 2: Create Prefabs (Required)
Each panel needs prefabs for its dynamic content:

**QuestLogUI:**
- Quest Entry Prefab (Button + Text)
- Objective Entry Prefab (Text + Progress Bar)

**InventoryUI:**
- Item Slot Prefab (Image + Text)

**CraftingUI:**
- Recipe Entry Prefab (Button + Text + Icon)
- Material Entry Prefab (Text + Icon)
- Queue Entry Prefab (Text + Progress Bar + Button)

**TradingUI:**
- Merchant Item Entry Prefab (Button + Text + Icon + Price)
- Player Item Entry Prefab (Button + Text + Icon + Price)

**DialogueUI:**
- Choice Button Prefab (Button + Text)

**FactionReputationUI:**
- Faction Entry Prefab (Button + Text)

**StatsUI:**
- Stat Entry Prefab (Text + Text + Button)
- Skill Entry Prefab (Text + Progress Bar)

**AbilitiesUI:**
- Ability Slot Prefab (Image + Text)
- Available Ability Prefab (Button + Text + Icon)

**StatusEffectsUI:**
- Effect Icon Prefab (Image + Text)

**CombatLogUI:**
- Log Entry Prefab (Text)
- Damage Number Prefab (Text with CanvasGroup)

**LootUI:**
- Loot Item Entry Prefab (Button + Text + Icon)
- Notification Prefab (Text)
- Roll Player Entry Prefab (Text + Icon)

### Step 3: Initialize Systems (Code)
```csharp
using MythHunter.UI.Managers;

// In your game initialization code:
void Start()
{
    var uiManager = UIManager.Instance;

    // Initialize each panel with its Framework system
    uiManager.GetQuestLogUI().Initialize(questManager);
    uiManager.GetInventoryUI().Initialize(playerInventory);
    uiManager.GetCraftingUI().Initialize(craftingSystem);
    uiManager.GetTradingUI().Initialize(tradingSystem, merchant);
    uiManager.GetDialogueUI().Initialize(dialogueSystem);
    uiManager.GetFactionReputationUI().Initialize(factionManager);
    uiManager.GetStatsUI().Initialize(statsSystem, progressionSystem);
    uiManager.GetAbilitiesUI().Initialize(abilitySystem);
    uiManager.GetStatusEffectsUI().Initialize(statusEffectSystem);
    uiManager.GetCombatLogUI().Initialize(combatSystem);
    uiManager.GetLootUI().Initialize(lootSystem);
}
```

### Step 4: Wire Up Framework Systems
Each UI panel has TODO comments where Framework integration is needed:
```csharp
// Search for "TODO" in each file to find integration points
// Example locations:
// - GetActiveQuests() in QuestLogUI.cs
// - GetFilteredItems() in InventoryUI.cs
// - GetFilteredRecipes() in CraftingUI.cs
// etc.
```

### Step 5: Test
```
1. Run the game
2. Press Q to open Quest Log
3. Press I to open Inventory
4. Press ESC to close all panels
5. Verify keyboard shortcuts work
```

---

## Common Usage Patterns

### Opening Panels
```csharp
// Via UIManager (recommended)
UIManager.Instance.ShowPanel("Inventory");

// Or directly
UIManager.Instance.GetInventoryUI().Show();
```

### Triggering Specific UI
```csharp
// Show dialogue
var dialogueUI = UIManager.Instance.GetDialogueUI();
dialogueUI.StartDialogue(myDialogueInstance);

// Show loot window
var lootUI = UIManager.Instance.GetLootUI();
lootUI.ShowLootWindow(lootSourceObject);

// Show damage number (world space)
var combatLog = UIManager.Instance.GetCombatLogUI();
combatLog.ShowDamageNumber(worldPosition, 150, isCritical: true);

// Add combat log message
combatLog.AddLogEntry("Dragon defeated!", CombatLogUI.LogEntryType.System);

// Show loot notification
lootUI.ShowLootNotification("Epic Sword", 1, Color.magenta);
```

### Checking Panel State
```csharp
bool inventoryOpen = UIManager.Instance.IsPanelOpen("Inventory");
bool anyOpen = UIManager.Instance.IsAnyPanelOpen();
```

---

## Integration Checklist

### For Each Panel:
- [ ] Create UI GameObject hierarchy in scene
- [ ] Assign panel reference in UIManager
- [ ] Create required prefabs
- [ ] Assign prefab references in Inspector
- [ ] Wire up Framework system in Initialize()
- [ ] Implement TODO methods for data fetching
- [ ] Test show/hide/toggle functionality
- [ ] Test keyboard shortcuts
- [ ] Verify tooltips work
- [ ] Test search/filter (if applicable)

---

## Key Features by Panel

### QuestLogUI
- Active/completed tabs
- Objective tracking with progress bars
- Search & filter
- Reward display

### InventoryUI
- 8x6 grid (configurable)
- 9 equipment slots
- Drag-and-drop
- Tooltips on hover
- Weight/gold tracking

### CraftingUI
- Recipe browser
- Material checking
- Crafting queue
- Quantity selection

### TradingUI
- Buy/sell modes
- Reputation discounts
- Price calculations
- Dual inventory view

### DialogueUI
- Typewriter effect
- Speaker portraits
- Choice system
- Keyboard shortcuts (1-9)

### FactionReputationUI
- 7 standing levels (Hated to Exalted)
- Reputation bars
- Relationships display

### StatsUI
- Primary/secondary stats
- Skills with progress
- Attribute allocation
- Experience tracking

### AbilitiesUI
- 10 ability slots
- Drag-and-drop assignment
- Cooldown tracking
- Skill tree (placeholder)

### StatusEffectsUI
- Buff/debuff icons
- Duration timers
- Stack counts
- Tooltips

### CombatLogUI
- Scrolling text feed
- Floating damage numbers
- Message filtering
- Timestamped entries

### LootUI
- Loot windows
- Take all button
- Loot rolls (Need/Greed/Pass)
- Fade-in notifications

---

## UIManager Settings

Configure in Inspector:

**Allow Multiple Panels Open:**
- `true` = Can have multiple panels open simultaneously
- `false` = Opening a panel closes all others

**Pause Game When Panel Open:**
- `true` = Time.timeScale = 0 when any panel is open
- `false` = Game continues running with panels open

**Keyboard Shortcuts:**
- Customize all shortcut keys in Inspector
- Default keys listed above

---

## Troubleshooting

### Panel Won't Open
- Check panel reference is assigned in UIManager
- Check panel GameObject is active in hierarchy
- Check keyboard shortcut key in UIManager inspector

### Prefabs Not Showing
- Verify prefab references assigned in panel Inspector
- Check prefab structure matches expected hierarchy
- Ensure containers (Transform) are assigned

### Tooltips Not Working
- Check EventSystem exists in scene
- Verify GraphicRaycaster on Canvas
- Check tooltip panel assigned in Inspector

### Drag-and-Drop Not Working
- Ensure EventSystem in scene
- Verify items have Graphic component (Image/Text)
- Check PointerEventData handlers implemented

### Framework Integration Issues
- Search for "TODO" comments in panel scripts
- Implement data fetching methods
- Verify Framework system passed to Initialize()

---

## Next Steps

### Immediate (Required)
1. Create Unity prefabs for all entry types
2. Wire up Framework system references
3. Implement TODO data fetching methods
4. Add icons/sprites for visuals
5. Test all keyboard shortcuts

### Short Term (Recommended)
1. Add UI animations (fade, slide)
2. Implement sound effects
3. Add gamepad support
4. Test with actual game data
5. Optimize performance (pooling)

### Long Term (Optional)
1. Localization support
2. Mobile/touch controls
3. Accessibility features
4. UI themes/skins
5. State persistence (save/load)

---

## Performance Tips

1. **Use Object Pooling** for:
   - Damage numbers (frequently created/destroyed)
   - Loot notifications
   - Log entries
   - Effect icons

2. **Set Max Limits:**
   - Combat log: 100 entries (default)
   - Status effects: 10 visible
   - Notifications: 5 simultaneous

3. **Lazy Refresh:**
   - Panels only refresh when visible
   - Updates triggered by events, not every frame

4. **Proper Cleanup:**
   - All panels subscribe/unsubscribe events correctly
   - No memory leaks from event handlers

---

## Support Resources

### Documentation
- `UI_SYSTEMS_SUMMARY.md` - Complete technical documentation
- `QUICK_START_GUIDE.md` - This file
- Inline code comments in all scripts

### Code Structure
- All panels follow consistent patterns
- Common methods: Show(), Hide(), Toggle(), Initialize()
- Event-driven architecture
- Inspector-friendly with SerializeField attributes

---

## File Locations

### Scripts
```
Assets/_MythHunter/Code/UI/Panels/*.cs        - 11 panel scripts
Assets/_MythHunter/Code/UI/Managers/*.cs      - 1 manager script
```

### Documentation
```
Assets/_MythHunter/Code/UI/UI_SYSTEMS_SUMMARY.md    - Full documentation
Assets/_MythHunter/Code/UI/QUICK_START_GUIDE.md     - This guide
```

---

## Summary

You now have **11 comprehensive UI panels** ready to use with the RPG Framework:

1. Quest Log (Q)
2. Inventory (I)
3. Crafting (C)
4. Trading (T)
5. Dialogue (D)
6. Faction Reputation (F)
7. Character Stats (K)
8. Abilities (P)
9. Status Effects (HUD)
10. Combat Log (L)
11. Loot (O)

All coordinated by the **UIManager** with full keyboard support.

**Next Action:** Create Unity prefabs and wire up Framework systems!

---

**Happy RPG Development!**
