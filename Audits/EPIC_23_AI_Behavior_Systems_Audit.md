# EPIC 23 — AI & Behavior Systems Audit

## Audit Date
2026-10-25

## Audit Scope
Source-backed audit of existing AI, behavior, pathfinding, and state machine code in MythHunter to determine what should be extracted to Framework and what remains game-specific.

## Five-Filter Framework Analysis

For each candidate module, apply the five filters:
1. **Is it needed by ANY RPG game (not just MythHunter)?**
2. **Does it depend on game-specific logic?**
3. **Is it reusable across different RPG games?**
4. **Is it Framework Core or Game Layer?**
5. **Is there proof of the problem being solved?**

---

## Phase 23.1 — Pathfinding System

### Existing Code Location
- `Assets/_MythHunter/Code/Systems/Gameplay/Movement/IPathfindingSystem.cs`
- `Assets/_MythHunter/Code/Systems/Gameplay/Movement/PathfindingSystem.cs`

### Current Implementation
```csharp
public interface IPathfindingSystem : ISystem
{
    List<Vector3> FindPath(Vector3 start, Vector3 end, bool avoidObstacles = true);
    bool IsPathValid(List<Vector3> path);
    float CalculatePathLength(List<Vector3> path);
    bool IsPointReachable(Vector3 point, float movementPoints);
}
```

**Current behavior:**
- Returns direct line path (stub implementation)
- Uses `UnityEngine.Vector3` directly
- Comment: "В майбутньому може бути замінена на A* або інший алгоритм пошуку шляху"

### Five-Filter Assessment

#### 1. Needed by ANY RPG game?
**YES** — Pathfinding is universal for:
- Top-down RPGs (Diablo, Path of Exile)
- Turn-based RPGs (Divinity, XCOM)
- Action RPGs (Elden Ring, Dark Souls)
- Tactical RPGs (Fire Emblem, Final Fantasy Tactics)

#### 2. Depends on game-specific logic?
**PARTIALLY**
- Core pathfinding algorithms (A*, Dijkstra) — **NO** game dependency
- `UnityEngine.Vector3` — **YES** Unity dependency (not game-specific, but engine-specific)
- `movementPoints` parameter — **YES** game-specific (tactical RPG feature)

#### 3. Reusable across different RPG games?
**YES** — With abstraction:
- Position abstraction (not tied to Vector3)
- Path representation (generic waypoints)
- Navigation graph/grid abstraction

#### 4. Framework Core or Game Layer?
**BOTH**
- **Framework Core:** Abstract pathfinding contracts, A* algorithm, navigation graph
- **Game Layer:** Unity Vector3 adapter, movement points logic, MythHunter-specific obstacles

#### 5. Proof of problem?
**PARTIAL** — Stub exists with TODO comment. Real implementation needed.

### Extraction Decision
**EXTRACT to Framework with abstraction**

**Framework modules:**
- `RPGFramework.Pathfinding.Runtime` — Core contracts and algorithms
- Position abstraction (not Unity-specific)
- A* pathfinding algorithm
- Navigation graph abstraction

**Game Layer (MythHunter):**
- Unity Vector3 → Framework position adapter
- Unity NavMesh adapter (optional)
- Movement points validation
- Game-specific obstacle detection

---

## Phase 23.2 — Behavior Tree System

### Existing Code Location
**NOT FOUND** — No behavior tree implementation in current codebase.

### Five-Filter Assessment

#### 1. Needed by ANY RPG game?
**YES** — Behavior trees are standard for:
- Enemy AI (attack, patrol, flee)
- NPC behaviors (daily routines, reactions)
- Companion AI
- Boss fight patterns

#### 2. Depends on game-specific logic?
**NO** — Behavior tree structure is generic:
- Selector, Sequence, Decorator nodes are universal
- Leaf nodes (actions/conditions) can be game-specific implementations
- Tree executor is generic

#### 3. Reusable across different RPG games?
**YES** — Same behavior tree engine works for any game:
- Different games register different actions/conditions
- Tree structure is data-driven

#### 4. Framework Core or Game Layer?
**BOTH**
- **Framework Core:** Tree nodes, executor, blackboard
- **Game Layer:** Specific actions (AttackAction, PatrolAction, etc.)

#### 5. Proof of problem?
**NO** — No existing implementation. This is new functionality.

### Extraction Decision
**CREATE in Framework from scratch**

**Framework modules:**
- `RPGFramework.BehaviorTree.Runtime` — Core behavior tree engine
- Node types: Selector, Sequence, Decorator, Leaf
- Blackboard for shared state
- Tree executor

**Game Layer (MythHunter):**
- Concrete actions (Attack, Patrol, Flee, etc.)
- Concrete conditions (HealthBelow, EnemyInRange, etc.)
- Behavior tree data assets

---

## Phase 23.3 — State Machine System

### Existing Code Location
- `Assets/_MythHunter/Code/Core/StateMachine/IState.cs`
- `Assets/_MythHunter/Code/Core/StateMachine/BaseState.cs`
- `Assets/_MythHunter/Code/Core/StateMachine/GameStateMachine.cs`
- Individual states: `BootState`, `GameplayState`, `LobbyState`, etc.

### Current Implementation
```csharp
public interface IState<T> where T : Enum
{
    T StateId { get; }
    void Enter(T previousState);
    void Update();
    void Exit();
}
```

**Current usage:**
- `GameStateMachine` — manages game flow states (Boot, MainMenu, Gameplay, Lobby)
- States are game-specific (BootState loads game, GameplayState starts gameplay)
- Uses `UniTask` for async state transitions
- Tightly coupled to MythHunter DI and logging

### Five-Filter Assessment

#### 1. Needed by ANY RPG game?
**YES** — State machines are universal:
- Game flow (menu, loading, gameplay)
- Character states (idle, walking, attacking, dead)
- Quest states (not started, active, completed)
- Combat states (player turn, enemy turn, victory)

#### 2. Depends on game-specific logic?
**PARTIALLY**
- Generic state machine structure — **NO** dependency
- Specific states (BootState, GameplayState) — **YES** game-specific
- DI/logging integration — adapter dependency

#### 3. Reusable across different RPG games?
**YES** — Generic state machine works everywhere:
- Different games define different states
- Transition logic is configurable

#### 4. Framework Core or Game Layer?
**BOTH**
- **Framework Core:** `IState<T>`, `IStateMachine<T>`, generic executor
- **Game Layer:** Concrete game states, MythHunter-specific transitions

#### 5. Proof of problem?
**YES** — Existing working implementation in GameStateMachine.

### Extraction Decision
**EXTRACT to Framework (generic contracts only)**

**Framework modules:**
- `RPGFramework.StateMachine.Runtime` — Core contracts and generic state machine
- `IState<T>`, `IStateMachine<T>`
- Transition conditions abstraction
- Hierarchical state machine support

**Keep in Game Layer:**
- `GameStateMachine` (uses Framework contracts)
- All concrete game states (BootState, GameplayState, etc.)
- UniTask integration (game-specific async pattern)

---

## Phase 23.4 — Enemy AI System

### Existing Code Location
**NOT FOUND** — No enemy AI system in current codebase.

### Five-Filter Assessment

#### 1. Needed by ANY RPG game?
**YES** — Enemy AI is core to RPG combat:
- Target selection (closest, weakest, healer)
- Ability usage decisions
- Threat management
- Retreat/flee logic

#### 2. Depends on game-specific logic?
**PARTIALLY**
- Decision-making framework — **NO** dependency
- Specific combat abilities — **YES** game-specific
- Threat calculation — **CONFIGURABLE** (different games use different formulas)

#### 3. Reusable across different RPG games?
**YES** — With proper abstraction:
- Generic threat system
- Pluggable decision strategies
- Integration with Combat/Abilities systems

#### 4. Framework Core or Game Layer?
**BOTH**
- **Framework Core:** AI decision contracts, threat system, target selection
- **Game Layer:** Specific enemy behaviors, MythHunter combat integration

#### 5. Proof of problem?
**NO** — No existing implementation. New functionality needed.

### Extraction Decision
**CREATE in Framework with integration points**

**Framework modules:**
- `RPGFramework.AI.Runtime` — Core AI contracts
- Threat management system
- Target selection strategies
- Combat decision framework

**Dependencies:**
- `RPGFramework.Combat.Runtime` (already exists)
- `RPGFramework.Abilities.Runtime` (already exists)
- `RPGFramework.Stats.Runtime` (already exists)

**Game Layer (MythHunter):**
- Specific enemy archetypes (melee, ranged, caster)
- MythHunter combat logic integration
- AI difficulty tuning

---

## Phase 23.5 — NPC AI System

### Existing Code Location
**NOT FOUND** — No NPC AI system in current codebase.

### Five-Filter Assessment

#### 1. Needed by ANY RPG game?
**YES** — NPC behaviors are standard:
- Patrol/idle behaviors
- Dialogue triggers
- Quest markers
- Daily schedules (shopkeeper hours, guard patrols)

#### 2. Depends on game-specific logic?
**PARTIALLY**
- Schedule framework — **NO** dependency
- Specific NPC types — **YES** game-specific
- Dialogue triggers — **INTEGRATION** with Dialogue system

#### 3. Reusable across different RPG games?
**YES** — Generic NPC behavior framework:
- Time-based scheduling
- Patrol waypoints
- Interaction triggers

#### 4. Framework Core or Game Layer?
**BOTH**
- **Framework Core:** Schedule system, patrol behavior, trigger system
- **Game Layer:** Specific NPCs, MythHunter world integration

#### 5. Proof of problem?
**NO** — No existing implementation. New functionality needed.

### Extraction Decision
**CREATE in Framework with game integration**

**Framework modules:**
- `RPGFramework.AI.Runtime` (shared with Enemy AI)
- Schedule/routine system
- Patrol behavior
- Interaction trigger framework

**Dependencies:**
- `RPGFramework.Dialogue.Runtime` (already exists)
- `RPGFramework.Quests.Runtime` (already exists)

**Game Layer (MythHunter):**
- Specific NPC definitions
- MythHunter world/scene integration
- Quest marker logic

---

## Implementation Order (Dependency-Driven)

### Phase 1: State Machine (Foundation)
**Target:** Extract generic state machine from existing GameStateMachine
- Input: Working MythHunter GameStateMachine
- Output: `RPGFramework.StateMachine.Runtime`
- Risk: LOW (existing working code)

### Phase 2: Pathfinding
**Target:** Create abstract pathfinding with A* implementation
- Input: Stub PathfindingSystem
- Output: `RPGFramework.Pathfinding.Runtime`
- Risk: MEDIUM (new algorithm implementation)

### Phase 3: Behavior Tree
**Target:** Create behavior tree engine from scratch
- Input: None (new system)
- Output: `RPGFramework.BehaviorTree.Runtime`
- Risk: MEDIUM (new complex system)

### Phase 4: AI System (Enemy + NPC)
**Target:** Create unified AI framework
- Input: Existing Combat/Abilities/Dialogue/Quests systems
- Output: `RPGFramework.AI.Runtime`
- Dependencies: State Machine, Behavior Tree, Pathfinding
- Risk: HIGH (complex integration with many systems)

---

## Assembly Dependencies

```
RPGFramework.StateMachine.Runtime
  ├─ No Framework dependencies (pure .NET)

RPGFramework.Pathfinding.Runtime
  ├─ No Framework dependencies (pure algorithms)

RPGFramework.BehaviorTree.Runtime
  ├─ No Framework dependencies (pure .NET)

RPGFramework.AI.Runtime
  ├─ RPGFramework.ECS.Runtime (entity targeting)
  ├─ RPGFramework.Combat.Runtime (combat decisions)
  ├─ RPGFramework.Abilities.Runtime (ability usage)
  ├─ RPGFramework.Stats.Runtime (stat checks)
  ├─ RPGFramework.StateMachine.Runtime (AI states)
  ├─ RPGFramework.BehaviorTree.Runtime (behavior execution)
  ├─ RPGFramework.Pathfinding.Runtime (movement)
  ├─ RPGFramework.Dialogue.Runtime (NPC interactions)
  ├─ RPGFramework.Quests.Runtime (quest triggers)
```

---

## Unity Dependencies Assessment

### Vector3 Problem
Current `IPathfindingSystem` uses `UnityEngine.Vector3` — **violates Framework principle** (no Unity dependencies in Framework Core).

**Solution:**
- Framework uses abstract position (e.g., `struct Position2D { float X; float Y; }` or `Position3D`)
- Game Layer provides Vector3 ↔ Position adapter

### NavMesh Integration
Unity NavMesh is **Game Layer adapter**, not Framework Core.

**Pattern:**
```csharp
// Framework Core
public interface IPathfinder
{
    IReadOnlyList<Position3D> FindPath(Position3D start, Position3D goal);
}

// Game Layer (MythHunter)
public class UnityNavMeshPathfinder : IPathfinder
{
    public IReadOnlyList<Position3D> FindPath(Position3D start, Position3D goal)
    {
        // Convert to Vector3, use NavMesh, convert back
    }
}
```

---

## Validation Gates

For each phase:

1. **Framework Assembly:**
   - Zero Unity dependencies (`noEngineReferences: true`)
   - .NET test project compiles and passes
   - GitHub Actions passes

2. **Unity Integration:**
   - Unity EditMode tests pass
   - Full project compile with clean Console
   - Game bootstrap smoke test

3. **Game Layer Integration:**
   - Dual-component pattern (MythHunter adapter + Framework component)
   - Sample integration test demonstrating system working

---

## Rollback Plan

Each phase is independent PR:
- Phase 1 (State Machine) can merge without other phases
- Phase 2 (Pathfinding) can merge without other phases
- Phase 3 (Behavior Tree) can merge without other phases
- Phase 4 (AI) depends on 1-3 but can be reverted independently

If any phase fails validation, revert that PR and reassess.

---

## Open Questions

1. **Pathfinding Algorithms:**
   - A* only, or also Dijkstra, Theta*, JPS?
   - Grid-based, graph-based, or both?

2. **Behavior Tree Debugging:**
   - Runtime visualization needed?
   - Editor tooling in Phase 1 or deferred?

3. **State Machine Features:**
   - Hierarchical states (nested state machines)?
   - State history tracking?
   - Async state transitions (UniTask integration)?

4. **AI Difficulty:**
   - Framework responsibility or game-specific?
   - Parameterized decision weights?

5. **Performance:**
   - Pathfinding caching strategy?
   - Behavior tree execution frequency?
   - AI update batching?

---

## Conclusion

**EPIC 23 is feasible with phased extraction:**

- **Phase 1 (State Machine):** LOW risk — extract from existing code
- **Phase 2 (Pathfinding):** MEDIUM risk — new algorithm, clear requirements
- **Phase 3 (Behavior Tree):** MEDIUM risk — new system, well-documented pattern
- **Phase 4 (AI):** HIGH risk — complex integration, many dependencies

**Recommendation:** Start with **Phase 23.3 (State Machine)** as it has existing working code and is foundational for AI systems.

**Estimated Scope:**
- Phase 1: ~500 lines Framework + tests
- Phase 2: ~800 lines Framework + tests (A* algorithm)
- Phase 3: ~1000 lines Framework + tests (behavior tree engine)
- Phase 4: ~1500 lines Framework + tests (AI systems)
- **Total:** ~3800 lines + Game Layer integration

**Timeline Dependency:**
Phase 4 requires Phases 1-3 complete. Phases 1-3 can proceed in parallel if needed.
