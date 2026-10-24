# MythHunter → Framework Analysis

**Goal:** Identify MythHunter code that should be promoted to Framework

---

## What MythHunter Has That Framework Needs

### ✅ Already in Framework:
- ECS
- Combat, Stats, Inventory, Items, Loot
- Abilities, Progression, Skills, Status Effects
- Quests, Dialogue, Crafting, Trading
- Factions, Achievements, Guilds, Companions
- Environment, DynamicEvents, Housing
- Persistence, Networking
- Logging, Serialization, Pooling

### 🟡 In MythHunter - Should Be in Framework:

#### **1. Cloud/Analytics Integration** 📊
**Location:** `Code/Cloud/`
- AWS integration
- Analytics tracking
- Local cloud simulation

**Framework Value:** MEDIUM
- Many RPGs need analytics
- Cloud save is common
- **Action:** Extract as `RPGFramework.Cloud` (optional module)

#### **2. Dependency Injection (DI)** 🔧
**Location:** `Code/Core/DI/`
- Service locator
- Dependency injection container
- Installers

**Framework Value:** **HIGH**
- Critical for large RPG systems
- Already proven in MythHunter
- **Action:** Extract as `RPGFramework.DI` ✅

#### **3. Scene Management** 🎬
**Location:** `Code/Core/SceneManagement/`
- Loading screens
- Scene transitions
- Async scene loading

**Framework Value:** **HIGH**
- Every RPG needs scene management
- **Action:** Extract as `RPGFramework.SceneManagement` ✅

#### **4. State Machine** 🔄
**Location:** `Code/Core/StateMachine/`
- Generic state machine
- Game state management
- Phase system

**Framework Value:** **HIGH**
- Common RPG pattern (combat states, game phases)
- **Action:** Extract as `RPGFramework.StateMachine` ✅

#### **5. Resource Management** 📦
**Location:** `Code/Resources/`
- Resource loading
- Resource pooling
- Addressables integration

**Framework Value:** MEDIUM
- Useful but can vary by project
- **Action:** Consider extracting as `RPGFramework.ResourceManagement`

#### **6. Validation System** ✔️
**Location:** `Code/Core/Validation/`
- Data validation
- Debug validation
- Runtime checks

**Framework Value:** **HIGH**
- Essential for data integrity
- **Action:** Extract as `RPGFramework.Validation` ✅

#### **7. AI System** 🤖
**Location:** `Code/Systems/AI/`
- Behavior trees?
- Decision making?
- Pathfinding?

**Framework Value:** **VERY HIGH**
- Most RPGs need AI
- **Action:** Investigate and extract as `RPGFramework.AI` ✅✅✅

#### **8. Hero/Character System** 👤
**Location:** `Code/Entities/Heroes/`, `Code/Services/Heroes/`
- Hero archetypes
- Hero selection
- Character creation

**Framework Value:** MEDIUM
- Specific to MythHunter's design
- Framework has ECS entities (more generic)
- **Action:** Keep in MythHunter, document as example

#### **9. Replay System** 📹
**Location:** `Code/Replay/`
- Replay recording
- Replay playback

**Framework Value:** LOW-MEDIUM
- Niche feature
- **Action:** Keep in MythHunter as example

#### **10. Movement System** 🏃
**Location:** `Code/Components/Movement/`, `Code/Systems/Gameplay/`
- Character movement
- Physics integration
- Pathfinding

**Framework Value:** **HIGH**
- Every RPG needs movement
- **Action:** Extract as `RPGFramework.Movement` ✅

#### **11. Loading System** ⏳
**Location:** `Code/Systems/Loading/`
- Loading screens
- Progress tracking
- Async operations

**Framework Value:** MEDIUM
- Can work with SceneManagement
- **Action:** Merge into `RPGFramework.SceneManagement`

#### **12. Authoring Tools** 🛠️
**Location:** `Code/Authoring/`, `Editor/`
- Custom inspectors
- Wizards
- Editor tools

**Framework Value:** **HIGH**
- Framework needs good authoring tools
- **Action:** Extract as `RPGFramework.EditorTools` ✅

#### **13. Debug/Profiling** 🐛
**Location:** `Code/Debug/`
- Debug UI
- Event debugging
- Performance profiling
- Pool monitoring

**Framework Value:** **HIGH**
- Already have `RPGFramework.Tools`, enhance it
- **Action:** Merge into existing Tools ✅

#### **14. Navigation System** 🗺️
**Location:** `Code/UI/Navigation/`
- UI navigation
- Menu flow

**Framework Value:** MEDIUM
- Useful for UI-heavy RPGs
- **Action:** Document as pattern in UI guidelines

---

## Priority Extraction List

### MUST HAVE (Core Framework):
1. **DI Container** - RPGFramework.DI ✅
2. **State Machine** - RPGFramework.StateMachine ✅
3. **AI System** - RPGFramework.AI ✅✅✅
4. **Movement System** - RPGFramework.Movement ✅
5. **Validation** - RPGFramework.Validation ✅
6. **Scene Management** - RPGFramework.SceneManagement ✅

### SHOULD HAVE (Enhanced Framework):
7. **Authoring/Editor Tools** - Enhance RPGFramework.Editor
8. **Debug Tools** - Enhance RPGFramework.Tools
9. **Resource Management** - RPGFramework.ResourceManagement

### NICE TO HAVE (Optional Modules):
10. **Cloud/Analytics** - RPGFramework.Cloud (opt-in)
11. **Replay System** - RPGFramework.Replay (opt-in)

### KEEP IN MYTHHUNTER (Game-Specific):
- Hero archetypes (game design)
- Specific UI layouts
- Game-specific components
- Lobby system (multiplayer game specific)

---

## Architecture Notes

### Current Framework Missing:

1. **No DI/Service Locator**
   - Systems manually create dependencies
   - Hard to test in isolation
   - **Fix:** Add RPGFramework.DI

2. **No AI System**
   - Combat exists but no enemy AI
   - No behavior trees / decision making
   - **Fix:** Add RPGFramework.AI

3. **No Movement System**
   - Have stats/combat but no movement
   - **Fix:** Add RPGFramework.Movement

4. **No State Machine**
   - No generic state management
   - **Fix:** Add RPGFramework.StateMachine

5. **No Scene Management**
   - No loading screens / transitions
   - **Fix:** Add RPGFramework.SceneManagement

6. **Editor Tools Basic**
   - Have some but could be better
   - **Fix:** Enhance RPGFramework.Editor

---

## Recommended Framework Additions

### New Modules to Create:

```
Assets/_Framework/
  DI/                           ← NEW! Dependency Injection
    Runtime/
      IDependencyContainer.cs
      ServiceLocator.cs
      Installer.cs
    Tests/
      DITests.cs

  StateMachine/                 ← NEW! State Management
    Runtime/
      IStateMachine.cs
      State.cs
      StateTransition.cs
    Tests/
      StateMachineTests.cs

  AI/                           ← NEW! Artificial Intelligence
    Runtime/
      IBehaviorTree.cs
      IDecisionMaker.cs
      Blackboard.cs
    Tests/
      AITests.cs

  Movement/                     ← NEW! Character Movement
    Runtime/
      IMovementController.cs
      PathFinding.cs
      NavigationSystem.cs
    Tests/
      MovementTests.cs

  Validation/                   ← NEW! Data Validation
    Runtime/
      IValidator.cs
      ValidationRule.cs
      ValidationResult.cs
    Tests/
      ValidationTests.cs

  SceneManagement/              ← NEW! Scene Loading
    Runtime/
      ISceneLoader.cs
      LoadingScreen.cs
      SceneTransition.cs
    Tests/
      SceneManagementTests.cs
```

---

## Implementation Plan

### Phase 1: Extract Core Systems (Week 1)
1. Extract DI system from MythHunter
2. Extract StateMachine
3. Create tests for both
4. Document APIs

### Phase 2: Extract Gameplay Systems (Week 2)
1. Extract AI system
2. Extract Movement system
3. Create tests
4. Document APIs

### Phase 3: Extract Supporting Systems (Week 3)
1. Extract Validation
2. Extract SceneManagement
3. Enhance Editor tools
4. Create tests

### Phase 4: Polish & Integration (Week 4)
1. Ensure all new systems work together
2. Create examples for each
3. Update documentation
4. Test in fresh project

---

## What This Means for Framework

### Before (Current):
- 29 systems
- Gameplay-focused
- Missing infrastructure

### After (Complete):
- **35+ systems**
- Gameplay ✅
- Infrastructure ✅
- Editor tooling ✅
- AI ✅
- Full RPG framework

---

## Success Criteria

Framework is "complete" when it has:
- ✅ All gameplay systems (combat, quests, inventory, etc.) - DONE
- ✅ ECS foundation - DONE
- 🔄 DI/IoC container - EXTRACT FROM MYTHH
UNTER
- 🔄 AI system - EXTRACT
- 🔄 Movement system - EXTRACT
- 🔄 State machine - EXTRACT
- 🔄 Scene management - EXTRACT
- ✅ Serialization/Persistence - DONE
- ✅ Networking - DONE
- ⚠️ Strong editor tools - ENHANCE
- ⚠️ Validation system - ADD
- ✅ Debug/profiling tools - DONE
- ✅ Complete documentation - IN PROGRESS
- ✅ 100% test coverage - IN PROGRESS

---

## Next Steps

1. **Audit MythHunter's DI system** - Can it be extracted?
2. **Audit MythHunter's AI system** - What does it do?
3. **Audit Movement system** - How does it work?
4. **Plan extraction** - What needs to change?
5. **Extract systems one by one** - Test after each

---

## Bottom Line

**MythHunter has ~6 critical systems that Framework needs:**
1. DI Container
2. State Machine
3. AI System
4. Movement System
5. Scene Management
6. Validation

**Extracting these would make Framework truly complete.**
