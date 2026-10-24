# EPIC 07 Phase 2 - Research Findings

> Date: 2026-10-12
> Investigator: Claude Code
> Status: CRITICAL FINDINGS - Phase 2 Plan Needs Revision

## 🔴 Critical Finding: ISystemRegistry Conflict

### Current Initialization Flow (Phase 1)

```
GameBootstrapper.InitializeCore():
1. Modular Composition System Built:
   ├── FrameworkCoreModule.Configure()
   │   └── EMPTY (no registrations)
   │
   ├── GameLayerModule.Configure()
   │   └── new CoreInstaller().InstallBindings(container)
   │       ├── IEntityManager → EntityManager
   │       ├── IEventPool → EventPool
   │       ├── IEventBus → SimpleEventBus ⚠️
   │       ├── IGameStateMachine → GameStateMachine
   │       ├── ISystemRegistry → SystemRegistry ⚠️⚠️
   │       ├── IEcsWorld → EcsWorld
   │       ├── ... (9 more services)
   │       └── DIExtensionsInstaller.InstallBindings()
   │
   ├── InfrastructureModule.Configure()
   ├── GameplayModule.Configure()
   ├── [Optional] NetworkingModule.Configure()
   ├── [Optional] LobbyModule.Configure()
   └── [Optional] DebugToolsModule.Configure()

2. Composition Built and Initialized

3. Direct Installer Calls (AFTER modules):
   ├── LazyDependencyInjectorInstaller.InstallBindings()
   ├── EventsInstaller.InstallBindings()
   │   └── IEventBus → EventBus/NetworkEventBus ⚠️ OVERWRITES SimpleEventBus!
   ├── ProfileInstaller.InstallBindings()
   └── ParallelSystemsInstaller.InstallBindings()
       └── ISystemRegistry → ParallelSystemRegistry ⚠️⚠️ OVERWRITES SystemRegistry!
```

### Problem Statement

**Multiple conflicting registrations for critical services:**

1. **ISystemRegistry** - Registered TWICE:
   - `CoreInstaller` (via GameLayerModule): `SystemRegistry`
   - `ParallelSystemsInstaller` (after modules): `ParallelSystemRegistry` ← **WINS**

2. **IEventBus** - Registered TWICE:
   - `CoreInstaller` (via GameLayerModule): `SimpleEventBus`
   - `EventsInstaller` (after modules): `EventBus` or `NetworkEventBus` ← **WINS**

**Current behavior works by ACCIDENT** - the direct installer calls happen AFTER module configuration, so they overwrite the module registrations.

### Why This Is Critical

1. **Hidden Dependencies**:
   - GameLayerModule registers services that get silently overwritten
   - No warning, no error - just silent replacement

2. **Fragile Execution Order**:
   - If we move installers into modules (Phase 2 goal), order changes
   - Tests may break if execution order isn't correct

3. **Violates Single Responsibility**:
   - CoreInstaller registers 13+ services
   - Some are Framework (IEntityManager, ISystemRegistry)
   - Some are Game (IGameStateMachine, IGameFlowManager)
   - Some get overwritten anyway (ISystemRegistry, IEventBus)

## 📊 CoreInstaller Analysis

### File: `Assets/_MythHunter/Code/Core/Installers/CoreServices/CoreInstaller.cs`

**Lines**: 49
**Registers**: 13 services

| Service | Implementation | Layer | Overwritten? |
|---------|---------------|-------|--------------|
| IEntityManager | EntityManager | Framework ✅ | No |
| IEventPool | EventPool | Framework ✅ | No |
| **IEventBus** | **SimpleEventBus** | Framework | **YES** by EventsInstaller |
| IGameStateMachine | GameStateMachine | Game 🎮 | No |
| **ISystemRegistry** | **SystemRegistry** | Framework | **YES** by ParallelSystemsInstaller |
| IEcsWorld | EcsWorld | Framework ✅ | No |
| IDILifecycleManager | DILifecycleManager | Framework ✅ | No |
| IPhaseProvider | GamePhaseProvider | Game 🎮 | No |
| SceneLoader | SceneLoader | Game 🎮 | No |
| ISceneDispatcher | SceneDispatcher | Game 🎮 | No |
| IGameFlowManager | GameFlowManager | Game 🎮 | No |
| IGameSettingsService | GameSettingsService | Game 🎮 | No |
| AutoPreloadConfigurator | AutoPreloadConfigurator | Game 🎮 | No |

**Plus calls**: `DIExtensionsInstaller.InstallBindings()`

### Analysis

- **Framework services (7)**: IEntityManager, IEventPool, IEventBus, ISystemRegistry, IEcsWorld, IDILifecycleManager
- **Game services (6)**: IGameStateMachine, IPhaseProvider, SceneLoader, ISceneDispatcher, IGameFlowManager, IGameSettingsService, AutoPreloadConfigurator
- **Overwritten (2)**: IEventBus, ISystemRegistry

**Problem**: CoreInstaller is a **God Installer** that mixes Framework and Game concerns.

## 🎯 Phase 2 Strategy Options

### Option A: Keep CoreInstaller, Conditional Registrations

**Idea**: Modify CoreInstaller to SKIP registrations that will be overwritten

```csharp
public class CoreInstaller : DIInstaller
{
    public override void InstallBindings(IDIContainer container)
    {
        // Framework Core (always register)
        BindSingleton<IEntityManager, EntityManager>(container);
        BindSingleton<IEventPool, EventPool>(container);
        BindSingleton<IEcsWorld, EcsWorld>(container);
        BindSingleton<IDILifecycleManager, DILifecycleManager>(container);

        // SKIP IEventBus - EventsInstaller will handle
        // SKIP ISystemRegistry - ParallelSystemsInstaller will handle

        // Game Layer (always register)
        BindSingleton<IGameStateMachine, GameStateMachine>(container);
        BindSingleton<IPhaseProvider, GamePhaseProvider>(container);
        // ... etc
    }
}
```

**Pros**:
- Minimal changes to existing code
- Clear separation of concerns via comments

**Cons**:
- CoreInstaller still mixes Framework/Game
- Relies on manual coordination (comments, not enforcement)
- Fragile - easy to accidentally re-add registrations

### Option B: Split CoreInstaller into Framework + Game

**Idea**: Break CoreInstaller into two installers

**New Structure**:
```
FrameworkCoreInstaller (Framework services only):
├── IEntityManager → EntityManager
├── IEventPool → EventPool
├── IEcsWorld → EcsWorld
└── IDILifecycleManager → DILifecycleManager
(NO IEventBus, NO ISystemRegistry - modules handle those)

GameCoreInstaller (Game services only):
├── IGameStateMachine → GameStateMachine
├── IPhaseProvider → GamePhaseProvider
├── SceneLoader → SceneLoader
├── ISceneDispatcher → SceneDispatcher
├── IGameFlowManager → GameFlowManager
├── IGameSettingsService → GameSettingsService
└── AutoPreloadConfigurator → AutoPreloadConfigurator
```

**Module Usage**:
- `FrameworkCoreModule.Configure()` → calls `FrameworkCoreInstaller`
- `GameLayerModule.Configure()` → calls `GameCoreInstaller`
- `FrameworkCoreModule.Configure()` → ALSO registers `ISystemRegistry → ParallelSystemRegistry`
- `GameLayerModule.Configure()` → ALSO registers `IEventBus` (network-aware)

**Pros**:
- Clear Framework/Game separation
- Each module owns its registrations
- No overwrites, no conflicts
- Easier to test independently

**Cons**:
- More files (2 installers instead of 1)
- Requires refactoring CoreInstaller

### Option C: Inline All Registrations into Modules (No Installers)

**Idea**: Eliminate installers entirely, put registrations directly in modules

```csharp
FrameworkCoreModule.Configure():
    container.BindSingleton<IEntityManager, EntityManager>();
    container.BindSingleton<IEventPool, EventPool>();
    container.BindSingleton<ISystemRegistry, ParallelSystemRegistry>(); // Direct!
    container.BindSingleton<ISystemJobScheduler, SystemJobScheduler>();
    container.BindSingleton<IEcsWorld, EcsWorld>();
    container.BindSingleton<IDILifecycleManager, DILifecycleManager>();

GameLayerModule.Configure():
    container.BindSingleton<IGameStateMachine, GameStateMachine>();
    container.BindSingleton<IPhaseProvider, GamePhaseProvider>();
    // ... Network-aware event bus logic inline
    var networkMode = container.Resolve<EventNetworkMode>();
    if (networkMode == EventNetworkMode.Offline)
        container.BindSingleton<IEventBus, EventBus>();
    else
        // ...
```

**Pros**:
- Ultimate clarity - module owns its registrations
- No installer coordination needed
- No hidden dependencies
- Easier to understand module responsibilities

**Cons**:
- Most code changes
- Loses installer reusability (if needed elsewhere)
- Larger module files

## 🚨 Recommended Approach for Phase 2

**I recommend Option B: Split CoreInstaller**

### Rationale

1. **Clear Separation**: Framework vs Game is explicit
2. **Minimal Risk**: Keep installer pattern, just split it
3. **Maintainable**: Easy to understand which module owns what
4. **Testable**: Can test Framework and Game independently
5. **Future-Proof**: If we need to reuse installers elsewhere, we can

### Implementation Plan (Revised)

#### Step 1: Create FrameworkCoreInstaller

```csharp
// Assets/_Framework/Core/Installers/FrameworkCoreInstaller.cs
public class FrameworkCoreInstaller : DIInstaller
{
    public override void InstallBindings(IDIContainer container)
    {
        // Core ECS runtime - universal across all RPG games
        BindSingleton<IEntityManager, EntityManager>(container);
        BindSingleton<IEventPool, EventPool>(container);
        BindSingleton<IEcsWorld, EcsWorld>(container);
        BindSingleton<IDILifecycleManager, DILifecycleManager>(container);

        // Parallel system execution (Framework responsibility)
        BindSingleton<ISystemJobScheduler, SystemJobScheduler>(container);
        BindSingleton<ISystemRegistry, ParallelSystemRegistry>(container);

        // NO game-specific services here!
    }
}
```

#### Step 2: Create GameCoreInstaller

```csharp
// Assets/_MythHunter/Code/Core/Installers/GameCoreInstaller.cs
public class GameCoreInstaller : DIInstaller
{
    public override void InstallBindings(IDIContainer container)
    {
        // MythHunter game-specific core services
        BindSingleton<IGameStateMachine, GameStateMachine>(container);
        BindSingleton<IPhaseProvider, GamePhaseProvider>(container);
        BindSingleton<SceneLoader, SceneLoader>(container);
        BindSingleton<ISceneDispatcher, SceneDispatcher>(container);
        BindSingleton<IGameFlowManager, GameFlowManager>(container);
        BindSingleton<IGameSettingsService, GameSettingsService>(container);
        BindSingleton<AutoPreloadConfigurator, AutoPreloadConfigurator>(container);

        // DI extensions (creates GameObject with DontDestroyOnLoad)
        var diExtensionsInstaller = new DIExtensionsInstaller();
        diExtensionsInstaller.InstallBindings(container);

        // Event System (network-aware)
        RegisterEventSystem(container);
    }

    private void RegisterEventSystem(IDIContainer container)
    {
        var logger = container.Resolve<IMythLogger>();

        // Event infrastructure
        container.BindSingleton<IEventThrottler, EventThrottler>();
        container.BindSingleton<IEventThrottlerUpdateSystem, EventThrottlerUpdateSystem>();
        container.BindSingleton<IEventBatcher, EventBatcher>();
        container.BindSingleton<IEventStore, EventStore>();

        // Network-aware event bus selection
        var networkMode = container.Resolve<EventNetworkMode>();
        if (networkMode == EventNetworkMode.Offline)
        {
            logger.LogInfo("Offline mode: using EventBus");
            container.BindSingleton<IEventBus, EventBus>();
        }
        else
        {
            logger.LogInfo($"Network mode: {networkMode}, using NetworkEventBus");
            container.BindSingleton<INetworkEventBus, NetworkEventBus>();
            container.BindInstance<IEventBus>(container.Resolve<INetworkEventBus>());
        }

        // Timer system (registered in SystemRegistry)
        var systemRegistry = container.Resolve<ISystemRegistry>();
        container.BindSingleton<ITimerSystem, TimerSystem>();
        systemRegistry.RegisterSystem(container.Resolve<ITimerSystem>());

        container.Bind<EventLogger, EventLogger>();
    }
}
```

#### Step 3: Update FrameworkCoreModule

```csharp
protected override void OnConfigure()
{
    var container = Container as IDIContainer;
    // Register Framework Core services (including ParallelSystemRegistry)
    var frameworkInstaller = new RPGFramework.Core.Installers.FrameworkCoreInstaller();
    frameworkInstaller.InstallBindings(container);
}
```

#### Step 4: Update GameLayerModule

```csharp
protected override void OnConfigure()
{
    var container = Container as IDIContainer;
    // Register MythHunter Game Core services (including Events)
    var gameInstaller = new MythHunter.Core.Installers.GameCoreInstaller();
    gameInstaller.InstallBindings(container);
}
```

#### Step 5: Update InfrastructureModule

```csharp
protected override void OnConfigure()
{
    var container = Container as IDIContainer;

    // Existing: Resources, pooling, serialization, UI
    // ... existing code ...

    // ADD: Lazy Dependency Injector
    var lazyDIInstaller = new LazyDependencyInjectorInstaller();
    lazyDIInstaller.InstallBindings(container);

    // ADD: Profile Presenter
    container.BindSingleton<IProfilePresenter, ProfilePresenter>();
}
```

#### Step 6: Remove Direct Installer Calls from GameBootstrapper

```csharp
// DELETE these lines (110-120):
// var lazyInjectorInstaller = new MythHunter.Core.Installers.LazyDependencyInjectorInstaller();
// lazyInjectorInstaller.InstallBindings(_container);
//
// var eventsInstaller = new MythHunter.Core.Installers.EventsInstaller();
// eventsInstaller.InstallBindings(_container);
//
// var profileInstaller = new MythHunter.Core.Installers.ProfileInstaller();
// profileInstaller.InstallBindings(_container);
//
// var parallelSystemsInstaller = new MythHunter.Core.Installers.ParallelSystemsInstaller();
// parallelSystemsInstaller.InstallBindings(_container);
```

#### Step 7: Mark Old CoreInstaller as Obsolete

```csharp
[Obsolete("Split into FrameworkCoreInstaller and GameCoreInstaller. See EPIC-07-Phase2-Research-Findings.md")]
public class CoreInstaller : DIInstaller
{
    // Keep for backwards compatibility during transition
    // Remove in Phase 3
}
```

## 📝 Updated Phase 2 Checklist

- [ ] Create `RPGFramework.Core.Installers.FrameworkCoreInstaller`
- [ ] Create `MythHunter.Core.Installers.GameCoreInstaller` (includes EventsInstaller logic)
- [ ] Update `FrameworkCoreModule.OnConfigure()` to use FrameworkCoreInstaller
- [ ] Update `GameLayerModule.OnConfigure()` to use GameCoreInstaller
- [ ] Update `InfrastructureModule.OnConfigure()` to add LazyDI + Profile
- [ ] Remove 4 direct installer calls from GameBootstrapper
- [ ] Mark old `CoreInstaller` as [Obsolete]
- [ ] Full compilation check
- [ ] Run EditMode tests (target: 58/60+)
- [ ] Test offline mode
- [ ] Test multiplayer mode
- [ ] Verify no duplicate registrations
- [ ] Verify module initialization order
- [ ] Create PR for Phase 2

## 🎓 Key Learnings

1. **Hidden Overwrites Are Dangerous**: Current system works by accident (installer order)
2. **God Classes Are Fragile**: CoreInstaller mixes too many concerns
3. **Framework/Game Separation Matters**: Can't just move installers without understanding dependencies
4. **Execution Order Is Critical**: DI registration order determines what "wins"
5. **Testing Is Essential**: Need tests that verify WHICH implementation is registered

## 🔗 References

- Phase 2 Initial Plan: `EPIC-07-Phase2-Plan.md`
- CoreInstaller: `Assets/_MythHunter/Code/Core/Installers/CoreServices/CoreInstaller.cs:34`
- ParallelSystemsInstaller: `Assets/_MythHunter/Code/Core/Installers/CoreServices/ParallelSystemsInstaller.cs:23`
- EventsInstaller: `Assets/_MythHunter/Code/Core/Installers/CoreServices/EventsInstaller.cs:37`
- GameLayerModule: `Assets/_MythHunter/Code/Core/Composition/Modules/GameLayerModule.cs:44`
