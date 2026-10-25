# EPIC 23.3 — State Machine System Extraction

## Task Owner
Framework extraction — Phase 23.3 of EPIC 23 (AI & Behavior Systems)

## Objective
Extract generic state machine contracts and implementation from MythHunter's game-specific `GameStateMachine` into reusable Framework module.

## Current State Analysis

### Existing MythHunter Code
**Location:** `Assets/_MythHunter/Code/Core/StateMachine/`

**Files:**
- `IState.cs` — Generic state interface with `Enter`, `Update`, `Exit`
- `BaseState.cs` — Abstract base class with DI container, UniTask async support, context
- `GameStateMachine.cs` — Game-specific state machine for Boot/Menu/Gameplay flow
- Individual states: `BootState`, `GameplayState`, `LobbyState`, `MainMenuState`, etc.

**Dependencies:**
- `RPGFramework.DI` (IDIContainer)
- `Cysharp.Threading.Tasks` (UniTask for async state transitions)
- `MythHunter.Utils.Logging` (IMythLogger)

**Assessment:**
- `IState<T>` is **Framework-worthy** (generic, reusable)
- `BaseState<T>` has **mixed concerns** (DI/UniTask are game-specific)
- `GameStateMachine` is **Game Layer** (specific to MythHunter boot flow)

## Extraction Strategy

### What Goes to Framework
**Assembly:** `RPGFramework.StateMachine.Runtime`

**Contracts (pure .NET, no dependencies):**
1. `IState<TStateId>` — Core state interface
   - `TStateId StateId { get; }`
   - `void Enter(TStateId previousState)`
   - `void Update()`
   - `void Exit()`

2. `IStateMachine<TStateId>` — Core state machine interface
   - `TStateId CurrentState { get; }`
   - `void Initialize()`
   - `void Update()`
   - `void ChangeState(TStateId newState)`
   - `void RegisterState(IState<TStateId> state)`
   - `bool HasState(TStateId stateId)`

3. `StateMachine<TStateId>` — Generic implementation
   - State registration
   - Transition logic (Exit → Enter)
   - Update delegation
   - Validation (initialized, registered states)

**Constraints:**
- **Zero Unity dependencies** (`noEngineReferences: true`)
- **Zero game dependencies** (no DI, logging, UniTask)
- **Pure .NET** (works in any C# environment)

### What Stays in Game Layer (MythHunter)
1. `BaseState<T>` — Game-specific base class with:
   - DI container integration
   - UniTask async transitions
   - State context (object passing)
   - Inherits Framework `IState<T>`

2. `GameStateMachine` — MythHunter game flow controller:
   - Wraps Framework `StateMachine<GameStateType>`
   - Adds DI container
   - Adds logger
   - Async state transition support (UniTask)

3. All concrete states:
   - `BootState`, `GameplayState`, `LobbyState`, etc.
   - MythHunter-specific initialization logic

## Implementation

### Framework Module Structure
```
Assets/_Framework/StateMachine/
├── Runtime/
│   ├── RPGFramework.StateMachine.Runtime.asmdef
│   ├── IState.cs
│   ├── IStateMachine.cs
│   └── StateMachine.cs
└── Tests/
    ├── RPGFramework.StateMachine.Runtime.Tests.asmdef
    └── StateMachineTests.cs
```

### Test Coverage
**Framework tests (17 tests):**
1. Initialize — sets initialized flag
2. Initialize — throws on duplicate call
3. RegisterState — adds state to machine
4. RegisterState — null state throws ArgumentNullException
5. RegisterState — duplicate state throws InvalidOperationException
6. HasState — registered state returns true
7. HasState — unregistered state returns false
8. ChangeState — without Initialize throws
9. ChangeState — unregistered state throws
10. ChangeState — valid transition calls Enter on new state
11. ChangeState — valid transition calls Exit on previous state
12. ChangeState — transition chain maintains correct previousState
13. CurrentState — before any transition returns default
14. CurrentState — after transition returns new state
15. Update — without Initialize throws
16. Update — no current state does not throw
17. Update — with current state calls Update
18. Complex scenario — multiple transitions maintain lifecycle

## Game Layer Adapter Pattern

### MythHunter GameStateMachine Migration
**Before (current):**
```csharp
public class GameStateMachine : IGameStateMachine
{
    private Dictionary<GameStateType, BaseState<GameStateType>> _states;
    private BaseState<GameStateType> _currentState;
    // ... DI, logging, manual state management
}
```

**After (using Framework):**
```csharp
public class GameStateMachine : IGameStateMachine
{
    private readonly StateMachine<GameStateType> _stateMachine; // Framework
    private readonly IDIContainer _container;
    private readonly IMythLogger _logger;

    public GameStateMachine(IDIContainer container)
    {
        _stateMachine = new StateMachine<GameStateType>();
        _container = container;
        _logger = container.Resolve<IMythLogger>();
    }

    public void Initialize()
    {
        // Register states (with DI)
        _stateMachine.RegisterState(new BootState(_container));
        _stateMachine.RegisterState(new GameplayState(_container));
        // ...

        _stateMachine.Initialize();
        _stateMachine.ChangeState(GameStateType.Boot);
    }

    public void ChangeState(GameStateType newState)
    {
        _logger.LogInfo($"Changing state to {newState}");
        _stateMachine.ChangeState(newState); // Delegate to Framework
    }

    public GameStateType CurrentState => _stateMachine.CurrentState;
}
```

### BaseState Adapter
**Keep in MythHunter** (implements Framework `IState<T>`):
```csharp
public abstract class BaseState<T> : IState<T> where T : Enum
{
    protected readonly IDIContainer _container;
    protected object _stateContext;

    public abstract T StateId { get; }

    public virtual void Enter(T previousState) { }

    public virtual async UniTask EnterAsync(T previousState)
    {
        Enter(previousState);
        await UniTask.CompletedTask;
    }

    public virtual void Update() { }
    public virtual void Exit() { }

    public void SetStateContext(object context) => _stateContext = context;
}
```

## Validation Gates

### 1. Framework .NET Tests
- [ ] All 17 StateMachine tests pass
- [ ] Pure .NET compilation (no Unity/game dependencies)
- [ ] GitHub Actions workflow passes

### 2. Unity EditMode Tests
- [ ] Unity imports Framework assembly
- [ ] EditMode tests discover and pass all Framework tests
- [ ] Full project compiles with clean Console

### 3. Game Layer Integration
- [ ] MythHunter `GameStateMachine` migrated to use Framework
- [ ] All concrete states implement Framework `IState<GameStateType>`
- [ ] Game boots successfully through state transitions
- [ ] No behavioral regressions

## Acceptance Criteria

**Framework Module:**
- ✅ `RPGFramework.StateMachine.Runtime` assembly created
- ✅ `noEngineReferences: true` enforced
- ✅ Zero dependencies on Unity, DI, logging, or UniTask
- ✅ 17+ tests passing (characterization + lifecycle)
- ✅ .NET 8 test project compiles and passes
- ✅ GitHub Actions workflow created

**Game Layer:**
- [ ] MythHunter `GameStateMachine` uses Framework `StateMachine<T>`
- [ ] `BaseState<T>` implements Framework `IState<T>`
- [ ] All game states unchanged (Boot, Gameplay, Lobby, etc.)
- [ ] Unity EditMode tests pass (including existing game tests)
- [ ] Full project compile with clean Console
- [ ] Game bootstrap smoke test passes

## Rollback Plan

If integration fails:
1. Revert PR
2. Framework module remains (can be used later)
3. MythHunter continues using current `GameStateMachine`
4. No game functionality lost

## Branch Strategy

**Branch:** `feature/epic-23-3-state-machine`
**Base:** `dev`

**Commit sequence:**
1. Add Framework `RPGFramework.StateMachine.Runtime` + tests
2. Add .NET test project + GitHub workflow
3. (Next PR) Migrate MythHunter to use Framework contracts

**PR Title:** "feat: Add RPGFramework.StateMachine.Runtime (EPIC 23.3)"

## Open Questions

1. **Async State Transitions:**
   - Framework uses sync `Enter()`
   - MythHunter needs async `EnterAsync()` with UniTask
   - **Resolution:** Keep async in `BaseState<T>` adapter (Game Layer)

2. **State Context:**
   - Framework doesn't support `SetStateContext(object)`
   - MythHunter uses it for passing data between states
   - **Resolution:** Keep context in `BaseState<T>` adapter (Game Layer)

3. **Re-entering Same State:**
   - Current code allows re-entering same state
   - Should Framework prevent it or allow it?
   - **Resolution:** Allow it (commented option to disable)

4. **Hierarchical State Machines:**
   - Future enhancement (nested state machines)?
   - **Resolution:** Deferred to Phase 2 if needed

## Timeline

**Phase 1 (Current):** Framework extraction
- Framework assembly + tests ✅
- .NET test project ✅
- Unity validation ⏳
- PR merge ⏳

**Phase 2 (Next):** Game Layer migration
- Migrate `GameStateMachine` to use Framework
- Update `BaseState<T>` to implement `IState<T>`
- Verify all states work
- Full validation + merge

## Dependencies

**Upstream (Framework):**
- None (pure .NET)

**Downstream (Game Layer):**
- MythHunter `GameStateMachine`
- MythHunter `BaseState<T>`
- All concrete game states

**Future AI Systems (EPIC 23.4):**
- Enemy AI states (Idle, Patrol, Chase, Attack, Flee)
- NPC AI states (Idle, Wander, Interact, Sleep)
- Will use Framework `StateMachine<T>`

## Success Metrics

1. **Framework Independence:**
   - `noEngineReferences: true` validated
   - Zero Unity/game imports

2. **Test Coverage:**
   - 17+ Framework tests passing
   - 100% test pass rate in .NET and Unity

3. **Game Compatibility:**
   - MythHunter continues working
   - No performance regression
   - No behavioral changes

4. **Reusability:**
   - Any RPG game can use `StateMachine<T>`
   - No MythHunter-specific logic in Framework

---

**Status:** Framework module complete, awaiting Unity validation
**Next:** Unity EditMode test results, then Game Layer migration
