# EPIC 17 - Dialogue System Plan

**Date:** 2026-10-18
**Target Assembly:** `RPGFramework.Dialogue.Runtime`
**Pattern:** Framework extraction + MythHunter integration (dual-component)

---

## Overview

Universal Dialogue System for managing conversations, dialogue trees, branching choices, conditions, and NPC interactions. The system supports:
- Dialogue trees with branching conversations
- Player choices with consequences
- Conditional dialogue (based on quests, stats, items, etc.)
- Speaker management (NPCs, player)
- Dialogue events and triggers
- Localization support (future)

---

## Architecture

### Framework Layer (RPGFramework.Dialogue.Runtime)
Pure .NET 8.0, `noEngineReferences: true`, zero MythHunter coupling

**Responsibilities:**
- Dialogue tree structure and traversal
- Dialogue node management (lines, choices, conditionals)
- Conversation state tracking
- Speaker definitions
- Event emission (DialogueStarted, DialogueEnded, LineSpoken, ChoiceSelected)

### Game Layer (MythHunter Integration)
Unity-dependent, game-specific metadata

**Responsibilities:**
- UI presentation (dialogue windows, character portraits)
- Audio/voice acting playback
- Camera control during dialogues
- Character animations
- Quest integration
- Save/load dialogue state

---

## Phase 1: Framework Module

### Contracts (6 interfaces)

#### IDialogue
```csharp
public interface IDialogue
{
    string Id { get; }
    string Title { get; }
    string Description { get; }
    string RootNodeId { get; }
    IReadOnlyList<IDialogueNode> Nodes { get; }
    IDialogueNode? GetNode(string nodeId);
}
```

#### IDialogueNode
```csharp
public interface IDialogueNode
{
    string Id { get; }
    DialogueNodeType NodeType { get; } // Line, Choice, Branch, End
    string SpeakerId { get; }
    string Text { get; }
    IReadOnlyList<string> NextNodeIds { get; }
    IReadOnlyList<IDialogueChoice> Choices { get; }
    IDialogueCondition? Condition { get; }
}
```

#### IDialogueChoice
```csharp
public interface IDialogueChoice
{
    string Id { get; }
    string Text { get; }
    string TargetNodeId { get; }
    IDialogueCondition? Condition { get; }
    bool IsEnabled { get; }
}
```

#### IDialogueCondition
```csharp
public interface IDialogueCondition
{
    ConditionType Type { get; } // QuestComplete, StatCheck, ItemOwned, Custom
    string ParameterId { get; }
    int RequiredValue { get; }
    bool Evaluate(IDialogueContext context);
}
```

#### IDialogueContext
```csharp
public interface IDialogueContext
{
    int EntityId { get; }
    string CurrentNodeId { get; }
    Dictionary<string, int> Variables { get; }
    bool HasCompletedQuest(string questId);
    bool HasItem(string itemId);
    int GetStatValue(string statId);
}
```

#### IDialogueManager
```csharp
public interface IDialogueManager
{
    bool StartDialogue(int entityId, string dialogueId);
    bool AdvanceDialogue(int entityId, string? choiceId = null);
    void EndDialogue(int entityId);
    IDialogueProgress? GetDialogueProgress(int entityId);
    IDialogueNode? GetCurrentNode(int entityId);
    IReadOnlyList<IDialogueChoice> GetAvailableChoices(int entityId);
}
```

### Data Structures (6 structs)

1. **DialogueData** - Serializable dialogue definition
2. **DialogueNodeData** - Serializable node
3. **DialogueChoiceData** - Serializable choice
4. **DialogueConditionData** - Serializable condition
5. **DialogueProgressData** - Active dialogue state (with visited nodes)
6. **ActiveDialoguesData** - ECS component (IComponent)

### Enums (3)

1. **DialogueNodeType**: Line, Choice, Branch, End
2. **ConditionType**: QuestComplete, StatCheck, ItemOwned, VariableCheck, Custom
3. **DialogueState**: NotStarted, Active, Completed, Interrupted

### Events (4)

1. **DialogueStartedEvent** - Dialogue initiated
2. **DialogueEndedEvent** - Dialogue concluded
3. **LineSpokenEvent** - Line displayed
4. **ChoiceSelectedEvent** - Player choice made

### Implementations (6 classes)

1. **Dialogue** - Implements IDialogue
2. **DialogueNode** - Implements IDialogueNode
3. **DialogueChoice** - Implements IDialogueChoice
4. **DialogueCondition** - Implements IDialogueCondition
5. **DialogueProgress** - Implements IDialogueProgress (state management)
6. **DialogueManager** - Implements IDialogueManager (ECS-based)

### Test Suite
- ~30 comprehensive tests covering:
  - Dialogue creation and structure
  - Node traversal
  - Choice selection
  - Conditional branching
  - State management
  - Multiple dialogues per entity

---

## Phase 2: MythHunter Integration

### Components

#### MythHunterDialogueComponent
```csharp
[Serializable]
public struct MythHunterDialogueComponent
{
    public Dictionary<string, DialogueMetadata> Metadata;

    // Methods:
    // - GetMetadata(dialogueId)
    // - GetDialoguesBySpeaker(speakerId)
    // - GetQuestDialogues()
}
```

### Data

#### DialogueMetadata
```csharp
[Serializable]
public struct DialogueMetadata
{
    public string SpeakerName;
    public string SpeakerPortraitPath;
    public string SpeakerVoiceActorId;
    public string BackgroundMusicPath;
    public string CameraAngle; // Closeup, Medium, Wide
    public bool PauseGameTime;
    public bool ShowInDialogueLog;
    public string Category; // Main, Side, Ambient, Quest
    public string TriggerQuestId; // Optional: Quest that unlocks this dialogue
}
```

### Adapters

#### DialogueSystemIntegration
- Wraps IDialogueManager
- Manages metadata
- Provides helper methods:
  - StartDialogue(entityId, dialogueId, metadata?)
  - AdvanceDialogue()
  - EndDialogue()
  - GetCurrentNode()
  - GetAvailableChoices() with metadata
  - GetDialogueHistory()

### Sample Content

#### SampleDialogues.cs
5 sample dialogues:
1. **GreetingVillageElder** - Simple linear greeting
2. **QuestAcceptanceDialog** - Quest acceptance with choices
3. **ShopKeeperBargain** - Conditional choices based on gold
4. **BranchingStoryChoice** - Major story decision
5. **CompanionConversation** - Multi-path conversation tree

---

## Dependencies

**Framework References:**
- `RPGFramework.ECS.Runtime` - Component system, IEntityManager
- `RPGFramework.Quests.Runtime` (optional) - Quest condition checks

**Integration References:**
- MythHunter logging (IMythLogger)
- Quest System Integration (quest condition evaluation)
- Inventory System Integration (item condition checks)
- Stats System Integration (stat condition checks)

---

## Success Criteria

### Phase 1 (Framework)
- [ ] All 6 contracts defined
- [ ] All 6 implementations complete
- [ ] ~30 Framework tests created (100% pass rate)
- [ ] Assembly compiles with `noEngineReferences: true`
- [ ] Zero Unity dependencies in Framework
- [ ] Events defined and emitted
- [ ] Conditional branching supported
- [ ] Multiple concurrent dialogues per entity

### Phase 2 (Integration)
- [ ] MythHunterDialogueComponent created
- [ ] DialogueSystemIntegration adapter complete
- [ ] Game metadata system functional
- [ ] 5 sample dialogues created
- [ ] Dual-component pattern followed
- [ ] Integration follows existing patterns (Quest, Abilities, etc.)

### Phase 3 (Documentation)
- [ ] EPIC-17-COMPLETE.md written
- [ ] All phases documented

---

## Implementation Plan

### Step 1: Create directory structure
```
Assets/_Framework/Dialogue/
├── Runtime/
│   ├── Contracts/ (6 files)
│   ├── Data/ (6 files)
│   ├── Enums/ (3 files)
│   ├── Events/ (4 files)
│   ├── Implementations/ (6 files)
│   └── RPGFramework.Dialogue.Runtime.asmdef
└── Tests/
    ├── DialogueSystemTests.cs
    └── RPGFramework.Dialogue.Tests.asmdef
```

### Step 2: Create enums (simple start)
- DialogueNodeType.cs
- ConditionType.cs
- DialogueState.cs

### Step 3: Create contracts
- IDialogue, IDialogueNode, IDialogueChoice
- IDialogueCondition, IDialogueContext
- IDialogueManager

### Step 4: Create data structures
- DialogueData, DialogueNodeData, DialogueChoiceData
- DialogueConditionData, DialogueProgressData
- ActiveDialoguesData (IComponent)

### Step 5: Create events
- DialogueStartedEvent, DialogueEndedEvent
- LineSpokenEvent, ChoiceSelectedEvent

### Step 6: Create implementations (Task subagent for efficiency)
- Dialogue, DialogueNode, DialogueChoice
- DialogueCondition, DialogueProgress
- DialogueManager

### Step 7: Create test suite (Task subagent)
- ~30 comprehensive tests

### Step 8: Create assembly definition
- RPGFramework.Dialogue.Runtime.asmdef

### Step 9: Commit Phase 1
```bash
git add Assets/_Framework/Dialogue
git commit -m "feat: EPIC 17 Phase 1 - Dialogue System Framework module"
```

### Step 10: Create integration layer
- MythHunterDialogueComponent
- DialogueMetadata
- DialogueSystemIntegration
- SampleDialogues

### Step 11: Commit Phase 2
```bash
git add Assets/_MythHunter/Code/GameLogic/Dialogue
git commit -m "feat: EPIC 17 Phase 2 - Dialogue System MythHunter Integration"
```

### Step 12: Create documentation
- EPIC-17-COMPLETE.md

### Step 13: Commit Phase 3
```bash
git commit -m "docs: EPIC 17 Phase 3 - Dialogue System completion documentation"
```

---

## Estimated Metrics

**Framework:**
- Files: ~28 files
- Lines: ~3,200
- Tests: ~30 tests
- Assembly: RPGFramework.Dialogue.Runtime

**Integration:**
- Files: ~4 files
- Lines: ~1,400
- Sample dialogues: 5

**Total:**
- Files: ~32 files
- Lines: ~4,600
- Tests: ~30 tests

---

## Notes

- **Localization**: Dialogue text will use simple strings initially. Localization IDs can be added later.
- **Voice Acting**: DialogueMetadata includes VoiceActorId for future audio integration.
- **Quest Integration**: Dialogue conditions can check quest completion via IDialogueContext.
- **Dialogue Variables**: Context supports custom variables for branching logic (e.g., "MetVillageElder": 1).
- **Dialogue History**: ActiveDialoguesData tracks visited nodes for replay prevention.

---

**EPIC 17 Dialogue System: READY TO IMPLEMENT** ✅
