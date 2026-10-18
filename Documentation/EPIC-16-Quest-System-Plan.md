# EPIC 16 — Quest System Implementation Plan

**Date:** 2026-10-18
**Status:** Planning
**Target:** Extract universal Quest System to Framework, integrate with MythHunter

---

## Overview

Quest System provides quest definitions, objectives, progress tracking, rewards, and quest chains. This is a greenfield extraction (no existing implementation in MythHunter).

**Key Features:**
- Quest definitions with multiple objectives
- Objective types: Kill, Collect, Interact, Explore, Escort
- Progress tracking per quest and objective
- Quest states: NotStarted, Active, Completed, Failed
- Quest chains and prerequisites
- Reward system (Items, XP, Currency)
- Event-driven architecture

---

## Architecture Design

### Core Contracts (Interfaces)

**IQuest** - Quest definition
- `string Id { get; }`
- `string Title { get; }`
- `string Description { get; }`
- `IReadOnlyList<IQuestObjective> Objectives { get; }`
- `IReadOnlyList<IQuestReward> Rewards { get; }`
- `IReadOnlyList<string> PrerequisiteQuestIds { get; }`
- `int MinimumLevel { get; }`
- `bool IsRepeatable { get; }`

**IQuestObjective** - Single quest objective
- `string Id { get; }`
- `ObjectiveType Type { get; }` (Kill, Collect, Interact, Explore, Escort, Custom)
- `string TargetId { get; }` (enemy ID, item ID, NPC ID, location ID)
- `int RequiredCount { get; }`
- `string Description { get; }`
- `bool IsOptional { get; }`

**IQuestProgress** - Player's progress on a quest
- `string QuestId { get; }`
- `QuestState State { get; }` (NotStarted, Active, Completed, Failed, Abandoned)
- `IReadOnlyDictionary<string, int> ObjectiveProgress { get; }` (objectiveId -> current count)
- `DateTime? StartedAt { get; }`
- `DateTime? CompletedAt { get; }`

**IQuestReward** - Quest reward definition
- `RewardType Type { get; }` (Item, Experience, Currency, Unlock)
- `string RewardId { get; }` (item ID, unlock ID)
- `int Amount { get; }`

**IQuestManager** - ECS-based quest management
- `bool CanAcceptQuest(int entityId, string questId)`
- `bool AcceptQuest(int entityId, string questId)`
- `bool UpdateObjectiveProgress(int entityId, string questId, string objectiveId, int progress)`
- `bool CompleteQuest(int entityId, string questId)`
- `bool AbandonQuest(int entityId, string questId)`
- `IQuestProgress? GetQuestProgress(int entityId, string questId)`
- `IReadOnlyList<IQuestProgress> GetActiveQuests(int entityId)`
- `IReadOnlyList<IQuestProgress> GetCompletedQuests(int entityId)`

**IQuestRegistry** - Quest database
- `IQuest? GetQuest(string questId)`
- `IReadOnlyList<IQuest> GetAllQuests()`
- `void RegisterQuest(IQuest quest)`

---

### Data Structures

**QuestData** - Serializable quest definition
```csharp
public struct QuestData
{
    public string Id;
    public string Title;
    public string Description;
    public QuestObjectiveData[] Objectives;
    public QuestRewardData[] Rewards;
    public string[] PrerequisiteQuestIds;
    public int MinimumLevel;
    public bool IsRepeatable;
}
```

**QuestObjectiveData** - Serializable objective
```csharp
public struct QuestObjectiveData
{
    public string Id;
    public ObjectiveType Type;
    public string TargetId;
    public int RequiredCount;
    public string Description;
    public bool IsOptional;
}
```

**QuestProgressData** - ECS component data
```csharp
public struct QuestProgressData
{
    public string QuestId;
    public QuestState State;
    public Dictionary<string, int> ObjectiveProgress;
    public long StartedAtTicks;
    public long? CompletedAtTicks;
}
```

**ActiveQuestsData** - ECS component (contains all active quests for entity)
```csharp
public struct ActiveQuestsData : IComponent
{
    public Dictionary<string, QuestProgressData> ActiveQuests;
    public List<string> CompletedQuestIds;
    public int TotalQuestsCompleted;
}
```

---

### Enums

**ObjectiveType**
- Kill
- Collect
- Interact
- Explore
- Escort
- Custom

**QuestState**
- NotStarted
- Active
- InProgress (same as Active, for clarity)
- Completed
- Failed
- Abandoned

**RewardType**
- Item
- Experience
- Currency
- Unlock
- Custom

---

### Events

**QuestAcceptedEvent**
- `int EntityId`
- `string QuestId`
- `DateTime AcceptedAt`

**QuestCompletedEvent**
- `int EntityId`
- `string QuestId`
- `DateTime CompletedAt`
- `IReadOnlyList<IQuestReward> Rewards`

**QuestFailedEvent**
- `int EntityId`
- `string QuestId`
- `string FailureReason`

**QuestAbandonedEvent**
- `int EntityId`
- `string QuestId`

**ObjectiveProgressedEvent**
- `int EntityId`
- `string QuestId`
- `string ObjectiveId`
- `int CurrentProgress`
- `int RequiredProgress`
- `bool IsCompleted`

---

## Implementation Phases

### Phase 1: Framework Module Extraction

**Files to create:**

`Assets/_Framework/Quests/Runtime/`
- `Contracts/IQuest.cs`
- `Contracts/IQuestObjective.cs`
- `Contracts/IQuestProgress.cs`
- `Contracts/IQuestReward.cs`
- `Contracts/IQuestManager.cs`
- `Contracts/IQuestRegistry.cs`
- `Data/QuestData.cs`
- `Data/QuestObjectiveData.cs`
- `Data/QuestProgressData.cs`
- `Data/QuestRewardData.cs`
- `Data/ActiveQuestsData.cs`
- `Enums/ObjectiveType.cs`
- `Enums/QuestState.cs`
- `Enums/RewardType.cs`
- `Events/QuestAcceptedEvent.cs`
- `Events/QuestCompletedEvent.cs`
- `Events/QuestFailedEvent.cs`
- `Events/QuestAbandonedEvent.cs`
- `Events/ObjectiveProgressedEvent.cs`
- `Implementations/Quest.cs`
- `Implementations/QuestObjective.cs`
- `Implementations/QuestProgress.cs`
- `Implementations/QuestReward.cs`
- `Implementations/QuestManager.cs`
- `Implementations/QuestRegistry.cs`
- `RPGFramework.Quests.Runtime.asmdef`

**Assembly Definition:**
```json
{
    "name": "RPGFramework.Quests.Runtime",
    "rootNamespace": "RPGFramework.Quests",
    "references": [
        "RPGFramework.ECS.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true
}
```

**Test Coverage (20-25 tests):**
- Quest creation and property access
- Objective creation with different types
- Quest prerequisite validation
- Quest acceptance (with/without prerequisites)
- Objective progress updates
- Quest completion detection
- Quest failure scenarios
- Quest abandonment
- Quest registry operations
- Repeatable quest logic
- Reward system
- Event emission verification

---

### Phase 2: MythHunter Integration

**Files to create:**

`Assets/_MythHunter/Code/GameLogic/Quests/`
- `Components/MythHunterQuestComponent.cs` - Game metadata
- `Adapters/QuestSystemIntegration.cs` - Adapter layer
- `Data/QuestMetadata.cs` - UI icons, quest giver NPCs, cutscenes
- `Sample/SampleQuests.cs` - Example quest definitions

**Game Metadata:**
```csharp
public struct QuestMetadata
{
    public string QuestId;
    public string IconPath;
    public string QuestGiverNpcId;
    public string QuestLocationId;
    public string CompletionCutsceneId;
    public AudioClip AcceptedSound;
    public AudioClip CompletedSound;
    public bool ShowInJournal;
    public string Category; // Main, Side, Repeatable
}
```

**Integration Tests (15-20 tests):**
- Component lifecycle
- Adapter wrapping
- Metadata attachment
- Sample quest validation
- Integration with existing systems (Progression, Inventory, Items)

---

### Phase 3: Documentation

**Files to create:**
- `Documentation/EPIC-16-COMPLETE.md` - Completion summary

---

## Dependencies

**Framework Dependencies:**
- `RPGFramework.ECS.Runtime` - Entity/Component system
- No Unity dependencies (pure .NET 8.0)

**Integration Dependencies:**
- `RPGFramework.Quests.Runtime` - Framework module
- `RPGFramework.Progression.Runtime` - For XP rewards
- `RPGFramework.Inventory.Runtime` - For item rewards
- `RPGFramework.Items.Runtime` - For item definitions
- MythHunter game layer

---

## Quest System Features

### Core Features (Phase 1)
1. Quest definition with multiple objectives
2. Objective types: Kill, Collect, Interact, Explore, Escort
3. Progress tracking per objective
4. Quest states: NotStarted, Active, Completed, Failed, Abandoned
5. Prerequisites and quest chains
6. Rewards: Items, XP, Currency, Unlocks
7. Repeatable quests
8. Event-driven notifications

### Game Layer Features (Phase 2)
1. Quest UI metadata (icons, categories)
2. Quest giver NPC associations
3. Location-based quests
4. Cutscene integration
5. Audio feedback
6. Quest journal filtering
7. Sample quest library

---

## Success Criteria

**Phase 1 (Framework):**
- [ ] All contracts defined
- [ ] All implementations complete
- [ ] 20-25 Framework tests passing (100%)
- [ ] Assembly compiles with `noEngineReferences: true`
- [ ] Zero Unity dependencies
- [ ] Events emit correctly
- [ ] Quest chains work correctly
- [ ] Repeatable quest logic functional

**Phase 2 (Integration):**
- [ ] MythHunterQuestComponent created
- [ ] QuestSystemIntegration adapter complete
- [ ] Game metadata system functional
- [ ] Sample quests created
- [ ] 15-20 integration tests passing (100%)
- [ ] Full Unity project compiles
- [ ] Clean Console

**Phase 3 (Documentation):**
- [ ] EPIC-16-COMPLETE.md written
- [ ] All commits documented

---

## Estimated Effort

**Phase 1:** ~1 day (Framework extraction + tests)
**Phase 2:** ~0.5 day (MythHunter integration + tests)
**Phase 3:** ~0.25 day (Documentation)
**Total:** ~1.75 days

**Expected Output:**
- ~22 Framework files
- ~4 Integration files
- ~2,500 lines of code
- 35-45 tests (100% pass rate)
- 1 new Framework assembly

---

## Integration with Existing Systems

**Progression System:**
- Quest completion awards XP via ProgressionTracker
- Level requirements for quest acceptance

**Inventory System:**
- Item rewards added to inventory
- Collect objectives track inventory items

**Items System:**
- Quest items defined in ItemRegistry
- Quest rewards reference item IDs

**Skills System:**
- Quests can unlock skills
- Skill prerequisites for quests

**Status Effects:**
- Quest buffs/debuffs during active quests
- Quest failure conditions based on effects

---

## Risk Assessment

**Low Risk:**
- Clear domain model (quests are well-understood)
- No Unity dependencies needed
- Event-driven architecture proven in previous EPICs

**Medium Risk:**
- Quest chain complexity (prerequisite validation)
- Integration with multiple existing systems
- Repeatable quest state management

**Mitigation:**
- Start with simple linear quests
- Add chain support incrementally
- Comprehensive test coverage for state transitions

---

## Next Steps

1. Review and approve this plan
2. Create Framework module structure
3. Implement contracts and data structures
4. Implement QuestManager and QuestRegistry
5. Write comprehensive test suite
6. Create MythHunter integration layer
7. Document completion

---

**Ready to proceed with Phase 1!**
