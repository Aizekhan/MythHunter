# How to See the Framework in Action - Practical Guide

**Last Updated:** 2026-10-20
**Target Audience:** Developers, stakeholders, new team members
**Purpose:** Step-by-step guide to demonstrate RPG Framework functionality in MythHunter

---

## 🎯 Overview

This guide shows you how to see the RPG Framework working in practice within the MythHunter game. Each section demonstrates a different system with concrete examples you can verify yourself.

**What You'll See:**
- How Framework systems integrate with MythHunter game code
- How the dual-component architecture works
- How pure .NET calculations produce game results
- How systems interact with each other

**Prerequisites:**
- Unity 6000.0.45f1 installed
- MythHunter project loaded in Unity
- Basic understanding of Unity Editor

---

## 📚 Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Quest System Demo](#quest-system-demo)
3. [Dialogue System Demo](#dialogue-system-demo)
4. [Crafting System Demo](#crafting-system-demo)
5. [Trading System Demo](#trading-system-demo)
6. [Faction System Demo](#faction-system-demo)
7. [Cross-System Integration](#cross-system-integration)
8. [Running Tests](#running-tests)
9. [Inspecting Framework Code](#inspecting-framework-code)

---

## 🏗️ Architecture Overview

### The Dual-Component Pattern

The Framework uses a **dual-component architecture** that separates concerns:

**Framework Layer (Pure .NET 8.0):**
- Location: `Assets/_Framework/`
- No Unity dependencies
- Universal RPG logic (works in ANY RPG game)
- Examples: `QuestManager`, `DialogueManager`, `FactionManager`

**Game Layer (MythHunter-Specific):**
- Location: `Assets/_MythHunter/Code/GameLogic/`
- Unity MonoBehaviour components
- Game-specific metadata and Unity integration
- Examples: `MythHunterQuestComponent`, `MythHunterFactionComponent`

### How They Work Together:

```
Player interacts with Unity GameObject
         ↓
MythHunterComponent (MonoBehaviour) receives event
         ↓
Component calls FrameworkManager methods
         ↓
Framework performs calculations (pure .NET)
         ↓
Results returned to MythHunterComponent
         ↓
Component updates Unity GameObject visual state
```

### Example File Locations:

```
Assets/
├── _Framework/                    ← Pure .NET Framework
│   ├── Quests/
│   │   ├── Runtime/
│   │   │   ├── Contracts/
│   │   │   │   └── IQuestManager.cs       ← Interface
│   │   │   └── Implementations/
│   │   │       └── QuestManager.cs        ← Logic (NO Unity)
│   │   └── Tests/
│   │       └── QuestSystemTests.cs        ← Unit tests
│   │
│   └── Factions/
│       ├── Runtime/
│       │   ├── Contracts/
│       │   │   └── IFactionManager.cs
│       │   └── Implementations/
│       │       └── FactionManager.cs      ← Logic (NO Unity)
│       └── Tests/
│           └── FactionSystemTests.cs
│
└── _MythHunter/                   ← Game-Specific Integration
    └── Code/
        └── GameLogic/
            ├── Quests/
            │   ├── Components/
            │   │   └── MythHunterQuestComponent.cs    ← Unity wrapper
            │   ├── Adapters/
            │   │   └── QuestSystemIntegration.cs      ← Bridge
            │   └── Sample/
            │       └── SampleQuests.cs                ← Example data
            │
            └── Factions/
                ├── Components/
                │   └── MythHunterFactionComponent.cs  ← Unity wrapper
                ├── Adapters/
                │   └── FactionSystemIntegration.cs    ← Bridge
                └── Sample/
                    └── SampleFactions.cs              ← Example data
```

---

## 🎮 Quest System Demo

### Step 1: Understand the Quest System

**Framework Code:**
- Location: `Assets/_Framework/Quests/Runtime/`
- Key Files:
  - `IQuestManager.cs` - Quest lifecycle management interface
  - `QuestManager.cs` - Implementation (pure .NET)
  - `QuestData.cs` - Quest definition structure
  - `ObjectiveData.cs` - Objective definition
  - `RewardData.cs` - Reward definition

**MythHunter Integration:**
- Location: `Assets/_MythHunter/Code/GameLogic/Quests/`
- Key Files:
  - `MythHunterQuestComponent.cs` - MonoBehaviour wrapper
  - `QuestSystemIntegration.cs` - Sample quest registration
  - `SampleQuests.cs` - Example quest definitions

### Step 2: View Sample Quests

**Action:** Open `Assets/_MythHunter/Code/GameLogic/Quests/Sample/SampleQuests.cs`

**What You'll See:**
```csharp
// Example quest: "The Missing Artifact"
integration.RegisterQuest(
    questId: "quest_missing_artifact",
    name: "The Missing Artifact",
    description: "The museum's prized artifact has been stolen...",
    questType: QuestType.Main,
    level: 5,
    objectives: new List<ObjectiveData> {
        new ObjectiveData {
            ObjectiveId = "obj_find_thief",
            Description = "Find the thief",
            ObjectiveType = ObjectiveType.Investigate,
            TargetCount = 1,
            IsOptional = false
        },
        new ObjectiveData {
            ObjectiveId = "obj_recover_artifact",
            Description = "Recover the artifact",
            ObjectiveType = ObjectiveType.Collect,
            TargetCount = 1,
            IsOptional = false
        }
    },
    rewards: new RewardData {
        Experience = 500,
        Gold = 250,
        Items = new Dictionary<string, int> {
            { "item_museum_badge", 1 }
        }
    }
);
```

**This Shows:** How quest data is defined in a universal format that works for any RPG.

### Step 3: Run Quest Tests

**Action:** Open Terminal and run:
```bash
unity test "D:\MythHunter-Git" --mode EditMode --output "TestResults/QuestTests.xml" --verbose --timeout 180
```

**Filter for Quest Tests in Results:**
Look for test names containing "Quest" in the XML output.

**What You'll See:**
- `QuestManager_StartQuest_ActivatesQuest` ✓
- `QuestManager_UpdateObjective_TracksProgress` ✓
- `QuestManager_CompleteQuest_AwardsRewards` ✓
- And more...

**This Proves:** Quest logic works correctly through automated tests.

### Step 4: Trace Code Execution

**Scenario:** Player accepts quest "The Missing Artifact"

**Code Flow:**
1. Unity UI calls `MythHunterQuestComponent.StartQuest("quest_missing_artifact")`
2. Component retrieves quest from integration: `_integration.GetQuest("quest_missing_artifact")`
3. Component calls Framework: `_questManager.StartQuest(entityId, questData)`
4. Framework updates ECS component: `_entityManager.AddComponent<ActiveQuestsData>(...)`
5. Framework returns success
6. Component logs: `[QuestSystem] Entity {entityId} started quest: The Missing Artifact`

**File to Trace:** `Assets/_MythHunter/Code/GameLogic/Quests/Components/MythHunterQuestComponent.cs:45`

---

## 💬 Dialogue System Demo

### Step 1: Understand the Dialogue System

**Framework Code:**
- Location: `Assets/_Framework/Dialogue/Runtime/`
- Key Files:
  - `IDialogueManager.cs` - Dialogue flow management
  - `DialogueManager.cs` - Branching conversation logic
  - `DialogueData.cs` - Dialogue definition with nodes
  - `DialogueNodeData.cs` - Individual dialogue node (text + choices)

**MythHunter Integration:**
- Location: `Assets/_MythHunter/Code/GameLogic/Dialogue/`
- Key Files:
  - `MythHunterDialogueComponent.cs` - Unity wrapper
  - `DialogueSystemIntegration.cs` - Sample dialogue registration
  - `SampleDialogues.cs` - Example dialogues with branching

### Step 2: View Sample Dialogue

**Action:** Open `Assets/_MythHunter/Code/GameLogic/Dialogue/Sample/SampleDialogues.cs`

**What You'll See:**
```csharp
// Example: Merchant dialogue with faction reputation check
integration.RegisterDialogue(
    dialogueId: "dlg_merchant_greeting",
    name: "Merchant Greeting",
    nodes: new List<DialogueNodeData> {
        new DialogueNodeData {
            NodeId = "start",
            SpeakerId = "npc_merchant",
            Text = "Welcome, traveler! Looking to buy or sell?",
            Choices = new List<DialogueChoiceData> {
                new DialogueChoiceData {
                    ChoiceId = "choice_buy",
                    Text = "I'd like to buy something.",
                    NextNodeId = "node_buy_menu"
                },
                new DialogueChoiceData {
                    ChoiceId = "choice_sell",
                    Text = "I have items to sell.",
                    NextNodeId = "node_sell_menu",
                    Condition = new DialogueConditionData {
                        ConditionType = ConditionType.HasItems,
                        RequiredValue = 1
                    }
                }
            }
        }
    }
);
```

**This Shows:** Branching conversations with conditional choices based on game state.

### Step 3: Trace Dialogue Flow

**Scenario:** Player talks to merchant NPC

**Code Flow:**
1. Player clicks NPC → `MythHunterDialogueComponent.StartDialogue("dlg_merchant_greeting")`
2. Component calls Framework: `_dialogueManager.StartDialogue(entityId, dialogueData)`
3. Framework finds "start" node, returns text + available choices
4. Component displays dialogue UI with filtered choices (based on conditions)
5. Player selects choice → `AdvanceDialogue(choiceId)`
6. Framework navigates to next node, returns new text + choices
7. Repeat until reaching end node

**File to Trace:** `Assets/_MythHunter/Code/GameLogic/Dialogue/Components/MythHunterDialogueComponent.cs:50`

---

## 🔨 Crafting System Demo

### Step 1: Understand the Crafting System

**Framework Code:**
- Location: `Assets/_Framework/Crafting/Runtime/`
- Key Files:
  - `ICraftingManager.cs` - Recipe management and crafting
  - `CraftingManager.cs` - Time-based crafting logic
  - `RecipeData.cs` - Recipe definition with ingredients
  - `CraftingProgressData.cs` - In-progress crafting tracking

**MythHunter Integration:**
- Location: `Assets/_MythHunter/Code/GameLogic/Crafting/`
- Key Files:
  - `MythHunterCraftingComponent.cs` - Unity wrapper
  - `CraftingSystemIntegration.cs` - Sample recipe registration
  - `SampleCraftingRecipes.cs` - Example recipes

### Step 2: View Sample Recipe

**Action:** Open `Assets/_MythHunter/Code/GameLogic/Crafting/Sample/SampleCraftingRecipes.cs`

**What You'll See:**
```csharp
// Example: Health Potion recipe
integration.RegisterRecipe(
    recipeId: "recipe_health_potion",
    name: "Health Potion",
    category: CraftingCategory.Alchemy,
    ingredients: new Dictionary<string, int> {
        { "item_red_herb", 2 },
        { "item_water_vial", 1 }
    },
    result: new CraftingResultData {
        ItemId = "item_health_potion",
        Quantity = 1,
        Quality = ItemQuality.Common
    },
    craftingTime: 5.0f,        // 5 seconds
    requiredSkillLevel: 1,
    experienceGain: 10
);
```

**This Shows:** Recipe system with ingredients, skill requirements, and time-based crafting.

### Step 3: Simulate Crafting

**Scenario:** Player crafts a Health Potion

**Code Flow:**
1. Player opens crafting menu, selects Health Potion recipe
2. Component checks: `_craftingManager.CanCraft(entityId, recipeData)`
3. Framework verifies:
   - Player has ingredients (2 red herbs, 1 water vial)
   - Player skill level ≥ 1
   - No crafting in progress
4. Player clicks "Craft" → `StartCrafting(recipeId)`
5. Framework starts timer: `CraftingProgressData { RecipeId, StartTime, Duration: 5.0f }`
6. Component polls progress: `GetCraftingProgress(entityId)`
7. After 5 seconds → `CompleteCrafting(entityId)`
8. Framework:
   - Removes ingredients from inventory
   - Adds Health Potion to inventory
   - Awards 10 XP to Alchemy skill
   - Returns success

**File to Trace:** `Assets/_MythHunter/Code/GameLogic/Crafting/Components/MythHunterCraftingComponent.cs:60`

---

## 💰 Trading System Demo

### Step 1: Understand the Trading System

**Framework Code:**
- Location: `Assets/_Framework/Trading/Runtime/`
- Key Files:
  - `ITradingManager.cs` - Trade offer management
  - `TradingManager.cs` - Trade execution and reputation
  - `TradeOfferData.cs` - Trade offer definition
  - `TraderData.cs` - NPC trader configuration

**MythHunter Integration:**
- Location: `Assets/_MythHunter/Code/GameLogic/Trading/`
- Key Files:
  - `MythHunterTradingComponent.cs` - Unity wrapper
  - `TradingSystemIntegration.cs` - Sample trader registration
  - `SampleTraders.cs` - Example traders with offers

### Step 2: View Sample Trader

**Action:** Open `Assets/_MythHunter/Code/GameLogic/Trading/Sample/SampleTraders.cs`

**What You'll See:**
```csharp
// Example: Blacksmith trader
integration.RegisterTrader(
    traderId: "trader_blacksmith",
    name: "Master Blacksmith",
    traderType: TraderType.Merchant,
    offers: new List<TradeOfferData> {
        new TradeOfferData {
            OfferId = "trade_iron_sword",
            RequiredItems = new Dictionary<string, int> {
                { "item_iron_ingot", 3 },
                { "item_wood_plank", 1 }
            },
            OfferedItems = new Dictionary<string, int> {
                { "item_iron_sword", 1 }
            },
            GoldAmount = -50,  // Player pays 50 gold
            RequiredReputation = 0,
            MaxUses = -1       // Unlimited
        }
    },
    reputationGain: 5,
    reputationLoss: -10
);
```

**This Shows:** Trading with reputation requirements and consequences.

### Step 3: Simulate Trade

**Scenario:** Player trades with Blacksmith

**Code Flow:**
1. Player interacts with Blacksmith NPC → `MythHunterTradingComponent.OpenTradeWindow("trader_blacksmith")`
2. Component retrieves offers: `_integration.GetTrader("trader_blacksmith")`
3. Framework filters offers based on player reputation: `GetAvailableOffers(entityId, traderId)`
4. Component displays available trades
5. Player selects "Iron Sword" trade → `ProposeTrade(offerId)`
6. Framework verifies:
   - Player has 3 iron ingots, 1 wood plank
   - Player has 50 gold
   - Offer not exhausted
7. Player confirms → `AcceptTrade(offerId)`
8. Framework:
   - Removes items and gold from player
   - Adds Iron Sword to inventory
   - Increases Blacksmith reputation by 5
   - Increments offer use count
   - Returns success

**File to Trace:** `Assets/_MythHunter/Code/GameLogic/Trading/Components/MythHunterTradingComponent.cs:55`

---

## 🏛️ Faction System Demo

### Step 1: Understand the Faction System

**Framework Code:**
- Location: `Assets/_Framework/Factions/Runtime/`
- Key Files:
  - `IFactionManager.cs` - Reputation management
  - `FactionManager.cs` - Reputation with relationship cascades
  - `IFactionRegistry.cs` - Faction storage
  - `FactionReputationData.cs` - Per-entity reputation tracking

**MythHunter Integration:**
- Location: `Assets/_MythHunter/Code/GameLogic/Factions/`
- Key Files:
  - `MythHunterFactionComponent.cs` - Unity wrapper
  - `FactionSystemIntegration.cs` - Sample faction registration
  - `SampleFactions.cs` - 8 factions with 10 relationships

### Step 2: View Faction Relationships

**Action:** Open `Assets/_MythHunter/Code/GameLogic/Factions/Sample/SampleFactions.cs`

**What You'll See:**
```csharp
// Example factions:
integration.RegisterFaction("guild_mages", "Mages Guild", FactionType.Guild, ...);
integration.RegisterFaction("guild_thieves", "Thieves Guild", FactionType.Criminal, ...);
integration.RegisterFaction("cult_shadow", "Shadow Cult", FactionType.Cult, ...);

// Example relationships:
integration.RegisterRelationship(
    "guild_mages",
    "guild_thieves",
    FactionRelationType.Enemy,
    reputationModifier: -0.5f    // Gaining rep with Mages → lose 50% with Thieves
);

integration.RegisterRelationship(
    "guild_mages",
    "cult_shadow",
    FactionRelationType.Rival,
    reputationModifier: -0.3f    // Gaining rep with Mages → lose 30% with Shadow Cult
);
```

**This Shows:** Complex faction relationships with cascading reputation effects.

### Step 3: Simulate Reputation Change

**Scenario:** Player completes quest for Mages Guild (+50 rep)

**Code Flow:**
1. Quest completes → `MythHunterFactionComponent.ModifyReputation("guild_mages", 50)`
2. Component calls Framework: `_factionManager.ModifyReputation(entityId, "guild_mages", 50)`
3. Framework updates Mages Guild reputation: 0 → +50 (Standing: Neutral → Friendly)
4. Framework applies relationship cascades:
   - Thieves Guild (Enemy, -0.5 modifier): 0 → -25 (Standing: Neutral → Unfriendly)
   - Shadow Cult (Rival, -0.3 modifier): 0 → -15 (Standing: Neutral)
   - Order of Light (Friendly, +0.3 modifier): 0 → +15 (Standing: Neutral)
5. Component logs all changes:
   ```
   [FactionSystem] Entity 1 reputation with guild_mages: 50 (Friendly)
   [FactionSystem] Cascade: guild_thieves: -25 (Unfriendly)
   [FactionSystem] Cascade: cult_shadow: -15 (Neutral)
   [FactionSystem] Cascade: order_light: +15 (Neutral)
   ```

**File to Trace:** `Assets/_MythHunter/Code/GameLogic/Factions/Components/MythHunterFactionComponent.cs:70`

### Step 4: View Reputation Standings

**Reputation Range and Standings:**
- **Hated:** -100 to -75 (locked out of faction services)
- **Hostile:** -74 to -50 (attacked on sight)
- **Unfriendly:** -49 to -25 (expensive prices, limited dialogue)
- **Neutral:** -24 to +24 (default state)
- **Friendly:** +25 to +49 (discounts, more dialogue options)
- **Honored:** +50 to +74 (special quests, faction items)
- **Revered:** +75 to +100 (maximum benefits, unique rewards)

**This Shows:** Industry-standard reputation system with meaningful tiers.

---

## 🔗 Cross-System Integration

### Scenario: Complete Quest Chain with Faction Consequences

**Story:** Player helps Mages Guild fight Shadow Cult

**System Interactions:**

1. **Quest System:**
   - Player accepts quest "Investigate Shadow Cult Activity"
   - Objectives: Find cult hideout, retrieve stolen tome, report back

2. **Dialogue System:**
   - Player talks to Mages Guild Archmage
   - Archmage offers quest (conditional: must be Neutral+ with Mages Guild)
   - Player accepts

3. **Combat/Loot (existing systems):**
   - Player fights cult members
   - Loots stolen tome from cult hideout

4. **Quest System:**
   - Player completes "retrieve tome" objective
   - Returns to Archmage, completes quest

5. **Faction System:**
   - Quest completion awards +50 Mages Guild reputation
   - Cascade effects:
     - Shadow Cult: -25 reputation (Enemy relationship, -0.5 modifier)
     - Thieves Guild: -25 reputation (Enemy relationship, -0.5 modifier)
     - Order of Light: +15 reputation (Friendly relationship, +0.3 modifier)

6. **Trading System:**
   - Mages Guild merchant now offers discounts (Friendly standing)
   - Shadow Cult trader refuses service (Unfriendly standing)

7. **Dialogue System:**
   - Archmage offers follow-up quest (requires Friendly+ standing) ✓
   - Shadow Cult NPCs have hostile dialogue options

**This Demonstrates:** How Framework systems work together to create emergent gameplay.

---

## 🧪 Running Tests

### Option 1: Run All Framework Tests

**Command:**
```bash
unity test "D:\MythHunter-Git" --mode EditMode --output "TestResults/AllTests.xml" --verbose --timeout 300
```

**Expected Result:**
- Total tests: 512
- Passed: 512
- Failed: 0
- Exit code: 0

**What This Proves:**
- All Framework logic works correctly
- No Unity dependencies cause issues
- Integration layer is solid

### Option 2: Run Tests for Specific System

**Quest System:**
```bash
unity test "D:\MythHunter-Git" --mode EditMode --test-filter "QuestSystem" --output "TestResults/QuestTests.xml" --verbose --timeout 180
```

**Faction System:**
```bash
unity test "D:\MythHunter-Git" --mode EditMode --test-filter "FactionSystem" --output "TestResults/FactionTests.xml" --verbose --timeout 180
```

**Dialogue System:**
```bash
unity test "D:\MythHunter-Git" --mode EditMode --test-filter "DialogueSystem" --output "TestResults/DialogueTests.xml" --verbose --timeout 180
```

### Option 3: Run Tests in Unity Editor

**Steps:**
1. Open Unity Editor
2. Window → General → Test Runner
3. Select "EditMode" tab
4. Click "Run All" or select specific test suite
5. View results in Test Runner window

**This Shows:** Visual test execution and debugging capability.

---

## 🔍 Inspecting Framework Code

### Verify Zero Unity Dependencies

**Action:** Open any Framework file and check using directives

**Example:** `Assets/_Framework/Factions/Runtime/Implementations/FactionManager.cs`

**What You'll See:**
```csharp
using System;
using System.Collections.Generic;
using RPGFramework.Core.ECS;
using RPGFramework.Factions.Contracts;
using RPGFramework.Factions.Data;

// NO Unity-specific usings:
// ❌ using UnityEngine;
// ❌ using UnityEngine.Events;
// ❌ using UnityEditor;
```

**This Proves:** Framework is truly universal and portable.

### Verify Assembly Definitions

**Action:** Open `Assets/_Framework/Factions/Runtime/RPGFramework.Factions.Runtime.asmdef`

**What You'll See:**
```json
{
    "name": "RPGFramework.Factions.Runtime",
    "references": [
        "RPGFramework.Core.ECS",
        "RPGFramework.Logging"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false
}
```

**Notice:**
- References only other Framework assemblies
- NO references to Unity assemblies
- NO references to MythHunter assemblies

**This Proves:** Strict dependency boundaries are enforced.

### Compare Framework vs MythHunter Code

**Framework File:** `Assets/_Framework/Factions/Runtime/Implementations/FactionManager.cs`
```csharp
// Pure .NET logic - works anywhere
public void ModifyReputation(int entityId, string factionId, int delta)
{
    var data = GetOrCreateReputationData(entityId);
    data.ModifyReputation(factionId, delta);
    ApplyRelationshipCascades(entityId, factionId, delta, data);
    _entityManager.AddComponent(entityId, data);
}
```

**MythHunter File:** `Assets/_MythHunter/Code/GameLogic/Factions/Components/MythHunterFactionComponent.cs`
```csharp
// Unity wrapper - calls Framework
public void ModifyReputation(string factionId, int delta)
{
    _factionManager.ModifyReputation(_entityId, factionId, delta);
    UnityEngine.Debug.Log($"[FactionSystem] Entity {_entityId} reputation with {factionId}...");
    RefreshUI();  // Update Unity UI elements
}
```

**This Shows:** Clear separation between universal logic and Unity-specific presentation.

---

## 🎓 Key Takeaways

### What Makes This Framework Universal:

1. **Pure .NET Logic:**
   - All calculations in Framework layer
   - Zero Unity dependencies
   - Works in any .NET 8.0+ environment

2. **Clean Interfaces:**
   - `IQuestManager`, `IFactionManager`, etc.
   - Easy to mock for testing
   - Easy to replace implementations

3. **Data-Driven Design:**
   - Quests, dialogues, recipes defined in data structures
   - No hard-coded game logic
   - JSON serialization support

4. **ECS Integration:**
   - All state stored in ECS components
   - No hidden singleton state
   - Clean data flow

5. **Backward Compatibility:**
   - Dual-component pattern preserves existing code
   - Gradual migration possible
   - Old and new systems coexist

### How to Verify Framework Quality:

✅ **Run Tests:** 512/512 passing proves logic correctness
✅ **Check Dependencies:** Assembly definitions enforce boundaries
✅ **Inspect Code:** No `using UnityEngine` in Framework files
✅ **Trace Execution:** Follow code from Unity → Framework → Unity
✅ **Test Scenarios:** Cross-system interactions work correctly

---

## 📖 Next Steps

### For Developers:
1. Read this guide thoroughly
2. Run all tests to verify functionality
3. Trace code execution for one system end-to-end
4. Experiment with sample data (add your own quests/dialogues/factions)
5. Read Framework interface documentation

### For Stakeholders:
1. Understand the dual-component architecture
2. See how Framework enables reusability
3. Recognize the value of 100% test coverage
4. Appreciate the clean separation of concerns

### For New Team Members:
1. Clone repository
2. Run tests to verify setup
3. Follow one complete scenario (Quest → Faction → Trading)
4. Read Framework code for one system
5. Compare Framework implementation to MythHunter integration

---

**Questions or Issues?**
- Check `STATUS.md` for current project state
- Check `HANDOFF_NEXT_CHAT.md` for latest session notes
- Review test files for usage examples
- Inspect sample data files for configuration patterns

**Happy Exploring!** 🎮
